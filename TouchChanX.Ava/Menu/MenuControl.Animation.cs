using Avalonia.Animation;
using Avalonia.Styling;

namespace TouchChanX.Ava.Menu;

public partial class MenuControl
{
    private static readonly TimeSpan PageTransitionInDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan PageTransitionOutDuration = TimeSpan.FromMilliseconds(250);

    private static Animation CreateOpacityAnimation(bool reverse = false) => new()
    {
        Duration = reverse ? PageTransitionOutDuration : PageTransitionInDuration,
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