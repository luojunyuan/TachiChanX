using R3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using R3.ObservableEvents;

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
    public Shared.TouchDockAnchor FakeTouchDockAnchor { get; set; } = Shared.TouchDockAnchor.Default;

    public MenuControl()
    {
        InitializeComponent();

        this.Events().IsVisibleChanged
            .Skip(1)
            .Where(evt => (bool)evt.NewValue)
            .SubscribeAwait(async (_, _) =>
            {
                var endOffset = new Point((ActualWidth - 300) / 2, (ActualHeight - 300) / 2);
                await StartAnimationAsync(Menu, endOffset);
                IsExpanded = true;
            });
    }

    private const int TouchSpacing = Shared.TouchSpacing;

    private const int TouchSize = 80;

    private Size ActualSize => new(ActualWidth, ActualHeight);

    private static Point AnchorPoint(Shared.TouchDockAnchor anchor, Size window)
    {
        var width = window.Width;
        var height = window.Height;
        var alignRight = width - TouchSize - TouchSpacing;
        var alignBottom = height - TouchSize - TouchSpacing;

        return anchor switch
        {
            Shared.TouchDockAnchor.TopLeft => new Point(TouchSpacing, TouchSpacing),
            Shared.TouchDockAnchor.TopRight => new Point(alignRight, TouchSpacing),
            Shared.TouchDockAnchor.BottomLeft => new Point(TouchSpacing, alignBottom),
            Shared.TouchDockAnchor.BottomRight => new Point(alignRight, alignBottom),
            Shared.TouchDockAnchor.Left x => new Point(TouchSpacing, x.Scale * height - TouchSize / 2 - TouchSpacing),
            Shared.TouchDockAnchor.Top x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, TouchSpacing),
            Shared.TouchDockAnchor.Right x => new Point(alignRight, x.Scale * height - TouchSize / 2 - TouchSpacing),
            Shared.TouchDockAnchor.Bottom x => new Point(x.Scale * width - TouchSize / 2 - TouchSpacing, alignBottom),
            _ => default,
        };
    }
}
