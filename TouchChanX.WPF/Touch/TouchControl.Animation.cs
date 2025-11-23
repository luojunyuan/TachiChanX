using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using R3.ObservableEvents;
using R3;

namespace TouchChanX.WPF.Touch;

public partial class TouchControl
{
    private static readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);

    private static readonly PropertyPath TranslateXPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.XProperty})");
    private static readonly PropertyPath TranslateYPropertyChain = new($"({RenderTransformProperty}).({TranslateTransform.YProperty})");

    private static readonly Subject<bool> AnimationRunningSubject = new();
    public static Observable<bool> AnimationRunning => AnimationRunningSubject;

    private static void RunReleaseTranslationAnimation(Point targetPos, FrameworkElement control)
    {
        if (control.RenderTransform is not TranslateTransform moveTransform)
            return;

        var xAnimation = new DoubleAnimation() { Duration = ReleaseToEdgeDuration, To = targetPos.X };
        var yAnimation = new DoubleAnimation() { Duration = ReleaseToEdgeDuration, To = targetPos.Y };
        var moveStoryboard = new Storyboard();

        Storyboard.SetTarget(xAnimation, control);
        Storyboard.SetTarget(yAnimation, control);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        xAnimation.Freeze();
        yAnimation.Freeze();
        moveStoryboard.Children.Add(xAnimation);
        moveStoryboard.Children.Add(yAnimation);

        moveStoryboard.Events().Completed
            .Take(1)
            .Do(_ => (moveTransform.X, moveTransform.Y) = (targetPos.X, targetPos.Y))
            .Do(_ => AnimationRunningSubject.OnNext(false))
            .Subscribe(_ => moveStoryboard.Remove());

        AnimationRunningSubject.OnNext(true);
        moveStoryboard.Begin();
    }
}
