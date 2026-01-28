using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using R3.ObservableEvents;
using R3;

namespace TouchChanX.WPF.Touch;

public partial class TouchControl
{
    private static readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan OpacityFadeOutDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan OpacityFadeInDuration = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan OpacityFadeDelay = TimeSpan.FromMilliseconds(4000);
    private const double OpacityHalf = 0.4;
    private const double OpacityFull = 1.0;

    private static readonly PropertyPath TranslateXPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.XProperty})");
    private static readonly PropertyPath TranslateYPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.YProperty})");

    private static readonly Subject<bool> AnimationRunningSubject = new();
    public static Observable<bool> AnimationRunning => AnimationRunningSubject;

    private static Task AnimateTouchToEdgeAsync(Point targetPos, FrameworkElement control)
    {
        if (control.RenderTransform is not TranslateTransform moveTransform)
            return Task.CompletedTask;

        var xAnimation = new DoubleAnimation() 
        { Duration = ReleaseToEdgeDuration, To = targetPos.X, FillBehavior = FillBehavior.Stop };
        var yAnimation = new DoubleAnimation() 
        { Duration = ReleaseToEdgeDuration, To = targetPos.Y, FillBehavior = FillBehavior.Stop };
        var moveStoryboard = new Storyboard();

        Storyboard.SetTarget(xAnimation, control);
        Storyboard.SetTarget(yAnimation, control);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        moveStoryboard.Children.Add(xAnimation);
        moveStoryboard.Children.Add(yAnimation);

        var tcs = new TaskCompletionSource();
        moveStoryboard.Events().Completed
            .Do(_ => (moveTransform.X, moveTransform.Y) = (targetPos.X, targetPos.Y))
            .Do(_ => AnimationRunningSubject.OnNext(false))
            .Subscribe(_ => tcs.SetResult());
        // TIP: Do 没有严肃的先后顺序，副作用也不应该相互依赖

        moveStoryboard.Freeze();

        AnimationRunningSubject.OnNext(true);
        moveStoryboard.Begin();

        return tcs.Task;
    }

    private static readonly DoubleAnimation FadeInOpacityAnimation = new()
    {
        From = OpacityHalf,
        To = OpacityFull,
        Duration = OpacityFadeInDuration,
    };

    private static readonly DoubleAnimation FadeOutOpacityAnimation = new()
    {
        From = OpacityFull,
        To = OpacityHalf,
        Duration = OpacityFadeOutDuration,
    };

    private static void InitializeStaticAnimations()
    {
        FadeInOpacityAnimation.Freeze();
        FadeOutOpacityAnimation.Freeze();
    }

    private static void RunFadeInAnimaion(FrameworkElement touch)
    {
        touch.BeginAnimation(OpacityProperty, FadeInOpacityAnimation);
    }

    private static void RunFadeOutAnimaion(FrameworkElement touch)
    {
        touch.BeginAnimation(OpacityProperty, FadeOutOpacityAnimation);
    }
}
