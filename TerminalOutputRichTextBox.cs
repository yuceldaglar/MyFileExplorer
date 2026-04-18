using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface for terminal output. RichEdit draws its own blinking insertion point
	/// when the selection is a zero-length range at the end of text; this control suppresses that
	/// for a display-only feel while keeping normal RichTextBox scrolling and selection colors.
	/// </summary>
	internal sealed class TerminalOutputRichTextBox : RichTextBox
	{
		private const int WM_SETFOCUS = 0x0007;
		/// <summary>Rich Edit: hide selection and insertion point when the control loses focus.</summary>
		private const int EM_HIDESELECTION = 0x400 + 59;

		private byte _deferredCaretSuppressPosted;

		public TerminalOutputRichTextBox()
		{
			// Avoid the output surface taking keyboard focus like an editor; command line stays primary.
			SetStyle(ControlStyles.Selectable, false);
			SelectionChanged += (_, _) => SuppressCaret();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			// Belt-and-suspenders with <see cref="HideSelection"/> — some RichEdit builds need the message.
			_ = SendMessageW(Handle, EM_HIDESELECTION, (IntPtr)1, IntPtr.Zero);
			SuppressCaret();
		}

		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			SuppressCaret();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			SuppressCaret();
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == WM_SETFOCUS && IsHandleCreated)
				SuppressCaret();
		}

		/// <summary>
		/// Call after programmatically moving the selection or appending text so RichEdit does not leave a visible caret.
		/// </summary>
		internal void SuppressCaret()
		{
			if (!IsHandleCreated)
				return;
			HideCaret(Handle);
			// RichEdit sometimes recreates the caret on the next message pump; coalesce BeginInvoke under rapid output.
			if (_deferredCaretSuppressPosted != 0)
				return;
			_deferredCaretSuppressPosted = 1;
			BeginInvoke(SuppressCaretDeferred);
		}

		private void SuppressCaretDeferred()
		{
			_deferredCaretSuppressPosted = 0;
			if (!IsHandleCreated || IsDisposed)
				return;
			HideCaret(Handle);
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);
	}
}
