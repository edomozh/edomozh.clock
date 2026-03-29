using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Edomozh.Clock.Native;

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

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
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

        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        if (enable)
        {
            exStyle |= WS_EX_TRANSPARENT;
        }
        else
        {
            exStyle &= ~WS_EX_TRANSPARENT;
        }

        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
    }

    public static void SetOverlayMode(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
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

    public static bool IsPositionOnScreen(double x, double y)
    {
        return x >= SystemParameters.VirtualScreenLeft &&
               x < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth &&
               y >= SystemParameters.VirtualScreenTop &&
               y < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
    }

    public static (double X, double Y) ClampToScreen(double x, double y, double width, double height)
    {
        double left = SystemParameters.VirtualScreenLeft;
        double top = SystemParameters.VirtualScreenTop;
        double right = left + SystemParameters.VirtualScreenWidth;
        double bottom = top + SystemParameters.VirtualScreenHeight;

        x = Math.Max(left, Math.Min(x, right - width));
        y = Math.Max(top, Math.Min(y, bottom - height));

        return (x, y);
    }
}
