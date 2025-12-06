using R3;
using R3.ObservableEvents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TouchChanX.WPF.Touch;

/// <summary>
/// TouchControl.xaml 的交互逻辑
/// </summary>
public partial class TouchControl : UserControl
{
    public TouchControl()
    {
        InitializeComponent();
        InitializeStaticAnimations();

        TouchSubscribe();
    }

    private const int TouchSpacing = 2;
    
    private TouchControl Container => this;

    private Size CurrentContainerSize => new(this.ActualWidth, this.ActualHeight);

    private Size CurrentTouchSize => new(Touch.ActualWidth, Touch.ActualHeight);

    private void TouchSubscribe()
    {
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
                var pressPos = pressEvent.GetPosition(Container);

                return 
                    pointerMovedStream
                    .Where(moveEvent =>
                    {
                        var movePos = moveEvent.GetPosition(Container);
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
                        var distanceToOrigin = movedEvent.GetPosition(Container);
                        var delta = distanceToOrigin - distanceToElement;

                        return new { NewPosition = (Point)delta, MovedEvent = movedEvent };
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
                CurrentContainerSize,
                new Rect(item.NewPosition, CurrentTouchSize)))
            .Select(item => item.MovedEvent);

        // 订阅边缘释放事件 -> 触发 dragEndedStream
        boundaryExceededStream
            .Subscribe(raiseMouseReleasedSubject.OnNext);

        var touchDockedStream =
            dragEndedStream
            .Select(pointer =>
            {
                var distanceToOrigin = pointer.GetPosition(Container);
                var distanceToElement = pointer.GetPosition(Touch);
                var touchPos = distanceToOrigin - distanceToElement;
                return
                    PositionCalculator.CalculateTouchFinalPosition(
                        CurrentContainerSize,
                        new Rect((Point)touchPos, CurrentTouchSize),
                        TouchSpacing);
            })
            .SelectMany(finalPos => 
                Observable.FromAsync(_ => new(AnimateTouchToEdgeAsync(finalPos, Touch))))
            .Share();

        // 订阅变透明动画
        Observable.Merge(
            WhenWindowReady,
            //whenTouchVisible,
            touchDockedStream)
            .Select(_ =>
                Observable.Timer(OpacityFadeDelay)
                .TakeUntil(pointerPressedStream))
            .Switch() // 既要重新计时，又要检查取消流，所以不是 Debounce
            .ObserveOn(App.UISyncContext)
            .Subscribe(_ => RunFadeOutAnimaion(Touch));

        // 订阅透明恢复动画
        pointerPressedStream
            .Where(_ => Math.Abs(Touch.Opacity - OpacityFull) >= 0.01)
            .Subscribe(_ => RunFadeInAnimaion(Touch));

        TouchMiscSubscribe();
    }

    private Observable<Unit> WhenWindowReady => 
        Container.Events().SizeChanged
            .Where(sizeEvent => sizeEvent.NewSize.Width > CurrentTouchSize.Width)
            .Take(1)
            .Select(_ => Unit.Default);

    private Rect TouchDockRect
    {
        get => new(MoveTransform.X, MoveTransform.Y, Touch.Width, Touch.Height);
        set => (MoveTransform.X, MoveTransform.Y, Touch.Width) = (value.X, value.Y, value.Width);
    }

    private void TouchMiscSubscribe()
    {
        var whenTouchVisibled =
            Container.Events().IsVisibleChanged
            .Skip(1)
            .Select(_ => Container.IsVisible)
            .Do(v => Debug.WriteLine($"{v} touch visible"))
            .Subscribe();

        // TODO: 测试 Touch 一边拖动窗口大小一边改变的边缘场景

        // 订阅窗口大小改变时自动更新停靠的touch位置
        Container.Events().SizeChanged
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchDockRect, sizeEvent.NewSize, TouchSpacing))
            .Subscribe(rect => TouchDockRect = rect);

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
