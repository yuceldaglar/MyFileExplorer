using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface: not keyboard-selectable so the multiline Edit control never keeps focus
	/// and never shows the system insertion caret over the stream. Click still raises <see cref="Control.MouseDown"/>
	/// so the parent can move focus to the command line.
	/// </summary>
	internal sealed class TerminalOutputTextBox : TextBox
	{
		private const int WM_SETFOCUS = 0x0007;
		private const int WM_KILLFOCUS = 0x0008;

		public TerminalOutputTextBox()
		{
			SetStyle(ControlStyles.Selectable, false);
			// Read-only log is not a typing surface; arrow avoids an I-beam over non-editable output.
			Cursor = Cursors.Arrow;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputTB.OnMouseDown",
				$"Button={e.Button} Clicks={e.Clicks} Location=({e.X},{e.Y})");
			base.OnMouseDown(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputTB.OnGotFocus", $"Handle={Handle}");
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputTB.OnLostFocus", $"Handle={Handle}");
			base.OnLostFocus(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_SETFOCUS || m.Msg == WM_KILLFOCUS)
			{
				var name = m.Msg == WM_SETFOCUS ? "WM_SETFOCUS" : "WM_KILLFOCUS";
				TerminalDiagnosticLog.Line("OutputTB.WndProc",
					$"{name}(0x{m.Msg:X4}) wParam=0x{m.WParam.ToInt64():X} lParam=0x{m.LParam.ToInt64():X}");
			}

			base.WndProc(ref m);
		}
	}
}
