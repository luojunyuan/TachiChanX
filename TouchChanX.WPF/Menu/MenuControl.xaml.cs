using R3;
using R3.ObservableEvents;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;

namespace TouchChanX.WPF.Menu;

public partial class MenuControl
{
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(MenuControl), new PropertyMetadata(false));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}

/// <summary>
/// MenuControl.xaml 的交互逻辑
/// </summary>
public partial class MenuControl : UserControl
{
    private TouchDockAnchor _fakeTouchDockAnchor = TouchDockAnchor.Default;

    public void ShowAt(Rect touchRect)
    {
        this.Visibility = Visibility.Visible;
        this.UpdateLayout();

        this._fakeTouchDockAnchor = TouchDockAnchor.SnapFromRect(ContainerSize, touchRect);
    }

    public MenuControl()
    {
        InitializeComponent();

        // Open
        this.Events().IsVisibleChanged
            .Where(_ => IsVisible)
            .SubscribeAwait(async (_, _) =>
            {
                var point = AnchorPoint(_fakeTouchDockAnchor, ContainerSize);
                
                await MenuOpenAnimationAsync(MenuBorder, CenterPosition, point);

                IsExpanded = true;
            });

        // Close
        this.Events().PreviewMouseLeftButtonUp
            .Where(e => e.OriginalSource == MenuBackground)
            .SelectAwait(async (_, _) =>
            {
                IsExpanded = false;

                var touchAnchor = AnchorPoint(_fakeTouchDockAnchor, ContainerSize);

                await MenuCloseAnimationAsync(MenuBorder, CenterPosition, touchAnchor);

                return Unit.Default;
            })
            .Prepend(Unit.Default)
            .Subscribe(_ => this.Visibility = Visibility.Collapsed);
    }

    private const int TouchSpacing = Shared.TouchSpacing;

    private const double TouchSize = Shared.TouchSize;

    private const double MenuSize = Shared.MenuSize;

    private Size ContainerSize => new(ActualWidth, ActualHeight);

    private Point CenterPosition => new(
        (ContainerSize.Width - MenuSize) / 2,
        (ContainerSize.Height - MenuSize) / 2);

    private static Point AnchorPoint(TouchDockAnchor anchor, Size window)
    {
        var width = window.Width;
        var height = window.Height;
        var alignRight = width - TouchSize - TouchSpacing;
        var alignBottom = height - TouchSize - TouchSpacing;

        return anchor switch
        {
            TouchDockAnchor.TopLeft => new Point(TouchSpacing, TouchSpacing),
            TouchDockAnchor.TopRight => new Point(alignRight, TouchSpacing),
            TouchDockAnchor.BottomLeft => new Point(TouchSpacing, alignBottom),
            TouchDockAnchor.BottomRight => new Point(alignRight, alignBottom),
            TouchDockAnchor.Left x => new Point(TouchSpacing, x.Scale * height - TouchSize / 2 - TouchSpacing),
            TouchDockAnchor.Top x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, TouchSpacing),
            TouchDockAnchor.Right x => new Point(alignRight, x.Scale * height - TouchSize / 2 - TouchSpacing),
            TouchDockAnchor.Bottom x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, alignBottom),
            _ => default,
        };
    }
}
