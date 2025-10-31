using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using R3;
using R3.ObservableEvents;

namespace TouchChanX.Ava.Touch;

public partial class TouchControl : UserControl
{
    private const int TouchSpacing = Shared.Constants.TouchSpacing;

    private readonly Observable<Shared.TouchDockAnchor> _clicked;
    public Observable<Shared.TouchDockAnchor> Clicked => _clicked;

    private readonly TranslateTransform _moveTransform = new() { X = TouchSpacing, Y = TouchSpacing };

    public TouchControl()
    {
        InitializeComponent();
        Touch.RenderTransform = _moveTransform;

        TouchSubscribe(out _clicked);
    }

    private void TouchSubscribe(out Observable<Shared.TouchDockAnchor> onClicked)
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
                        .Where(moveEvent =>
                        {
                            var pressPos = pressEvent.GetPosition(container);
                            var movePos = moveEvent.GetPosition(container);
                            return pressPos != movePos;
                        })
                        .Take(1)
                        .TakeUntil(pointerReleasedStream))
                .Share();

        var clickStream = 
            pointerPressedStream
            .SelectMany(_ =>
                pointerReleasedStream
                .Take(1)
                .TakeUntil(dragStartedStream))
            .Delay(OpacityFadeInDuration)
            .ThrottleFirst(OpacityFadeInDuration)
            .Share();

        // 订阅点击事件
        onClicked = clickStream
            .ObserveOn(App.UISyncContext)
            .Where(_ => container.IsVisible)
            .Select(_ => Shared.TouchDockAnchor.FromRect(container.Bounds.Size, TouchDockRect));

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

        var touchDockedSubject = new Subject<Unit>();
        var whenTouchDocked = touchDockedSubject.Share();

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
                // HACK: 缓解动画完成渲染不精确造成的明显错误视觉效果
                await Task.Delay(50, _);
                touchDockedSubject.OnNext(Unit.Default);
            });

        // 订阅透明恢复动画
        pointerPressedStream
            .Where(_ => Math.Abs(Touch.Opacity - OpacityFull) >= 0.01)
            .SubscribeAwait(async (_, _) => await RunFadeInAnimationAsync());
        
        var whenWindowReady = 
            container.Events().SizeChanged
            .Where(sizeEvent => sizeEvent.NewSize.Width > Touch.Bounds.Size.Width)
            .Take(1)
            .Select(_ => Unit.Default)
            .Share();
        
        var whenTouchVisible = 
            container.Events().Loaded
                .SelectMany(_ => container.GetObservable(IsVisibleProperty).ToObservable())
                .Skip(1)
                .Where(isVisible => isVisible)
                .Select(_ => Unit.Default)
                .Share();
        
        // 订阅变透明动画
        Observable.Merge(
            whenWindowReady,
            whenTouchDocked,
            whenTouchVisible)
            .Select(_ =>
                Observable.Timer(OpacityFadeDelay)
                    .TakeUntil(pointerPressedStream))
            .Switch()
            .SubscribeAwait(async (_, _) => await RunFadeOutAnimationAsync());
        
        // TODO: 还需检查拖动的时候窗口大小改变的情景
        
        // 订阅窗口大小改变时自动更新停靠的touch位置
        container.Events().SizeChanged
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchDockRect, sizeEvent.NewSize, TouchSpacing))
            .Subscribe(rect => TouchDockRect = rect);

        // 订阅回调设置容器窗口的可观察区域
        clickStream
            .Merge(dragStartedStream)
            .ObserveOn(App.UISyncContext)
            .Subscribe(_ => ResetWindowObservableRegion?.Invoke(this.Bounds.Size));
        whenTouchDocked
            .Merge(whenTouchVisible)
            .Merge(whenWindowReady)
            .Subscribe(_ => SetWindowObservableRegion?.Invoke(TouchDockRect));
        // FIXME: 窗口最小化时，可能会导致 Touch 重新定位到左上角，导致 Touch 不可见
    }

    public Action<Size>? ResetWindowObservableRegion { get; set; }
    public Action<Rect>? SetWindowObservableRegion { get; set; }

    private Rect TouchDockRect
    {
        get => new(_moveTransform.X, _moveTransform.Y, Touch.Width, Touch.Height);
        set => (_moveTransform.X, _moveTransform.Y, Touch.Width) = (value.X, value.Y, value.Width);
    }
}