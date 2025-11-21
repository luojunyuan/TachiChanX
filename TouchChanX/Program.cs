using R3;
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

await using var fileStream = TouchChanX.EmbeddedResource.KleeGreen;

var processResult = await GameStartup.GetOrLaunchGameWithSplashAsync(gamePath, fileStream);
if (processResult.IsFailure(out var processError, out var process))
{
    OsPlatformApi.MessageBox.Show(processError.Message);
    return;
}

// 用于挂载程序意外退出的情景
process.EnableRaisingEvents = true;
process.Exited += (_, _) => Environment.Exit(0);

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

// TODO: 测试真实环境下是否需要强制将游戏窗口提前
