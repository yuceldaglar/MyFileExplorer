namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface: not keyboard-selectable so the multiline Edit control never keeps focus
	/// and never shows the system insertion caret over the stream. Click still raises <see cref="Control.MouseDown"/>
	/// so the parent can move focus to the command line.
	/// </summary>
	internal sealed class TerminalOutputTextBox : TextBox
	{
		public TerminalOutputTextBox()
		{
			SetStyle(ControlStyles.Selectable, false);
			// Read-only log is not a typing surface; arrow avoids an I-beam over non-editable output.
			Cursor = Cursors.Arrow;
		}
	}
}
