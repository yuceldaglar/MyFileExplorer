using System.Runtime.InteropServices;

namespace MyFileExplorer
{
	/// <summary>
	/// Read-only log surface for terminal output. RichEdit can draw a blinking insertion point at the
	/// selection; we use <see cref="HideSelection"/> / <c>EM_HIDESELECTION</c>, non-selectable style, and
	/// <see cref="HideCaret"/> only while this control actually has focus — calling <c>HideCaret</c> on
	/// this HWND when the command line owns the thread caret can hide the caret for the wrong control.
	/// </summary>
	internal sealed class TerminalOutputRichTextBox : RichTextBox
	{
		/// <summary>Rich Edit: hide selection and insertion point when the control loses focus.</summary>
		private const int EM_HIDESELECTION = 0x400 + 59;

		private byte _deferredCaretSuppressPosted;

		public TerminalOutputRichTextBox()
		{
			// Avoid the output surface taking keyboard focus like an editor; command line stays primary.
			SetStyle(ControlStyles.Selectable, false);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			// Belt-and-suspenders with <see cref="HideSelection"/> — some RichEdit builds need the message.
			_ = SendMessageW(Handle, EM_HIDESELECTION, (IntPtr)1, IntPtr.Zero);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			SuppressCaret();
		}

		/// <summary>
		/// Hides the system caret only when this control owns it. Safe to call after appends even when unfocused (no-op).
		/// </summary>
		internal void SuppressCaret()
		{
			if (!IsHandleCreated || !Focused)
				return;
			HideCaret(Handle);
			if (_deferredCaretSuppressPosted != 0)
				return;
			_deferredCaretSuppressPosted = 1;
			BeginInvoke(SuppressCaretDeferred);
		}

		private void SuppressCaretDeferred()
		{
			_deferredCaretSuppressPosted = 0;
			if (!IsHandleCreated || IsDisposed || !Focused)
				return;
			HideCaret(Handle);
		}

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessageW(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);
	}
}
