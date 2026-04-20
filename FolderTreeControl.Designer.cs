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
			components = new System.ComponentModel.Container();
			folderTreeView = new TreeView();
			treeContextMenuStrip = new ContextMenuStrip(components);
			treeOpenToolStripMenuItem = new ToolStripMenuItem();
			treeOpenInNewTabToolStripMenuItem = new ToolStripMenuItem();
			treeExpandToolStripMenuItem = new ToolStripMenuItem();
			treeCollapseToolStripMenuItem = new ToolStripMenuItem();
			treeSep1 = new ToolStripSeparator();
			treeCutToolStripMenuItem = new ToolStripMenuItem();
			treeCopyToolStripMenuItem = new ToolStripMenuItem();
			treePasteToolStripMenuItem = new ToolStripMenuItem();
			treeSep2 = new ToolStripSeparator();
			treeNewFolderToolStripMenuItem = new ToolStripMenuItem();
			treeSep3 = new ToolStripSeparator();
			treeRenameToolStripMenuItem = new ToolStripMenuItem();
			treeDeleteToolStripMenuItem = new ToolStripMenuItem();
			treeSep4 = new ToolStripSeparator();
			treePropertiesToolStripMenuItem = new ToolStripMenuItem();
			treeContextMenuStrip.SuspendLayout();
			SuspendLayout();
			//
			// folderTreeView
			//
			folderTreeView.Dock = DockStyle.Fill;
			folderTreeView.LabelEdit = true;
			folderTreeView.Location = new Point(0, 0);
			folderTreeView.Name = "folderTreeView";
			folderTreeView.Size = new Size(200, 200);
			folderTreeView.TabIndex = 0;
			folderTreeView.ContextMenuStrip = treeContextMenuStrip;
			folderTreeView.BeforeExpand += FolderTreeView_BeforeExpand;
			folderTreeView.AfterSelect += FolderTreeView_AfterSelect;
			folderTreeView.AfterLabelEdit += TreeRename_AfterLabelEdit;
			//
			// treeContextMenuStrip
			//
			treeContextMenuStrip.ImageScalingSize = new Size(20, 20);
			treeContextMenuStrip.Items.AddRange(new ToolStripItem[] {
				treeOpenToolStripMenuItem,
				treeOpenInNewTabToolStripMenuItem,
				treeExpandToolStripMenuItem,
				treeCollapseToolStripMenuItem,
				treeSep1,
				treeCutToolStripMenuItem,
				treeCopyToolStripMenuItem,
				treePasteToolStripMenuItem,
				treeSep2,
				treeNewFolderToolStripMenuItem,
				treeSep3,
				treeRenameToolStripMenuItem,
				treeDeleteToolStripMenuItem,
				treeSep4,
				treePropertiesToolStripMenuItem
			});
			treeContextMenuStrip.Name = "treeContextMenuStrip";
			treeContextMenuStrip.Size = new Size(181, 298);
			treeContextMenuStrip.Opening += TreeContextMenu_Opening;
			//
			// treeOpenToolStripMenuItem
			//
			treeOpenToolStripMenuItem.Name = "treeOpenToolStripMenuItem";
			treeOpenToolStripMenuItem.Size = new Size(180, 26);
			treeOpenToolStripMenuItem.Text = "Open";
			treeOpenToolStripMenuItem.Click += TreeOpen_Click;
			//
			// treeOpenInNewTabToolStripMenuItem
			//
			treeOpenInNewTabToolStripMenuItem.Name = "treeOpenInNewTabToolStripMenuItem";
			treeOpenInNewTabToolStripMenuItem.Size = new Size(180, 26);
			treeOpenInNewTabToolStripMenuItem.Text = "Open in another tab";
			treeOpenInNewTabToolStripMenuItem.Click += TreeOpenInNewTab_Click;
			//
			// treeExpandToolStripMenuItem / treeCollapseToolStripMenuItem / treeSep1 / ...
			//
			treeExpandToolStripMenuItem.Name = "treeExpandToolStripMenuItem";
			treeExpandToolStripMenuItem.Size = new Size(180, 26);
			treeExpandToolStripMenuItem.Text = "Expand";
			treeExpandToolStripMenuItem.Click += TreeExpand_Click;
			treeCollapseToolStripMenuItem.Name = "treeCollapseToolStripMenuItem";
			treeCollapseToolStripMenuItem.Size = new Size(180, 26);
			treeCollapseToolStripMenuItem.Text = "Collapse";
			treeCollapseToolStripMenuItem.Click += TreeCollapse_Click;
			treeSep1.Name = "treeSep1";
			treeSep1.Size = new Size(177, 6);
			treeCutToolStripMenuItem.Name = "treeCutToolStripMenuItem";
			treeCutToolStripMenuItem.Size = new Size(180, 26);
			treeCutToolStripMenuItem.Text = "Cut";
			treeCutToolStripMenuItem.Click += TreeCut_Click;
			treeCopyToolStripMenuItem.Name = "treeCopyToolStripMenuItem";
			treeCopyToolStripMenuItem.Size = new Size(180, 26);
			treeCopyToolStripMenuItem.Text = "Copy";
			treeCopyToolStripMenuItem.Click += TreeCopy_Click;
			treePasteToolStripMenuItem.Name = "treePasteToolStripMenuItem";
			treePasteToolStripMenuItem.Size = new Size(180, 26);
			treePasteToolStripMenuItem.Text = "Paste";
			treePasteToolStripMenuItem.Click += TreePaste_Click;
			treeSep2.Name = "treeSep2";
			treeSep2.Size = new Size(177, 6);
			treeNewFolderToolStripMenuItem.Name = "treeNewFolderToolStripMenuItem";
			treeNewFolderToolStripMenuItem.Size = new Size(180, 26);
			treeNewFolderToolStripMenuItem.Text = "New folder";
			treeNewFolderToolStripMenuItem.Click += TreeNewFolder_Click;
			treeSep3.Name = "treeSep3";
			treeSep3.Size = new Size(177, 6);
			treeRenameToolStripMenuItem.Name = "treeRenameToolStripMenuItem";
			treeRenameToolStripMenuItem.Size = new Size(180, 26);
			treeRenameToolStripMenuItem.Text = "Rename";
			treeRenameToolStripMenuItem.Click += TreeRename_Click;
			treeDeleteToolStripMenuItem.Name = "treeDeleteToolStripMenuItem";
			treeDeleteToolStripMenuItem.Size = new Size(180, 26);
			treeDeleteToolStripMenuItem.Text = "Delete";
			treeDeleteToolStripMenuItem.Click += TreeDelete_Click;
			treeSep4.Name = "treeSep4";
			treeSep4.Size = new Size(177, 6);
			treePropertiesToolStripMenuItem.Name = "treePropertiesToolStripMenuItem";
			treePropertiesToolStripMenuItem.Size = new Size(180, 26);
			treePropertiesToolStripMenuItem.Text = "Properties";
			treePropertiesToolStripMenuItem.Click += TreeProperties_Click;
			//
			// FolderTreeControl
			//
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(folderTreeView);
			Name = "FolderTreeControl";
			Size = new Size(200, 200);
			treeContextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private TreeView folderTreeView;
		private ContextMenuStrip treeContextMenuStrip;
		private ToolStripMenuItem treeOpenToolStripMenuItem;
		private ToolStripMenuItem treeOpenInNewTabToolStripMenuItem;
		private ToolStripMenuItem treeExpandToolStripMenuItem;
		private ToolStripMenuItem treeCollapseToolStripMenuItem;
		private ToolStripSeparator treeSep1;
		private ToolStripMenuItem treeCutToolStripMenuItem;
		private ToolStripMenuItem treeCopyToolStripMenuItem;
		private ToolStripMenuItem treePasteToolStripMenuItem;
		private ToolStripSeparator treeSep2;
		private ToolStripMenuItem treeNewFolderToolStripMenuItem;
		private ToolStripSeparator treeSep3;
		private ToolStripMenuItem treeRenameToolStripMenuItem;
		private ToolStripMenuItem treeDeleteToolStripMenuItem;
		private ToolStripSeparator treeSep4;
		private ToolStripMenuItem treePropertiesToolStripMenuItem;
	}
}
