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
			pathItemComboBox.Items = Program.SavedPathItems;
			pathItemComboBox.SelectedFolderChanged += PathItemComboBox_SelectedFolderChanged;
			if (Program.SavedPathItems.Count > 0)
				pathItemComboBox.SelectedItem = Program.SavedPathItems[0]; // show first folder in tree on load
		}

		private void PathItemComboBox_SelectedFolderChanged(object? sender, FolderEventArgs e)
		{
			folderTreeControl.RootPath = e.FolderPath;
		}
	}
}
