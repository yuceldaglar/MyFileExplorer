namespace MyFileExplorer
{
	/// <summary>
	/// Holds the UI controls and metadata for one explorer tab.
	/// </summary>
	internal sealed class ExplorerTabSession
	{
		public ExplorerTabSession(TabPage tabPage, ExplorerLayoutControl explorer)
		{
			TabPage = tabPage;
			Explorer = explorer;
		}

		public TabPage TabPage { get; }
		public ExplorerLayoutControl Explorer { get; }
	}
}
