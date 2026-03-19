using System.ComponentModel;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that lays out a path combo and folder tree on the left of a divider, and folder contents on the right.
	/// </summary>
	public partial class ExplorerLayoutControl : UserControl
	{
		/// <summary>
		/// Gets the PathItem ComboBox at the top of the left panel.
		/// </summary>
		[Browsable(false)]
		public PathItemComboBoxControl PathItemComboBox => pathItemComboBox;

		/// <summary>
		/// Gets the folder tree control below the combo on the left.
		/// </summary>
		[Browsable(false)]
		public FolderTreeControl FolderTree => folderTreeControl;

		/// <summary>
		/// Gets the folder contents control on the right side of the divider.
		/// </summary>
		[Browsable(false)]
		public FolderContentsControl FolderContents => folderContentsControl;

		/// <summary>
		/// Gets the terminal control in the bottom panel spanning full width.
		/// </summary>
		[Browsable(false)]
		public TerminalControl Terminal => terminalControl;

		public ExplorerLayoutControl()
		{
			InitializeComponent();
			folderTreeControl.FolderSelected += FolderTreeControl_FolderSelected;
			folderContentsControl.FolderDoubleClick += FolderContentsControl_FolderDoubleClick;
			folderContentsControl.RefreshRequested += FolderContentsControl_RefreshRequested;
			pathItemComboBox.SelectedFolderChanged += PathItemComboBox_SelectedFolderChanged;
			pathItemComboBox.Items = Program.SavedPathItems;
			if (Program.SavedPathItems.Count > 0)
				pathItemComboBox.SelectedItem = Program.SavedPathItems[0]; // show first folder in tree on load
			ApplyInitialSelection();
		}

		private void ApplyInitialSelection()
		{
			// Ensure tree/content are initialized even if selection changed before event wiring
			// or if re-selecting the same item does not re-fire SelectedIndexChanged.
			var selectedPath = pathItemComboBox.SelectedPath;
			if (string.IsNullOrWhiteSpace(selectedPath))
				return;
			folderTreeControl.RootPath = selectedPath;
			folderContentsControl.CurrentPath = selectedPath;
		}

		private void FolderContentsControl_RefreshRequested(object? sender, EventArgs e)
		{
			folderTreeControl.RefreshTree();
		}

		private void FolderTreeControl_FolderSelected(object? sender, FolderEventArgs e)
		{
			folderContentsControl.CurrentPath = e.FolderPath;
			SyncTerminalDirectory(e.FolderPath);
		}

		private void PathItemComboBox_SelectedFolderChanged(object? sender, FolderEventArgs e)
		{
			folderTreeControl.RootPath = e.FolderPath;
			SelectRootNodeIfAvailable(e.FolderPath);
		}

		private void FolderContentsControl_FolderDoubleClick(object? sender, FolderEventArgs e)
		{
			folderContentsControl.CurrentPath = e.FolderPath;
			SyncTerminalDirectory(e.FolderPath);
			var path = e.FolderPath;
			BeginInvoke(() => SelectFolderInTree(path));
		}

		private void SyncTerminalDirectory(string selectedPath)
		{
			if (string.IsNullOrWhiteSpace(selectedPath) || !Directory.Exists(selectedPath))
				return;

			// Keep terminal location aligned with active explorer navigation source.
			if (terminalControl.ShellType == TerminalShellType.PowerShell)
			{
				var escapedPath = selectedPath.Replace("'", "''", StringComparison.Ordinal);
				terminalControl.SendCommand($"Set-Location -LiteralPath '{escapedPath}'");
			}
			else
			{
				var escapedPath = selectedPath.Replace("\"", "\"\"", StringComparison.Ordinal);
				terminalControl.SendCommand($"cd /d \"{escapedPath}\"");
			}
		}

		private void SelectFolderInTree(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return;
			path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			var treeView = folderTreeControl.TreeView;
			var rootPath = folderTreeControl.RootPath?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.IsNullOrEmpty(rootPath) || !path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
				return; // path not under current tree root
			var node = FindNodeByPath(treeView.Nodes, path);
			if (node != null)
			{
				treeView.SelectedNode = node;
				node.EnsureVisible();
				treeView.Focus();
			}
		}

		private void SelectRootNodeIfAvailable(string rootPath)
		{
			if (string.IsNullOrWhiteSpace(rootPath))
				return;

			var treeView = folderTreeControl.TreeView;
			if (treeView.Nodes.Count == 0)
				return;

			var rootNode = treeView.Nodes[0];
			if (rootNode.Tag is not string tag || !string.Equals(tag, rootPath, StringComparison.OrdinalIgnoreCase))
			{
				rootNode = FindNodeByPath(treeView.Nodes, rootPath) ?? rootNode;
			}

			treeView.SelectedNode = rootNode;
			rootNode.EnsureVisible();
		}

		private static TreeNode? FindNodeByPath(TreeNodeCollection nodes, string path)
		{
			foreach (TreeNode node in nodes)
			{
				if (node.Tag is not string tag)
					continue;
				if (tag == "__unloaded__")
					continue; // placeholder node, skip
				if (string.Equals(tag, path, StringComparison.OrdinalIgnoreCase))
					return node;
				if (node.Nodes.Count > 0 && !node.IsExpanded)
					node.Expand();
				var found = FindNodeByPath(node.Nodes, path);
				if (found != null)
					return found;
			}
			return null;
		}
	}
}
