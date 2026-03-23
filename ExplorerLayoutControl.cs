using System.ComponentModel;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that lays out a path combo and folder tree on the left of a divider, and folder contents on the right.
	/// </summary>
	public partial class ExplorerLayoutControl : UserControl
	{
		private bool _isRestoringState;

		/// <summary>
		/// Raised when the current folder path shown by this explorer instance changes.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the active folder path changes.")]
		public event EventHandler<FolderEventArgs>? CurrentPathChanged;

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
		/// Gets the current folder path displayed by this explorer instance.
		/// </summary>
		[Browsable(false)]
		public string CurrentPath => folderContentsControl.CurrentPath;

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
			OnCurrentPathChanged(selectedPath);
		}

		private void FolderContentsControl_RefreshRequested(object? sender, EventArgs e)
		{
			folderTreeControl.RefreshTree();
		}

		private void FolderTreeControl_FolderSelected(object? sender, FolderEventArgs e)
		{
			folderContentsControl.CurrentPath = e.FolderPath;
			if (!_isRestoringState)
				SyncTerminalDirectory(e.FolderPath);
			OnCurrentPathChanged(e.FolderPath);
		}

		private void PathItemComboBox_SelectedFolderChanged(object? sender, FolderEventArgs e)
		{
			folderTreeControl.RootPath = e.FolderPath;
			SelectRootNodeIfAvailable(e.FolderPath);
			OnCurrentPathChanged(e.FolderPath);
		}

		private void FolderContentsControl_FolderDoubleClick(object? sender, FolderEventArgs e)
		{
			folderContentsControl.CurrentPath = e.FolderPath;
			SyncTerminalDirectory(e.FolderPath);
			OnCurrentPathChanged(e.FolderPath);
			RequestSelectFolderInTree(e.FolderPath);
		}

		/// <summary>
		/// Refreshes both file contents and tree for this explorer instance.
		/// </summary>
		public void RefreshExplorer()
		{
			folderContentsControl.RefreshContents();
			folderTreeControl.RefreshTree();
		}

		internal ExplorerTabState CaptureState()
		{
			return new ExplorerTabState
			{
				CurrentPath = CurrentPath ?? string.Empty,
				TreeSelectedPath = folderTreeControl.SelectedFolderPath ?? string.Empty,
				MainSplitterDistance = splitContainer.SplitterDistance,
				MainSplitterRatio = GetSplitterRatio(splitContainer),
				TerminalSplitterDistance = outerSplitContainer.SplitterDistance,
				TerminalSplitterRatio = GetSplitterRatio(outerSplitContainer),
				Terminal = terminalControl.CaptureState()
			};
		}

		internal void RestoreState(ExplorerTabState? state)
		{
			if (state == null)
				return;

			ApplySplitterStateSafe(splitContainer, state.MainSplitterDistance, state.MainSplitterRatio);
			ApplySplitterStateSafe(outerSplitContainer, state.TerminalSplitterDistance, state.TerminalSplitterRatio);

			var treeSelectedPath = state.TreeSelectedPath?.Trim() ?? string.Empty;
			var currentPath = state.CurrentPath?.Trim() ?? string.Empty;
			var restorePath = !string.IsNullOrWhiteSpace(treeSelectedPath) && Directory.Exists(treeSelectedPath)
				? treeSelectedPath
				: (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath) ? currentPath : string.Empty);

			try
			{
				_isRestoringState = true;
				if (!string.IsNullOrWhiteSpace(restorePath))
					RestorePath(restorePath);

				terminalControl.RestoreState(state.Terminal);
			}
			finally
			{
				_isRestoringState = false;
			}
		}

		private void SyncTerminalDirectory(string selectedPath)
		{
			terminalControl.SyncWorkingDirectory(selectedPath);
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

		private void RestorePath(string path)
		{
			var savedRoot = Program.SavedPathItems
				.Where(x => !string.IsNullOrWhiteSpace(x.Path) && path.StartsWith(x.Path, StringComparison.OrdinalIgnoreCase))
				.OrderByDescending(x => x.Path.Length)
				.FirstOrDefault();

			if (savedRoot != null)
			{
				pathItemComboBox.SelectedItem = savedRoot;
				folderContentsControl.CurrentPath = path;
				OnCurrentPathChanged(path);
				SelectFolderInTree(path);
				return;
			}

			folderTreeControl.RootPath = path;
			folderContentsControl.CurrentPath = path;
			OnCurrentPathChanged(path);
			SelectFolderInTree(path);
		}

		private void RequestSelectFolderInTree(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || IsDisposed)
				return;

			if (IsHandleCreated)
			{
				BeginInvoke(() => SelectFolderInTree(path));
				return;
			}

			SelectFolderInTree(path);
		}

		private static void ApplySplitterStateSafe(SplitContainer container, int requestedDistance, double requestedRatio)
		{
			void Apply()
			{
				var available = GetSplitterSpan(container);
				if (available <= 0)
					return;

				var requested = requestedDistance;
				if (requestedRatio > 0 && requestedRatio < 1)
					requested = (int)Math.Round(available * requestedRatio, MidpointRounding.AwayFromZero);
				else if (requested <= 0)
					return;

				var min = container.Panel1MinSize;
				var max = Math.Max(min, available - container.Panel2MinSize);
				var bounded = Math.Min(Math.Max(requested, min), max);
				if (bounded > 0)
					container.SplitterDistance = bounded;
			}

			if (!container.IsHandleCreated)
			{
				container.HandleCreated += (_, _) => Apply();
				return;
			}

			Apply();
			if (!container.IsDisposed)
				container.BeginInvoke(new MethodInvoker(Apply));
		}

		private static int GetSplitterSpan(SplitContainer container)
		{
			return container.Orientation == Orientation.Vertical
				? container.Width - container.SplitterWidth
				: container.Height - container.SplitterWidth;
		}

		private static double GetSplitterRatio(SplitContainer container)
		{
			var span = GetSplitterSpan(container);
			if (span <= 0)
				return 0;

			return Math.Clamp(container.SplitterDistance / (double)span, 0d, 1d);
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

		private void OnCurrentPathChanged(string path)
		{
			CurrentPathChanged?.Invoke(this, new FolderEventArgs(path));
		}
	}
}
