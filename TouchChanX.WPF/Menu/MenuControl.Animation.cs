using R3;
using R3.ObservableEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TouchChanX.WPF.Menu;

public partial class MenuControl // Animation
{
    private static readonly TimeSpan PageTransitionInDuration = TimeSpan.FromMilliseconds(400);

    // 用于解析到没有 TranformGroup 的 PropertyPath
    private static readonly PropertyPath TranslateXPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.XProperty})");
    private static readonly PropertyPath TranslateYPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.YProperty})");
    private static readonly PropertyPath WidthPropertyPath = new(WidthProperty);
    private static readonly PropertyPath HeightPropertyPath = new(HeightProperty);
    private static readonly PropertyPath OpacityPropertyPath = new(OpacityProperty);

    private static readonly Subject<bool> AnimationRunningSubject = new();
    public static Observable<bool> AnimationRunning => AnimationRunningSubject;

    public Task MenuOpenAnimationAsync()
    {
        (MenuMoveTransform.X, MenuMoveTransform.Y) = (TouchAnchor.X, TouchAnchor.Y);

        var transformStoryboard = new Storyboard()
        {
            Children =
            [
                ..BuildMenuTransitionAnimations(MenuBorder, CenterPosition),
                ApplyOpacityAnimation(FakeTouch, false),
            ]
        };

        var tcs = new TaskCompletionSource();
        transformStoryboard.Events().Completed
            .Do(_ => (MenuMoveTransform.X, MenuMoveTransform.Y) = (0, 0))
            .Do(_ => AnimationRunningSubject.OnNext(false))
            .Subscribe(_ => tcs.SetResult());

        transformStoryboard.Freeze();

        AnimationRunningSubject.OnNext(true);
        transformStoryboard.Begin();

        return tcs.Task;
    }

    public Task MenuCloseAnimationAsync()
    {
        (MenuMoveTransform.X, MenuMoveTransform.Y) = (CenterPosition.X, CenterPosition.Y);

        var transformStoryboard = new Storyboard()
        {
            Children =
            [
                ..BuildMenuTransitionAnimations(MenuBorder, TouchAnchor, false),
                ApplyOpacityAnimation(FakeTouch, true),
            ]
        };

        var tcs = new TaskCompletionSource();
        transformStoryboard.Events().Completed
            .Do(_ => (MenuMoveTransform.X, MenuMoveTransform.Y) = (TouchAnchor.X, TouchAnchor.Y))
            .Do(_ => AnimationRunningSubject.OnNext(false))
            .Subscribe(_ => tcs.SetResult());

        transformStoryboard.Freeze();

        AnimationRunningSubject.OnNext(true);
        transformStoryboard.Begin();

        return tcs.Task;
    }

    private static DoubleAnimation[] BuildMenuTransitionAnimations(FrameworkElement menu, Point destPos, bool isOpening = true)
    {
        var xAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = destPos.X, FillBehavior = FillBehavior.Stop };
        var yAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = destPos.Y, FillBehavior = FillBehavior.Stop };
        var widthAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = isOpening ? MenuSize : TouchSize };
        var heightAnimation = widthAnimation.Clone();

        Storyboard.SetTarget(xAnimation, menu);
        Storyboard.SetTarget(yAnimation, menu);
        Storyboard.SetTarget(widthAnimation, menu);
        Storyboard.SetTarget(heightAnimation, menu);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        Storyboard.SetTargetProperty(widthAnimation, WidthPropertyPath);
        Storyboard.SetTargetProperty(heightAnimation, HeightPropertyPath);

        return [xAnimation, yAnimation, widthAnimation, heightAnimation];
    }

    private static DoubleAnimation ApplyOpacityAnimation(FrameworkElement target, bool isShowing)
    {
        double from = isShowing ? 0.0 : 1.0;
        double to = isShowing ? 1.0 : 0.0;

        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = PageTransitionInDuration,
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, OpacityPropertyPath);

        return animation;
    }
}
