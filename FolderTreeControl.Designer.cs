namespace MyFileExplorer
{
	partial class FolderTreeControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			folderTreeView = new TreeView();
			SuspendLayout();
			//
			// folderTreeView
			//
			folderTreeView.Dock = DockStyle.Fill;
			folderTreeView.Location = new Point(0, 0);
			folderTreeView.Name = "folderTreeView";
			folderTreeView.Size = new Size(200, 200);
			folderTreeView.TabIndex = 0;
			folderTreeView.BeforeExpand += FolderTreeView_BeforeExpand;
			folderTreeView.AfterSelect += FolderTreeView_AfterSelect;
			//
			// FolderTreeControl
			//
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(folderTreeView);
			Name = "FolderTreeControl";
			Size = new Size(200, 200);
			ResumeLayout(false);
		}

		#endregion

		private TreeView folderTreeView;
	}
}
