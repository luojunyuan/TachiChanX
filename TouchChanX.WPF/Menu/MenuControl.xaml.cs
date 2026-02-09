using R3;
using R3.ObservableEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        this._fakeTouchDockAnchor = TouchDockAnchor.SnapFromRect(ContainerSize, touchRect);
    }

    private readonly ReadOnlyReactiveProperty<Size> _containerSizeState;
    private Size ContainerSize => _containerSizeState.CurrentValue;

    private Point CenterPosition => new(
        (ContainerSize.Width - MenuSize) / 2,
        (ContainerSize.Height - MenuSize) / 2);

    private Point TouchAnchor => AnchorPoint(_fakeTouchDockAnchor, ContainerSize);

    public MenuControl()
    {
        InitializeComponent();
        this.Visibility = Visibility.Collapsed;

        _containerSizeState = this.Events().Loaded
            .Select(_ => VisualTreeHelper.GetParent(this).Required<FrameworkElement>())
            .SelectMany(p =>
                p.Events().SizeChanged
                .Select(e => e.NewSize)
                .Prepend(new Size(p.ActualWidth, p.ActualHeight)))
            .ToReadOnlyReactiveProperty();

        // Open
        this.Events().IsVisibleChanged
            .Where(_ => IsVisible)
            .SubscribeAwait(async (_, _) =>
            {
                await MenuOpenAnimationAsync();

                IsExpanded = true;
            });

        // Close
        this.Events().PreviewMouseLeftButtonUp
            .Where(e => e.OriginalSource == MenuBackground)
            .SubscribeAwait(async (_, _) =>
            {
                IsExpanded = false;

                await MenuCloseAnimationAsync();

                this.Visibility = Visibility.Collapsed;
            });
    }

    private const int TouchSpacing = Shared.TouchSpacing;

    private const double TouchSize = Shared.TouchSize;

    private const double MenuSize = Shared.MenuSize;

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
