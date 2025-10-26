using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using R3;
using TouchChanX.Ava.Menu.Pages;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl
{
    private static readonly TimeSpan PageTransitionInDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan PageTransitionOutDuration = TimeSpan.FromMilliseconds(250);

    private readonly Subject<bool> _animationRunningSubject = new();

    private async Task PlayTransitionMainPageStoryboardAsync(PageBase innerPage)
    {
        var innerOpacityStoryboard = CreateOpacityAnimation(true).AsStoryboard(InnerPageHost);
        var innerTranslateStoryboard = innerPage.BuildPageTranslateStoryboard(Menu.Width, true);
        var mainPageOpacityStoryboard = CreateOpacityAnimation().AsStoryboard(MainPage);

        _animationRunningSubject.OnNext(true);
        await Storyboard.PlayMultiAsync(innerTranslateStoryboard, innerOpacityStoryboard);
        await mainPageOpacityStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task PlayTransitionInnerPageStoryboardAsync(PageBase innerPage)
    {
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
    
    private async Task PlayCloseMenuStoryboardAsync(Point pos)
    {
        var menuTransitionAnimation = BuildMenuTransitionAnimation(pos, true);
        var opacityAnimation = CreateOpacityAnimation(true);
        var touchOpacityAnimation = CreateOpacityAnimation();
        var transitionStoryboard = new Storyboard()
        {
            Animations =
            [
                (Menu, menuTransitionAnimation),
                (PagePanel, opacityAnimation),
                (FakeTouch, touchOpacityAnimation),
            ]
        };
        
        _animationRunningSubject.OnNext(true);
        await transitionStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task PlayShowMenuStoryboardAsync(Point pos)
    {
        var menuTransitionAnimation = BuildMenuTransitionAnimation(pos);
        var opacityAnimation = CreateOpacityAnimation();
        var touchOpacityAnimation = CreateOpacityAnimation(true);
        var transitionStoryboard = new Storyboard
        {
            Animations =
            [
                (Menu, menuTransitionAnimation),
                (PagePanel, opacityAnimation),
                (FakeTouch, touchOpacityAnimation),
            ]
        };
        
        _animationRunningSubject.OnNext(true);
        await transitionStoryboard.PlayAsync();
        _animationRunningSubject.OnNext(false);
    }
    
    private static Animation BuildMenuTransitionAnimation(Point startOffset, bool reverse = false) => new()
    {
        Duration = reverse ? PageTransitionOutDuration : PageTransitionInDuration,
        FillMode = FillMode.Forward,
        Easing = reverse ? new LinearEasing() : new CubicEaseOut(),
        PlaybackDirection = reverse ? PlaybackDirection.Reverse : PlaybackDirection.Normal,
        Children =
        {
            new KeyFrame
            {
                Cue = new Cue(0d),
                Setters =
                {
                    new Setter(TranslateTransform.XProperty, startOffset.X),
                    new Setter(TranslateTransform.YProperty, startOffset.Y),
                    new Setter(WidthProperty, 80d),
                }
            },
            new KeyFrame
            {
                Cue = new Cue(1d),
                Setters =
                {
                    new Setter(TranslateTransform.XProperty, 0d),
                    new Setter(TranslateTransform.YProperty, 0d),
                    new Setter(WidthProperty, 300d),
                }
            }
        }
    };

    private static Animation CreateOpacityAnimation(bool reverse = false) => new()
    {
        Duration = reverse ? PageTransitionOutDuration : PageTransitionInDuration,
        FillMode = FillMode.Forward,
        Easing = reverse ? new CubicEaseIn() : new CubicEaseOut(),
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