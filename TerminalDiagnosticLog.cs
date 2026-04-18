using System.Diagnostics;
using System.Text;

namespace MyFileExplorer
{
	/// <summary>
	/// Writes verbose terminal UI / shell diagnostics to a log file and <see cref="Debug"/>.
	/// Default path: <c>%LocalApplicationData%\MyFileExplorer\terminal-diagnostic.log</c>
	/// </summary>
	internal static class TerminalDiagnosticLog
	{
		private static readonly object _sync = new();
		private static readonly string? _filePath;

		static TerminalDiagnosticLog()
		{
			try
			{
				var dir = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"MyFileExplorer");
				Directory.CreateDirectory(dir);
				_filePath = Path.Combine(dir, "terminal-diagnostic.log");
			}
			catch
			{
				_filePath = null;
			}
		}

		/// <summary>When false, all writes are no-ops.</summary>
		public static bool Enabled { get; set; } = true;

		/// <summary>Full path to the diagnostic file, or null if initialization failed.</summary>
		public static string? FilePath => _filePath;

		/// <summary>Short, control-character-safe preview for logging shell lines (not full content).</summary>
		public static string SafePreview(string? value, int maxChars = 160)
		{
			if (string.IsNullOrEmpty(value))
				return "(empty)";

			var truncated = value.Length > maxChars;
			var span = truncated ? value.AsSpan(0, maxChars) : value.AsSpan();
			var sb = new StringBuilder(span.Length + 24);
			foreach (var c in span)
			{
				if (c == '\r')
					sb.Append("\\r");
				else if (c == '\n')
					sb.Append("\\n");
				else if (c == '\t')
					sb.Append("\\t");
				else if (char.IsControl(c))
					sb.Append("\\u").Append(((ushort)c).ToString("X4"));
				else
					sb.Append(c);
			}

			if (truncated)
				sb.Append("...(trunc)");
			return sb.ToString();
		}

		public static void Line(string category, string message)
		{
			if (!Enabled)
				return;

			var ts = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff K");
			var tid = Environment.CurrentManagedThreadId;
			var line = $"{ts} | thread={tid,5} | {category,-22} | {message}";

			Debug.WriteLine(line);

			if (_filePath == null)
				return;

			lock (_sync)
			{
				try
				{
					File.AppendAllText(_filePath, line + Environment.NewLine, Encoding.UTF8);
				}
				catch
				{
					// Swallow — diagnostics must not break the terminal.
				}
			}
		}

		public static void FocusSnapshot(string where, Control terminal, Control output, TextBox command)
		{
			Form? form = null;
			try
			{
				form = terminal.FindForm();
			}
			catch
			{
				// ignored
			}

			var active = form?.ActiveControl;
			var activeName = active?.Name ?? "(null)";
			var activeType = active?.GetType().Name ?? "?";

			Line("FocusSnapshot",
				$"{where} | Terminal.ContainsFocus={terminal.ContainsFocus} | " +
				$"output.Focused={output.Focused} ContainsFocus={output.ContainsFocus} Handle={(output.IsHandleCreated ? output.Handle : IntPtr.Zero)} | " +
				$"command.Focused={command.Focused} ContainsFocus={command.ContainsFocus} Handle={(command.IsHandleCreated ? command.Handle : IntPtr.Zero)} | " +
				$"Form.ActiveControl={activeName} ({activeType})");
		}
	}
}
