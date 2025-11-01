using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace TouchChanX.Ava;

public class App : Application
{
    public static readonly AvaloniaSynchronizationContext UISyncContext = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 只有 TouchChanX.Rider 作为启动项目时会进入这里
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.RendererDiagnostics.DebugOverlays = 
                RendererDebugOverlays.Fps | 
                RendererDebugOverlays.DirtyRects | 
                RendererDebugOverlays.LayoutTimeGraph |
                RendererDebugOverlays.RenderTimeGraph;
        }

        base.OnFrameworkInitializationCompleted();
    }
}