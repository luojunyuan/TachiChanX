using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava.Touch;

public partial class TouchControl : UserControl
{
    private readonly TranslateTransform _moveTransform = new() { X = 2, Y = 2 };
    
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
            ((Border)Touch).Events().PointerPressed.Share()
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
        
        var clickStream = 
            pointerPressedStream
            .SelectMany(pressEvent =>
                pointerReleasedStream
                .Where(releaseEvent =>
                {
                    var pressPos = pressEvent.GetPosition(this);
                    var releasePos = releaseEvent.GetPosition(this);
                    return pressPos == releasePos;
                })
                .Take(1))
            .Delay(OpacityFadeInDuration)
            .ThrottleFirst(OpacityFadeInDuration)
            .Share();

        var dragStartedStream =
            pointerPressedStream
                .SelectMany(pressEvent =>
                    pointerMovedStream
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

        // 订阅移动事件
        draggingStream
            .Select(item => item.Delta)
            .Subscribe(newPos =>
                (_moveTransform.X, _moveTransform.Y) = (newPos.X, newPos.Y));

        var boundaryExceededStream =
            draggingStream
                .Where(item => PositionCalculator.IsBeyondBoundary(
                    container.Bounds.Size, new Rect(item.Delta.X, item.Delta.Y, Touch.Width, Touch.Width)))
                .Select(item => item.MovedEvent);

        // 订阅边缘释放事件
        boundaryExceededStream
            .Subscribe(raisePointerReleasedSubject.OnNext);

        var translationDockedSubject = new Subject<Unit>();

        // 订阅释放动画
        dragEndedStream
            .Select(pointer =>
            {
                var distanceToOrigin = pointer.GetPosition(container);
                var distanceToElement = pointer.GetPosition(Touch);
                var touchPos = distanceToOrigin - distanceToElement;
                return (touchPos,
                    PositionCalculator.CalculateTouchFinalPosition(container.Bounds.Size,
                        new Rect(touchPos, Touch.Bounds.Size), 2));
            })
            .SubscribeAwait(async (positions, _) =>
            {
                await RunReleaseTranslationAnimationAsync(positions);
                translationDockedSubject.OnNext(Unit.Default);
            });

        // 订阅透明恢复动画
        pointerPressedStream.SubscribeAwait(async (_, _) => await RunFadeInAnimationAsync());
        
        var whenWindowReady = 
            ((UserControl)this).Events().SizeChanged
            .Where(sizeEvent => sizeEvent.NewSize.Width > Touch.Bounds.Size.Width)
            .Take(1).Select(_ => Unit.Default);
        
        // 订阅变透明动画
        Observable.Merge(
            whenWindowReady,
            translationDockedSubject,
            clickStream.Select(_ => Unit.Default))
            .Select(_ =>
                Observable.Timer(OpacityFadeDelay)
                    .TakeUntil(pointerPressedStream))
            .Switch()
            .SubscribeAwait(async (_, _) => await RunFadeOutAnimationAsync());
        
        // 订阅执行任何动画期间都禁止整个页面再次交互
        _animationRunningSubject.Subscribe(running => this.IsHitTestVisible = !running);
    }
}