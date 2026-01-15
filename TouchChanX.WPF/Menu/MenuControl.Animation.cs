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

    private static readonly Subject<bool> AnimationRunningSubject = new();
    public static Observable<bool> AnimationRunning => AnimationRunningSubject;

    public static Task StartAnimationAsync(FrameworkElement menu, Point pos)
    {
        if (menu.RenderTransform is not TranslateTransform moveTransform)
            return Task.CompletedTask;

        var xAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.X, FillBehavior = FillBehavior.Stop };
        var yAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.Y, FillBehavior = FillBehavior.Stop };
        var widthAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = MenuSize };
        var heightAnimation = widthAnimation.Clone();

        var transformStoryboard = new Storyboard();
        Storyboard.SetTarget(xAnimation, menu);
        Storyboard.SetTarget(yAnimation, menu);
        Storyboard.SetTarget(widthAnimation, menu);
        Storyboard.SetTarget(heightAnimation, menu);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        Storyboard.SetTargetProperty(widthAnimation, WidthPropertyPath);
        Storyboard.SetTargetProperty(heightAnimation, HeightPropertyPath);
        transformStoryboard.Children.Add(xAnimation);
        transformStoryboard.Children.Add(yAnimation);
        transformStoryboard.Children.Add(widthAnimation);
        transformStoryboard.Children.Add(heightAnimation);
        var tcs = new TaskCompletionSource();
        transformStoryboard.Events().Completed
            .Do(_ => (moveTransform.X, moveTransform.Y) = (0, 0))
            .Subscribe(_ => tcs.SetResult());
        transformStoryboard.Freeze();

        transformStoryboard.Begin();

        return tcs.Task;
    }


    public static Task StartAnimationAsync2(FrameworkElement menu, Point pos)
    {
        var xAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.X, FillBehavior = FillBehavior.Stop };
        var yAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.Y, FillBehavior = FillBehavior.Stop };
        var widthAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = 80 };
        var heightAnimation = widthAnimation.Clone();

        var transformStoryboard = new Storyboard();
        Storyboard.SetTarget(xAnimation, menu);
        Storyboard.SetTarget(yAnimation, menu);
        Storyboard.SetTarget(widthAnimation, menu);
        Storyboard.SetTarget(heightAnimation, menu);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        Storyboard.SetTargetProperty(widthAnimation, WidthPropertyPath);
        Storyboard.SetTargetProperty(heightAnimation, HeightPropertyPath);
        transformStoryboard.Children.Add(xAnimation);
        transformStoryboard.Children.Add(yAnimation);
        transformStoryboard.Children.Add(widthAnimation);
        transformStoryboard.Children.Add(heightAnimation);
        var tcs = new TaskCompletionSource();
        transformStoryboard.Events().Completed.Subscribe(_ => tcs.SetResult());
        transformStoryboard.Freeze();

        transformStoryboard.Begin();

        return tcs.Task;
    }
}
