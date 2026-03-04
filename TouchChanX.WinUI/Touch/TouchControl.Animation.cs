using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Foundation;

namespace TouchChanX.WinUI.Touch;

public partial class TouchControl
{
    private static readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);

    private static void AnimateTouchToEdge(Point targetPos, CompositeTransform touchTransform)
    {
        var storyboard = new Storyboard();

        var animX = new DoubleAnimation
        {
            To = targetPos.X,
            Duration = ReleaseToEdgeDuration,
        };
        Storyboard.SetTarget(animX, touchTransform);
        Storyboard.SetTargetProperty(animX, nameof(CompositeTransform.TranslateX));

        var animY = new DoubleAnimation
        {
            To = targetPos.Y,
            Duration = ReleaseToEdgeDuration,
        };
        Storyboard.SetTarget(animY, touchTransform);
        Storyboard.SetTargetProperty(animY, nameof(CompositeTransform.TranslateY));

        storyboard.Children.Add(animX);
        storyboard.Children.Add(animY);
        storyboard.Begin();
    }
}
