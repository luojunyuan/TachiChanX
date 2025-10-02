using Avalonia.Controls;
using R3;
using TouchChanX.Ava.Menu.Pages;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl : UserControl
{
    public MenuControl()
    {
        InitializeComponent();
        
        PageBase.BackRequested += async (s, _) => await ReturnToMainPageAsync(s);
        foreach (var items in MainPage.Children)
        {
            items.PointerPressed += async (s, _) => await GoToInnerPageAsync(s);
        }
        
        // 订阅执行任何动画期间都禁止整个页面再次交互
        _animationRunningSubject.Subscribe(running => this.IsHitTestVisible = !running);
    }
    
    private readonly Subject<bool> _animationRunningSubject = new();

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