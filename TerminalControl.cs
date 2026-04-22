using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that hosts an interactive shell session with command input and streaming output.
	/// </summary>
	public partial class TerminalControl : UserControl
	{
		/// <summary>
		/// When true (default), writes verbose diagnostics to <see cref="TerminalDiagnosticLog"/>.
		/// Log file: <c>%LocalApplicationData%\MyFileExplorer\terminal-diagnostic.log</c>
		/// </summary>
		public static bool TerminalDiagnosticLoggingEnabled
		{
			get => TerminalDiagnosticLog.Enabled;
			set => TerminalDiagnosticLog.Enabled = value;
		}

		/// <summary>Path to the diagnostic log file, or null if unavailable.</summary>
		public static string? TerminalDiagnosticLogFilePath => TerminalDiagnosticLog.FilePath;

		private const int MaxPersistedOutputChars = 64 * 1024;
		private const int MaxRuntimeOutputChars = 16 * 1024 * 1024;
		private const int MaxHistoryEntriesPerDirectory = 100;
		private const int MaxHistoryDirectories = 200;
		private const string DefaultHistoryDirectoryKey = "__default__";
		private readonly object _suppressedOutputLock = new();
		private readonly List<string> _suppressedOutputFragments = new();
		private readonly Dictionary<string, List<string>> _directoryHistory = new(StringComparer.OrdinalIgnoreCase);
		private Process? _shellProcess;
		private StreamWriter? _shellInput;
		private bool _suppressShellSelectionChanged;
		private TerminalShellType _shellType = TerminalShellType.PowerShell;
		private string _lastKnownWorkingDirectory = string.Empty;
		private int _historyIndex = -1;
		private string _historyDraftInput = string.Empty;
		private string _historyContextKey = DefaultHistoryDirectoryKey;
		private int _diagnosticAppendLineCount;
		private bool _pendingFocusCommandInput;
		private int _runtimeOutputCharEstimate;
		private int _outputLineMaxPixelWidth;
		private string _shellStartupWorkingDirectory = string.Empty;

		/// <summary>
		/// Raised when a new line of output is appended.
		/// </summary>
		[Category("Action")]
		[Description("Raised when a new output line is received from the shell process.")]
		public event EventHandler<TerminalOutputEventArgs>? OutputReceived;

		/// <summary>
		/// Raised when a command is sent to the shell process.
		/// </summary>
		[Category("Action")]
		[Description("Raised when a command is sent to the shell process.")]
		public event EventHandler<TerminalCommandEventArgs>? CommandSent;

		/// <summary>
		/// Raised when the shell process exits.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the shell process exits.")]
		public event EventHandler? ShellExited;

		/// <summary>
		/// Gets the read-only output log (<see cref="TerminalOutputListBox"/>): one shell line per item, not an Edit control, so no insertion caret appears over the stream.
		/// </summary>
		[Browsable(false)]
		public ListBox OutputListBox => outputListBox;

		/// <summary>
		/// Gets the command input text box.
		/// </summary>
		[Browsable(false)]
		public TextBox CommandTextBox => commandTextBox;

		/// <summary>
		/// Gets or sets the selected shell type.
		/// </summary>
		[Category("Behavior")]
		[Description("Shell process used by the terminal. Changing this while running restarts the terminal process.")]
		[DefaultValue(TerminalShellType.PowerShell)]
		public TerminalShellType ShellType
		{
			get => _shellType;
			set
			{
				if (_shellType == value)
					return;
				_shellType = value;
				ApplyShellTypeToCombo();
				if (IsShellRunning)
					RestartShell();
			}
		}

		/// <summary>
		/// Gets or sets whether the shell should start automatically when the control handle is created.
		/// </summary>
		[Category("Behavior")]
		[Description("When true, starts the shell automatically when the control is created.")]
		[DefaultValue(true)]
		public bool AutoStartShell { get; set; } = true;

		/// <summary>
		/// Gets whether the shell process is currently running.
		/// </summary>
		[Browsable(false)]
		public bool IsShellRunning => _shellProcess is { HasExited: false };

		public TerminalControl()
		{
			InitializeComponent();
			TerminalDiagnosticLog.Line("Init",
				$"TerminalControl ctor; diagnostic file={TerminalDiagnosticLog.FilePath ?? "(none)"}");
			outputListBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
			outputListBox.GotFocus += OutputListBox_GotFocus;
			outputListBox.LostFocus += (_, _) =>
				TerminalDiagnosticLog.Line("output.LostFocus(ev)", "LostFocus event on output ListBox");
			commandTextBox.GotFocus += (_, _) =>
				TerminalDiagnosticLog.FocusSnapshot("command.GotFocus(ev)", this, outputListBox, commandTextBox);
			commandTextBox.LostFocus += (_, _) =>
				TerminalDiagnosticLog.Line("command.LostFocus(ev)", "LostFocus event on command TextBox");
			commandTextBox.HandleCreated += (_, _) =>
			{
				TerminalDiagnosticLog.Line("command.HandleCreated", $"Handle={commandTextBox.Handle}");
				FlushPendingFocusCommandInput("command.HandleCreated");
			};
			PopulateShellCombo();
			UpdateStartStopButtonText();
		}

		protected override void OnEnter(EventArgs e)
		{
			TerminalDiagnosticLog.Line("TerminalControl.OnEnter", "FocusCommandInput before base.OnEnter (child WM_SETFOCUS can run first otherwise)");
			ScheduleFocusCommandInput();
			base.OnEnter(e);
			TerminalDiagnosticLog.Line("TerminalControl.OnEnter", "after base.OnEnter");
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			TerminalDiagnosticLog.Line("TerminalControl.OnHandleCreated",
				$"DesignMode={IsInDesignMode()} AutoStartShell={AutoStartShell} Handle={Handle}");
			FlushPendingFocusCommandInput("TerminalControl.OnHandleCreated");
			if (IsInDesignMode())
				return;
			if (AutoStartShell)
				StartShell();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			TerminalDiagnosticLog.Line("TerminalControl.OnHandleDestroyed", $"Handle={Handle}");
			StopShell();
			base.OnHandleDestroyed(e);
		}

		/// <summary>
		/// Starts the shell process if it is not already running.
		/// </summary>
		public void StartShell()
		{
			TerminalDiagnosticLog.Line("StartShell", $"enter IsDesign={IsInDesignMode()} IsRunning={IsShellRunning} ShellType={_shellType}");
			if (IsInDesignMode() || IsShellRunning)
				return;

			ProcessStartInfo startInfo;
			try
			{
				startInfo = BuildStartInfo(_shellType, _lastKnownWorkingDirectory);
				TerminalDiagnosticLog.Line("StartShell",
					$"BuildStartInfo FileName={startInfo.FileName} Args={TerminalDiagnosticLog.SafePreview(startInfo.Arguments, 120)} WD={startInfo.WorkingDirectory}");
			}
			catch (Exception ex)
			{
				TerminalDiagnosticLog.Line("StartShell", $"BuildStartInfo failed: {ex.GetType().Name}: {ex.Message}");
				AppendOutputLine($"Failed to resolve shell executable: {ex.Message}");
				return;
			}

			var process = new Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			};

			process.OutputDataReceived += ShellProcess_OutputDataReceived;
			process.ErrorDataReceived += ShellProcess_ErrorDataReceived;
			process.Exited += ShellProcess_Exited;

			try
			{
				process.Start();
				_shellProcess = process;
				_shellInput = process.StandardInput;
				_shellInput.AutoFlush = true;
				_shellStartupWorkingDirectory = startInfo.WorkingDirectory ?? string.Empty;
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				TerminalDiagnosticLog.Line("StartShell",
					$"process started Id={process.Id} HasExited={process.HasExited} startupWD={_shellStartupWorkingDirectory}");
				AppendOutputLine($"Started {_shellType} session.");
			}
			catch (Exception ex)
			{
				TerminalDiagnosticLog.Line("StartShell", $"process.Start failed: {ex.GetType().Name}: {ex.Message}");
				AppendOutputLine($"Failed to start shell: {ex.Message}");
				process.OutputDataReceived -= ShellProcess_OutputDataReceived;
				process.ErrorDataReceived -= ShellProcess_ErrorDataReceived;
				process.Exited -= ShellProcess_Exited;
				process.Dispose();
				_shellProcess = null;
				_shellInput = null;
			}
			finally
			{
				UpdateStartStopButtonText();
				// Shell can start from SendCommand before child handles exist; defer focus until command HWND exists.
				ScheduleFocusCommandInput();
			}
		}

		/// <summary>
		/// Stops the shell process if it is running.
		/// </summary>
		public void StopShell()
		{
			TerminalDiagnosticLog.Line("StopShell", $"enter hadProcess={_shellProcess != null}");
			var process = _shellProcess;
			var input = _shellInput;
			_shellProcess = null;
			_shellInput = null;

			if (input != null)
			{
				try
				{
					input.WriteLine("exit");
					input.Flush();
				}
				catch
				{
					// Best effort.
				}
				finally
				{
					input.Dispose();
				}
			}

			if (process != null)
			{
				try
				{
					if (!process.HasExited)
					{
						if (!process.WaitForExit(1200))
							process.Kill(entireProcessTree: true);
					}
				}
				catch
				{
					// Process may already be gone.
				}
				finally
				{
					process.OutputDataReceived -= ShellProcess_OutputDataReceived;
					process.ErrorDataReceived -= ShellProcess_ErrorDataReceived;
					process.Exited -= ShellProcess_Exited;
					process.Dispose();
				}
			}

			UpdateStartStopButtonText();
			TerminalDiagnosticLog.Line("StopShell", "exit");
		}

		/// <summary>
		/// Sends a command to the running shell process.
		/// </summary>
		/// <param name="command">Command text to execute.</param>
		public void SendCommand(string command)
		{
			SendCommand(command, echoCommand: true);
		}

		internal void SyncWorkingDirectory(string path)
		{
			TerminalDiagnosticLog.Line("SyncWorkingDirectory", $"path={TerminalDiagnosticLog.SafePreview(path, 200)} exists={!string.IsNullOrWhiteSpace(path) && Directory.Exists(path)}");
			if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
				return;

			_lastKnownWorkingDirectory = NormalizePersistedDirectory(path);
			ResetHistoryNavigationIfContextChanged();
			var command = BuildSetDirectoryCommand(path);
			SendCommand(command, echoCommand: false, suppressOutput: true);
		}

		internal TerminalState CaptureState()
		{
			var outputText = JoinOutputListLines();
			TerminalDiagnosticLog.Line("CaptureState", $"raw outputLen={outputText.Length}");
			if (outputText.Length > MaxPersistedOutputChars)
				outputText = outputText[^MaxPersistedOutputChars..];

			return new TerminalState
			{
				ShellType = _shellType,
				LastWorkingDirectory = _lastKnownWorkingDirectory ?? string.Empty,
				OutputText = outputText,
				DirectoryHistory = CaptureDirectoryHistorySnapshot()
			};
		}

		internal void RestoreState(TerminalState? state)
		{
			if (state == null)
			{
				TerminalDiagnosticLog.Line("RestoreState", "state=null; skip");
				return;
			}

			TerminalDiagnosticLog.Line("RestoreState",
				$"ShellType={state.ShellType} outputLen={state.OutputText?.Length ?? 0} cwdLen={state.LastWorkingDirectory?.Length ?? 0} isShellRunning={IsShellRunning} startupWD={_shellStartupWorkingDirectory}");
			ShellType = state.ShellType;
			_lastKnownWorkingDirectory = NormalizePersistedDirectory(state.LastWorkingDirectory);
			TerminalDiagnosticLog.Line("RestoreState",
				$"normalized restored WD={_lastKnownWorkingDirectory} shellRunning={IsShellRunning}");
			if (IsShellRunning
				&& !string.IsNullOrWhiteSpace(_shellStartupWorkingDirectory)
				&& !string.IsNullOrWhiteSpace(_lastKnownWorkingDirectory)
				&& !string.Equals(_shellStartupWorkingDirectory, _lastKnownWorkingDirectory, StringComparison.OrdinalIgnoreCase))
			{
				TerminalDiagnosticLog.Line("RestoreState",
					"Detected startup/restored directory mismatch while shell already running. Persisted WD is not automatically applied in current restore flow.");
			}
			ApplyRestoredWorkingDirectoryToRunningShell();
			PopulateOutputListFromText(LimitPersistedOutput(state.OutputText ?? string.Empty));
			RestoreDirectoryHistory(state.DirectoryHistory);
			ResetHistoryNavigation();
			ScheduleFocusCommandInput();
		}

		private void ApplyRestoredWorkingDirectoryToRunningShell()
		{
			if (!IsShellRunning || string.IsNullOrWhiteSpace(_lastKnownWorkingDirectory))
				return;

			if (!Directory.Exists(_lastKnownWorkingDirectory))
			{
				TerminalDiagnosticLog.Line("RestoreState",
					$"Skip applying restored WD because directory does not exist: {_lastKnownWorkingDirectory}");
				return;
			}

			var command = BuildSetDirectoryCommand(_lastKnownWorkingDirectory);
			TerminalDiagnosticLog.Line("RestoreState",
				$"Applying restored working directory to running shell: {TerminalDiagnosticLog.SafePreview(_lastKnownWorkingDirectory, 200)}");
			SendCommand(command, echoCommand: false, suppressOutput: true);
		}

		private void SendCommand(string command, bool echoCommand, bool suppressOutput = false)
		{
			TerminalDiagnosticLog.Line("SendCommand",
				$"echo={echoCommand} suppress={suppressOutput} len={command?.Length ?? 0} preview={TerminalDiagnosticLog.SafePreview(command, 200)}");
			if (string.IsNullOrWhiteSpace(command))
				return;

			if (!IsShellRunning)
				StartShell();

			if (!IsShellRunning || _shellInput == null)
			{
				TerminalDiagnosticLog.Line("SendCommand", "aborted: shell not running or no stdin");
				return;
			}

			var trimmedCommand = command.Trim();
			if (IsClearCommand(trimmedCommand))
			{
				TerminalDiagnosticLog.Line("SendCommand", "clear command -> ClearOutput");
				ClearOutput();
				return;
			}

			if (suppressOutput)
			{
				lock (_suppressedOutputLock)
				{
					_suppressedOutputFragments.Add(trimmedCommand);
				}
				TerminalDiagnosticLog.Line("SendCommand", $"suppress fragment recorded: {TerminalDiagnosticLog.SafePreview(trimmedCommand, 80)}");
			}

			if (echoCommand)
				AppendOutputLine($"> {trimmedCommand}");
			TryUpdateWorkingDirectoryFromCommand(trimmedCommand);

			try
			{
				_shellInput.WriteLine(trimmedCommand);
				TerminalDiagnosticLog.Line("SendCommand", "WriteLine to stdin OK");
				if (echoCommand)
					CommandSent?.Invoke(this, new TerminalCommandEventArgs(trimmedCommand));
				TryRecordCommandHistory(trimmedCommand, echoCommand);
			}
			catch (Exception ex)
			{
				TerminalDiagnosticLog.Line("SendCommand", $"WriteLine failed: {ex.GetType().Name}: {ex.Message}");
				AppendOutputLine($"Failed to send command: {ex.Message}");
			}
		}

		public void ClearOutput()
		{
			TerminalDiagnosticLog.Line("ClearOutput", $"before clear items={outputListBox.Items.Count} estChars={_runtimeOutputCharEstimate}");
			outputListBox.Items.Clear();
			_runtimeOutputCharEstimate = 0;
			_outputLineMaxPixelWidth = 0;
			outputListBox.HorizontalExtent = 0;
			TerminalDiagnosticLog.Line("ClearOutput", "after clear");
		}

		private void PopulateShellCombo()
		{
			_suppressShellSelectionChanged = true;
			shellComboBox.Items.Clear();
			shellComboBox.Items.Add(TerminalShellType.PowerShell);
			shellComboBox.Items.Add(TerminalShellType.CommandPrompt);
			shellComboBox.SelectedItem = _shellType;
			_suppressShellSelectionChanged = false;
		}

		private void ApplyShellTypeToCombo()
		{
			_suppressShellSelectionChanged = true;
			shellComboBox.SelectedItem = _shellType;
			_suppressShellSelectionChanged = false;
		}

		private void RestartShell()
		{
			TerminalDiagnosticLog.Line("RestartShell", "enter");
			StopShell();
			StartShell();
			TerminalDiagnosticLog.Line("RestartShell", "exit");
		}

		private static ProcessStartInfo BuildStartInfo(TerminalShellType shellType, string? workingDirectory)
		{
			var (fileName, arguments) = shellType switch
			{
				TerminalShellType.PowerShell => ("powershell.exe", "-NoLogo -NoExit"),
				TerminalShellType.CommandPrompt => ("cmd.exe", "/Q /K"),
				_ => throw new InvalidOperationException($"Unsupported shell type '{shellType}'.")
			};

			return new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				WorkingDirectory = ResolveWorkingDirectory(workingDirectory),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding = Encoding.UTF8
			};
		}

		private static string ResolveWorkingDirectory(string? workingDirectory)
		{
			if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
				return workingDirectory;

			return Environment.CurrentDirectory;
		}

		private void AppendOutputLine(string line)
		{
			if (IsDisposed)
			{
				TerminalDiagnosticLog.Line("AppendOutputLine", "skip: disposed");
				return;
			}

			if (ShouldSuppressOutputLine(line))
			{
				TerminalDiagnosticLog.Line("AppendOutputLine", $"suppressed line len={line?.Length ?? 0} preview={TerminalDiagnosticLog.SafePreview(line, 100)}");
				return;
			}

			if (outputListBox.InvokeRequired)
			{
				TerminalDiagnosticLog.Line("AppendOutputLine",
					$"BeginInvoke marshal len={line?.Length ?? 0} preview={TerminalDiagnosticLog.SafePreview(line, 100)}");
				outputListBox.BeginInvoke(new Action<string>(AppendOutputLine), line);
				return;
			}

			var beforeCount = outputListBox.Items.Count;
			_diagnosticAppendLineCount++;
			var previewNote = _diagnosticAppendLineCount <= 400
				? TerminalDiagnosticLog.SafePreview(line, 120)
				: $"(preview omitted after 400 lines; n={_diagnosticAppendLineCount})";
			TerminalDiagnosticLog.Line("AppendOutputLine",
				$"UI beforeItems={beforeCount} lineLen={line?.Length ?? 0} n={_diagnosticAppendLineCount} preview={previewNote}");

			var normalized = line ?? string.Empty;
			outputListBox.Items.Add(normalized);
			_runtimeOutputCharEstimate += normalized.Length + Environment.NewLine.Length;
			TrimOutputHeadIfOverBudget();
			UpdateOutputHorizontalExtentForLine(normalized);
			ScrollOutputListToBottom();
			TerminalDiagnosticLog.Line("AppendOutputLine",
				$"after items={outputListBox.Items.Count} estChars={_runtimeOutputCharEstimate}");
			OutputReceived?.Invoke(this, new TerminalOutputEventArgs(normalized));
		}

		private bool ShouldSuppressOutputLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
				return false;

			lock (_suppressedOutputLock)
			{
				for (var i = 0; i < _suppressedOutputFragments.Count; i++)
				{
					var fragment = _suppressedOutputFragments[i];
					if (string.IsNullOrWhiteSpace(fragment))
						continue;
					if (!line.Contains(fragment, StringComparison.OrdinalIgnoreCase))
						continue;

					_suppressedOutputFragments.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		private bool IsInDesignMode() => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

		private void UpdateStartStopButtonText()
		{
			if (IsDisposed)
				return;

			if (startStopButton.InvokeRequired)
			{
				startStopButton.BeginInvoke(new Action(UpdateStartStopButtonText));
				return;
			}

			startStopButton.Text = IsShellRunning ? "Stop" : "Start";
		}

		private void StartStopButton_Click(object? sender, EventArgs e)
		{
			TerminalDiagnosticLog.Line("StartStopButton_Click", $"IsShellRunning={IsShellRunning}");
			if (IsShellRunning)
				StopShell();
			else
				StartShell();
			ScheduleFocusCommandInput();
		}

		private void ClearButton_Click(object? sender, EventArgs e)
		{
			TerminalDiagnosticLog.Line("ClearButton_Click", "click");
			ClearOutput();
		}

		private void CommandTextBox_KeyDown(object? sender, KeyEventArgs e)
		{
			TerminalDiagnosticLog.Line("command.KeyDown",
				$"KeyData={e.KeyData} KeyCode={e.KeyCode} Modifiers={e.Modifiers} SuppressKeyPress={e.SuppressKeyPress}");
			if (e.Control && e.KeyCode == Keys.L)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				TerminalDiagnosticLog.Line("command.KeyDown", "Ctrl+L -> ClearOutput");
				ClearOutput();
				ScheduleFocusCommandInput();
				return;
			}

			if (e.KeyCode == Keys.Up)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				TerminalDiagnosticLog.Line("command.KeyDown", "Up -> history");
				NavigateHistory(moveUp: true);
				return;
			}

			if (e.KeyCode == Keys.Down)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				TerminalDiagnosticLog.Line("command.KeyDown", "Down -> history");
				NavigateHistory(moveUp: false);
				return;
			}

			if (e.KeyCode != Keys.Enter)
				return;

			e.Handled = true;
			e.SuppressKeyPress = true;
			var command = commandTextBox.Text;
			TerminalDiagnosticLog.Line("command.KeyDown", $"Enter -> execute len={command.Length} preview={TerminalDiagnosticLog.SafePreview(command, 200)}");
			commandTextBox.Clear();
			SendCommand(command);
			ResetHistoryNavigation();
			ScheduleFocusCommandInput();
		}

		/// <summary>
		/// Clicking the log moves focus to the command line. There is no keystroke forwarding from the log to
		/// the input: Windows delivers character messages only to the HWND with keyboard focus; if the log had
		/// focus, typed input would stay in the read-only Edit (often a beep), not in the command box.
		/// </summary>
		private void OutputListBox_MouseDown(object? sender, MouseEventArgs e)
		{
			TerminalDiagnosticLog.Line("output.MouseDown",
				$"Button={e.Button} Clicks={e.Clicks} ({e.X},{e.Y}) -> FocusCommandInput");
			TerminalDiagnosticLog.FocusSnapshot("output.MouseDown.before", this, outputListBox, commandTextBox);
			ScheduleFocusCommandInput();
			TerminalDiagnosticLog.FocusSnapshot("output.MouseDown.after", this, outputListBox, commandTextBox);
		}

		private void OutputListBox_GotFocus(object? sender, EventArgs e)
		{
			TerminalDiagnosticLog.Line("output.GotFocus(ev)", "CLR GotFocus (see OutputLB.WndProc WM_SETFOCUS + DivertFocusFromOutput)");
			TerminalDiagnosticLog.FocusSnapshot("output.GotFocus(ev)", this, outputListBox, commandTextBox);
		}

		private void ShellComboBox_SelectedIndexChanged(object? sender, EventArgs e)
		{
			if (_suppressShellSelectionChanged || shellComboBox.SelectedItem is not TerminalShellType selected)
				return;

			if (_shellType == selected)
				return;

			TerminalDiagnosticLog.Line("ShellCombo", $"changed to {selected} (restart if running={IsShellRunning})");
			_shellType = selected;
			if (IsShellRunning)
				RestartShell();
		}

		private void ShellProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
				AppendOutputLine(e.Data);
		}

		private void ShellProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
				AppendOutputLine(e.Data);
		}

		private void ShellProcess_Exited(object? sender, EventArgs e)
		{
			TerminalDiagnosticLog.Line("shell.Exited", $"ShellType={_shellType}");
			AppendOutputLine($"[{_shellType}] session exited.");
			ShellExited?.Invoke(this, EventArgs.Empty);
			UpdateStartStopButtonText();
		}

		private static string LimitPersistedOutput(string output)
		{
			if (string.IsNullOrEmpty(output))
				return string.Empty;
			return output.Length <= MaxPersistedOutputChars
				? output
				: output[^MaxPersistedOutputChars..];
		}

		private static string NormalizePersistedDirectory(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return string.Empty;
			return Directory.Exists(path) ? path : string.Empty;
		}

		private void TryUpdateWorkingDirectoryFromCommand(string command)
		{
			if (string.IsNullOrWhiteSpace(command))
				return;

			if (_shellType == TerminalShellType.PowerShell)
			{
				if (TryExtractPowerShellLocation(command, out var path))
				{
					_lastKnownWorkingDirectory = path;
					ResetHistoryNavigationIfContextChanged();
				}
				return;
			}

			if (TryExtractCmdLocation(command, out var cmdPath))
			{
				_lastKnownWorkingDirectory = cmdPath;
				ResetHistoryNavigationIfContextChanged();
			}
		}

		private static bool TryExtractPowerShellLocation(string command, out string path)
		{
			path = string.Empty;
			const string prefix = "Set-Location -LiteralPath '";
			if (!command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !command.EndsWith('\''))
				return false;

			var content = command[prefix.Length..^1];
			content = content.Replace("''", "'", StringComparison.Ordinal);
			if (!Directory.Exists(content))
				return false;

			path = content;
			return true;
		}

		private static bool TryExtractCmdLocation(string command, out string path)
		{
			path = string.Empty;
			const string prefix = "cd /d \"";
			if (!command.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !command.EndsWith('"'))
				return false;

			var content = command[prefix.Length..^1];
			content = content.Replace("\"\"", "\"", StringComparison.Ordinal);
			if (!Directory.Exists(content))
				return false;

			path = content;
			return true;
		}

		private string BuildSetDirectoryCommand(string path)
		{
			if (_shellType == TerminalShellType.PowerShell)
			{
				var escaped = path.Replace("'", "''", StringComparison.Ordinal);
				return $"Set-Location -LiteralPath '{escaped}'";
			}

			var cmdEscaped = path.Replace("\"", "\"\"", StringComparison.Ordinal);
			return $"cd /d \"{cmdEscaped}\"";
		}

		private static bool IsClearCommand(string command)
		{
			return string.Equals(command, "clear", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(command, "cls", StringComparison.OrdinalIgnoreCase);
		}

		private void TryRecordCommandHistory(string command, bool isUserCommand)
		{
			if (!isUserCommand || string.IsNullOrWhiteSpace(command) || IsClearCommand(command))
				return;

			var key = GetCurrentHistoryKey();
			if (!_directoryHistory.TryGetValue(key, out var history))
			{
				history = new List<string>();
				_directoryHistory[key] = history;
			}

			if (history.Count == 0 || !string.Equals(history[^1], command, StringComparison.Ordinal))
				history.Add(command);

			if (history.Count > MaxHistoryEntriesPerDirectory)
				history.RemoveRange(0, history.Count - MaxHistoryEntriesPerDirectory);

			TrimDirectoryHistoryIfNeeded();
		}

		private void NavigateHistory(bool moveUp)
		{
			var history = GetCurrentDirectoryHistory();
			if (history.Count == 0)
				return;

			if (_historyIndex == -1)
			{
				_historyDraftInput = commandTextBox.Text;
				_historyIndex = history.Count;
			}

			if (moveUp)
			{
				if (_historyIndex <= 0)
					return;
				_historyIndex--;
				ApplyHistoryText(history[_historyIndex]);
				return;
			}

			if (_historyIndex < history.Count - 1)
			{
				_historyIndex++;
				ApplyHistoryText(history[_historyIndex]);
				return;
			}

			_historyIndex = history.Count;
			ApplyHistoryText(_historyDraftInput);
		}

		private void ApplyHistoryText(string text)
		{
			commandTextBox.Text = text ?? string.Empty;
			commandTextBox.SelectionStart = commandTextBox.TextLength;
			commandTextBox.SelectionLength = 0;
		}

		private List<string> GetCurrentDirectoryHistory()
		{
			var key = GetCurrentHistoryKey();
			if (_historyContextKey != key)
			{
				_historyContextKey = key;
				_historyIndex = -1;
				_historyDraftInput = string.Empty;
			}

			if (!_directoryHistory.TryGetValue(key, out var history))
			{
				history = new List<string>();
				_directoryHistory[key] = history;
			}

			return history;
		}

		private string GetCurrentHistoryKey()
		{
			var normalized = NormalizePersistedDirectory(_lastKnownWorkingDirectory);
			return string.IsNullOrWhiteSpace(normalized) ? DefaultHistoryDirectoryKey : normalized;
		}

		private void ResetHistoryNavigationIfContextChanged()
		{
			var key = GetCurrentHistoryKey();
			if (string.Equals(_historyContextKey, key, StringComparison.OrdinalIgnoreCase))
				return;

			_historyContextKey = key;
			_historyIndex = -1;
			_historyDraftInput = string.Empty;
		}

		private void ResetHistoryNavigation()
		{
			_historyContextKey = GetCurrentHistoryKey();
			_historyIndex = -1;
			_historyDraftInput = string.Empty;
		}

		private Dictionary<string, List<string>> CaptureDirectoryHistorySnapshot()
		{
			var snapshot = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var pair in _directoryHistory)
			{
				var key = NormalizeHistoryKey(pair.Key);
				if (string.IsNullOrWhiteSpace(key))
					continue;

				var items = pair.Value
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.TakeLast(MaxHistoryEntriesPerDirectory)
					.ToList();

				if (items.Count > 0)
					snapshot[key] = items;
			}

			return snapshot;
		}

		private void RestoreDirectoryHistory(Dictionary<string, List<string>>? persistedHistory)
		{
			_directoryHistory.Clear();
			if (persistedHistory == null || persistedHistory.Count == 0)
				return;

			foreach (var pair in persistedHistory)
			{
				var key = NormalizeHistoryKey(pair.Key);
				if (string.IsNullOrWhiteSpace(key) || pair.Value == null)
					continue;

				var list = pair.Value
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => x.Trim())
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.TakeLast(MaxHistoryEntriesPerDirectory)
					.ToList();

				if (list.Count > 0)
					_directoryHistory[key] = list;
			}

			TrimDirectoryHistoryIfNeeded();
		}

		private static string NormalizeHistoryKey(string? key)
		{
			if (string.IsNullOrWhiteSpace(key))
				return string.Empty;

			var normalized = key.Trim();
			if (string.Equals(normalized, DefaultHistoryDirectoryKey, StringComparison.Ordinal))
				return DefaultHistoryDirectoryKey;

			if (Directory.Exists(normalized))
			{
				try
				{
					normalized = Path.GetFullPath(normalized);
				}
				catch
				{
					// Keep original when full-path normalization fails.
				}
			}

			return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		private void TrimDirectoryHistoryIfNeeded()
		{
			if (_directoryHistory.Count <= MaxHistoryDirectories)
				return;

			var keysToRemove = _directoryHistory.Keys
				.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
				.Take(_directoryHistory.Count - MaxHistoryDirectories)
				.ToList();

			foreach (var key in keysToRemove)
				_directoryHistory.Remove(key);
		}

		/// <summary>
		/// Called from <see cref="TerminalOutputListBox"/> immediately after the output list receives
		/// <c>WM_SETFOCUS</c>, so keyboard focus moves to the command line without leaving the log focused.
		/// </summary>
		internal void DivertFocusFromOutput()
		{
			TerminalDiagnosticLog.Line("DivertFocusFromOutput", "after output WM_SETFOCUS");
			ScheduleFocusCommandInput();
		}

		/// <summary>
		/// Ensures focus moves to the command line once its handle exists (shell may start before child HWND creation).
		/// </summary>
		private void ScheduleFocusCommandInput()
		{
			if (IsDisposed)
				return;
			if (!IsHandleCreated || !commandTextBox.IsHandleCreated)
			{
				_pendingFocusCommandInput = true;
				TerminalDiagnosticLog.Line("ScheduleFocusCommandInput",
					$"pending focus; terminalHandle={IsHandleCreated} commandHandle={commandTextBox.IsHandleCreated}");
				return;
			}

			FocusCommandInput();
		}

		private void FlushPendingFocusCommandInput(string source)
		{
			if (!_pendingFocusCommandInput || IsDisposed)
				return;
			if (!IsHandleCreated || !commandTextBox.IsHandleCreated)
			{
				TerminalDiagnosticLog.Line("FlushPendingFocus",
					$"{source}: still waiting terminalHandle={IsHandleCreated} commandHandle={commandTextBox.IsHandleCreated}");
				return;
			}

			_pendingFocusCommandInput = false;
			TerminalDiagnosticLog.Line("FlushPendingFocus", $"{source}: running FocusCommandInput");
			FocusCommandInput();
		}

		private void FocusCommandInput()
		{
			if (IsDisposed || !commandTextBox.IsHandleCreated)
			{
				TerminalDiagnosticLog.Line("FocusCommandInput", "early exit: disposed or command handle missing");
				return;
			}

			if (commandTextBox.InvokeRequired)
			{
				TerminalDiagnosticLog.Line("FocusCommandInput", "BeginInvoke(FocusCommandInput) — not on UI thread");
				commandTextBox.BeginInvoke(new Action(FocusCommandInput));
				return;
			}

			TerminalDiagnosticLog.FocusSnapshot("FocusCommandInput.before", this, outputListBox, commandTextBox);
			if (!commandTextBox.Focused)
			{
				TerminalDiagnosticLog.Line("FocusCommandInput", "calling commandTextBox.Focus()");
				commandTextBox.Focus();
			}
			else
			{
				TerminalDiagnosticLog.Line("FocusCommandInput", "commandTextBox.Focused already true; skipping Focus()");
			}

			commandTextBox.SelectionStart = commandTextBox.TextLength;
			commandTextBox.SelectionLength = 0;
			PropagateActiveControlUpFrom(commandTextBox);
			TerminalDiagnosticLog.FocusSnapshot("FocusCommandInput.after", this, outputListBox, commandTextBox);
		}

		private static void PropagateActiveControlUpFrom(Control focusedChild)
		{
			for (var walk = focusedChild; walk.Parent is ContainerControl cc; walk = cc)
			{
				try
				{
					if (cc.ActiveControl != walk)
						cc.ActiveControl = walk;
				}
				catch (ArgumentException)
				{
					break;
				}
			}
		}

		private string JoinOutputListLines()
		{
			var n = outputListBox.Items.Count;
			if (n == 0)
				return string.Empty;
			var sb = new StringBuilder();
			for (var i = 0; i < n; i++)
			{
				if (i > 0)
					sb.AppendLine();
				sb.Append(outputListBox.Items[i]?.ToString() ?? string.Empty);
			}

			return sb.ToString();
		}

		private void PopulateOutputListFromText(string text)
		{
			outputListBox.BeginUpdate();
			try
			{
				outputListBox.Items.Clear();
				_runtimeOutputCharEstimate = 0;
				_outputLineMaxPixelWidth = 0;
				outputListBox.HorizontalExtent = 0;
				if (string.IsNullOrEmpty(text))
					return;
				foreach (var segment in text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
				{
					outputListBox.Items.Add(segment);
					_runtimeOutputCharEstimate += segment.Length + Environment.NewLine.Length;
				}

				RecomputeOutputHorizontalExtent();
			}
			finally
			{
				outputListBox.EndUpdate();
			}

			ScrollOutputListToBottom();
		}

		private void TrimOutputHeadIfOverBudget()
		{
			while (_runtimeOutputCharEstimate > MaxRuntimeOutputChars && outputListBox.Items.Count > 0)
			{
				var head = outputListBox.Items[0]?.ToString() ?? string.Empty;
				_runtimeOutputCharEstimate -= head.Length + Environment.NewLine.Length;
				outputListBox.Items.RemoveAt(0);
			}
		}

		private void UpdateOutputHorizontalExtentForLine(string line)
		{
			var w = TextRenderer.MeasureText(line, outputListBox.Font, Size.Empty,
				TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding).Width + 24;
			if (w <= _outputLineMaxPixelWidth)
				return;
			_outputLineMaxPixelWidth = w;
			outputListBox.HorizontalExtent = _outputLineMaxPixelWidth;
		}

		private void RecomputeOutputHorizontalExtent()
		{
			var max = 0;
			var font = outputListBox.Font;
			for (var i = 0; i < outputListBox.Items.Count; i++)
			{
				var s = outputListBox.Items[i]?.ToString() ?? string.Empty;
				var w = TextRenderer.MeasureText(s, font, Size.Empty,
					TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding).Width + 24;
				if (w > max)
					max = w;
			}

			_outputLineMaxPixelWidth = max;
			outputListBox.HorizontalExtent = max;
		}

		private void ScrollOutputListToBottom()
		{
			if (!outputListBox.IsHandleCreated)
				return;
			var count = outputListBox.Items.Count;
			if (count == 0)
				return;
			var ih = Math.Max(outputListBox.ItemHeight, 1);
			var visible = Math.Max(1, outputListBox.ClientSize.Height / ih);
			outputListBox.TopIndex = Math.Max(0, count - visible);
		}
	}

	/// <summary>
	/// Supported shell process types for <see cref="TerminalControl"/>.
	/// </summary>
	public enum TerminalShellType
	{
		PowerShell,
		CommandPrompt
	}

	/// <summary>
	/// Event args carrying one output line from the terminal.
	/// </summary>
	public sealed class TerminalOutputEventArgs : EventArgs
	{
		public string Line { get; }

		public TerminalOutputEventArgs(string line)
		{
			Line = line ?? string.Empty;
		}
	}

	/// <summary>
	/// Event args carrying a command submitted to the terminal.
	/// </summary>
	public sealed class TerminalCommandEventArgs : EventArgs
	{
		public string Command { get; }

		public TerminalCommandEventArgs(string command)
		{
			Command = command ?? string.Empty;
		}
	}
}
