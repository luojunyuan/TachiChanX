using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;

namespace TouchChanX.Ava.Touch;

public partial class TouchControl // Animation
{
    
    private readonly TimeSpan _releaseToEdgeDuration = TimeSpan.FromMilliseconds(200);

    private async Task RunReleaseTranslationAnimationAsync(Point startPos, Point stopPos)
    {
        var animation = new Animation
        {
            Duration = _releaseToEdgeDuration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame 
                { 
                    Cue = new Cue(0d), 
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, startPos.X),
                        new Setter(TranslateTransform.YProperty, startPos.Y),
                    } 
                },
                new KeyFrame
                {
                    Cue = new Cue(1d), 
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, stopPos.X),
                        new Setter(TranslateTransform.YProperty, stopPos.Y),
                    }
                }
            }
        };

        await animation.RunAsync(Touch, CancellationToken.None);
    }
}