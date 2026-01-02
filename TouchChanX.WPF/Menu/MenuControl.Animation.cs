using R3;
using R3.ObservableEvents;
using System.Windows;
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

    public static Task StartAnimationAsync(FrameworkElement menu, Point pos)
    {
        var xAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.X, FillBehavior = FillBehavior.Stop };
        var yAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = pos.Y, FillBehavior = FillBehavior.Stop };
        var widthAnimation = new DoubleAnimation()
        { Duration = PageTransitionInDuration, To = 300 };

        var transformStoryboard = new Storyboard();
        Storyboard.SetTarget(xAnimation, menu);
        Storyboard.SetTarget(yAnimation, menu);
        Storyboard.SetTarget(widthAnimation, menu);
        Storyboard.SetTargetProperty(xAnimation, TranslateXPropertyChain);
        Storyboard.SetTargetProperty(yAnimation, TranslateYPropertyChain);
        Storyboard.SetTargetProperty(widthAnimation, WidthPropertyPath);
        transformStoryboard.Children.Add(xAnimation);
        transformStoryboard.Children.Add(yAnimation);
        transformStoryboard.Children.Add(widthAnimation);
        var tcs = new TaskCompletionSource();
        transformStoryboard.Events().Completed.Subscribe(_ => tcs.SetResult());
        transformStoryboard.Freeze();

        transformStoryboard.Begin();

        return tcs.Task;
    }
}
