using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Edomozh.Clock.Helpers;

public static class WindowHelper
{
    #region Constants

    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_TRANSPARENT = 0x00000020;  // Click-through
    private const int WS_EX_LAYERED = 0x00080000;      // Layered window (for transparency)
    private const int WS_EX_TOOLWINDOW = 0x00000080;   // Hide from Alt+Tab
    private const int WS_EX_NOACTIVATE = 0x08000000;   // Don't activate on click

    #endregion

    #region P/Invoke

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr newValue);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr32(IntPtr hwnd, int index, IntPtr newValue);

    private static IntPtr GetWindowLongPtr(IntPtr hwnd, int index)
    {
        return IntPtr.Size == 8 
            ? GetWindowLongPtr64(hwnd, index) 
            : GetWindowLongPtr32(hwnd, index);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr newValue)
    {
        return IntPtr.Size == 8 
            ? SetWindowLongPtr64(hwnd, index, newValue) 
            : SetWindowLongPtr32(hwnd, index, newValue);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint flags);

    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    #endregion

    public static void SetClickThrough(Window window, bool enable)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();

        if (enable)
        {
            exStyle |= WS_EX_TRANSPARENT;
        }
        else
        {
            exStyle &= ~WS_EX_TRANSPARENT;
        }

        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
    }

    public static void SetOverlayMode(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
        exStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(exStyle));
    }

    public static void SetTopmost(Window window, bool topmost)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        SetWindowPos(
            hwnd,
            topmost ? HWND_TOPMOST : HWND_NOTOPMOST,
            0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }
}
