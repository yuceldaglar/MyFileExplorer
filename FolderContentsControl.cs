using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that displays the contents of a folder with a toolbar and configurable view style (like Windows File Explorer).
	/// </summary>
	public partial class FolderContentsControl : UserControl
	{
		private string _currentPath = string.Empty;
		private View _currentView = View.LargeIcon;
		private readonly string _instanceId = Guid.NewGuid().ToString("N")[..8];
		private int _loadSequence;
		private int _refreshSequence;
		private int _newFolderSequence;
		private readonly Dictionary<string, int> _fileIconIndexByExtension = new(StringComparer.OrdinalIgnoreCase);
		private static readonly object s_logLock = new();
		private static readonly string s_logDirectory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"MyFileExplorer",
			"Logs");
		private static readonly string s_logFilePath = Path.Combine(s_logDirectory, "FolderContentsControl.log");

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
				Log($"CurrentPath set requested. old='{_currentPath}' new='{value ?? string.Empty}'");
				if (string.Equals(_currentPath, value, StringComparison.OrdinalIgnoreCase))
				{
					Log("CurrentPath set ignored because value is unchanged.");
					return;
				}
				_currentPath = value ?? string.Empty;
				Log($"CurrentPath updated. loading contents for '{_currentPath}'.");
				LoadContents("CurrentPath setter");
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

		/// <summary>
		/// Occurs when the user chooses to open a folder in a new tab from the context menu.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the user requests opening a folder in a new tab.")]
		public event EventHandler<FolderEventArgs>? OpenFolderInNewTabRequested;

		public FolderContentsControl()
		{
			InitializeComponent();
			SetupImageLists();
			EnsureDetailsColumns();
			UpdateViewMenuCheckState();
			Log($"FolderContentsControl created. HandleCreated={IsHandleCreated} InitialView={_currentView}");
		}

		private void SetupImageLists()
		{
			_fileIconIndexByExtension.Clear();
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

		private void LoadContents(string source)
		{
			var loadId = Interlocked.Increment(ref _loadSequence);
			Log($"LoadContents#{loadId} BEGIN source='{source}' path='{_currentPath}' beforeCount={listView.Items.Count}");
			listView.Items.Clear();
			LogListViewState($"LoadContents#{loadId} after clear");

			if (string.IsNullOrWhiteSpace(_currentPath))
			{
				Log($"LoadContents#{loadId} skipped: current path is empty/whitespace.");
				return;
			}

			var dir = new DirectoryInfo(_currentPath);
			if (!dir.Exists)
			{
				Log($"LoadContents#{loadId} skipped: directory does not exist.");
				return;
			}

			try
			{
				var comparer = StringComparer.OrdinalIgnoreCase;
				var directories = dir.GetDirectories().OrderBy(d => d.Name, comparer).ToArray();
				var files = dir.GetFiles().OrderBy(f => f.Name, comparer).ToArray();
				Log($"LoadContents#{loadId} found directories={directories.Length} files={files.Length}");
				// Folders first
				foreach (var subDir in directories)
					AddFolderItem(subDir, $"LoadContents#{loadId}");
				// Then files
				foreach (var file in files)
					AddFileItem(file, $"LoadContents#{loadId}");
				LogListViewState($"LoadContents#{loadId} completed population", includeItems: true);
			}
			catch (UnauthorizedAccessException)
			{
				Log($"LoadContents#{loadId} caught UnauthorizedAccessException for '{_currentPath}'.");
				var item = new ListViewItem("(Access denied)") { ForeColor = Color.Gray };
				listView.Items.Add(item);
				LogListViewState($"LoadContents#{loadId} added access denied item");
			}
			catch (DirectoryNotFoundException)
			{
				Log($"LoadContents#{loadId} caught DirectoryNotFoundException for '{_currentPath}'.");
				var item = new ListViewItem("(Not found)") { ForeColor = Color.Gray };
				listView.Items.Add(item);
				LogListViewState($"LoadContents#{loadId} added not found item");
			}
			Log($"LoadContents#{loadId} END");
		}

		private void AddFolderItem(DirectoryInfo subDir, string source)
		{
			const int folderIconIndex = 0;
			var item = new ListViewItem(subDir.Name, folderIconIndex)
			{
				Tag = new FolderItemTag(subDir.FullName, true, subDir.LastWriteTime, 0, "File folder")
			};
			AddDetailsSubitems(item, subDir.Name, 0, "File folder", subDir.LastWriteTime);
			listView.Items.Add(item);
			Log($"{source} AddFolderItem path='{subDir.FullName}' itemCount={listView.Items.Count}");
		}

		private void AddFileItem(FileInfo file, string source)
		{
			var fileIconIndex = ResolveFileIconIndex(file);
			var typeDesc = GetFileTypeDescription(file.Extension);
			var item = new ListViewItem(file.Name, fileIconIndex)
			{
				Tag = new FolderItemTag(file.FullName, false, file.LastWriteTime, file.Length, typeDesc)
			};
			AddDetailsSubitems(item, file.Name, file.Length, typeDesc, file.LastWriteTime);
			listView.Items.Add(item);
			Log($"{source} AddFileItem path='{file.FullName}' itemCount={listView.Items.Count}");
		}

		private int ResolveFileIconIndex(FileInfo file)
		{
			// Index 1 is guaranteed by SetupImageLists as generic file fallback.
			const int fallbackFileIconIndex = 1;
			var extension = file.Extension ?? string.Empty;
			if (string.IsNullOrWhiteSpace(extension))
				return fallbackFileIconIndex;

			if (_fileIconIndexByExtension.TryGetValue(extension, out var cachedIndex))
				return cachedIndex;

			var largeList = listView.LargeImageList;
			var smallList = listView.SmallImageList;
			if (largeList == null || smallList == null)
				return fallbackFileIconIndex;

			var expectedIndex = largeList.Images.Count;
			var added = ShellIconHelper.AddFileTypeIcon(extension, largeList, smallList);
			if (!added)
			{
				_fileIconIndexByExtension[extension] = fallbackFileIconIndex;
				return fallbackFileIconIndex;
			}

			_fileIconIndexByExtension[extension] = expectedIndex;
			return expectedIndex;
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
			if (item.Tag is not FolderItemTag tag)
				return;
			if (tag.IsFolder)
				FolderDoubleClick?.Invoke(this, new FolderEventArgs(tag.FullPath));
			else
				FileOperations.Open(tag.FullPath);
		}

		private void ListView_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter && listView.SelectedItems.Count == 1)
			{
				var item = listView.SelectedItems[0];
				if (item.Tag is FolderItemTag tag)
				{
					if (tag.IsFolder)
						FolderDoubleClick?.Invoke(this, new FolderEventArgs(tag.FullPath));
					else
						FileOperations.Open(tag.FullPath);
					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
			else if (e.KeyCode == Keys.F2)
			{
				StartRenameSelected();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Delete && (e.Modifiers & Keys.Shift) == 0)
			{
				DeleteSelected();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Delete && (e.Modifiers & Keys.Shift) != 0)
			{
				PermanentDeleteSelected();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.C)
			{
				CopySelected();
				e.Handled = true;
			}
			else if (e.Control && e.KeyCode == Keys.X)
			{
				CutSelected();
				e.Handled = true;
			}
			else if (e.Control && e.KeyCode == Keys.V)
			{
				PasteHere();
				e.Handled = true;
			}
		}

		/// <summary>
		/// Refreshes the folder contents list from disk. Raises <see cref="RefreshRequested"/> so the host can refresh the tree.
		/// </summary>
		public void RefreshContents()
		{
			var refreshId = Interlocked.Increment(ref _refreshSequence);
			Log($"RefreshContents#{refreshId} BEGIN currentPath='{_currentPath}'");
			LogListViewState($"RefreshContents#{refreshId} initial state", includeItems: true);
			var path = _currentPath;
			_currentPath = "";
			LoadContents($"RefreshContents#{refreshId}-phase1-clear");
			_currentPath = path ?? "";
			LoadContents($"RefreshContents#{refreshId}-phase2-reload");
			LogListViewState($"RefreshContents#{refreshId} before RefreshRequested invoke", includeItems: true);
			RefreshRequested?.Invoke(this, EventArgs.Empty);
			Log($"RefreshContents#{refreshId} END (RefreshRequested invoked)");
		}

		/// <summary>
		/// Occurs when the user or code requests a refresh (e.g. Refresh button). Host can refresh the tree as well.
		/// </summary>
		public event EventHandler? RefreshRequested;

		private void StartRenameSelected()
		{
			if (listView.SelectedItems.Count != 1)
				return;
			var item = listView.SelectedItems[0];
			if (item.Tag is not FolderItemTag)
				return;
			item.BeginEdit();
		}

		private void ListView_AfterLabelEdit(object? sender, LabelEditEventArgs e)
		{
			if (e.Item < 0 || e.Item >= listView.Items.Count || listView.Items[e.Item].Tag is not FolderItemTag tag)
				return;
			Log($"AfterLabelEdit triggered. itemIndex={e.Item} originalPath='{tag.FullPath}' newLabel='{e.Label ?? "<null>"}'");
			if (e.Label == null)
			{
				Log("AfterLabelEdit ignored: label is null (rename canceled by user).");
				return;
			}
			var newName = e.Label.Trim();
			if (string.IsNullOrEmpty(newName) || string.Equals(tag.FullPath, Path.Combine(Path.GetDirectoryName(tag.FullPath) ?? "", newName), StringComparison.OrdinalIgnoreCase))
			{
				Log("AfterLabelEdit ignored: empty name or unchanged target path.");
				return;
			}
			var dir = Path.GetDirectoryName(tag.FullPath) ?? _currentPath;
			var newPath = Path.Combine(dir, newName);
			try
			{
				Log($"AfterLabelEdit renaming '{tag.FullPath}' -> '{newPath}'");
				if (tag.IsFolder)
					Directory.Move(tag.FullPath, newPath);
				else
					File.Move(tag.FullPath, newPath);
				Log("AfterLabelEdit rename succeeded. Scheduling deferred RefreshContents.");
				// Refreshing synchronously inside AfterLabelEdit can cause transient duplicate visuals
				// because ListView is still finalizing edit UI state at this moment.
				BeginInvoke(new Action(() => RefreshContents()));
			}
			catch (Exception ex)
			{
				Log($"AfterLabelEdit rename failed: {ex.GetType().Name}: {ex.Message}");
				e.CancelEdit = true;
			}
		}

		private void DeleteSelected()
		{
			var paths = GetSelectedPaths().ToList();
			if (paths.Count == 0)
				return;
			FileClipboard.Clear();
			if (FileOperations.RecycleBinDelete(paths))
				RefreshContents();
		}

		private void PermanentDeleteSelected()
		{
			var paths = GetSelectedPaths().ToList();
			if (paths.Count == 0)
				return;
			var result = MessageBox.Show($"Permanently delete {paths.Count} item(s)? This cannot be undone.", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (result != DialogResult.Yes)
				return;
			FileClipboard.Clear();
			foreach (var p in paths)
			{
				try
				{
					if (Directory.Exists(p))
						Directory.Delete(p, recursive: true);
					else if (File.Exists(p))
						File.Delete(p);
				}
				catch { /* continue */ }
			}
			RefreshContents();
		}

		private void CopySelected()
		{
			var paths = GetSelectedPaths().ToList();
			if (paths.Count > 0)
				FileClipboard.SetCopy(paths);
		}

		private void CutSelected()
		{
			var paths = GetSelectedPaths().ToList();
			if (paths.Count > 0)
				FileClipboard.SetCut(paths);
		}

		private void PasteHere()
		{
			if (string.IsNullOrWhiteSpace(_currentPath) || !Directory.Exists(_currentPath))
				return;
			FileClipboard.PasteTo(_currentPath);
			RefreshContents();
		}

		private void NewFolderHere()
		{
			var newFolderId = Interlocked.Increment(ref _newFolderSequence);
			Log($"NewFolderHere#{newFolderId} BEGIN currentPath='{_currentPath}'");
			if (string.IsNullOrWhiteSpace(_currentPath) || !Directory.Exists(_currentPath))
			{
				Log($"NewFolderHere#{newFolderId} aborted: current path invalid.");
				return;
			}
			var basePath = Path.Combine(_currentPath, "New folder");
			var path = basePath;
			for (int i = 0; Directory.Exists(path); i++)
				path = $"{basePath} ({(i + 2)})";
			try
			{
				Log($"NewFolderHere#{newFolderId} creating directory '{path}'");
				Directory.CreateDirectory(path);
				Log($"NewFolderHere#{newFolderId} directory created. existsNow={Directory.Exists(path)}");
				RefreshContents();
				LogListViewState($"NewFolderHere#{newFolderId} after RefreshContents", includeItems: true);
				// Select new item and start rename
				BeginInvoke(() =>
				{
					Log($"NewFolderHere#{newFolderId} BeginInvoke selection/rename started.");
					var name = Path.GetFileName(path);
					var matchCount = 0;
					foreach (ListViewItem item in listView.Items)
					{
						if (item.Text == name && item.Tag is FolderItemTag t && t.IsFolder && string.Equals(t.FullPath, path, StringComparison.OrdinalIgnoreCase))
						{
							matchCount++;
							item.Selected = true;
							item.EnsureVisible();
							item.BeginEdit();
							break;
						}
					}
					Log($"NewFolderHere#{newFolderId} BeginInvoke selection completed. matchCount={matchCount}");
					LogListViewState($"NewFolderHere#{newFolderId} after BeginInvoke selection", includeItems: true);
				});
			}
			catch (Exception ex)
			{
				Log($"NewFolderHere#{newFolderId} ERROR {ex.GetType().Name}: {ex.Message}");
			}
			Log($"NewFolderHere#{newFolderId} END");
		}

		private IEnumerable<string> GetSelectedPaths()
		{
			foreach (ListViewItem item in listView.SelectedItems)
			{
				if (item.Tag is FolderItemTag tag)
					yield return tag.FullPath;
			}
		}

		private void ContextMenuStrip_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			var hit = listView.HitTest(listView.PointToClient(Cursor.Position));
			var onItem = hit.Item != null && hit.Item.Tag is FolderItemTag;
			openToolStripMenuItem.Visible = onItem;
			var singleFolder = listView.SelectedItems.Count == 1
				&& listView.SelectedItems[0].Tag is FolderItemTag singleTag
				&& singleTag.IsFolder;
			openInNewTabToolStripMenuItem.Visible = onItem && singleFolder;
			renameToolStripMenuItem.Visible = onItem;
			deleteToolStripMenuItem.Visible = onItem;
			toolStripSeparator2.Visible = onItem;
			cutToolStripMenuItem.Visible = onItem;
			copyToolStripMenuItem.Visible = onItem;
			pasteToolStripMenuItem.Visible = true;
			toolStripSeparator3.Visible = true;
			newFolderToolStripMenuItem.Visible = true;
			refreshContextToolStripMenuItem.Visible = true;
			toolStripSeparator4.Visible = onItem;
			propertiesToolStripMenuItem.Visible = onItem;
			var canPaste = FileClipboard.HasPaths && !string.IsNullOrWhiteSpace(_currentPath) && Directory.Exists(_currentPath);
			pasteToolStripMenuItem.Enabled = canPaste;
			var hasSelection = listView.SelectedItems.Count > 0;
			var singleSelection = listView.SelectedItems.Count == 1;
			openToolStripMenuItem.Enabled = hasSelection;
			openInNewTabToolStripMenuItem.Enabled = singleFolder;
			renameToolStripMenuItem.Enabled = singleSelection;
			deleteToolStripMenuItem.Enabled = hasSelection;
			cutToolStripMenuItem.Enabled = hasSelection;
			copyToolStripMenuItem.Enabled = hasSelection;
			propertiesToolStripMenuItem.Enabled = hasSelection;
		}

		private void RefreshToolStripButton_Click(object? sender, EventArgs e) => RefreshContents();

		private void OpenContextMenu_Click(object? sender, EventArgs e)
		{
			if (listView.SelectedItems.Count == 0)
				return;
			var item = listView.SelectedItems[0];
			if (item.Tag is FolderItemTag tag)
			{
				if (tag.IsFolder)
					FolderDoubleClick?.Invoke(this, new FolderEventArgs(tag.FullPath));
				else
					FileOperations.Open(tag.FullPath);
			}
		}

		private void OpenInNewTabContextMenu_Click(object? sender, EventArgs e)
		{
			if (listView.SelectedItems.Count != 1)
				return;
			if (listView.SelectedItems[0].Tag is not FolderItemTag tag || !tag.IsFolder)
				return;
			OpenFolderInNewTabRequested?.Invoke(this, new FolderEventArgs(tag.FullPath));
		}

		private void RenameContextMenu_Click(object? sender, EventArgs e) => StartRenameSelected();
		private void DeleteContextMenu_Click(object? sender, EventArgs e) => DeleteSelected();
		private void CutContextMenu_Click(object? sender, EventArgs e) => CutSelected();
		private void CopyContextMenu_Click(object? sender, EventArgs e) => CopySelected();
		private void PasteContextMenu_Click(object? sender, EventArgs e) => PasteHere();
		private void PropertiesContextMenu_Click(object? sender, EventArgs e) => ShowProperties();
		private void PasteEmptyContextMenu_Click(object? sender, EventArgs e) => PasteHere();
		private void NewFolderEmptyContextMenu_Click(object? sender, EventArgs e)
		{
			Log("Context menu click: New folder");
			NewFolderHere();
		}

		private void RefreshEmptyContextMenu_Click(object? sender, EventArgs e)
		{
			Log("Context menu click: Refresh");
			RefreshContents();
		}

		private void ShowProperties()
		{
			var paths = GetSelectedPaths().ToList();
			if (paths.Count == 0)
				return;
			FilePropertiesForm.ShowForPath(FindForm(), paths[0]);
		}

		internal sealed record FolderItemTag(string FullPath, bool IsFolder, DateTime LastWriteTime, long Size, string TypeDescription);

		private void Log(string message)
		{
			try
			{
				var line = $"{DateTime.Now:O} [FolderContentsControl:{_instanceId}] [T{Environment.CurrentManagedThreadId}] {message}";
				Debug.WriteLine(line);
				lock (s_logLock)
				{
					Directory.CreateDirectory(s_logDirectory);
					File.AppendAllText(s_logFilePath, line + Environment.NewLine, Encoding.UTF8);
				}
			}
			catch
			{
				// Logging must never break UI behavior.
			}
		}

		private void LogListViewState(string context, bool includeItems = false)
		{
			try
			{
				var total = listView.Items.Count;
				var selected = listView.SelectedItems.Count;
				var keyedItems = listView.Items
					.Cast<ListViewItem>()
					.Select(i =>
					{
						var key = i.Tag is FolderItemTag tag ? tag.FullPath : $"<no-tag>:{i.Text}";
						return new { Item = i, Key = key };
					})
					.ToList();
				var duplicateGroups = keyedItems
					.GroupBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
					.Where(g => g.Count() > 1)
					.ToList();
				Log($"{context} | totalItems={total} selectedItems={selected} duplicateKeyGroups={duplicateGroups.Count}");

				if (includeItems || duplicateGroups.Count > 0)
				{
					for (int i = 0; i < keyedItems.Count; i++)
					{
						var entry = keyedItems[i];
						var text = entry.Item.Text;
						var details = entry.Item.Tag is FolderItemTag tag
							? $"path='{tag.FullPath}' isFolder={tag.IsFolder}"
							: "path='<no-tag>'";
						Log($"{context} | item[{i}] text='{text}' {details}");
					}
				}

				foreach (var dup in duplicateGroups)
					Log($"{context} | DUPLICATE key='{dup.Key}' count={dup.Count()}");
			}
			catch (Exception ex)
			{
				Log($"{context} | failed to inspect list state: {ex.GetType().Name}: {ex.Message}");
			}
		}
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
