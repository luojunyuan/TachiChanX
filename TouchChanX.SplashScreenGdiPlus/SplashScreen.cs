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

        double scale = GetDpiScale();

        const int baseLogicalWidth = 96;
        const int baseLogicalHeight = 96;

        int width = (int)Math.Round(baseLogicalWidth * scale);
        int height = (int)Math.Round(baseLogicalHeight * scale);

        var (x, y) = CenterToPrimaryScreen(width, height);

        _hWndSplash = PInvoke.CreateWindowEx(
            WINDOW_EX_STYLE.WS_EX_TOOLWINDOW |
            WINDOW_EX_STYLE.WS_EX_TRANSPARENT |
            WINDOW_EX_STYLE.WS_EX_TOPMOST |
            WINDOW_EX_STYLE.WS_EX_NOACTIVATE |
            WINDOW_EX_STYLE.WS_EX_LAYERED,
            className,
            windowTitle,
            WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE,
            x, y, width, height,
            HWND.Null, null, null, null);

        using var sizedBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(sizedBitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            g.Clear(Color.Transparent);
            g.DrawImage(image, new Rectangle(0, 0, width, height));
        }

        var screenDc = PInvoke.GetDC(HWND.Null);
        var memDc = PInvoke.CreateCompatibleDC(screenDc);

        var hBitmap = sizedBitmap.GetHbitmap(Color.FromArgb(0));
        var oldBitmap = PInvoke.SelectObject(memDc, (HGDIOBJ)hBitmap);

        var blend = new BLENDFUNCTION
        {
            BlendOp = (byte)PInvoke.AC_SRC_OVER,
            BlendFlags = 0,
            SourceConstantAlpha = 255,
            AlphaFormat = (byte)PInvoke.AC_SRC_ALPHA
        };

        var ptSrc = new Point { X = 0, Y = 0 };
        var ptDest = new Point { X = x, Y = y };
        var size = new SIZE { cx = width, cy = height };

        PInvoke.UpdateLayeredWindow(
            _hWndSplash,
            screenDc,
            &ptDest,
            &size,
            memDc,
            &ptSrc,
            new COLORREF(0),
            &blend,
            UPDATE_LAYERED_WINDOW_FLAGS.ULW_ALPHA);

        PInvoke.SelectObject(memDc, oldBitmap);
        PInvoke.DeleteObject((HGDIOBJ)hBitmap);
        PInvoke.DeleteDC(memDc);
        _ = PInvoke.ReleaseDC(HWND.Null, screenDc);
    }

    protected void ReleaseResources()
    {
        _image.Dispose();
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

    /// <summary>
    /// 只在主屏幕上获取 DPI 缩放比例，避免多屏幕环境下的复杂性
    /// </summary>
    private static double GetDpiScale()
    {
        var monitor = PInvoke.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);

        if (monitor != nint.Zero && OperatingSystem.IsWindowsVersionAtLeast(8, 1))
        {
            PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpi, out _);

            return dpi == 0 ? 1 : dpi / 96d;
        }

        return 1.0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static LRESULT WndProc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        => PInvoke.DefWindowProc(hwnd, uMsg, wParam, lParam);
}
