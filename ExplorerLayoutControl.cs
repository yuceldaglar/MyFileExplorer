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

		public ExplorerLayoutControl()
		{
			InitializeComponent();
			folderTreeControl.FolderSelected += (s, e) => folderContentsControl.CurrentPath = e.FolderPath;
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

		private void PathItemComboBox_SelectedFolderChanged(object? sender, FolderEventArgs e)
		{
			folderTreeControl.RootPath = e.FolderPath;
		}

		private void FolderContentsControl_FolderDoubleClick(object? sender, FolderEventArgs e)
		{
			folderContentsControl.CurrentPath = e.FolderPath;
			var path = e.FolderPath;
			BeginInvoke(() => SelectFolderInTree(path));
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
