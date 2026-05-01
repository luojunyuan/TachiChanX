using R3;
using System;
using System.Reflection;
using TouchChanX.Win32;
using TouchChanX.Win32.Interop;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TouchChanX2;

/// <summary>
/// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
/// </summary>
public sealed partial class App : CoreIsland.Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    public App(nint gameWindowHandle)
    {
        _gameWindowHandle = gameWindowHandle;
    }

    private nint _gameWindowHandle = nint.Zero;

    /// <inheritdoc/>
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        var window = new TouchChanX.WinUI.MainWindow()
        {
        };

        var field = typeof(CoreIsland.Window).GetField("_hwnd", BindingFlags.NonPublic | BindingFlags.Instance);
        var field2 = typeof(CoreIsland.Window).GetField("_coreHwnd", BindingFlags.NonPublic | BindingFlags.Instance);
        var field3 = typeof(CoreIsland.Window).GetField("_xamlHwnd", BindingFlags.NonPublic | BindingFlags.Instance);
        dynamic hwndExpando = field.GetValue(window);
            string hex = hwndExpando.ToString(); // 得到 "0x123abc..."
        nint hwnd = nint.Parse(hex.AsSpan(2), System.Globalization.NumberStyles.HexNumber);
        //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        dynamic coreExpando = field2.GetValue(window);
        dynamic xamlExpando = field3.GetValue(window);
        string hex2 = coreExpando.ToString();
        string hex3 = xamlExpando.ToString();

        nint _coreHwnd = nint.Parse(hex2.AsSpan(2), System.Globalization.NumberStyles.HexNumber);
        nint _xamlHwnd = nint.Parse(hex3.AsSpan(2), System.Globalization.NumberStyles.HexNumber);


        OsPlatformApi.ToggleWindowStyle(hwnd, WindowStyles.TiledWindow, false);
        // SetParent 的基础条件之一，可以让子窗口与父窗口统一焦点
        OsPlatformApi.ToggleWindowStyle(hwnd, WindowStyles.Child, true);
        // 必须给 WinUI 外层 Win32 窗口添加 Layered 样式，以使窗口作为子窗口时分层背景正常可见
        OsPlatformApi.ToggleWindowExStyle(hwnd, ExtendedWindowStyles.Layered, true);
        // NOTE: 设置为子窗口后，window.AppWindow 会返回 null、Activated 事件一次也不激活了
        //OsPlatformApi.SetParentWindowQwQ(hwnd, _gameWindowHandle);

        OsPlatformApi.ToggleWindowStyle(_coreHwnd, WindowStyles.Child, true);
        OsPlatformApi.ToggleWindowStyle(_xamlHwnd, WindowStyles.Child, true);
        OsPlatformApi.SetParentWindowQwQ(_coreHwnd, _gameWindowHandle);
        OsPlatformApi.SetParentWindowQwQ(_xamlHwnd, _gameWindowHandle);

        // 确保在设置为子窗口后，重定位对齐父窗口左上角
        GameWindowService.ClientSizeChanged(_gameWindowHandle)
            .Subscribe(size => OsPlatformApi.ResizeWindow(hwnd, size));

        TouchChanX.WinUI.Touch.TouchControl.ObservableRegionResetRequested
            .Merge(TouchChanX.WinUI.Menu.MenuControl.ObservableRegionResetRequested)
            .Subscribe(_ => OsPlatformApi.ResetWindowOriginalObservableRegion(hwnd));
        TouchChanX.WinUI.Touch.TouchControl.ObservableTouchRegionChanged
            .Select(touchRect => touchRect.Scale(window.Dpi).ToGdiRect())
            .Subscribe(rect => OsPlatformApi.SetWindowObservableRegion(hwnd, rect));

        // TODO: 监视父窗口销毁事件，把窗口设置到新的 gameWindowHandle 上
        GameWindowService.WindowDestroyed(_gameWindowHandle).Subscribe(_ =>
        {
            // 会循环询找新的窗口，直到找到一个有效的窗口或者超时就直接退出程序
            _gameWindowHandle = nint.Zero;
            System.Diagnostics.Debug.WriteLine("Game window destroyed");

            OsPlatformApi.SetParentWindowQwQ(hwnd, _gameWindowHandle);
            GameWindowService.ClientSizeChanged(_gameWindowHandle)
                .Subscribe(size => OsPlatformApi.ResizeWindow(hwnd, size));
        });

        //window.InitializeBindings();
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

    extension(CoreIsland.Window window)
    {
        public double Dpi => window.Content.XamlRoot.RasterizationScale;
    }
}