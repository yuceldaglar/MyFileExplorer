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
			toolStrip = new ToolStrip();
			viewDropDownButton = new ToolStripDropDownButton();
			largeIconsToolStripMenuItem = new ToolStripMenuItem();
			smallIconsToolStripMenuItem = new ToolStripMenuItem();
			listToolStripMenuItem = new ToolStripMenuItem();
			detailsToolStripMenuItem = new ToolStripMenuItem();
			tilesToolStripMenuItem = new ToolStripMenuItem();
			listView = new ListView();
			SuspendLayout();
			//
			// toolStrip
			//
			toolStrip.Dock = DockStyle.Top;
			toolStrip.ImageScalingSize = new Size(20, 20);
			toolStrip.Items.AddRange(new ToolStripItem[] { viewDropDownButton });
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
			// listView
			//
			listView.Dock = DockStyle.Fill;
			listView.FullRowSelect = true;
			listView.Location = new Point(0, 28);
			listView.Name = "listView";
			listView.Size = new Size(400, 372);
			listView.HideSelection = false;
			listView.TabIndex = 1;
			listView.UseCompatibleStateImageBehavior = false;
			listView.View = View.LargeIcon;
			listView.DoubleClick += ListView_DoubleClick;
			//
			// FolderContentsControl
			//
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(listView);
			Controls.Add(toolStrip); // Added after listView so toolbar stays on top when both are docked
			Name = "FolderContentsControl";
			Size = new Size(400, 400);
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
		private ListView listView;
	}
}
