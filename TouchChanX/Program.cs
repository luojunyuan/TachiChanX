using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using R3;
using TouchChanX.Ava;
using TouchChanX.Win32;
using TouchChanX.Win32.Interop;

if (args.Length == 0)
{
    OsPlatformApi.MessageBox.Show("Please provide the game path as the first argument.");
    return;
}

var gamePathResult = GameStartup.PrepareValidGamePath(args[0]);
if (gamePathResult.IsFailure(out var pathError, out var gamePath))
{
    OsPlatformApi.MessageBox.Show(pathError.Message);
    return;
}

var processResult = await GameStartup.GetOrLaunchGameWithSplashAsync(gamePath, new());
if (processResult.IsFailure(out var processError, out var process))
{
    OsPlatformApi.MessageBox.Show(processError.Message);
    return;
}

var handleResult = await GameStartup.FindGoodWindowHandleAsync(process);
if (handleResult.IsFailure(out var error, out var gameWindowHandle))
{
    switch (error)
    {
        case WindowHandleNotFoundError:
            OsPlatformApi.MessageBox.Show("Timeout! Failed to find a valid window of game");
            return;
        case ProcessExitedError:
        case ProcessPendingExitedError:
            return;
    }
}

// 用于挂载程序意外退出的情景
process.EnableRaisingEvents = true;
process.Exited += (_, _) => Environment.Exit(0);

var uiThread = new Thread(() =>
{
    Thread.CurrentThread.Name = "UI Thread";

    var app = AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace();

    app.Start(AppMain, args);
});
uiThread.SetApartmentState(ApartmentState.STA);
uiThread.Start();

return;

void AppMain(Application app, string[] args)
{
    var window = new MainWindow()
    {
        Background = Brushes.Transparent,
        SystemDecorations = SystemDecorations.None,
        Position = default,
        ShowActivated = false,
    };

    var handle = TopLevel.GetTopLevel(window)?.TryGetPlatformHandle()?.Handle ?? throw new InvalidOperationException();

    window.Opened += async (_, _) =>
    {
        // NOTE: 仅在 Opened 事件后能够有效调整 win32 样式，并且子窗口才可与游戏窗口共享焦点
        OsPlatformApi.ToggleWindowStyle(handle, true, WindowStyle.Child);
        OsPlatformApi.ToggleWindowExStyle(handle, true, ExtendedWindowStyle.Layered);
        // 可选移除的其他样式
        OsPlatformApi.ToggleWindowExStyle(handle, false, ExtendedWindowStyle.AppWindow);
        OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.ClipChildren);
        OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.MinimizeBox);
        OsPlatformApi.ToggleWindowStyle(handle, false, WindowStyle.MaxmizeBox);
        // LJY: Avalonia#19923 三天前的 PR 针对 SystemDecorations.None 窗口自动移除了 WS_SYSMENU

        await OsPlatformApi.SetParentWindowAsync(handle, gameWindowHandle);
    };

    GameWindowService.ClientSizeChanged(gameWindowHandle)
        .Subscribe(size =>
        {
            window.Width = size.Width / window.DesktopScaling;
            window.Height = size.Height / window.DesktopScaling;
        });

    window.Touch.ResetWindowObservableRegion = avaSize =>
    {
        OsPlatformApi.ResetWindowOriginalObservableRegion(handle, new((int)(window.Width * window.DesktopScaling), (int)(window.Height * window.DesktopScaling)));
    };
    window.Touch.SetWindowObservableRegion = avaRect =>
    {
        OsPlatformApi.SetWindowObservableRegion(handle, new(
            (int)(avaRect.X * window.DesktopScaling),
            (int)(avaRect.Y * window.DesktopScaling),
            (int)(avaRect.Width * window.DesktopScaling),
            (int)(avaRect.Height * window.DesktopScaling)));
    };

    app.Run(window);
}