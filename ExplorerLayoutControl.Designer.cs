namespace MyFileExplorer
{
	partial class ExplorerLayoutControl
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
			splitContainer = new SplitContainer();
			pathItemComboBox = new PathItemComboBoxControl();
			folderTreeControl = new FolderTreeControl();
			folderContentsControl = new FolderContentsControl();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			SuspendLayout();
			// 
			// splitContainer
			// 
			splitContainer.Dock = DockStyle.Fill;
			splitContainer.Location = new Point(0, 0);
			splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			splitContainer.Panel1.Controls.Add(folderTreeControl);
			splitContainer.Panel1.Controls.Add(pathItemComboBox);
			splitContainer.Panel1MinSize = 100;
			// 
			// splitContainer.Panel2
			// 
			splitContainer.Panel2.Controls.Add(folderContentsControl);
			splitContainer.Panel2MinSize = 100;
			splitContainer.Size = new Size(600, 400);
			splitContainer.SplitterDistance = 250;
			splitContainer.TabIndex = 0;
			// 
			// pathItemComboBox
			// 
			pathItemComboBox.Dock = DockStyle.Top;
			pathItemComboBox.Location = new Point(0, 0);
			pathItemComboBox.Name = "pathItemComboBox";
			pathItemComboBox.Size = new Size(250, 28);
			pathItemComboBox.TabIndex = 0;
			// 
			// folderTreeControl
			// 
			folderTreeControl.Dock = DockStyle.Fill;
			folderTreeControl.Location = new Point(0, 28);
			folderTreeControl.Name = "folderTreeControl";
			folderTreeControl.Size = new Size(250, 372);
			folderTreeControl.TabIndex = 1;
			// 
			// folderContentsControl
			// 
			folderContentsControl.Dock = DockStyle.Fill;
			folderContentsControl.Location = new Point(0, 0);
			folderContentsControl.Name = "folderContentsControl";
			folderContentsControl.Size = new Size(346, 400);
			folderContentsControl.TabIndex = 0;
			// 
			// ExplorerLayoutControl
			// 
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(splitContainer);
			Name = "ExplorerLayoutControl";
			Size = new Size(600, 400);
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private SplitContainer splitContainer;
		private PathItemComboBoxControl pathItemComboBox;
		private FolderTreeControl folderTreeControl;
		private FolderContentsControl folderContentsControl;
	}
}
