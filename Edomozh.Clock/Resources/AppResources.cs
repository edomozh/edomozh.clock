using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Icon = System.Drawing.Icon;
using Application = System.Windows.Application;

namespace Edomozh.Clock.Resources;

internal static class AppResources
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private static readonly Lazy<Icon> _clockIcon = new(CreateClockIcon, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly Lazy<ImageSource> _clockIconImageSource = new(CreateClockIconImageSource, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Icon ClockIcon => _clockIcon.Value;
    public static ImageSource ClockIconImageSource => _clockIconImageSource.Value;

    private static Icon CreateClockIcon()
    {
        var streamInfo = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/clock.ico"));
        if (streamInfo != null)
        {
            using var bitmap = new Bitmap(streamInfo.Stream);
            var hIcon = bitmap.GetHicon();
            // Create a copy that owns the handle (Icon.FromHandle doesn't take ownership)
            var icon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);
            return icon;
        }
        return SystemIcons.Application;
    }

    private static ImageSource CreateClockIconImageSource()
    {
        return new BitmapImage(new Uri("pack://application:,,,/Resources/clock.ico"));
    }
}
