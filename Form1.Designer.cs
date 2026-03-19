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
			explorerLayoutControl1 = new ExplorerLayoutControl();
			SuspendLayout();
			// 
			// explorerLayoutControl1
			// 
			explorerLayoutControl1.Dock = DockStyle.Fill;
			explorerLayoutControl1.Location = new Point(0, 0);
			explorerLayoutControl1.Name = "explorerLayoutControl1";
			explorerLayoutControl1.Size = new Size(800, 450);
			explorerLayoutControl1.TabIndex = 0;
			explorerLayoutControl1.Load += explorerLayoutControl1_Load;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(explorerLayoutControl1);
			KeyPreview = true;
			Name = "Form1";
			Text = "Form1";
			KeyDown += Form1_KeyDown;
			ResumeLayout(false);
		}

		#endregion

		private ExplorerLayoutControl explorerLayoutControl1;
	}
}
