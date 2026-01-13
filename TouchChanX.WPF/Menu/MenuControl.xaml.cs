using R3;
using R3.ObservableEvents;
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
    public TouchDockAnchor FakeTouchDockAnchor { get; set; } = TouchDockAnchor.Default;

    public MenuControl()
    {
        InitializeComponent();

        this.Events().IsVisibleChanged
            .Where(_ => IsVisible)
            .SubscribeAwait(async (_, _) =>
            {
                this.UpdateLayout();
                var endOffset = new Point((ActualWidth - MenuSize) / 2, (ActualHeight - MenuSize) / 2);
                await StartAnimationAsync(Menu, endOffset);
                IsExpanded = true;
            });
    }

    private const int TouchSpacing = Shared.TouchSpacing;

    private const double TouchSize = Shared.TouchSize;

    private const double MenuSize = Shared.MenuSize;

    private Size ActualSize => new(ActualWidth, ActualHeight);

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
