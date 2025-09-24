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
    private const int TouchSpacing = 2;
    private readonly TranslateTransform _moveTransform = new() { X = TouchSpacing, Y = TouchSpacing };

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
                    var pressPos = pressEvent.GetPosition(container);
                    var releasePos = releaseEvent.GetPosition(container);
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
                            var pressPos = pressEvent.GetPosition(container);
                            var movePos = moveEvent.GetPosition(container);
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
                                var distanceToOrigin = movedEvent.GetPosition(container);
                                var delta = distanceToOrigin - distanceToElement;

                                return new { NewPosition = delta, MovedEvent = movedEvent };
                            });
                })
                .Share();

        // 订阅移动事件
        draggingStream
            .Select(item => item.NewPosition)
            .Subscribe(newPos =>
                (_moveTransform.X, _moveTransform.Y) = (newPos.X, newPos.Y));

        var boundaryExceededStream =
            draggingStream
                .Where(item => PositionCalculator.IsBeyondBoundary(
                    container.Bounds.Size, new Rect(item.NewPosition.X, item.NewPosition.Y, Touch.Width, Touch.Height)))
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
                        new Rect(touchPos, Touch.Bounds.Size), TouchSpacing));
            })
            .SubscribeAwait(async (positions, _) =>
            {
                await RunReleaseTranslationAnimationAsync(positions);
                translationDockedSubject.OnNext(Unit.Default);
            });

        // 订阅透明恢复动画
        pointerPressedStream.SubscribeAwait(async (_, _) => await RunFadeInAnimationAsync());
        
        var whenWindowReady = 
            container.Events().SizeChanged
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

        // TEST: 还需检查拖动的时候窗口大小改变的情景
        // 订阅窗口大小改变时自动更新停靠的touch位置
        container.Events().SizeChanged
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchDockRect, sizeEvent.NewSize, TouchSpacing))
            .Subscribe(rect => TouchDockRect = rect);
    }
    
    private Rect TouchDockRect
    {
        get => new(_moveTransform.X, _moveTransform.Y, Touch.Width, Touch.Height);
        set => (_moveTransform.X, _moveTransform.Y, Touch.Width) = (value.X, value.Y, value.Width);
    }
}