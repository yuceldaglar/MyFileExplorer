namespace MyFileExplorer
{
	partial class PathItemComboBoxControl
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
			comboBox = new ComboBox();
			SuspendLayout();
			// 
			// comboBox
			// 
			comboBox.Dock = DockStyle.Fill;
			comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBox.FormattingEnabled = true;
			comboBox.Location = new Point(0, 0);
			comboBox.Name = "comboBox";
			comboBox.Size = new Size(200, 33);
			comboBox.TabIndex = 0;
			comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
			// 
			// PathItemComboBoxControl
			// 
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(comboBox);
			Name = "PathItemComboBoxControl";
			Size = new Size(200, 28);
			ResumeLayout(false);
		}

		#endregion

		private ComboBox comboBox;
	}
}
