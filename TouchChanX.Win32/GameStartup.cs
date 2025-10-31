using LightResults;
using TouchChanX.SplashScreenGdiPlus;
using WindowsShortcutFactory;

namespace TouchChanX.Win32;

public static partial class GameStartup
{
    /// <summary>
    /// 准备有效的游戏路径
    /// </summary>
    public static Result<string> PrepareValidGamePath(string path)
    {
        if (!File.Exists(path))
            return Result.Failure<string>($"Game path \"{path}\" not found, please check if it exist.");

        var isNotLnkFile = !Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase);

        if (isNotLnkFile)
            return path;

        string? resolvedPath;
        try
        {
            resolvedPath = WindowsShortcut.Load(path).Path;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return Result.Failure<string>($"Failed when resolve \"{path}\", please try start from game folder.");
        }

        if (!File.Exists(resolvedPath))
            return Result.Failure<string>($"Resolved link path \"{resolvedPath}\" not found, please try start from game folder.");

        return resolvedPath;
    }

    public static async Task<Result<Process>> GetOrLaunchGameWithSplashAsync(string path, SynchronizationContext syncContext)
    {
        var process = await GetWindowProcessByPathAsync(path);
        if (process is not null)
        {
            await Interop.OsPlatformApi.TryRestoreWindowAsync(process.MainWindowHandle);
            return process;
        }

        await using var fileStream = EmbeddedResource.KleeGreen;

        using var splash = SplashScreen.Create(fileStream);
        splash.Show();
        return await LaunchGameAsync(path);
    }

    /// <summary>
    /// 启动游戏进程
    /// </summary>
    private static async Task<Result<Process>> LaunchGameAsync(string path)
    {
        // NOTE: NUKITASHI2(steam) 会先启动一个进程闪现黑屏窗口，然后再重新启动游戏进程

        // TODO: 通过 LE 启动，思考检查游戏id好的方法，处理超时和错误情况
        // 考虑 LE 通过注册表查找还是通过配置文件，还是通过指定路径来启动
        // 考虑侵入式的设计对 Locale Emulator 的支持
        // Environment.GetCommandLineArgs().Contains("-le")

        // NOTE: 设置 WorkingDirectory 在游戏路径，避免部分游戏无法索引自身资源导致异常
        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = Path.GetDirectoryName(path),
            EnvironmentVariables = { ["__COMPAT_LAYER"] = "HighDpiAware" }
        };
        _ = await StartProcessAsync(startInfo);

        const int WaitMainWindowTimeout = 20000;
        const int UIMinimumResponseTime = 50;

        // NOTE: 这是反复-超时任务的最佳实践，基于任务驱动
        using var cts = new CancellationTokenSource(WaitMainWindowTimeout);
        var timeoutToken = cts.Token;

        while (!timeoutToken.IsCancellationRequested)
        {
            var gameProcess = await GetWindowProcessByPathAsync(path);

            if (gameProcess != null)
            {
                // leProc?.kill()
                return gameProcess;
            }

            await Task.Delay(UIMinimumResponseTime, CancellationToken.None);
        }

        return Result.Failure<Process>("Failed to start game within the timeout period.");
    }

    private static Task<Process?> StartProcessAsync(ProcessStartInfo startInfo) => Task.Run(() => Process.Start(startInfo));

    /// <summary>
    /// 尝试通过限定的程序路径获取对应正在运行的，存在 MainWindowHandle 的进程
    /// </summary>
    private static Task<Process?> GetWindowProcessByPathAsync(string gamePath)
    {
        var friendlyName = Path.GetFileNameWithoutExtension(gamePath);
        // FUTURE: .log main.bin situation
        return Task.Run(() =>
            Process.GetProcessesByName(friendlyName)
                .FirstOrDefault(p =>
                {
                    if (p.MainWindowHandle == nint.Zero)
                        return false;

                    var mainModule = p.HasExited ? null : p.MainModule;
                    return mainModule?.FileName.Equals(gamePath, StringComparison.OrdinalIgnoreCase) ?? false;
                }));
    }
}
