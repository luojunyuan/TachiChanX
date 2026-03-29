using TouchChanX.Win32;
using TouchChanX.Win32.Interop;
using R3;

namespace TouchChanX;

// NOTE: App.xaml 仅是为了过编译所必需的硬编码文件名
// 有移除所有 xaml 的方法，但是如果窗口或控件需要使用 xaml 那么似乎 App.xaml 还是必须的

public partial class WinUIApp(nint gameWindowHandle)
{
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
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
        OsPlatformApi.SetParentWindow(hwnd, gameWindowHandle);

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
    extension(Windows.Foundation.Rect rect)
    {
        public Windows.Foundation.Rect Scale(double f) =>
            new(rect.X * f, rect.Y * f, rect.Width * f, rect.Height * f);

        public System.Drawing.Rectangle ToGdiRect() =>
            new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    extension(Microsoft.UI.Xaml.Window window)
    {
        public double Dpi => window.Content.XamlRoot.RasterizationScale;
    }
}