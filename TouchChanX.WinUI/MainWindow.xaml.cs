using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using R3;
using R3.ObservableEvents;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TouchChanX.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Touch.Clicked
            .Select(rect => Menu.TouchDockAnchor.SnapFromRect(this.Content.ActualSize.ToSize(), rect))
            .Subscribe(MenuTouch.ShowAt);
    }

    private Visibility InvertVisible(Visibility visible) => visible == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
}

public static class Shared
{
    public const int TouchSpacing = 2;

    public const double TouchSize = 80.0;
    public const double MenuSize = TouchSize * 4;

    extension(FrameworkElement element)
    {
        public Observable<bool> IsVisibleChanged => Observable.Create<bool>(observer =>
        {
            var token = element.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (_, _) =>
                observer.OnNext(element.Visibility == Visibility.Visible));

            return Disposable.Create(() => element.UnregisterPropertyChangedCallback(UIElement.VisibilityProperty, token));
        });
    }
}
