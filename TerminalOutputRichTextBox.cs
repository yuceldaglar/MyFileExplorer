using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface for terminal output: hides the system caret on focus so the pane reads as display-only.
	/// </summary>
	internal sealed class TerminalOutputRichTextBox : RichTextBox
	{
		private const int WM_SETFOCUS = 0x0007;

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == WM_SETFOCUS && IsHandleCreated)
				HideCaret(Handle);
		}

		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);
	}
}
