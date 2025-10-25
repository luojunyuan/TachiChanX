using Avalonia;
using Avalonia.Controls;
using R3;
using TouchChanX.Ava.Menu.Pages;
using TouchChanX.Ava.Menu.Pages.Components;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl : UserControl
{
    private const int TouchSpacing = Shared.Constants.TouchSpacing;
    
    private const double TouchSize = 80;

    public event EventHandler? Closed;
    
    public Shared.TouchDockAnchor FakeTouchDockAnchor { get; set; } = Shared.TouchDockAnchor.Default;
    
    public MenuControl()
    {
        InitializeComponent();

        MainPage.MenuButtons
            .Select(item => item.Clicked.Select(_ => item))
            .Merge()
            .SubscribeAwait(async (item, _) => await GoToInnerPageAsync(item));
        
        this.PointerPressed += async (_, _) => await CloseMenuAsync();
        
        // 订阅执行任何动画期间都禁止整个页面再次交互
        _animationRunningSubject.Subscribe(running => this.IsHitTestVisible = !running);
    }
    
    private Point AnchorAtTopLeftPoint(Shared.TouchDockAnchor anchor, Rect? bounds = null)
    {
        var width = bounds?.Width ?? this.Bounds.Width;
        var height = bounds?.Height ?? this.Bounds.Height;
        var alignRight = width - TouchSize - TouchSpacing;
        var alignBottom = height - TouchSize - TouchSpacing;

        return anchor switch
        {
            { IsTopLeft: true } => ConvertToCenterCoord(new Point(TouchSpacing, TouchSpacing)),
            { IsTopRight: true } => ConvertToCenterCoord(new Point(alignRight, TouchSpacing)),
            { IsBottomLeft: true } => ConvertToCenterCoord(new Point(TouchSpacing, alignBottom)),
            { IsBottomRight: true } => ConvertToCenterCoord(new Point(alignRight, alignBottom)),
            Shared.TouchDockAnchor.Left x => 
                ConvertToCenterCoord(new Point(TouchSpacing, x.Scale * height - TouchSize / 2)),
            Shared.TouchDockAnchor.Top x => 
                ConvertToCenterCoord(new Point(x.Scale * width - TouchSize / 2, TouchSpacing)),
            Shared.TouchDockAnchor.Right x => 
                ConvertToCenterCoord(new Point(alignRight, x.Scale * height - TouchSize / 2)),
            Shared.TouchDockAnchor.Bottom x => 
                ConvertToCenterCoord(new Point(x.Scale * width - TouchSize / 2, alignBottom)),
            _ => default,
        };
        
        Point ConvertToCenterCoord(Point point) => 
            new(point.X - width / 2 + TouchSize / 2, point.Y- height / 2 + TouchSize / 2);
    }
    
    public async Task ShowMenuAsync(Rect bounds)
    {
        var pos = AnchorAtTopLeftPoint(FakeTouchDockAnchor, bounds);
        await PlayShowMenuStoryboardAsync(pos);
    }

    private async Task CloseMenuAsync()
    {
        var pos = AnchorAtTopLeftPoint(FakeTouchDockAnchor);
        await PlayCloseMenuStoryboardAsync(pos);
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private async Task GoToInnerPageAsync(object? sender)
    {
        PageBase innerPage = sender switch
        {
            MenuButton { Tag: "Device" } entryItem => new DevicePage { EntryCell = entryItem.Cell },
            MenuButton { Tag: "Game" } entryItem => new GamePage { EntryCell = entryItem.Cell },
            _ => throw new NotSupportedException(),
        };

        innerPage.BackRequested.SubscribeAwait(async (_, _) => await ReturnToMainPageAsync(innerPage));

        InnerPageHost.Content = innerPage;
        
        await PlayTransitionInnerPageStoryboardAsync(innerPage);
    }

    private async Task ReturnToMainPageAsync(object? sender)
    {
        if (sender is not PageBase innerPage) 
            return;

        await PlayTransitionMainPageStoryboardAsync(innerPage);

        InnerPageHost.Content = null;
    }
}