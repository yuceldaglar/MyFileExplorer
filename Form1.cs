namespace MyFileExplorer
{
	public partial class Form1 : Form
	{
		private const string AppTitle = "Developers File Explorer";
		private readonly List<ExplorerTabSession> _tabSessions = new();
		private ContextMenuStrip? _tabsContextMenu;

		public Form1()
		{
			InitializeComponent();
			InitializeTabUi();
			RestoreSessionOrCreateDefaultTab();
		}

		private void Form1_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.T)
			{
				CreateNewTab(selectTab: true);
				e.Handled = true;
				e.SuppressKeyPress = true;
				return;
			}

			if (e.Control && e.KeyCode == Keys.W)
			{
				CloseActiveTab();
				e.Handled = true;
				e.SuppressKeyPress = true;
				return;
			}

			if (e.Control && e.Shift && e.KeyCode == Keys.Tab)
			{
				SelectPreviousTab();
				e.Handled = true;
				e.SuppressKeyPress = true;
				return;
			}

			if (e.Control && e.KeyCode == Keys.Tab)
			{
				SelectNextTab();
				e.Handled = true;
				e.SuppressKeyPress = true;
				return;
			}

			if (e.KeyCode == Keys.F5)
			{
				GetActiveSession()?.Explorer.RefreshExplorer();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			SaveSessionState();

			foreach (var session in _tabSessions.ToArray())
				DisposeSession(session);
			_tabSessions.Clear();
			base.OnFormClosing(e);
		}

		private void InitializeTabUi()
		{
			explorerTabControl.ShowToolTips = true;
			explorerTabControl.SelectedIndexChanged += ExplorerTabControl_SelectedIndexChanged;
			explorerTabControl.MouseUp += ExplorerTabControl_MouseUp;

			_tabsContextMenu = new ContextMenuStrip();
			var newTabItem = new ToolStripMenuItem("New tab", null, (_, _) => CreateNewTab(selectTab: true));
			var closeTabItem = new ToolStripMenuItem("Close tab", null, (_, _) => CloseActiveTab());
			_tabsContextMenu.Items.AddRange([newTabItem, closeTabItem]);
		}

		private void ExplorerTabControl_SelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateWindowTitle();
		}

		private void ExplorerTabControl_MouseUp(object? sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right || _tabsContextMenu == null)
				return;

			for (var i = 0; i < explorerTabControl.TabPages.Count; i++)
			{
				if (!explorerTabControl.GetTabRect(i).Contains(e.Location))
					continue;

				explorerTabControl.SelectedIndex = i;
				_tabsContextMenu.Show(explorerTabControl, e.Location);
				break;
			}
		}

		private ExplorerTabSession CreateNewTab(bool selectTab)
		{
			var explorer = new ExplorerLayoutControl
			{
				Dock = DockStyle.Fill
			};
			var tabPage = new TabPage("New tab");
			tabPage.Controls.Add(explorer);

			var session = new ExplorerTabSession(tabPage, explorer);
			_tabSessions.Add(session);

			explorer.CurrentPathChanged += Explorer_CurrentPathChanged;
			explorer.OpenFolderInNewTabRequested += Explorer_OpenFolderInNewTabRequested;
			explorerTabControl.TabPages.Add(tabPage);

			if (selectTab)
				explorerTabControl.SelectedTab = tabPage;

			UpdateTabTitle(session);
			UpdateWindowTitle();
			return session;
		}

		private void RestoreSessionOrCreateDefaultTab()
		{
			var state = SessionStateStore.Load();
			if (state?.Tabs == null || state.Tabs.Count == 0)
			{
				CreateNewTab(selectTab: true);
				return;
			}

			foreach (var tabState in state.Tabs)
			{
				var session = CreateNewTab(selectTab: false);
				try
				{
					session.Explorer.RestoreState(tabState);
					UpdateTabTitle(session);
				}
				catch
				{
					// Continue restoring remaining tabs.
				}
			}

			if (_tabSessions.Count == 0)
			{
				CreateNewTab(selectTab: true);
				return;
			}

			var selectedIndex = Math.Clamp(state.SelectedTabIndex, 0, _tabSessions.Count - 1);
			explorerTabControl.SelectedIndex = selectedIndex;
			UpdateWindowTitle();
		}

		private void SaveSessionState()
		{
			if (_tabSessions.Count == 0)
				return;

			var state = new AppSessionState
			{
				SelectedTabIndex = Math.Max(0, explorerTabControl.SelectedIndex)
			};

			foreach (var session in _tabSessions)
			{
				try
				{
					state.Tabs.Add(session.Explorer.CaptureState());
				}
				catch
				{
					// Skip broken tab state but continue saving others.
				}
			}

			if (state.Tabs.Count == 0)
				return;

			SessionStateStore.Save(state);
		}

		private void Explorer_CurrentPathChanged(object? sender, FolderEventArgs e)
		{
			if (sender is not ExplorerLayoutControl explorer)
				return;

			var session = _tabSessions.FirstOrDefault(x => ReferenceEquals(x.Explorer, explorer));
			if (session == null)
				return;

			UpdateTabTitle(session);
			if (ReferenceEquals(GetActiveSession(), session))
				UpdateWindowTitle();
		}

		private void Explorer_OpenFolderInNewTabRequested(object? sender, FolderEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.FolderPath) || !Directory.Exists(e.FolderPath))
				return;

			var session = CreateNewTab(selectTab: true);
			session.Explorer.NavigateToFolder(e.FolderPath);
		}

		private void CloseActiveTab()
		{
			var session = GetActiveSession();
			if (session == null)
				return;

			if (_tabSessions.Count == 1)
			{
				Close();
				return;
			}

			DisposeSession(session);
			_tabSessions.Remove(session);
			explorerTabControl.TabPages.Remove(session.TabPage);
			session.TabPage.Dispose();
			UpdateWindowTitle();
		}

		private void DisposeSession(ExplorerTabSession session)
		{
			session.Explorer.CurrentPathChanged -= Explorer_CurrentPathChanged;
			session.Explorer.OpenFolderInNewTabRequested -= Explorer_OpenFolderInNewTabRequested;
			session.Explorer.Dispose();
		}

		private ExplorerTabSession? GetActiveSession()
		{
			var selectedTab = explorerTabControl.SelectedTab;
			if (selectedTab == null)
				return null;
			return _tabSessions.FirstOrDefault(x => ReferenceEquals(x.TabPage, selectedTab));
		}

		private static string BuildTabTitle(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return "New tab";

			var normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			if (string.IsNullOrWhiteSpace(normalizedPath))
				return path;

			var leaf = Path.GetFileName(normalizedPath);
			return string.IsNullOrWhiteSpace(leaf) ? normalizedPath : leaf;
		}

		private void UpdateTabTitle(ExplorerTabSession session)
		{
			var path = session.Explorer.CurrentPath;
			session.TabPage.Text = BuildTabTitle(path);
			session.TabPage.ToolTipText = string.IsNullOrWhiteSpace(path) ? "New tab" : path;
		}

		private void UpdateWindowTitle()
		{
			var activeSession = GetActiveSession();
			if (activeSession == null)
			{
				Text = AppTitle;
				return;
			}

			Text = $"{activeSession.TabPage.Text} - {AppTitle}";
		}

		private void SelectNextTab()
		{
			var tabCount = explorerTabControl.TabPages.Count;
			if (tabCount < 2)
				return;

			var next = (explorerTabControl.SelectedIndex + 1) % tabCount;
			explorerTabControl.SelectedIndex = next;
		}

		private void SelectPreviousTab()
		{
			var tabCount = explorerTabControl.TabPages.Count;
			if (tabCount < 2)
				return;

			var current = explorerTabControl.SelectedIndex;
			var previous = current <= 0 ? tabCount - 1 : current - 1;
			explorerTabControl.SelectedIndex = previous;
		}
	}
}
