using System.Text.Json;

namespace MyFileExplorer
{
	internal sealed class AppSessionState
	{
		public int SelectedTabIndex { get; set; }
		public List<ExplorerTabState> Tabs { get; set; } = new();
	}

	internal sealed class ExplorerTabState
	{
		public string CurrentPath { get; set; } = string.Empty;
		public string TreeSelectedPath { get; set; } = string.Empty;
		public int MainSplitterDistance { get; set; }
		public double MainSplitterRatio { get; set; }
		public int TerminalSplitterDistance { get; set; }
		public double TerminalSplitterRatio { get; set; }
		public TerminalState Terminal { get; set; } = new();
	}

	internal sealed class TerminalState
	{
		public TerminalShellType ShellType { get; set; } = TerminalShellType.PowerShell;
		public string LastWorkingDirectory { get; set; } = string.Empty;
		public string OutputText { get; set; } = string.Empty;
		public Dictionary<string, List<string>> DirectoryHistory { get; set; } = new();
	}

	internal static class SessionStateStore
	{
		private static readonly JsonSerializerOptions s_jsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		};

		private static readonly string s_sessionFilePath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"MyFileExplorer",
			"session.json");

		public static AppSessionState? Load()
		{
			try
			{
				if (!File.Exists(s_sessionFilePath))
					return null;

				var json = File.ReadAllText(s_sessionFilePath);
				return JsonSerializer.Deserialize<AppSessionState>(json, s_jsonOptions);
			}
			catch
			{
				return null;
			}
		}

		public static void Save(AppSessionState state)
		{
			ArgumentNullException.ThrowIfNull(state);
			try
			{
				var directory = Path.GetDirectoryName(s_sessionFilePath);
				if (!string.IsNullOrWhiteSpace(directory))
					Directory.CreateDirectory(directory);

				var json = JsonSerializer.Serialize(state, s_jsonOptions);
				File.WriteAllText(s_sessionFilePath, json);
			}
			catch
			{
				// Best effort persistence only.
			}
		}
	}
}
