using Avalonia.Controls;
using Avalonia.Media;

namespace TouchChanX.Ava;

public partial class MainWindow
{
    public void LaunchCodeBackup()
    {
        var window = new MainWindow()
        {
            Background = Brushes.Transparent,
            SystemDecorations = SystemDecorations.None,
            Position = default,
            ShowActivated = false,
        };

        var handle = TopLevel.GetTopLevel(window)?.TryGetPlatformHandle()?.Handle ?? throw new InvalidOperationException();

        //window.Opened += async (_, _) =>
        //{
        //    // 只针对 Win32RenderingMode.AngleEgl 渲染后端有效
        //    // NOTE: 仅在 Opened 事件后能够有效调整 Win32 样式，并且此时子窗口方可与游戏窗口正常实现焦点共享
        //    OsPlatformApi.ToggleWindowStyle(handle, true, WindowStyle.Child);
        //    OsPlatformApi.ToggleWindowExStyle(handle, true, ExtendedWindowStyle.Layered);
        //    // 可选移除的其他样式
        //    OsPlatformApi.ToggleWindowExStyle(handle, false, ExtendedWindowStyle.AppWindow);
        //    OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.ClipChildren);
        //    OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.MinimizeBox);
        //    OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.MaximizeBox);
        //    // LJY: Avalonia#19923 三天前的 PR 针对 SystemDecorations.None 窗口自动移除了 WS_SYSMENU

        //    await OsPlatformApi.SetParentWindowAsync(handle, gameWindowHandle);
        //};

        //GameWindowService.ClientSizeChanged(gameWindowHandle)
        //    .Where(size => size is { Width: > 320, Height: > 240 })
        //    .Subscribe(size =>
        //    {
        //        window.Width = size.Width / window.DesktopScaling;
        //        window.Height = size.Height / window.DesktopScaling;
        //    });

        //window.Touch.ResetWindowObservableRegion = _ =>
        //{
        //    OsPlatformApi.ResetWindowOriginalObservableRegion(handle, new(
        //        (int)(window.Width * window.DesktopScaling),
        //        (int)(window.Height * window.DesktopScaling)));
        //};
        //window.Touch.SetWindowObservableRegion = avaRect =>
        //{
        //    OsPlatformApi.SetWindowObservableRegion(handle, new(
        //        (int)(avaRect.X * window.DesktopScaling),
        //        (int)(avaRect.Y * window.DesktopScaling),
        //        (int)(avaRect.Width * window.DesktopScaling),
        //        (int)(avaRect.Height * window.DesktopScaling)));
        //};
    }
}
