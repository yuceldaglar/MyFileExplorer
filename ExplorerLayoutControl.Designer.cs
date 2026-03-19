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
			outerSplitContainer = new SplitContainer();
			splitContainer = new SplitContainer();
			folderTreeControl = new FolderTreeControl();
			pathItemComboBox = new PathItemComboBoxControl();
			folderContentsControl = new FolderContentsControl();
			terminalControl = new TerminalControl();
			((System.ComponentModel.ISupportInitialize)outerSplitContainer).BeginInit();
			outerSplitContainer.Panel1.SuspendLayout();
			outerSplitContainer.Panel2.SuspendLayout();
			outerSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
			splitContainer.Panel1.SuspendLayout();
			splitContainer.Panel2.SuspendLayout();
			splitContainer.SuspendLayout();
			SuspendLayout();
			// 
			// outerSplitContainer
			// 
			outerSplitContainer.Dock = DockStyle.Fill;
			outerSplitContainer.Location = new Point(0, 0);
			outerSplitContainer.Name = "outerSplitContainer";
			outerSplitContainer.Orientation = Orientation.Horizontal;
			// 
			// outerSplitContainer.Panel1
			// 
			outerSplitContainer.Panel1.Controls.Add(splitContainer);
			outerSplitContainer.Panel1MinSize = 120;
			// 
			// outerSplitContainer.Panel2
			// 
			outerSplitContainer.Panel2.Controls.Add(terminalControl);
			outerSplitContainer.Panel2MinSize = 80;
			outerSplitContainer.Size = new Size(1024, 768);
			outerSplitContainer.SplitterDistance = 537;
			outerSplitContainer.TabIndex = 0;
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
			splitContainer.Panel2MinSize = 200;
			splitContainer.Size = new Size(1024, 537);
			splitContainer.SplitterDistance = 300;
			splitContainer.TabIndex = 0;
			// 
			// folderTreeControl
			// 
			folderTreeControl.Dock = DockStyle.Fill;
			folderTreeControl.Location = new Point(0, 28);
			folderTreeControl.Name = "folderTreeControl";
			folderTreeControl.Size = new Size(300, 509);
			folderTreeControl.TabIndex = 1;
			// 
			// pathItemComboBox
			// 
			pathItemComboBox.Dock = DockStyle.Top;
			pathItemComboBox.Location = new Point(0, 0);
			pathItemComboBox.Name = "pathItemComboBox";
			pathItemComboBox.Size = new Size(300, 28);
			pathItemComboBox.TabIndex = 0;
			// 
			// folderContentsControl
			// 
			folderContentsControl.Dock = DockStyle.Fill;
			folderContentsControl.Location = new Point(0, 0);
			folderContentsControl.Name = "folderContentsControl";
			folderContentsControl.Size = new Size(720, 537);
			folderContentsControl.TabIndex = 0;
			// 
			// terminalControl
			// 
			terminalControl.Dock = DockStyle.Fill;
			terminalControl.Location = new Point(0, 0);
			terminalControl.Name = "terminalControl";
			terminalControl.Size = new Size(1024, 227);
			terminalControl.TabIndex = 0;
			// 
			// ExplorerLayoutControl
			// 
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(outerSplitContainer);
			Name = "ExplorerLayoutControl";
			Size = new Size(1024, 768);
			outerSplitContainer.Panel1.ResumeLayout(false);
			outerSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)outerSplitContainer).EndInit();
			outerSplitContainer.ResumeLayout(false);
			splitContainer.Panel1.ResumeLayout(false);
			splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
			splitContainer.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private SplitContainer outerSplitContainer;
		private SplitContainer splitContainer;
		private PathItemComboBoxControl pathItemComboBox;
		private FolderTreeControl folderTreeControl;
		private FolderContentsControl folderContentsControl;
		private TerminalControl terminalControl;
	}
}
