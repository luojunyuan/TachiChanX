using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using TouchChanX.Win32.Interop;
using TouchChanX.WPF;

namespace TouchChanX;

internal static class WpfStartup
{
    public static int Run(nint ownerHwnd)
    {
        var app = new App();
        app.InitializeComponent();

        var mainWindow =
            new MainWindow()
                .UseTransparentChromeWindow();

        var grid = mainWindow.Content as Grid;
        grid!.Children.Add(new Border()
        {
            BorderThickness = new(1),
            BorderBrush = Brushes.Red,
        });

        mainWindow.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(mainWindow).Handle;
            OsPlatformApi.SetOwnerWindow(hwnd, ownerHwnd);
            OsPlatformApi.ToggleWindowExStyle(hwnd, ExtendedWindowStyles.AppWindow, false);
        };

        return app.Run(mainWindow);
    }
}

public static class WindowExtension
{
    extension<T>(T window) where T : Window
    {
        public T UseTransparentChromeWindow()
        {
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.Background = Brushes.Transparent;

            WindowChrome.SetWindowChrome(window, new WindowChrome
            {
                GlassFrameThickness = WindowChrome.GlassFrameCompleteThickness,
                CaptionHeight = 0,
            });

            return window;
        }
    }
}