using Avalonia;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using R3;

namespace TouchChanX.Ava.Touch;

public partial class TouchControl // Animation
{
    private static readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan OpacityFadeOutDuration = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan OpacityFadeInDuration = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan OpacityFadeDelay = TimeSpan.FromMilliseconds(4000);
    private const double OpacityHalf = 0.4;
    private const double OpacityFull = 1d;
    
    private readonly Subject<bool> _animationRunningSubject = new();
    public Observable<bool> AnimationRunning => _animationRunningSubject;
    
    private async Task RunReleaseTranslationAnimationAsync((Point StartPos, Point StopPos) pair)
    {
        var (startPos, stopPos) = pair;
        
        var animation = new Animation
        {
            Duration = ReleaseToEdgeDuration,
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

        _animationRunningSubject.OnNext(true);
        await animation.RunAsync(Touch, CancellationToken.None);
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task RunFadeOutAnimationAsync()
    {
        var animation = new Animation
        {
            Duration = OpacityFadeOutDuration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame 
                { 
                    Cue = new Cue(0d), 
                    Setters =
                    {
                        new Setter(OpacityProperty, OpacityFull),
                    } 
                },
                new KeyFrame
                {
                    Cue = new Cue(1d), 
                    Setters =
                    {
                        new Setter(OpacityProperty, OpacityHalf),
                    }
                }
            }
        };
        
        _animationRunningSubject.OnNext(true);
        await animation.RunAsync(Touch);
        _animationRunningSubject.OnNext(false);
    }
    
    private async Task RunFadeInAnimationAsync()
    {
        var animation = new Animation
        {
            Duration = OpacityFadeInDuration,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame 
                { 
                    Cue = new Cue(0d), 
                    Setters =
                    {
                        new Setter(OpacityProperty, Touch.Opacity),
                    } 
                },
                new KeyFrame
                {
                    Cue = new Cue(1d), 
                    Setters =
                    {
                        new Setter(OpacityProperty, OpacityFull),
                    }
                }
            }
        };

        _animationRunningSubject.OnNext(true);
        await animation.RunAsync(Touch);
        _animationRunningSubject.OnNext(false);
    }
}