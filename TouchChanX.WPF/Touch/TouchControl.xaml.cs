using R3;
using R3.ObservableEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TouchChanX.WPF.Touch;

/// <summary>
/// TouchControl.xaml 的交互逻辑
/// </summary>
public partial class TouchControl : UserControl
{
    public const double AssistiveTouchCircleThickness = 1.0;

    private const int TouchSpacing = 2;

    public TouchControl()
    {
        InitializeComponent();

        TouchSubscribe();
    }

    private void TouchSubscribe()
    {
        var container = this;
        var raiseMouseReleasedSubject = new Subject<MouseEventArgs>();

        var pointerPressedStream =
            Touch.Events().PreviewMouseLeftButtonDown
            .Do(_ => Touch.CaptureMouse())
            .Share();
        var pointerMovedStream =
            Touch.Events().PreviewMouseMove
            .Share();
        var pointerReleasedStream =
            Touch.Events().PreviewMouseLeftButtonUp
            .Cast<MouseButtonEventArgs, MouseEventArgs>()
            .Merge(raiseMouseReleasedSubject)
            .Do(_ => Touch.ReleaseMouseCapture())
            .Share();

        var dragStartedStream =
            pointerPressedStream
            .SelectMany(pressEvent =>
            {
                var pressPos = pressEvent.GetPosition(container);

                return 
                    pointerMovedStream
                    .Where(moveEvent =>
                    {
                        var movePos = moveEvent.GetPosition(container);
                        return (pressPos - movePos).Length > 1;
                    })
                    .Take(1)
                    .TakeUntil(pointerReleasedStream);
            })
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
                (MoveTransform.X, MoveTransform.Y) = (newPos.X, newPos.Y));

        var boundaryExceededStream =
            draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                new Size(container.ActualWidth, container.ActualHeight),
                new Rect(item.NewPosition.X, item.NewPosition.Y, Touch.Width, Touch.Height)))
            .Select(item => item.MovedEvent);

        // 订阅边缘释放事件 -> 触发 dragEndedStream
        boundaryExceededStream
            .Subscribe(raiseMouseReleasedSubject.OnNext);

        // 订阅释放动画
        dragEndedStream
            .Select(pointer =>
            {
                var distanceToOrigin = pointer.GetPosition(container);
                var distanceToElement = pointer.GetPosition(Touch);
                var touchPos = distanceToOrigin - distanceToElement;
                return
                    PositionCalculator.CalculateTouchFinalPosition(
                        new Size(container.ActualWidth, container.ActualHeight),
                        new Rect(touchPos.X, touchPos.Y, Touch.Width, Touch.Height),
                        TouchSpacing);
            })
            .Subscribe(finalPos => 
                RunReleaseTranslationAnimation(finalPos, Touch));

        TouchMiscSubscribe();
    }

    private void TouchMiscSubscribe()
    {
        var container = this;

        //// 订阅透明恢复动画
        //pointerPressedStream
        //    .Where(_ => Math.Abs(Touch.Opacity - OpacityFull) >= 0.01)
        //    .SubscribeAwait(async (_, _) => await RunFadeInAnimationAsync());

        var whenWindowSizeChanged = container.Events().SizeChanged.Share();

        var whenWindowReady =
            whenWindowSizeChanged
            .Where(sizeEvent => sizeEvent.NewSize.Width > Touch.ActualWidth)
            .Take(1)
            .Select(_ => Unit.Default);

        //var whenTouchVisible =
        //    container.Events().Loaded
        //    .SelectMany(_ => container.GetObservable(IsVisibleProperty).ToObservable())
        //    .Skip(1)
        //    .Where(isVisible => isVisible)
        //    .Select(_ => Unit.Default)
        //    .Share();

        // 订阅变透明动画
        //Observable.Merge(
        //    whenWindowReady,
        //    whenTouchDocked,
        //    whenTouchVisible)
        //    .Select(_ =>
        //        Observable.Timer(OpacityFadeDelay)
        //            .TakeUntil(pointerPressedStream))
        //    .Switch()
        //    .SubscribeAwait(async (_, _) => await RunFadeOutAnimationAsync());

        // TODO: 测试 Touch 一边拖动窗口大小一边改变的边缘场景

        // 订阅窗口大小改变时自动更新停靠的touch位置
        //whenWindowSizeChanged
        //    .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
        //        sizeEvent.PreviousSize, TouchDockRect, sizeEvent.NewSize, TouchSpacing))
        //    .Subscribe(rect => TouchDockRect = rect);

        //// 订阅回调设置容器窗口的可观察区域
        //clickStream
        //    .Merge(dragStartedStream)
        //    .ObserveOn(App.UISyncContext)
        //    .Subscribe(_ => ResetWindowObservableRegion?.Invoke(this.Bounds.Size));
        //whenTouchDocked
        //    .Merge(whenTouchVisible)
        //    .Merge(whenWindowSizeChanged.Select(_ => Unit.Default))
        //    .Subscribe(_ => SetWindowObservableRegion?.Invoke(TouchDockRect));
    }
}
