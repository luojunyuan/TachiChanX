using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using TouchChanX.Ava.Menu.Pages;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl : UserControl
{
    public MenuControl()
    {
        InitializeComponent();
        
        IPageBase.BackRequested += async (_, _) => await ReturnToMainPageAsync();
    }
    
    private static readonly TimeSpan PageTransitionInDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan PageTransitionOutDuration = TimeSpan.FromMilliseconds(250);

    private void OnGoInnerPage(object? sender, PointerPressedEventArgs e)
    {
        var (innerPage, entryCell) = ((IPageBase, MenuCell))(sender switch
        {
            MenuItem { Tag: "Device" } entryItem => (new DevicePage(), entryItem.Cell),
            MenuItem { Tag: "Game" } entryItem => (new GamePage(), entryItem.Cell),
            _ => throw new NotSupportedException(),
        });
        
        InnerPageHost.Content = innerPage;
        var innerPageStoryboard = innerPage.BuildPageEnterStoryboard(entryCell, Menu.Width);

        var opacityStoryboard = new Storyboard
        {
            Animations = 
            [
                (InnerPageHost, CreateOpacityAnimation(PageTransitionInDuration)),
                (MainPage, CreateOpacityAnimation(PageTransitionInDuration, true)),
            ]
        };

        innerPageStoryboard.PlayAsync();
        opacityStoryboard.PlayAsync();
    }
    
    private async Task ReturnToMainPageAsync()
    {
        var devicePage = InnerPageHost.Content as IPageBase;
        if (devicePage == null) return;

        var exitStoryboard = devicePage.BuildPageExitStoryboard(Menu.Width, PageTransitionOutDuration);
        await exitStoryboard.PlayAsync();

        var fadeInStoryboard = new Storyboard
        {
            Animations =
            [
                (MainPage, CreateOpacityAnimation(PageTransitionInDuration, reverse: false))
            ]
        };
        await fadeInStoryboard.PlayAsync();

        InnerPageHost.Content = null;
    }

    private static Animation CreateOpacityAnimation(TimeSpan duration, bool reverse = false) => new()
    {
        Duration = duration,
        FillMode = FillMode.Forward,
        PlaybackDirection = reverse ? PlaybackDirection.Reverse : PlaybackDirection.Normal,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0d),
                Setters =
                {
                    new Setter(OpacityProperty, 0d),
                    new Setter(IsEnabledProperty, !reverse), // 仅在隐藏时需要设置为 false
                }
            },
            new KeyFrame
            {
                Cue = new Cue(1d),
                Setters =
                {
                    new Setter(OpacityProperty, 1d),
                    new Setter(IsEnabledProperty, true),
                }
            }
        }
    };
    
    
}