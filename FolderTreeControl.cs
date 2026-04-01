using System.ComponentModel;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that displays folders and subfolders as a tree view starting from a configurable root path.
	/// </summary>
	public partial class FolderTreeControl : UserControl
	{
		private string _rootPath = string.Empty;

		/// <summary>
		/// Gets the TreeView that displays the folder hierarchy. Use this to handle selection (e.g. AfterSelect) or customize appearance.
		/// </summary>
		[Browsable(false)]
		public TreeView TreeView => folderTreeView;

		[Browsable(false)]
		public string? SelectedFolderPath => GetSelectedFolderPath();

		/// <summary>
		/// Occurs when a folder is selected in the tree (e.g. user clicks a node). Event args contain the folder path.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the user selects a folder node in the tree.")]
		public event EventHandler<FolderEventArgs>? FolderSelected;

		/// <summary>
		/// Gets or sets the root directory path. The tree view displays folders starting from this path.
		/// Setting this property refreshes the tree.
		/// </summary>
		[Category("Behavior")]
		[Description("The root directory path. The tree displays folders starting from this path.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string RootPath
		{
			get => _rootPath;
			set
			{
				if (string.Equals(_rootPath, value, StringComparison.OrdinalIgnoreCase))
					return;
				_rootPath = value ?? string.Empty;
				LoadRoot();
			}
		}

		public FolderTreeControl()
		{
			InitializeComponent();
			// Keep selected node visually highlighted even when focus is elsewhere (e.g. combo box).
			folderTreeView.HideSelection = false;
		}

		/// <summary>
		/// Loads the root node from <see cref="RootPath"/> and populates the tree.
		/// </summary>
		private void LoadRoot()
		{
			folderTreeView.Nodes.Clear();

			if (string.IsNullOrWhiteSpace(_rootPath))
				return;

			var rootDir = new DirectoryInfo(_rootPath);
			if (!rootDir.Exists)
				return;

			var rootNode = CreateFolderNode(rootDir);
			folderTreeView.Nodes.Add(rootNode);
			LoadSubfolders(rootNode);
		}

		private static TreeNode CreateFolderNode(DirectoryInfo dir)
		{
			var node = new TreeNode(dir.Name)
			{
				Tag = dir.FullName
			};
			// Placeholder so the node shows as expandable; replaced on first expand
			node.Nodes.Add(new TreeNode { Tag = UnloadedMarker });
			return node;
		}

		private const string UnloadedMarker = "__unloaded__";

		private void LoadSubfolders(TreeNode parentNode)
		{
			string? path = parentNode.Tag as string;
			if (string.IsNullOrEmpty(path))
				return;

			parentNode.Nodes.Clear();
			try
			{
				var dir = new DirectoryInfo(path);
				foreach (var subDir in dir.GetDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
				{
					var childNode = CreateFolderNode(subDir);
					parentNode.Nodes.Add(childNode);
				}
			}
			catch (UnauthorizedAccessException)
			{
				parentNode.Nodes.Add("(Access denied)");
			}
			catch (DirectoryNotFoundException)
			{
				parentNode.Nodes.Add("(Not found)");
			}
		}

		private void FolderTreeView_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
		{
			var node = e.Node;
			if (node?.Nodes.Count != 1)
				return;
			if (node.Nodes[0].Tag is string tag && tag == UnloadedMarker)
				LoadSubfolders(node);
		}

		private void FolderTreeView_AfterSelect(object? sender, TreeViewEventArgs e)
		{
			if (e.Node?.Tag is string path && path != UnloadedMarker)
				FolderSelected?.Invoke(this, new FolderEventArgs(path));
		}

		/// <summary>
		/// Reloads the tree from the current <see cref="RootPath"/>.
		/// </summary>
		public void RefreshTree()
		{
			var path = _rootPath;
			_rootPath = "";
			LoadRoot();
			_rootPath = path ?? "";
			LoadRoot();
		}

		private void RefreshTree(string? preferredSelectedPath)
		{
			RefreshTree();
			SelectPath(preferredSelectedPath);
		}

		private string? GetSelectedFolderPath()
		{
			var node = folderTreeView.SelectedNode;
			if (node?.Tag is not string path || path == UnloadedMarker)
				return null;
			return path;
		}

		private void SelectPath(string? targetPath)
		{
			if (string.IsNullOrWhiteSpace(targetPath) || folderTreeView.Nodes.Count == 0)
				return;

			var normalizedTarget = NormalizePath(targetPath);
			var rootNode = folderTreeView.Nodes[0];
			if (rootNode.Tag is not string rootTag)
				return;

			var normalizedRoot = NormalizePath(rootTag);
			if (!IsSameOrParentPath(normalizedRoot, normalizedTarget))
				normalizedTarget = normalizedRoot;

			var nodeToSelect = FindNodeByPathAlongBranch(rootNode, normalizedTarget);
			if (nodeToSelect == null)
				return;

			folderTreeView.SelectedNode = nodeToSelect;
			nodeToSelect.EnsureVisible();
		}

		private TreeNode? FindNodeByPathAlongBranch(TreeNode rootNode, string targetPath)
		{
			if (rootNode.Tag is not string rootTag)
				return null;

			var current = rootNode;
			var currentPath = NormalizePath(rootTag);
			targetPath = NormalizePath(targetPath);
			if (!IsSameOrParentPath(currentPath, targetPath))
				return null;

			while (!string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
			{
				EnsureChildrenLoaded(current);

				TreeNode? next = null;
				var nextLength = -1;
				foreach (TreeNode child in current.Nodes)
				{
					if (child.Tag is not string childTag || childTag == UnloadedMarker)
						continue;

					var childPath = NormalizePath(childTag);
					if (!IsSameOrParentPath(childPath, targetPath))
						continue;

					if (childPath.Length > nextLength)
					{
						next = child;
						nextLength = childPath.Length;
					}
				}

				if (next?.Tag is not string nextTag)
					return current; // closest loaded/available parent

				var nextPath = NormalizePath(nextTag);
				if (string.Equals(nextPath, currentPath, StringComparison.OrdinalIgnoreCase))
					return current;

				current = next;
				currentPath = nextPath;
			}

			return current;
		}

		private void EnsureChildrenLoaded(TreeNode node)
		{
			if (node.Nodes.Count == 1 && node.Nodes[0].Tag is string marker && marker == UnloadedMarker)
				LoadSubfolders(node);
		}

		private static bool IsSameOrParentPath(string? parentPath, string? childPath)
		{
			var normalizedParent = NormalizePath(parentPath);
			var normalizedChild = NormalizePath(childPath);
			if (string.IsNullOrWhiteSpace(normalizedParent) || string.IsNullOrWhiteSpace(normalizedChild))
				return false;

			if (string.Equals(normalizedParent, normalizedChild, StringComparison.OrdinalIgnoreCase))
				return true;

			if (normalizedChild.Length <= normalizedParent.Length)
				return false;

			if (!normalizedChild.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase))
				return false;

			var separator = normalizedChild[normalizedParent.Length];
			return separator == Path.DirectorySeparatorChar || separator == Path.AltDirectorySeparatorChar;
		}

		private static string NormalizePath(string? path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return string.Empty;
			return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		private void TreeContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			var path = GetSelectedFolderPath();
			var hasPath = !string.IsNullOrEmpty(path) && Directory.Exists(path);
			treeOpenToolStripMenuItem.Enabled = hasPath;
			treeExpandToolStripMenuItem.Enabled = folderTreeView.SelectedNode != null && folderTreeView.SelectedNode.Nodes.Count > 0;
			treeCollapseToolStripMenuItem.Enabled = folderTreeView.SelectedNode != null && folderTreeView.SelectedNode.IsExpanded;
			treeCutToolStripMenuItem.Enabled = hasPath;
			treeCopyToolStripMenuItem.Enabled = hasPath;
			treePasteToolStripMenuItem.Enabled = hasPath && FileClipboard.HasPaths;
			treeNewFolderToolStripMenuItem.Enabled = hasPath;
			treeRenameToolStripMenuItem.Enabled = hasPath;
			treeDeleteToolStripMenuItem.Enabled = hasPath;
			treePropertiesToolStripMenuItem.Enabled = hasPath;
		}

		private void TreeOpen_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (!string.IsNullOrEmpty(path))
				FolderSelected?.Invoke(this, new FolderEventArgs(path));
		}

		private void TreeExpand_Click(object? sender, EventArgs e) => folderTreeView.SelectedNode?.Expand();
		private void TreeCollapse_Click(object? sender, EventArgs e) => folderTreeView.SelectedNode?.Collapse();

		private void TreeCut_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (!string.IsNullOrEmpty(path))
				FileClipboard.SetCut(new[] { path });
		}

		private void TreeCopy_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (!string.IsNullOrEmpty(path))
				FileClipboard.SetCopy(new[] { path });
		}

		private void TreePaste_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (!string.IsNullOrEmpty(path))
			{
				FileClipboard.PasteTo(path);
				RefreshTree();
			}
		}

		private void TreeNewFolder_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				return;
			var basePath = Path.Combine(path, "New folder");
			var newPath = basePath;
			for (int i = 0; Directory.Exists(newPath); i++)
				newPath = $"{basePath} ({(i + 2)})";
			try
			{
				Directory.CreateDirectory(newPath);
				RefreshTree();
			}
			catch { }
		}

		private void TreeRename_Click(object? sender, EventArgs e) => folderTreeView.SelectedNode?.BeginEdit();

		private void TreeRename_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
		{
			if (e.Label == null || e.Node?.Tag is not string oldPath)
				return;
			var newName = e.Label.Trim();
			if (string.IsNullOrEmpty(newName))
				return;
			var parent = Path.GetDirectoryName(oldPath);
			if (string.IsNullOrEmpty(parent))
				return;
			var newPath = Path.Combine(parent, newName);
			if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
				return;
			try
			{
				Directory.Move(oldPath, newPath);
				if (e.Node != null)
					e.Node.Tag = newPath;
			}
			catch
			{
				e.CancelEdit = true;
			}
		}

		private void TreeDelete_Click(object? sender, EventArgs e)
		{
			var selectedNode = folderTreeView.SelectedNode;
			if (selectedNode?.Tag is not string path || path == UnloadedMarker)
				return;

			var parentNode = selectedNode.Parent;
			var rootCollection = folderTreeView.Nodes;
			FileClipboard.Clear();
			if (FileOperations.RecycleBinDelete(path))
			{
				if (parentNode != null)
				{
					parentNode.Nodes.Remove(selectedNode);
					folderTreeView.SelectedNode = parentNode;
					parentNode.EnsureVisible();
					return;
				}

				rootCollection.Remove(selectedNode);
			}
		}

		private void TreeProperties_Click(object? sender, EventArgs e)
		{
			var path = GetSelectedFolderPath();
			if (string.IsNullOrEmpty(path))
				return;
			FilePropertiesForm.ShowForPath(FindForm(), path);
		}
	}
}
