using R3;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using TouchChanX.Win32;
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
            GameWindowService.SyncWindowTransform(hwnd, ownerHwnd);
            mainWindow.Touch.RegionResetRequested.Subscribe(_ => OsPlatformApi.ResetWindowOriginalObservableRegion(hwnd));
            mainWindow.Touch.RegionChangeRequested
                .Select(touchRect => touchRect.ScaleByDpi(mainWindow.GetDpi()).ToGdiRect())
                .Subscribe(touchRect => OsPlatformApi.SetWindowObservableRegion(hwnd, touchRect));
        };

        return app.Run(mainWindow);
    }
}

public static class WpfExtension
{
    extension(Rect rect)
    {
        public Rect ScaleByDpi(double dpi) =>
            new(rect.X * dpi, rect.Y * dpi, rect.Width * dpi, rect.Height * dpi);

        public System.Drawing.Rectangle ToGdiRect() =>
            new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
    }

    extension(Visual visual)
    {
        public double GetDpi() => VisualTreeHelper.GetDpi(visual).DpiScaleX;
    }

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

        public T DisableWPFTabletSupport()
        {
            return window;
        }
    }
}