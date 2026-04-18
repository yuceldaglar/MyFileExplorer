using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface: not keyboard-selectable and not an Edit control, so no insertion caret
	/// appears over the stream. Clicks still raise <see cref="Control.MouseDown"/> so the parent can
	/// move focus to the command line.
	/// </summary>
	internal sealed class TerminalOutputListBox : ListBox
	{
		private const int WM_MOUSEACTIVATE = 0x0021;
		private const int WM_SETFOCUS = 0x0007;
		private const int MA_NOACTIVATE = 3;

		public TerminalOutputListBox()
		{
			SetStyle(ControlStyles.Selectable, false);
			Cursor = Cursors.Arrow;
			IntegralHeight = false;
			SelectionMode = SelectionMode.None;
			BorderStyle = BorderStyle.None;
			HorizontalScrollbar = true;
			TabStop = false;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputLB.OnMouseDown",
				$"Button={e.Button} Clicks={e.Clicks} Location=({e.X},{e.Y})");
			base.OnMouseDown(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputLB.OnGotFocus", $"Handle={Handle}");
			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			TerminalDiagnosticLog.Line("OutputLB.OnLostFocus", $"Handle={Handle}");
			base.OnLostFocus(e);
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_MOUSEACTIVATE)
			{
				m.Result = (IntPtr)MA_NOACTIVATE;
				return;
			}

			if (m.Msg == WM_SETFOCUS)
			{
				TerminalDiagnosticLog.Line("OutputLB.WndProc",
					$"WM_SETFOCUS(0x{m.Msg:X4}) wParam=0x{m.WParam.ToInt64():X} lParam=0x{m.LParam.ToInt64():X}");
				base.WndProc(ref m);
				if (Parent is TerminalControl terminal)
					terminal.DivertFocusFromOutput();
				return;
			}

			base.WndProc(ref m);
		}
	}
}
