using System.ComponentModel;

namespace MyFileExplorer
{
	/// <summary>
	/// User control that contains a ComboBox and displays a list of <see cref="PathItem"/> values.
	/// </summary>
	public partial class PathItemComboBoxControl : UserControl
	{
		/// <summary>
		/// Gets the ComboBox that displays the PathItems. Use this to handle additional events or customize appearance.
		/// </summary>
		[Browsable(false)]
		public ComboBox ComboBox => comboBox;

		/// <summary>
		/// Gets or sets the list of PathItems displayed in the ComboBox. The dropdown shows each item's <see cref="PathItem.Name"/>.
		/// </summary>
		[Category("Data")]
		[Description("The list of PathItems shown in the ComboBox.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IEnumerable<PathItem>? Items
		{
			get => comboBox.DataSource as IEnumerable<PathItem>;
			set
			{
				var list = value?.ToList() ?? new List<PathItem>();
				comboBox.DataSource = null;
				comboBox.DisplayMember = nameof(PathItem.Name);
				comboBox.ValueMember = nameof(PathItem.Path);
				comboBox.DataSource = list;
			}
		}

		/// <summary>
		/// Gets or sets the currently selected PathItem, or null if none is selected.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PathItem? SelectedItem
		{
			get => comboBox.SelectedItem as PathItem;
			set => comboBox.SelectedItem = value;
		}

		/// <summary>
		/// Gets or sets the path of the currently selected item (by matching <see cref="PathItem.Path"/>), or null/empty if none.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string? SelectedPath
		{
			get => (comboBox.SelectedItem as PathItem)?.Path;
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					comboBox.SelectedIndex = -1;
					return;
				}
				var list = comboBox.DataSource as IList<PathItem>;
				if (list == null)
					return;
				var index = list.ToList().FindIndex(p => string.Equals(p.Path, value, StringComparison.OrdinalIgnoreCase));
				comboBox.SelectedIndex = index >= 0 ? index : -1;
			}
		}

		/// <summary>
		/// Occurs when the selected PathItem changes.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the user or code changes the selected item.")]
		public event EventHandler? SelectedItemChanged;

		/// <summary>
		/// Occurs when the selected folder (path) changes. Event args contain the path of the selected item.
		/// </summary>
		[Category("Action")]
		[Description("Raised when the selected item changes; use e.FolderPath to get the selected path.")]
		public event EventHandler<FolderEventArgs>? SelectedFolderChanged;

		public PathItemComboBoxControl()
		{
			InitializeComponent();
		}

		private void ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
		{
			SelectedItemChanged?.Invoke(this, EventArgs.Empty);
			SelectedFolderChanged?.Invoke(this, new FolderEventArgs(SelectedPath ?? string.Empty));
		}
	}
}
