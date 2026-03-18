using System.ComponentModel;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that displays the contents of a folder with a toolbar and configurable view style (like Windows File Explorer).
	/// </summary>
	public partial class FolderContentsControl : UserControl
	{
		private string _currentPath = string.Empty;
		private View _currentView = View.LargeIcon;

		/// <summary>
		/// Gets the ListView that displays the folder contents. Use this to handle selection or customize appearance.
		/// </summary>
		[Browsable(false)]
		public ListView ListView => listView;

		/// <summary>
		/// Gets or sets the folder path whose contents are displayed. Setting this property refreshes the list.
		/// </summary>
		[Category("Behavior")]
		[Description("The folder path to display. Contents are refreshed when this is set.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentPath
		{
			get => _currentPath;
			set
			{
				if (string.Equals(_currentPath, value, StringComparison.OrdinalIgnoreCase))
					return;
				_currentPath = value ?? string.Empty;
				LoadContents();
			}
		}

		/// <summary>
		/// Gets or sets the current view style (Large Icons, Small Icons, List, Details, Tiles).
		/// </summary>
		[Category("Appearance")]
		[Description("The view style for folder contents.")]
		[DefaultValue(View.LargeIcon)]
		public View ContentsView
		{
			get => _currentView;
			set
			{
				if (_currentView == value)
					return;
				_currentView = value;
				listView.View = value;
				EnsureDetailsColumns();
				UpdateViewMenuCheckState();
			}
		}

		/// <summary>
		/// Occurs when the user double-clicks a folder, so the host can navigate into it. Event args contain the folder path.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the user double-clicks a folder to navigate into it.")]
		public event EventHandler<FolderEventArgs>? FolderDoubleClick;

		public FolderContentsControl()
		{
			InitializeComponent();
			SetupImageLists();
			EnsureDetailsColumns();
			UpdateViewMenuCheckState();
		}

		private void SetupImageLists()
		{
			listView.LargeImageList = new ImageList
			{
				ImageSize = new Size(32, 32),
				ColorDepth = ColorDepth.Depth32Bit
			};
			listView.SmallImageList = new ImageList
			{
				ImageSize = new Size(16, 16),
				ColorDepth = ColorDepth.Depth32Bit
			};
			ShellIconHelper.AddFolderAndFileIcons(listView.LargeImageList, listView.SmallImageList);
		}

		private void EnsureDetailsColumns()
		{
			if (listView.View != View.Details)
			{
				if (listView.Columns.Count > 0)
					listView.Columns.Clear();
				return;
			}
			if (listView.Columns.Count == 0)
			{
				listView.Columns.Add("Name", 200);
				listView.Columns.Add("Size", 80);
				listView.Columns.Add("Type", 100);
				listView.Columns.Add("Date Modified", 120);
			}
		}

		private void LoadContents()
		{
			listView.Items.Clear();

			if (string.IsNullOrWhiteSpace(_currentPath))
				return;

			var dir = new DirectoryInfo(_currentPath);
			if (!dir.Exists)
				return;

			try
			{
				var comparer = StringComparer.OrdinalIgnoreCase;
				// Folders first
				foreach (var subDir in dir.GetDirectories().OrderBy(d => d.Name, comparer))
					AddFolderItem(subDir);
				// Then files
				foreach (var file in dir.GetFiles().OrderBy(f => f.Name, comparer))
					AddFileItem(file);
			}
			catch (UnauthorizedAccessException)
			{
				var item = new ListViewItem("(Access denied)") { ForeColor = Color.Gray };
				listView.Items.Add(item);
			}
			catch (DirectoryNotFoundException)
			{
				var item = new ListViewItem("(Not found)") { ForeColor = Color.Gray };
				listView.Items.Add(item);
			}
		}

		private void AddFolderItem(DirectoryInfo subDir)
		{
			const int folderIconIndex = 0;
			var item = new ListViewItem(subDir.Name, folderIconIndex)
			{
				Tag = new FolderItemTag(subDir.FullName, true, subDir.LastWriteTime, 0, "File folder")
			};
			AddDetailsSubitems(item, subDir.Name, 0, "File folder", subDir.LastWriteTime);
			listView.Items.Add(item);
		}

		private void AddFileItem(FileInfo file)
		{
			const int fileIconIndex = 1;
			var typeDesc = GetFileTypeDescription(file.Extension);
			var item = new ListViewItem(file.Name, fileIconIndex)
			{
				Tag = new FolderItemTag(file.FullName, false, file.LastWriteTime, file.Length, typeDesc)
			};
			AddDetailsSubitems(item, file.Name, file.Length, typeDesc, file.LastWriteTime);
			listView.Items.Add(item);
		}

		private static void AddDetailsSubitems(ListViewItem item, string name, long size, string typeDesc, DateTime dateModified)
		{
			item.SubItems.Add(size == 0 ? "" : FormatSize(size));
			item.SubItems.Add(typeDesc);
			item.SubItems.Add(dateModified.ToString("g"));
		}

		private static string FormatSize(long bytes)
		{
			if (bytes < 1024)
				return $"{bytes} B";
			if (bytes < 1024 * 1024)
				return $"{bytes / 1024.0:F1} KB";
			if (bytes < 1024 * 1024 * 1024)
				return $"{bytes / (1024.0 * 1024):F1} MB";
			return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
		}

		private static string GetFileTypeDescription(string extension)
		{
			if (string.IsNullOrEmpty(extension))
				return "File";
			return extension.TrimStart('.').ToUpperInvariant() + " File";
		}

		private void ViewMenuItem_Click(object? sender, EventArgs e)
		{
			if (sender is not ToolStripMenuItem mi)
				return;
			var view = mi == largeIconsToolStripMenuItem ? View.LargeIcon
				: mi == smallIconsToolStripMenuItem ? View.SmallIcon
				: mi == listToolStripMenuItem ? View.List
				: mi == detailsToolStripMenuItem ? View.Details
				: View.Tile;
			ContentsView = view;
		}

		private void UpdateViewMenuCheckState()
		{
			largeIconsToolStripMenuItem.Checked = _currentView == View.LargeIcon;
			smallIconsToolStripMenuItem.Checked = _currentView == View.SmallIcon;
			listToolStripMenuItem.Checked = _currentView == View.List;
			detailsToolStripMenuItem.Checked = _currentView == View.Details;
			tilesToolStripMenuItem.Checked = _currentView == View.Tile;
		}

		private void ListView_DoubleClick(object? sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0)
				return;
			var item = listView.SelectedItems[0];
			if (item.Tag is FolderItemTag tag && tag.IsFolder)
				FolderDoubleClick?.Invoke(this, new FolderEventArgs(tag.FullPath));
		}

		private sealed record FolderItemTag(string FullPath, bool IsFolder, DateTime LastWriteTime, long Size, string TypeDescription);
	}

	/// <summary>
	/// Event args for folder navigation (e.g. double-click on a folder).
	/// </summary>
	public class FolderEventArgs : EventArgs
	{
		public string FolderPath { get; }

		public FolderEventArgs(string folderPath)
		{
			FolderPath = folderPath ?? string.Empty;
		}
	}
}
