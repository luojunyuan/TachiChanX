using TouchChanX.Win32;
using TouchChanX.Win32.Interop;
using R3;
using Microsoft.UI.Xaml;

namespace TouchChanX;

public partial class App
{
    public App() : this(nint.Zero)
    {
        this.InitializeComponent();
    }
}

public partial class App(nint gameWindowHandle)
{
    /// <summary>
    /// WinUI 程序窗口入口点事件函数
    /// </summary>
    /// <remarks>QwQ: 耗时方法</remarks>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (gameWindowHandle == nint.Zero)
        {
            var preference = new Window();
            preference.Activate();
            return;
        }

        var window = new WinUI.MainWindow()
        {
            SystemBackdrop = new WinUIEx.TransparentTintBackdrop()
        };
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        OsPlatformApi.ToggleWindowStyle(hwnd, WindowStyles.TiledWindow, false);
        // SetParent 的基础条件之一，可以让子窗口与父窗口统一焦点
        OsPlatformApi.ToggleWindowStyle(hwnd, WindowStyles.Child, true);
        // 必须给 WinUI 外层 Win32 窗口添加 Layered 样式，以使窗口分层背景正常可见
        OsPlatformApi.ToggleWindowExStyle(hwnd, ExtendedWindowStyles.Layered, true);
        // NOTE: 设置为子窗口后，window.AppWindow 会返回 null、Activated 事件一次也不激活了
        OsPlatformApi.SetParentWindowQwQ(hwnd, gameWindowHandle);

        // 确保在设置为子窗口后，重定位对齐父窗口左上角
        GameWindowService.ClientSizeChanged(gameWindowHandle)
            .Subscribe(size => OsPlatformApi.ResizeWindow(hwnd, size));

        WinUI.Touch.TouchControl.ObservableRegionResetRequested?
            .Subscribe(_ => OsPlatformApi.ResetWindowOriginalObservableRegion(hwnd));
        WinUI.Touch.TouchControl.ObservableTouchRegionChanged?
            .Select(touchRect => touchRect.Scale(window.Dpi).ToGdiRect())
            .Subscribe(rect => OsPlatformApi.SetWindowObservableRegion(hwnd, rect));

        window.InitializeBindings();
        window.Activate();
    }
}

public static class WinUIExtension
{
    private const int AntiClippingOffset = 1;

    extension(Windows.Foundation.Rect rect)
    {
        public Windows.Foundation.Rect Scale(double f) =>
            new(rect.X * f, rect.Y * f, rect.Width * f, rect.Height * f);

        public System.Drawing.Rectangle ToGdiRect() =>
            new((int)rect.X, (int)rect.Y, (int)rect.Width + AntiClippingOffset, (int)rect.Height + AntiClippingOffset);
    }

    extension(Microsoft.UI.Xaml.Window window)
    {
        public double Dpi => window.Content.XamlRoot.RasterizationScale;
    }
}