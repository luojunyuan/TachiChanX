using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using TouchChanX.Ava;
using TouchChanX.Win32;
using TouchChanX.Win32.Interop;
using R3;

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

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

var app = AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .LogToTrace();

app.Start(AppMain, args);

void AppMain(Application app, string[] args)
{
    var window = new MainWindow()
    {
        Background = Brushes.Transparent,
        SystemDecorations = SystemDecorations.None,
        Position = default,
    };

    var handle = TopLevel.GetTopLevel(window)?.TryGetPlatformHandle()?.Handle ?? throw new InvalidOperationException();

    window.Loaded += async (_, _) =>
    {
        // Note: Ava 必须在 Loaded 事件后，Child Style 子窗口才可以和游戏窗口共享焦点，并且样式才能被正常应用或者移除
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

    app.Run(window);
}