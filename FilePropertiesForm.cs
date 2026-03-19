namespace MyFileExplorer
{
	/// <summary>
	/// Displays properties of a file or folder (name, type, location, size, dates, attributes).
	/// </summary>
	public partial class FilePropertiesForm : Form
	{
		/// <summary>
		/// Shows the properties form for the given path (file or folder). Uses the owner as parent for centering.
		/// </summary>
		public static void ShowForPath(IWin32Window? owner, string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return;
			path = path.Trim();
			using var form = new FilePropertiesForm(path);
			form.ShowDialog(owner);
		}

		public FilePropertiesForm(string path)
		{
			InitializeComponent();
			LoadProperties(path);
		}

		private void LoadProperties(string path)
		{
			textBoxName.Text = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? path;
			textBoxLocation.Text = Path.GetDirectoryName(path) ?? "";

			try
			{
				if (Directory.Exists(path))
					LoadFolderProperties(path);
				else if (File.Exists(path))
					LoadFileProperties(path);
				else
					SetUnknown();
			}
			catch
			{
				SetUnknown();
			}

			Text = "Properties of \"" + textBoxName.Text + "\"";
		}

		private void LoadFolderProperties(string path)
		{
			var dir = new DirectoryInfo(path);
			textBoxType.Text = "File folder";
			textBoxSize.Text = FormatFolderSize(path);
			textBoxCreated.Text = FormatDateTime(GetCreationTime(dir));
			textBoxModified.Text = FormatDateTime(dir.LastWriteTime);
			textBoxAccessed.Text = FormatDateTime(dir.LastAccessTime);
			textBoxAttributes.Text = FormatAttributes(dir.Attributes);
		}

		private void LoadFileProperties(string path)
		{
			var file = new FileInfo(path);
			var ext = file.Extension;
			textBoxType.Text = string.IsNullOrEmpty(ext) ? "File" : ext.TrimStart('.').ToUpperInvariant() + " File";
			textBoxSize.Text = FormatFileSize(file.Length);
			textBoxCreated.Text = FormatDateTime(GetCreationTime(file));
			textBoxModified.Text = FormatDateTime(file.LastWriteTime);
			textBoxAccessed.Text = FormatDateTime(file.LastAccessTime);
			textBoxAttributes.Text = FormatAttributes(file.Attributes);
		}

		private static DateTime GetCreationTime(FileSystemInfo info)
		{
			try
			{
				return info.CreationTime;
			}
			catch
			{
				return default;
			}
		}

		private static string FormatDateTime(DateTime dt)
		{
			if (dt == default)
				return "";
			return dt.ToString("g");
		}

		private static string FormatFileSize(long bytes)
		{
			if (bytes < 1024)
				return $"{bytes} bytes";
			if (bytes < 1024 * 1024)
				return $"{bytes / 1024.0:F2} KB";
			if (bytes < 1024 * 1024 * 1024)
				return $"{bytes / (1024.0 * 1024):F2} MB";
			return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
		}

		private static string FormatFolderSize(string path)
		{
			try
			{
				int fileCount = 0, folderCount = 0;
				CountItems(path, ref fileCount, ref folderCount);
				return $"{fileCount} File(s), {folderCount} Folder(s)";
			}
			catch
			{
				return "—";
			}
		}

		private static void CountItems(string dirPath, ref int fileCount, ref int folderCount)
		{
			foreach (var d in Directory.GetDirectories(dirPath))
			{
				folderCount++;
				try
				{
					CountItems(d, ref fileCount, ref folderCount);
				}
				catch { /* skip inaccessible */ }
			}
			try
			{
				fileCount += Directory.GetFiles(dirPath).Length;
			}
			catch { }
		}

		private static string FormatAttributes(FileAttributes attrs)
		{
			var list = new List<string>();
			if ((attrs & FileAttributes.ReadOnly) != 0) list.Add("Read-only");
			if ((attrs & FileAttributes.Hidden) != 0) list.Add("Hidden");
			if ((attrs & FileAttributes.System) != 0) list.Add("System");
			if ((attrs & FileAttributes.Archive) != 0) list.Add("Archive");
			if ((attrs & FileAttributes.Directory) != 0) list.Add("Directory");
			if ((attrs & FileAttributes.Compressed) != 0) list.Add("Compressed");
			return list.Count == 0 ? "—" : string.Join(", ", list);
		}

		private void SetUnknown()
		{
			textBoxType.Text = "—";
			textBoxSize.Text = "—";
			textBoxCreated.Text = "—";
			textBoxModified.Text = "—";
			textBoxAccessed.Text = "—";
			textBoxAttributes.Text = "—";
		}

		private void ButtonOk_Click(object? sender, EventArgs e)
		{
			Close();
		}
	}
}
