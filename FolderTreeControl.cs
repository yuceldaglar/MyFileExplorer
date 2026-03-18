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
	}
}
