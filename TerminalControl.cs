using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that hosts an interactive shell session with command input and streaming output.
	/// </summary>
	public partial class TerminalControl : UserControl
	{
		private const int MaxPersistedOutputChars = 64 * 1024;
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
		/// Gets the read-only output text box.
		/// </summary>
		[Browsable(false)]
		public RichTextBox OutputTextBox => outputTextBox;

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
			outputTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
			PopulateShellCombo();
			UpdateStartStopButtonText();
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			FocusCommandInput();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			if (IsInDesignMode())
				return;
			if (AutoStartShell)
				StartShell();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			StopShell();
			base.OnHandleDestroyed(e);
		}

		/// <summary>
		/// Starts the shell process if it is not already running.
		/// </summary>
		public void StartShell()
		{
			if (IsInDesignMode() || IsShellRunning)
				return;

			ProcessStartInfo startInfo;
			try
			{
				startInfo = BuildStartInfo(_shellType, _lastKnownWorkingDirectory);
			}
			catch (Exception ex)
			{
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
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				AppendOutputLine($"Started {_shellType} session.");
			}
			catch (Exception ex)
			{
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
				FocusCommandInput();
			}
		}

		/// <summary>
		/// Stops the shell process if it is running.
		/// </summary>
		public void StopShell()
		{
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
			if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
				return;

			_lastKnownWorkingDirectory = NormalizePersistedDirectory(path);
			ResetHistoryNavigationIfContextChanged();
			var command = BuildSetDirectoryCommand(path);
			SendCommand(command, echoCommand: false, suppressOutput: true);
		}

		internal TerminalState CaptureState()
		{
			var outputText = outputTextBox.Text ?? string.Empty;
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
				return;

			ShellType = state.ShellType;
			_lastKnownWorkingDirectory = NormalizePersistedDirectory(state.LastWorkingDirectory);
			outputTextBox.Text = LimitPersistedOutput(state.OutputText);
			outputTextBox.SelectionStart = outputTextBox.TextLength;
			ScrollOutputRichTextToBottom();
			RestoreDirectoryHistory(state.DirectoryHistory);
			ResetHistoryNavigation();
		}

		private void SendCommand(string command, bool echoCommand, bool suppressOutput = false)
		{
			if (string.IsNullOrWhiteSpace(command))
				return;

			if (!IsShellRunning)
				StartShell();

			if (!IsShellRunning || _shellInput == null)
				return;

			var trimmedCommand = command.Trim();
			if (IsClearCommand(trimmedCommand))
			{
				ClearOutput();
				return;
			}

			if (suppressOutput)
			{
				lock (_suppressedOutputLock)
				{
					_suppressedOutputFragments.Add(trimmedCommand);
				}
			}

			if (echoCommand)
				AppendOutputLine($"> {trimmedCommand}");
			TryUpdateWorkingDirectoryFromCommand(trimmedCommand);

			try
			{
				_shellInput.WriteLine(trimmedCommand);
				if (echoCommand)
					CommandSent?.Invoke(this, new TerminalCommandEventArgs(trimmedCommand));
				TryRecordCommandHistory(trimmedCommand, echoCommand);
			}
			catch (Exception ex)
			{
				AppendOutputLine($"Failed to send command: {ex.Message}");
			}
		}

		public void ClearOutput() => outputTextBox.Clear();

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
			StopShell();
			StartShell();
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
				return;
			if (ShouldSuppressOutputLine(line))
				return;

			if (outputTextBox.InvokeRequired)
			{
				outputTextBox.BeginInvoke(new Action<string>(AppendOutputLine), line);
				return;
			}

			outputTextBox.AppendText(line + Environment.NewLine);
			outputTextBox.SelectionStart = outputTextBox.TextLength;
			ScrollOutputRichTextToBottom();
			OutputReceived?.Invoke(this, new TerminalOutputEventArgs(line));
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
			if (IsShellRunning)
				StopShell();
			else
				StartShell();
			FocusCommandInput();
		}

		private void ClearButton_Click(object? sender, EventArgs e) => ClearOutput();

		private void CommandTextBox_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.L)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				ClearOutput();
				FocusCommandInput();
				return;
			}

			if (e.KeyCode == Keys.Up)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				NavigateHistory(moveUp: true);
				return;
			}

			if (e.KeyCode == Keys.Down)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;
				NavigateHistory(moveUp: false);
				return;
			}

			if (e.KeyCode != Keys.Enter)
				return;

			e.Handled = true;
			e.SuppressKeyPress = true;
			var command = commandTextBox.Text;
			commandTextBox.Clear();
			SendCommand(command);
			ResetHistoryNavigation();
			FocusCommandInput();
		}

		private void OutputTextBox_MouseDown(object? sender, MouseEventArgs e) => FocusCommandInput();

		private void ShellComboBox_SelectedIndexChanged(object? sender, EventArgs e)
		{
			if (_suppressShellSelectionChanged || shellComboBox.SelectedItem is not TerminalShellType selected)
				return;

			if (_shellType == selected)
				return;

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

		private void FocusCommandInput()
		{
			if (IsDisposed || !commandTextBox.IsHandleCreated)
				return;

			if (commandTextBox.InvokeRequired)
			{
				commandTextBox.BeginInvoke(new Action(FocusCommandInput));
				return;
			}

			if (!commandTextBox.Focused)
				commandTextBox.Focus();
			commandTextBox.SelectionStart = commandTextBox.TextLength;
			commandTextBox.SelectionLength = 0;
		}

		private void ScrollOutputRichTextToBottom()
		{
			if (!outputTextBox.IsHandleCreated)
				return;
			_ = SendMessageW(outputTextBox.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
		}

		private const int WM_VSCROLL = 0x0115;
		private const int SB_BOTTOM = 7;

		[DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
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
