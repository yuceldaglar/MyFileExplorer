using System.Text.Json;

namespace MyFileExplorer
{
	internal static class Program
	{
		/// <summary>
		/// Path items loaded from app settings (appsettings.json) at startup. Empty if the file is missing or invalid.
		/// </summary>
		public static IReadOnlyList<PathItem> SavedPathItems { get; private set; } = Array.Empty<PathItem>();

		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// To customize application configuration such as set high DPI settings or default font,
			// see https://aka.ms/applicationconfiguration.
			ApplicationConfiguration.Initialize();
			LoadPathItemsFromSettings();
			Application.Run(new Form1());
		}

		private static void LoadPathItemsFromSettings()
		{
			var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
			if (!File.Exists(path))
				return;
			try
			{
				var json = File.ReadAllText(path);
				var root = JsonSerializer.Deserialize<AppSettingsRoot>(json);
				var list = root?.PathItems ?? new List<PathItem>();
				// Substitute default paths when Path is empty so the tree shows folders
				SavedPathItems = list
					.Select(item => string.IsNullOrWhiteSpace(item.Path)
						? new PathItem(item.Name, GetDefaultPathForName(item.Name))
						: item)
					.ToList();
			}
			catch
			{
				SavedPathItems = Array.Empty<PathItem>();
			}
		}

		private static string GetDefaultPathForName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return string.Empty;
			var key = name.Trim();
			return key.ToUpperInvariant() switch
			{
				"DESKTOP" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				"DOCUMENTS" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				"DOWNLOAD" or "DOWNLOADS" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
				"MUSIC" => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
				"PICTURES" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				"VIDEOS" or "VIDEO" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
				_ => string.Empty
			};
		}

		private sealed class AppSettingsRoot
		{
			public List<PathItem>? PathItems { get; set; }
		}
	}
}