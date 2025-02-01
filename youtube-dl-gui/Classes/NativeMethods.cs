﻿#nullable enable
namespace youtube_dl_gui;

using System.Runtime.InteropServices;
using System.Windows.Forms;

internal static class NativeMethods {
    #region Ini
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    public static extern int WritePrivateProfileString(string lpAppName,
        string lpKeyName,
        string lpString,
        string lpFileName);
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    public static extern int GetPrivateProfileString(string lpAppName,
        string lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedString,
        int nSize,
        string lpFileName);
    #endregion

    #region System Hand Cursor for LinkLabelHand and Other Controls
    public static nint HAND = 32_649;
    public static readonly Cursor SystemHandCursor = new(LoadCursor(0, HAND));
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint LoadCursor(nint hInstance, nint lpCursorName);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint SetCursor(nint hCursor);
    #endregion

    #region SplitButton, ExtendedTextBox, ExtendedRichTextBox

    #region TextBox Text Margins
    public const int EM_SETMARGINS = 0xd3;
    public const int EC_RIGHTMARGIN = 2;
    public const int EC_LEFTMARGIN = 1;
    #endregion

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern nint SendMessage(nint hWnd, int wMsg, nint wParam, nint lParam);
    #endregion

    #region Clipboard scanner for batch downloader
    internal const int WM_CLIPBOARDUPDATE = 0x031D;
    internal const int WM_DESTROYCLIPBOARD = 0x0307;
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "AddClipboardFormatListener")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AddClipboardFormatListener(
        [In] nint hwnd
    );
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RemoveClipboardFormatListener")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool RemoveClipboardFormatListener(
        [In] nint hwnd
    );
    #endregion
}