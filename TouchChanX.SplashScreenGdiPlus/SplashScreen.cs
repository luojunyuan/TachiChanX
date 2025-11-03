using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace TouchChanX.SplashScreenGdiPlus;

public interface ISplashScreen : IDisposable
{
    void Show();
}

public static class SplashScreen
{
    public static ISplashScreen Create(Stream resource, SynchronizationContext? ctx = null)
        => ctx is null
            ? new ThreadSplashScreen(resource)
            : new SyncContextSplashScreen(resource, ctx);
}

public sealed class ThreadSplashScreen(Stream resource) : SplashScreenBase(resource), ISplashScreen
{
    private readonly ManualResetEvent _disposeEvent = new(false);

    public void Show()
    {
        var showCompletedEvent = new ManualResetEvent(false);
        new Thread(() =>
        {
            DisplaySplash();
            showCompletedEvent.Set();

            _disposeEvent.WaitOne();

            ReleaseResources();

            _disposeEvent.Dispose();
            showCompletedEvent.Dispose();
        }).Start();
        showCompletedEvent.WaitOne();
    }

    public void Dispose() => _disposeEvent.Set();
}

public sealed class SyncContextSplashScreen(Stream resource, SynchronizationContext synchronization) : SplashScreenBase(resource), ISplashScreen
{
    public void Show() => synchronization.Send(_ => DisplaySplash(), null);

    public void Dispose() => synchronization.Send(_ => ReleaseResources(), null);
}

public abstract class SplashScreenBase(Stream resource)
{
    private readonly Bitmap _image = LoadBitmap(resource);
    private HWND _hWndSplash;
    private HDC _hdc;

    private static Bitmap LoadBitmap(Stream s)
    {
        using var img = Image.FromStream(s);
        return new Bitmap(img);
    }

    protected unsafe void DisplaySplash()
    {
        var image = _image;

        const string className = "SplashScreen";
        const string windowTitle = "Splash Screen";
        fixed (char* lpClassName = className)
        {
            var wndClass = new WNDCLASSEXW
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                lpfnWndProc = &WndProc,
                lpszClassName = new(lpClassName),
                hInstance = default,
            };
            PInvoke.RegisterClassEx(in wndClass);
        }

        // 高分辨率图像缩放
        const double scaleFactor = 2.0;
        const double scaleFactorMid = 1.5;
        var dpi = (int)(GetDpiScale() * 100);

        var (width, height) = dpi switch
        {
            < 150 => ((int)(image.Width / scaleFactor), (int)(image.Height / scaleFactor)),
            < 175 => ((int)(image.Width / scaleFactorMid), (int)(image.Height / scaleFactorMid)),
            _ => (image.Width, image.Height),
        };

        var (x, y) = CenterToPrimaryScreen(width, height);

        _hWndSplash = PInvoke.CreateWindowEx(
            WINDOW_EX_STYLE.WS_EX_TOOLWINDOW |
            WINDOW_EX_STYLE.WS_EX_TRANSPARENT |
            WINDOW_EX_STYLE.WS_EX_TOPMOST |
            WINDOW_EX_STYLE.WS_EX_NOACTIVATE,
            className,
            windowTitle,
            WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE,
            x, y, width, height,
            HWND.Null, null, null, null);

        _hdc = PInvoke.GetDC(_hWndSplash);

        using var g = Graphics.FromHdc(_hdc);
        g.DrawImage(image, 0, 0, width, height);

        var originalStyle = PInvoke.GetWindowLong(_hWndSplash, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        _ = PInvoke.SetWindowLong(_hWndSplash, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            originalStyle | (int)WINDOW_EX_STYLE.WS_EX_LAYERED);

        PInvoke.SetLayeredWindowAttributes(_hWndSplash, new COLORREF((uint)Color.Green.ToArgb()), 0, LAYERED_WINDOW_ATTRIBUTES_FLAGS.LWA_COLORKEY);
    }

    protected void ReleaseResources()
    {
        _image.Dispose();
        _ = PInvoke.ReleaseDC(_hWndSplash, _hdc);
        PInvoke.DestroyWindow(_hWndSplash);
    }

    private static unsafe (int X, int Y) CenterToPrimaryScreen(int width, int height)
    {
        var rcWorkArea = new Rectangle();
        PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, &rcWorkArea, 0);
        int nX = Convert.ToInt32((rcWorkArea.Left + rcWorkArea.Right) / (double)2 - width / (double)2);
        int nY = Convert.ToInt32((rcWorkArea.Top + rcWorkArea.Bottom) / (double)2 - height / (double)2);
        return (nX, nY);
    }

    private static double GetDpiScale()
    {
        var monitor = PInvoke.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);

        if (monitor != nint.Zero && OperatingSystem.IsWindowsVersionAtLeast(8, 1))
        {
            PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpi, out _);

            return dpi == 0 ? 1 : dpi / 96d;
        }

        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static LRESULT WndProc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        => PInvoke.DefWindowProc(hwnd, uMsg, wParam, lParam);
}
