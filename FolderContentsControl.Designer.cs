namespace MyFileExplorer
{
	partial class FolderContentsControl
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
			toolStrip = new ToolStrip();
			viewDropDownButton = new ToolStripDropDownButton();
			largeIconsToolStripMenuItem = new ToolStripMenuItem();
			smallIconsToolStripMenuItem = new ToolStripMenuItem();
			listToolStripMenuItem = new ToolStripMenuItem();
			detailsToolStripMenuItem = new ToolStripMenuItem();
			tilesToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			refreshToolStripButton = new ToolStripButton();
			listView = new ListView();
			contextMenuStrip = new ContextMenuStrip(components);
			openToolStripMenuItem = new ToolStripMenuItem();
			openInNewTabToolStripMenuItem = new ToolStripMenuItem();
			renameToolStripMenuItem = new ToolStripMenuItem();
			deleteToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			cutToolStripMenuItem = new ToolStripMenuItem();
			copyToolStripMenuItem = new ToolStripMenuItem();
			pasteToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			newFolderToolStripMenuItem = new ToolStripMenuItem();
			refreshContextToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator4 = new ToolStripSeparator();
			propertiesToolStripMenuItem = new ToolStripMenuItem();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			//
			// toolStrip
			//
			toolStrip.Dock = DockStyle.Top;
			toolStrip.ImageScalingSize = new Size(20, 20);
			toolStrip.Items.AddRange(new ToolStripItem[] { viewDropDownButton, toolStripSeparator1, refreshToolStripButton });
			toolStrip.Location = new Point(0, 0);
			toolStrip.Name = "toolStrip";
			toolStrip.Size = new Size(400, 28);
			toolStrip.TabIndex = 0;
			toolStrip.Text = "toolStrip1";
			//
			// viewDropDownButton
			//
			viewDropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			viewDropDownButton.DropDownItems.AddRange(new ToolStripItem[] {
				largeIconsToolStripMenuItem,
				smallIconsToolStripMenuItem,
				listToolStripMenuItem,
				detailsToolStripMenuItem,
				tilesToolStripMenuItem
			});
			viewDropDownButton.Name = "viewDropDownButton";
			viewDropDownButton.Size = new Size(46, 24);
			viewDropDownButton.Text = "View";
			//
			// largeIconsToolStripMenuItem
			//
			largeIconsToolStripMenuItem.Name = "largeIconsToolStripMenuItem";
			largeIconsToolStripMenuItem.Size = new Size(180, 26);
			largeIconsToolStripMenuItem.Text = "Large Icons";
			largeIconsToolStripMenuItem.Click += ViewMenuItem_Click;
			//
			// smallIconsToolStripMenuItem
			//
			smallIconsToolStripMenuItem.Name = "smallIconsToolStripMenuItem";
			smallIconsToolStripMenuItem.Size = new Size(180, 26);
			smallIconsToolStripMenuItem.Text = "Small Icons";
			smallIconsToolStripMenuItem.Click += ViewMenuItem_Click;
			//
			// listToolStripMenuItem
			//
			listToolStripMenuItem.Name = "listToolStripMenuItem";
			listToolStripMenuItem.Size = new Size(180, 26);
			listToolStripMenuItem.Text = "List";
			listToolStripMenuItem.Click += ViewMenuItem_Click;
			//
			// detailsToolStripMenuItem
			//
			detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
			detailsToolStripMenuItem.Size = new Size(180, 26);
			detailsToolStripMenuItem.Text = "Details";
			detailsToolStripMenuItem.Click += ViewMenuItem_Click;
			//
			// tilesToolStripMenuItem
			//
			tilesToolStripMenuItem.Name = "tilesToolStripMenuItem";
			tilesToolStripMenuItem.Size = new Size(180, 26);
			tilesToolStripMenuItem.Text = "Tiles";
			tilesToolStripMenuItem.Click += ViewMenuItem_Click;
			//
			// toolStripSeparator1
			//
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 28);
			//
			// refreshToolStripButton
			//
			refreshToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
			refreshToolStripButton.Name = "refreshToolStripButton";
			refreshToolStripButton.Size = new Size(52, 24);
			refreshToolStripButton.Text = "Refresh";
			refreshToolStripButton.Click += RefreshToolStripButton_Click;
			//
			// listView
			//
			listView.Dock = DockStyle.Fill;
			listView.FullRowSelect = true;
			listView.HideSelection = false;
			listView.LabelEdit = true;
			listView.Location = new Point(0, 28);
			listView.Name = "listView";
			listView.Size = new Size(400, 372);
			listView.TabIndex = 1;
			listView.UseCompatibleStateImageBehavior = false;
			listView.View = View.LargeIcon;
			listView.AfterLabelEdit += ListView_AfterLabelEdit;
			listView.ContextMenuStrip = contextMenuStrip;
			listView.DoubleClick += ListView_DoubleClick;
			listView.KeyDown += ListView_KeyDown;
			//
			// contextMenuStrip
			//
			contextMenuStrip.ImageScalingSize = new Size(20, 20);
			contextMenuStrip.Items.AddRange(new ToolStripItem[] {
				openToolStripMenuItem,
				openInNewTabToolStripMenuItem,
				renameToolStripMenuItem,
				deleteToolStripMenuItem,
				toolStripSeparator2,
				cutToolStripMenuItem,
				copyToolStripMenuItem,
				pasteToolStripMenuItem,
				toolStripSeparator3,
				newFolderToolStripMenuItem,
				refreshContextToolStripMenuItem,
				toolStripSeparator4,
				propertiesToolStripMenuItem
			});
			contextMenuStrip.Name = "contextMenuStrip";
			contextMenuStrip.Size = new Size(181, 220);
			contextMenuStrip.Opening += ContextMenuStrip_Opening;
			//
			// openToolStripMenuItem
			//
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.Size = new Size(180, 26);
			openToolStripMenuItem.Text = "Open";
			openToolStripMenuItem.Click += OpenContextMenu_Click;
			//
			// openInNewTabToolStripMenuItem
			//
			openInNewTabToolStripMenuItem.Name = "openInNewTabToolStripMenuItem";
			openInNewTabToolStripMenuItem.Size = new Size(180, 26);
			openInNewTabToolStripMenuItem.Text = "Open in another tab";
			openInNewTabToolStripMenuItem.Click += OpenInNewTabContextMenu_Click;
			//
			// renameToolStripMenuItem
			//
			renameToolStripMenuItem.Name = "renameToolStripMenuItem";
			renameToolStripMenuItem.Size = new Size(180, 26);
			renameToolStripMenuItem.Text = "Rename";
			renameToolStripMenuItem.Click += RenameContextMenu_Click;
			//
			// deleteToolStripMenuItem
			//
			deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			deleteToolStripMenuItem.Size = new Size(180, 26);
			deleteToolStripMenuItem.Text = "Delete";
			deleteToolStripMenuItem.Click += DeleteContextMenu_Click;
			//
			// toolStripSeparator2
			//
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(177, 6);
			//
			// cutToolStripMenuItem
			//
			cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			cutToolStripMenuItem.Size = new Size(180, 26);
			cutToolStripMenuItem.Text = "Cut";
			cutToolStripMenuItem.Click += CutContextMenu_Click;
			//
			// copyToolStripMenuItem
			//
			copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			copyToolStripMenuItem.Size = new Size(180, 26);
			copyToolStripMenuItem.Text = "Copy";
			copyToolStripMenuItem.Click += CopyContextMenu_Click;
			//
			// pasteToolStripMenuItem
			//
			pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			pasteToolStripMenuItem.Size = new Size(180, 26);
			pasteToolStripMenuItem.Text = "Paste";
			pasteToolStripMenuItem.Click += PasteContextMenu_Click;
			//
			// toolStripSeparator3
			//
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new Size(177, 6);
			//
			// newFolderToolStripMenuItem
			//
			newFolderToolStripMenuItem.Name = "newFolderToolStripMenuItem";
			newFolderToolStripMenuItem.Size = new Size(180, 26);
			newFolderToolStripMenuItem.Text = "New folder";
			newFolderToolStripMenuItem.Click += NewFolderEmptyContextMenu_Click;
			//
			// refreshContextToolStripMenuItem
			//
			refreshContextToolStripMenuItem.Name = "refreshContextToolStripMenuItem";
			refreshContextToolStripMenuItem.Size = new Size(180, 26);
			refreshContextToolStripMenuItem.Text = "Refresh";
			refreshContextToolStripMenuItem.Click += RefreshEmptyContextMenu_Click;
			//
			// toolStripSeparator4
			//
			toolStripSeparator4.Name = "toolStripSeparator4";
			toolStripSeparator4.Size = new Size(177, 6);
			//
			// propertiesToolStripMenuItem
			//
			propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			propertiesToolStripMenuItem.Size = new Size(180, 26);
			propertiesToolStripMenuItem.Text = "Properties";
			propertiesToolStripMenuItem.Click += PropertiesContextMenu_Click;
			//
			// FolderContentsControl
			//
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(listView);
			Controls.Add(toolStrip); // Added after listView so toolbar stays on top when both are docked
			Name = "FolderContentsControl";
			Size = new Size(400, 400);
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ToolStrip toolStrip;
		private ToolStripDropDownButton viewDropDownButton;
		private ToolStripMenuItem largeIconsToolStripMenuItem;
		private ToolStripMenuItem smallIconsToolStripMenuItem;
		private ToolStripMenuItem listToolStripMenuItem;
		private ToolStripMenuItem detailsToolStripMenuItem;
		private ToolStripMenuItem tilesToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton refreshToolStripButton;
		private ListView listView;
		private ContextMenuStrip contextMenuStrip;
		private ToolStripMenuItem openToolStripMenuItem;
		private ToolStripMenuItem openInNewTabToolStripMenuItem;
		private ToolStripMenuItem renameToolStripMenuItem;
		private ToolStripMenuItem deleteToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem cutToolStripMenuItem;
		private ToolStripMenuItem copyToolStripMenuItem;
		private ToolStripMenuItem pasteToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator3;
		private ToolStripMenuItem newFolderToolStripMenuItem;
		private ToolStripMenuItem refreshContextToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator4;
		private ToolStripMenuItem propertiesToolStripMenuItem;
	}
}
