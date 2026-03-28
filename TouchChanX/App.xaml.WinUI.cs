using Microsoft.UI.Xaml;
using TouchChanX.Win32;
using TouchChanX.Win32.Interop;

namespace TouchChanX;

// NOTE: App.xaml 仅是为了过编译所必需的硬编码文件名
// 有移除所有 xaml 的方法，但是如果窗口或控件需要使用 xaml 那么似乎 App.xaml 还是必须的

public partial class WinUIApp(nint gameWindowHandle)
{
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new WinUI.MainWindow()
        {
            SystemBackdrop = new WinUIEx.TransparentTintBackdrop()
        };
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        OsPlatformApi.ToggleWindowStyle(hwnd, WindowStyles.TiledWindow, false);
        OsPlatformApi.SetOwnerWindow(hwnd, gameWindowHandle);
        GameWindowService.SyncWindowTransform(hwnd, gameWindowHandle);

        window.Activate();
    }
}
