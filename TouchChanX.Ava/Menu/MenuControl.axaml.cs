using Avalonia;
using Avalonia.Controls;
using R3;
using TouchChanX.Ava.Menu.Pages;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl : UserControl
{
    private const int TouchSpacing = 2;

    public event EventHandler? Closed;
    
    public Shared.TouchDockAnchor FakeTouchDockAnchor { get; set; } = Shared.TouchDockAnchor.Default;
    
    public MenuControl()
    {
        InitializeComponent();

        // Code smell
        PageBase.BackRequested += async (s, _) => await ReturnToMainPageAsync(s);

        MainPage.Children.Cast<MenuItem>()
            .Select(item => item.Clicked.Select(_ => item))
            .Merge()
            .SubscribeAwait(async (item, _) => await GoToInnerPageAsync(item));
        
        this.PointerPressed += async (_, _) => await CloseMenuAsync();
        
        // 订阅执行任何动画期间都禁止整个页面再次交互
        _animationRunningSubject.Subscribe(running => this.IsHitTestVisible = !running);
    }
    
    private readonly Subject<bool> _animationRunningSubject = new();

    private const double TouchSize = 80;
    
    private Point AnchorAtTopLeftPoint(Shared.TouchDockAnchor anchor)
    {
        var alignRight = this.Bounds.Width - TouchSize - TouchSpacing;
        var alignBottom = this.Bounds.Height - TouchSize - TouchSpacing;

        return anchor switch
        {
            { IsTopLeft: true } => ConvertToCenterCoord(new Point(TouchSpacing, TouchSpacing)),
            { IsTopRight: true } => ConvertToCenterCoord(new Point(alignRight, TouchSpacing)),
            { IsBottomLeft: true } => ConvertToCenterCoord(new Point(TouchSpacing, alignBottom)),
            { IsBottomRight: true } => ConvertToCenterCoord(new Point(alignRight, alignBottom)),
            Shared.TouchDockAnchor.Left x => 
                ConvertToCenterCoord(new Point(TouchSpacing, x.Scale * this.Bounds.Height - TouchSize / 2)),
            Shared.TouchDockAnchor.Top x => 
                ConvertToCenterCoord(new Point(x.Scale * this.Bounds.Width - TouchSize / 2, TouchSpacing)),
            Shared.TouchDockAnchor.Right x => 
                ConvertToCenterCoord(new Point(alignRight, x.Scale * this.Bounds.Height - TouchSize / 2)),
            Shared.TouchDockAnchor.Bottom x => 
                ConvertToCenterCoord(new Point(x.Scale * this.Bounds.Width - TouchSize / 2, alignBottom)),
            _ => default,
        };
        
        Point ConvertToCenterCoord(Point point) => 
            new(point.X - this.Bounds.Width / 2 + TouchSize / 2, point.Y- this.Bounds.Height / 2 + TouchSize / 2);
    }
    
    public async Task ShowMenuAsync()
    {
        var pos = AnchorAtTopLeftPoint(FakeTouchDockAnchor);
        var menuTransitionAnimation = BuildMenuTransitionAnimation(pos);
        var opacityAnimation = CreateOpacityAnimation();
        var transitionStoryboard = new Storyboard
        {
            Animations =
            [
                (Menu, menuTransitionAnimation),
                (PagePanel, opacityAnimation),
            ]
        };
        
        _animationRunningSubject.OnNext(true);
        await transitionStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task CloseMenuAsync()
    {
        var pos = AnchorAtTopLeftPoint(FakeTouchDockAnchor);
        var menuTransitionAnimation = BuildMenuTransitionAnimation(pos, true);
        var opacityAnimation = CreateOpacityAnimation(true);
        var transitionStoryboard = new Storyboard()
        {
            Animations =
            [
                (Menu, menuTransitionAnimation),
                (PagePanel, opacityAnimation),
            ]
        };
        
        _animationRunningSubject.OnNext(true);
        await transitionStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
        Closed?.Invoke(this, EventArgs.Empty);
    }
    
    private async Task GoToInnerPageAsync(object? sender)
    {
        PageBase innerPage = sender switch
        {
            MenuItem { Tag: "Device" } entryItem => new DevicePage { EntryCell = entryItem.Cell },
            MenuItem { Tag: "Game" } entryItem => new GamePage { EntryCell = entryItem.Cell },
            _ => throw new NotSupportedException(),
        };

        InnerPageHost.Content = innerPage;
        
        var innerPageStoryboard = innerPage.BuildPageTranslateStoryboard(Menu.Width);
        var opacityStoryboard = new Storyboard
        {
            Animations = 
            [
                (InnerPageHost, CreateOpacityAnimation()),
                (MainPage, CreateOpacityAnimation(true)),
            ]
        };

        _animationRunningSubject.OnNext(true);
        await Storyboard.PlayMultiAsync(innerPageStoryboard, opacityStoryboard);
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task ReturnToMainPageAsync(object? sender)
    {
        if (sender is not PageBase innerPage) 
            return;

        _animationRunningSubject.OnNext(true);
        var innerOpacityStoryboard = CreateOpacityAnimation(true).AsStoryboard(InnerPageHost);
        var innerTranslateStoryboard = innerPage.BuildPageTranslateStoryboard(Menu.Width, true);
        await Storyboard.PlayMultiAsync(innerTranslateStoryboard, innerOpacityStoryboard);

        var mainPageOpacityStoryboard = CreateOpacityAnimation().AsStoryboard(MainPage);
        await mainPageOpacityStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
        
        InnerPageHost.Content = null;
    }
}