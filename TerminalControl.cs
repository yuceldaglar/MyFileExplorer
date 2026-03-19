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
		private Process? _shellProcess;
		private StreamWriter? _shellInput;
		private bool _suppressShellSelectionChanged;
		private TerminalShellType _shellType = TerminalShellType.PowerShell;

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
				startInfo = BuildStartInfo(_shellType);
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
			if (string.IsNullOrWhiteSpace(command))
				return;

			if (!IsShellRunning)
				StartShell();

			if (!IsShellRunning || _shellInput == null)
				return;

			var trimmedCommand = command.Trim();
			AppendOutputLine($"> {trimmedCommand}");

			try
			{
				_shellInput.WriteLine(trimmedCommand);
				CommandSent?.Invoke(this, new TerminalCommandEventArgs(trimmedCommand));
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

		private static ProcessStartInfo BuildStartInfo(TerminalShellType shellType)
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
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding = Encoding.UTF8
			};
		}

		private void AppendOutputLine(string line)
		{
			if (IsDisposed)
				return;

			if (outputTextBox.InvokeRequired)
			{
				outputTextBox.BeginInvoke(new Action<string>(AppendOutputLine), line);
				return;
			}

			outputTextBox.AppendText(line + Environment.NewLine);
			outputTextBox.SelectionStart = outputTextBox.TextLength;
			outputTextBox.ScrollToCaret();
			OutputReceived?.Invoke(this, new TerminalOutputEventArgs(line));
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
			if (e.KeyCode != Keys.Enter)
				return;

			e.Handled = true;
			e.SuppressKeyPress = true;
			var command = commandTextBox.Text;
			commandTextBox.Clear();
			SendCommand(command);
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
