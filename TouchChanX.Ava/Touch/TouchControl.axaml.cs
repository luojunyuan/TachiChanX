using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava.Touch;

public partial class TouchControl : UserControl
{
    private readonly TranslateTransform _moveTransform = new();
    
    public TouchControl()
    {
        InitializeComponent();
        Touch.RenderTransform = _moveTransform;

        TouchSubscribe();
    }

    private void TouchSubscribe()
    {
        var container = this;
        var raisePointerReleasedSubject = new Subject<PointerEventArgs>();

        var pointerPressedStream =
            // ReSharper disable once RedundantCast
            ((Border)Touch).Events().PointerPressed
            .Where(e => e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            .Share();
        var pointerMovedStream =
            Touch.Events().PointerMoved
                .Where(e => e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
                .Share();
        var pointerReleasedStream =
            Touch.Events().PointerReleased
                .Select(releasedEvent => releasedEvent as PointerEventArgs)
                .Merge(raisePointerReleasedSubject)
                .Share();

        var dragStartedStream =
            pointerPressedStream
                .SelectMany(pressEvent =>
                    pointerMovedStream
                        .Skip(1)
                        .Where(moveEvent =>
                        {
                            var pressPos = pressEvent.GetPosition(this);
                            var movePos = moveEvent.GetPosition(this);
                            return pressPos != movePos;
                        })
                        .Take(1)
                        .TakeUntil(pointerReleasedStream))
                .Share();

        var dragEndedStream =
            dragStartedStream
                .SelectMany(_ =>
                    pointerReleasedStream
                        .Take(1))
                .Share();

        var draggingStream =
            dragStartedStream
                .SelectMany(pressedEvent =>
                {
                    var distanceToElement = pressedEvent.GetPosition(Touch);

                    return
                        pointerMovedStream
                            .TakeUntil(pointerReleasedStream)
                            .Select(movedEvent =>
                            {
                                var distanceToOrigin = movedEvent.GetPosition(this);
                                var delta = distanceToOrigin - distanceToElement;

                                return new { Delta = delta, MovedEvent = movedEvent };
                            });
                })
                .Share();

        draggingStream
            .Select(item => item.Delta)
            .Subscribe(newPos =>
                (_moveTransform.X, _moveTransform.Y) = (newPos.X, newPos.Y));

        var boundaryExceededStream =
            draggingStream
                .Where(item => PositionCalculator.IsBeyondBoundary(
                    container.Bounds.Size, new Rect(item.Delta.X, item.Delta.Y, Touch.Width, Touch.Width)))
                .Select(item => item.MovedEvent);

        boundaryExceededStream
            .Subscribe(raisePointerReleasedSubject.OnNext);

        var moveAnimationStartedStream = dragEndedStream;

        moveAnimationStartedStream
            .Select(pointer =>
            {
                var distanceToOrigin = pointer.GetPosition(container);
                var distanceToElement = pointer.GetPosition(Touch);
                var touchPos = distanceToOrigin - distanceToElement;
                return (touchPos,
                    PositionCalculator.CalculateTouchFinalPosition(container.Bounds.Size,
                        new Rect(touchPos, Touch.Bounds.Size)));
            })
            .SubscribeAwait(async (positionPair, _) =>
            {
                var (startPos, stopPos) = positionPair;
                await RunReleaseTranslationAnimationAsync(startPos, stopPos);
            });
    }
}