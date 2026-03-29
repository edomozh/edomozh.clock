using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Edomozh.Clock;

internal static class IconHelper
{
    public static ImageSource CreateClockIconSource()
    {
        const int size = 32;
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(System.Drawing.Color.Transparent);
        
        using var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2);
        g.DrawEllipse(pen, 2, 2, size - 5, size - 5);
        
        int cx = size / 2, cy = size / 2;
        g.DrawLine(pen, cx, cy, cx - 6, cy - 8);
        g.DrawLine(pen, cx, cy, cx + 6, cy - 10);
        
        var hIcon = bitmap.GetHicon();
        return Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }
}
