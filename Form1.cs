namespace MyFileExplorer
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void explorerLayoutControl1_Load(object sender, EventArgs e)
		{
		}

		private void Form1_KeyDown(object? sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				explorerLayoutControl1.FolderContents.RefreshContents();
				explorerLayoutControl1.FolderTree.RefreshTree();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}
	}
}
