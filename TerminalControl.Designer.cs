namespace MyFileExplorer
{
	partial class TerminalControl
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
			topPanel = new Panel();
			clearButton = new Button();
			startStopButton = new Button();
			shellComboBox = new ComboBox();
			shellLabel = new Label();
			outputTextBox = new TerminalOutputRichTextBox();
			commandTextBox = new TextBox();
			topPanel.SuspendLayout();
			SuspendLayout();
			// 
			// topPanel
			// 
			topPanel.Controls.Add(clearButton);
			topPanel.Controls.Add(startStopButton);
			topPanel.Controls.Add(shellComboBox);
			topPanel.Controls.Add(shellLabel);
			topPanel.Dock = DockStyle.Top;
			topPanel.Location = new Point(0, 0);
			topPanel.Name = "topPanel";
			topPanel.Padding = new Padding(8, 6, 8, 6);
			topPanel.Size = new Size(500, 40);
			topPanel.TabIndex = 0;
			// 
			// clearButton
			// 
			clearButton.Dock = DockStyle.Right;
			clearButton.Location = new Point(330, 6);
			clearButton.Name = "clearButton";
			clearButton.Size = new Size(82, 28);
			clearButton.TabIndex = 3;
			clearButton.Text = "Clear";
			clearButton.UseVisualStyleBackColor = true;
			clearButton.Click += ClearButton_Click;
			// 
			// startStopButton
			// 
			startStopButton.Dock = DockStyle.Right;
			startStopButton.Location = new Point(412, 6);
			startStopButton.Name = "startStopButton";
			startStopButton.Size = new Size(80, 28);
			startStopButton.TabIndex = 2;
			startStopButton.Text = "Start";
			startStopButton.UseVisualStyleBackColor = true;
			startStopButton.Click += StartStopButton_Click;
			// 
			// shellComboBox
			// 
			shellComboBox.Dock = DockStyle.Left;
			shellComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			shellComboBox.FormattingEnabled = true;
			shellComboBox.Location = new Point(67, 6);
			shellComboBox.Name = "shellComboBox";
			shellComboBox.Size = new Size(150, 33);
			shellComboBox.TabIndex = 1;
			shellComboBox.SelectedIndexChanged += ShellComboBox_SelectedIndexChanged;
			// 
			// shellLabel
			// 
			shellLabel.AutoSize = true;
			shellLabel.Dock = DockStyle.Left;
			shellLabel.Location = new Point(8, 6);
			shellLabel.Name = "shellLabel";
			shellLabel.Padding = new Padding(0, 5, 6, 0);
			shellLabel.Size = new Size(59, 30);
			shellLabel.TabIndex = 0;
			shellLabel.Text = "Shell:";
			// 
			// outputTextBox
			// 
			outputTextBox.BackColor = Color.Black;
			outputTextBox.BorderStyle = BorderStyle.None;
			outputTextBox.Dock = DockStyle.Fill;
			outputTextBox.ForeColor = Color.Gainsboro;
			outputTextBox.HideSelection = true;
			outputTextBox.Location = new Point(0, 40);
			outputTextBox.Name = "outputTextBox";
			outputTextBox.ReadOnly = true;
			outputTextBox.TabStop = false;
			outputTextBox.Size = new Size(500, 220);
			outputTextBox.TabIndex = 1;
			outputTextBox.Text = "";
			outputTextBox.MouseDown += OutputTextBox_MouseDown;
			// 
			// commandTextBox
			// 
			commandTextBox.Dock = DockStyle.Bottom;
			commandTextBox.Location = new Point(0, 260);
			commandTextBox.Name = "commandTextBox";
			commandTextBox.Size = new Size(500, 31);
			commandTextBox.TabIndex = 2;
			commandTextBox.KeyDown += CommandTextBox_KeyDown;
			// 
			// TerminalControl
			// 
			AutoScaleMode = AutoScaleMode.Inherit;
			Controls.Add(outputTextBox);
			Controls.Add(commandTextBox);
			Controls.Add(topPanel);
			Name = "TerminalControl";
			Size = new Size(500, 291);
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Panel topPanel;
		private Label shellLabel;
		private ComboBox shellComboBox;
		private Button startStopButton;
		private Button clearButton;
		private TerminalOutputRichTextBox outputTextBox;
		private TextBox commandTextBox;
	}
}
