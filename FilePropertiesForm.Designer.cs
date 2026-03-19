namespace MyFileExplorer
{
	partial class FilePropertiesForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			tableLayoutPanel = new TableLayoutPanel();
			labelName = new Label();
			textBoxName = new TextBox();
			labelType = new Label();
			textBoxType = new TextBox();
			labelLocation = new Label();
			textBoxLocation = new TextBox();
			labelSize = new Label();
			textBoxSize = new TextBox();
			labelCreated = new Label();
			textBoxCreated = new TextBox();
			labelModified = new Label();
			textBoxModified = new TextBox();
			labelAccessed = new Label();
			textBoxAccessed = new TextBox();
			labelAttributes = new Label();
			textBoxAttributes = new TextBox();
			buttonOk = new Button();
			tableLayoutPanel.SuspendLayout();
			SuspendLayout();
			//
			// tableLayoutPanel
			//
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel.Controls.Add(labelName, 0, 0);
			tableLayoutPanel.Controls.Add(textBoxName, 1, 0);
			tableLayoutPanel.Controls.Add(labelType, 0, 1);
			tableLayoutPanel.Controls.Add(textBoxType, 1, 1);
			tableLayoutPanel.Controls.Add(labelLocation, 0, 2);
			tableLayoutPanel.Controls.Add(textBoxLocation, 1, 2);
			tableLayoutPanel.Controls.Add(labelSize, 0, 3);
			tableLayoutPanel.Controls.Add(textBoxSize, 1, 3);
			tableLayoutPanel.Controls.Add(labelCreated, 0, 4);
			tableLayoutPanel.Controls.Add(textBoxCreated, 1, 4);
			tableLayoutPanel.Controls.Add(labelModified, 0, 5);
			tableLayoutPanel.Controls.Add(textBoxModified, 1, 5);
			tableLayoutPanel.Controls.Add(labelAccessed, 0, 6);
			tableLayoutPanel.Controls.Add(textBoxAccessed, 1, 6);
			tableLayoutPanel.Controls.Add(labelAttributes, 0, 7);
			tableLayoutPanel.Controls.Add(textBoxAttributes, 1, 7);
			tableLayoutPanel.Dock = DockStyle.Top;
			tableLayoutPanel.Location = new Point(12, 12);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.Padding = new Padding(0, 0, 0, 8);
			tableLayoutPanel.RowCount = 8;
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
			tableLayoutPanel.Size = new Size(460, 232);
			tableLayoutPanel.TabIndex = 0;
			//
			// labelName
			//
			labelName.Anchor = AnchorStyles.Left;
			labelName.AutoSize = true;
			labelName.Location = new Point(3, 6);
			labelName.Name = "labelName";
			labelName.Size = new Size(52, 20);
			labelName.TabIndex = 0;
			labelName.Text = "Name:";
			//
			// textBoxName
			//
			textBoxName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxName.Location = new Point(103, 3);
			textBoxName.Name = "textBoxName";
			textBoxName.ReadOnly = true;
			textBoxName.Size = new Size(354, 27);
			textBoxName.TabIndex = 1;
			textBoxName.TabStop = false;
			//
			// labelType
			//
			labelType.Anchor = AnchorStyles.Left;
			labelType.AutoSize = true;
			labelType.Location = new Point(3, 34);
			labelType.Name = "labelType";
			labelType.Size = new Size(43, 20);
			labelType.TabIndex = 2;
			labelType.Text = "Type:";
			//
			// textBoxType
			//
			textBoxType.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxType.Location = new Point(103, 31);
			textBoxType.Name = "textBoxType";
			textBoxType.ReadOnly = true;
			textBoxType.Size = new Size(354, 27);
			textBoxType.TabIndex = 3;
			textBoxType.TabStop = false;
			//
			// labelLocation
			//
			labelLocation.Anchor = AnchorStyles.Left;
			labelLocation.AutoSize = true;
			labelLocation.Location = new Point(3, 62);
			labelLocation.Name = "labelLocation";
			labelLocation.Size = new Size(74, 20);
			labelLocation.TabIndex = 4;
			labelLocation.Text = "Location:";
			//
			// textBoxLocation
			//
			textBoxLocation.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxLocation.Location = new Point(103, 59);
			textBoxLocation.Name = "textBoxLocation";
			textBoxLocation.ReadOnly = true;
			textBoxLocation.Size = new Size(354, 27);
			textBoxLocation.TabIndex = 5;
			textBoxLocation.TabStop = false;
			//
			// labelSize
			//
			labelSize.Anchor = AnchorStyles.Left;
			labelSize.AutoSize = true;
			labelSize.Location = new Point(3, 90);
			labelSize.Name = "labelSize";
			labelSize.Size = new Size(39, 20);
			labelSize.TabIndex = 6;
			labelSize.Text = "Size:";
			//
			// textBoxSize
			//
			textBoxSize.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxSize.Location = new Point(103, 87);
			textBoxSize.Name = "textBoxSize";
			textBoxSize.ReadOnly = true;
			textBoxSize.Size = new Size(354, 27);
			textBoxSize.TabIndex = 7;
			textBoxSize.TabStop = false;
			//
			// labelCreated
			//
			labelCreated.Anchor = AnchorStyles.Left;
			labelCreated.AutoSize = true;
			labelCreated.Location = new Point(3, 118);
			labelCreated.Name = "labelCreated";
			labelCreated.Size = new Size(62, 20);
			labelCreated.TabIndex = 8;
			labelCreated.Text = "Created:";
			//
			// textBoxCreated
			//
			textBoxCreated.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxCreated.Location = new Point(103, 115);
			textBoxCreated.Name = "textBoxCreated";
			textBoxCreated.ReadOnly = true;
			textBoxCreated.Size = new Size(354, 27);
			textBoxCreated.TabIndex = 9;
			textBoxCreated.TabStop = false;
			//
			// labelModified
			//
			labelModified.Anchor = AnchorStyles.Left;
			labelModified.AutoSize = true;
			labelModified.Location = new Point(3, 146);
			labelModified.Name = "labelModified";
			labelModified.Size = new Size(73, 20);
			labelModified.TabIndex = 10;
			labelModified.Text = "Modified:";
			//
			// textBoxModified
			//
			textBoxModified.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxModified.Location = new Point(103, 143);
			textBoxModified.Name = "textBoxModified";
			textBoxModified.ReadOnly = true;
			textBoxModified.Size = new Size(354, 27);
			textBoxModified.TabIndex = 11;
			textBoxModified.TabStop = false;
			//
			// labelAccessed
			//
			labelAccessed.Anchor = AnchorStyles.Left;
			labelAccessed.AutoSize = true;
			labelAccessed.Location = new Point(3, 174);
			labelAccessed.Name = "labelAccessed";
			labelAccessed.Size = new Size(72, 20);
			labelAccessed.TabIndex = 12;
			labelAccessed.Text = "Accessed:";
			//
			// textBoxAccessed
			//
			textBoxAccessed.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxAccessed.Location = new Point(103, 171);
			textBoxAccessed.Name = "textBoxAccessed";
			textBoxAccessed.ReadOnly = true;
			textBoxAccessed.Size = new Size(354, 27);
			textBoxAccessed.TabIndex = 13;
			textBoxAccessed.TabStop = false;
			//
			// labelAttributes
			//
			labelAttributes.Anchor = AnchorStyles.Left;
			labelAttributes.AutoSize = true;
			labelAttributes.Location = new Point(3, 202);
			labelAttributes.Name = "labelAttributes";
			labelAttributes.Size = new Size(82, 20);
			labelAttributes.TabIndex = 14;
			labelAttributes.Text = "Attributes:";
			//
			// textBoxAttributes
			//
			textBoxAttributes.Anchor = AnchorStyles.Left | AnchorStyles.Right;
			textBoxAttributes.Location = new Point(103, 199);
			textBoxAttributes.Name = "textBoxAttributes";
			textBoxAttributes.ReadOnly = true;
			textBoxAttributes.Size = new Size(354, 27);
			textBoxAttributes.TabIndex = 15;
			textBoxAttributes.TabStop = false;
			//
			// buttonOk
			//
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOk.Location = new Point(385, 256);
			buttonOk.Name = "buttonOk";
			buttonOk.Size = new Size(94, 32);
			buttonOk.TabIndex = 0;
			buttonOk.Text = "OK";
			buttonOk.UseVisualStyleBackColor = true;
			buttonOk.Click += ButtonOk_Click;
			//
			// FilePropertiesForm
			//
			AcceptButton = buttonOk;
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(484, 301);
			Controls.Add(buttonOk);
			Controls.Add(tableLayoutPanel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FilePropertiesForm";
			Padding = new Padding(12);
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Properties";
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TableLayoutPanel tableLayoutPanel;
		private Label labelName;
		private TextBox textBoxName;
		private Label labelType;
		private TextBox textBoxType;
		private Label labelLocation;
		private TextBox textBoxLocation;
		private Label labelSize;
		private TextBox textBoxSize;
		private Label labelCreated;
		private TextBox textBoxCreated;
		private Label labelModified;
		private TextBox textBoxModified;
		private Label labelAccessed;
		private TextBox textBoxAccessed;
		private Label labelAttributes;
		private TextBox textBoxAttributes;
		private Button buttonOk;
	}
}
