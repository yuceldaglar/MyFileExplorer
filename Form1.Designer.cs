namespace MyFileExplorer
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			explorerTabControl = new TabControl();
			SuspendLayout();
			// 
			// explorerTabControl
			// 
			explorerTabControl.Dock = DockStyle.Fill;
			explorerTabControl.Location = new Point(0, 0);
			explorerTabControl.Name = "explorerTabControl";
			explorerTabControl.SelectedIndex = 0;
			explorerTabControl.Size = new Size(800, 450);
			explorerTabControl.TabIndex = 0;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(explorerTabControl);
			KeyPreview = true;
			Name = "Form1";
			Text = "My File Explorer";
			KeyDown += Form1_KeyDown;
			ResumeLayout(false);
		}

		#endregion

		private TabControl explorerTabControl;
	}
}
