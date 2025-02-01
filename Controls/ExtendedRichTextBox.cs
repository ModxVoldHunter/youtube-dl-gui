﻿#nullable enable
/* ExtendedRichTextBox by murrty */

namespace murrty.controls;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using murrty.controls.natives;

public class ExtendedRichTextBox : RichTextBox {
    private bool fAutoWordSelection = false;

    [Category("Behavior")]
    [DefaultValue(false)]
    [Description("Disables auto-word selection.")]
    public new bool AutoWordSelection {
        get => fAutoWordSelection;
        set {
            fAutoWordSelection = value;
            base.AutoWordSelection = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the rich text box will only display text and now allow the user to select or highlight messages.
    /// <para>Setting BorderStyle to FixedNone is a good idea.</para>
    /// </summary>
    [Category("Behavior")]
    [DefaultValue(false)]
    [Description("If the control should ignore any inputs from the user.")]
    public bool ViewOnly {
        get; set;
    } = false;

    [DefaultValue(false)]
    public new bool HideSelection {
        get => base.HideSelection;
        set => base.HideSelection = value;
    }

    public ExtendedRichTextBox() {
        base.HideSelection = false;
        ScrollPos = new SCROLLINFO() {
            cbSize = Marshal.SizeOf<SCROLLINFO>(),
            fMask = ScrollInfoMask.SIF_ALL
        };
    }

    private const int WM_SETFOCUS = 0x07;
    private const int WM_ENABLE = 0x0A;
    private const int WM_SETCURSOR = 0x20;

    private const int WM_USER = 0x400;
    private const int WM_SETREDRAW = 0x000B;
    private const int EM_GETEVENTMASK = WM_USER + 59;
    private const int EM_SETEVENTMASK = WM_USER + 69;
    private const int EM_GETSCROLLPOS = WM_USER + 221;
    private const int EM_SETSCROLLPOS = WM_USER + 222;

    private System.Drawing.Point _ScrollPoint;
    private bool _Painting = true;
    private nint _EventMask;
    private int _SuspendIndex = 0;
    private int _SuspendLength = 0;
    private SCROLLINFO ScrollPos;

    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetScrollInfo(IntPtr hwnd, SBOrientation fnBar, ref SCROLLINFO lpsi);

    /// <summary>
    /// Appends text to the rich text box, scrolling to the bottom when necessary.
    /// </summary>
    /// <param name="text">Text to append.</param>
    public new void AppendText(string text) => AppendText(text, false);

    public void AppendLine(string text) => AppendText(text, true);

    public void AppendText(string text, bool newline) {
        if (text.Length < 1)
            return;

        GetScrollInfo(this.Handle, SBOrientation.SB_VERT, ref ScrollPos);
        //Console.WriteLine($"nPage {ScrollPos.nPage} | nPos {ScrollPos.nPos} | nMax {ScrollPos.nMax} || (nPos + nPage {ScrollPos.nPos + ScrollPos.nPage}) (nMax - nPos {ScrollPos.nMax - ScrollPos.nPos})");

        SuspendPaint();
        if (ScrollPos.ScrolledToBottom) {
            base.AppendText(newline && Text.Length > 0 ? Environment.NewLine + text : text);
            ResumePaint();
            NativeMethods.SendMessage(this.Handle, 0x115, 7, 0);
        }
        else {
            base.AppendText(newline && Text.Length > 0 ? Environment.NewLine + text : text);
            ResumePaint();
        }
    }

    public void ScrollToTop() {
        NativeMethods.SendMessage(this.Handle, 0x115, 6, 0);
    }

    public void ScrollToBottom() {
        NativeMethods.SendMessage(this.Handle, 0x115, 7, 0);
    }

    private void SuspendPaint() {
        if (_Painting) {
            _SuspendIndex = this.SelectionStart;
            _SuspendLength = this.SelectionLength;
            NativeMethods.SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);
            NativeMethods.SendMessage(this.Handle, WM_SETREDRAW, 0, 0);
            _EventMask = NativeMethods.SendMessage(this.Handle, EM_GETEVENTMASK, 0, 0);
            _Painting = false;
        }
    }
    private void ResumePaint() {
        if (!_Painting) {
            this.Select(_SuspendIndex, _SuspendLength);
            NativeMethods.SendMessage(this.Handle, EM_SETSCROLLPOS, 0, ref _ScrollPoint);
            NativeMethods.SendMessage(this.Handle, EM_SETEVENTMASK, 0, _EventMask);
            NativeMethods.SendMessage(this.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
            _Painting = true;
            this.Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        if (!AutoWordSelection) {
            base.AutoWordSelection = true;
            base.AutoWordSelection = false;
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    protected override void WndProc(ref Message m) {
        switch (m.Msg) {
            case WM_SETFOCUS when ViewOnly:
            case WM_ENABLE when ViewOnly:
            case WM_SETCURSOR when ViewOnly: {
                m.Result = IntPtr.Zero;
            } return;

            default: {
                base.WndProc(ref m);
            } break;
        }
    }
}

// I'm not moving this, figure it out.
[Flags]
internal enum ScrollInfoMask : uint {
    SIF_RANGE = 0x1,
    SIF_PAGE = 0x2,
    SIF_POS = 0x4,
    SIF_DISABLENOSCROLL = 0x8,
    SIF_TRACKPOS = 0x10,
    SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS),
}
[Flags]
internal enum SBOrientation : int {
    SB_HORZ = 0x0,
    SB_VERT = 0x1,
    SB_CTL = 0x2,
    SB_BOTH = SB_VERT | SB_CTL
}
[Serializable, StructLayout(LayoutKind.Sequential)]
internal struct SCROLLINFO {
    public int cbSize; // (uint) int is because of Marshal.SizeOf
    public ScrollInfoMask fMask;
    public int nMin;
    public int nMax;
    public uint nPage;
    public int nPos;
    public int nTrackPos;
    public readonly bool ScrolledToBottom => nPage + 2 >= nMax - nPos;
}