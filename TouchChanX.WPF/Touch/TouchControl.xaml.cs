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
    public Observable<Rect> Clicked { get; private set; } = null!;

    public Observable<Unit> RegionResetRequested { get; private set; } = null!;

    private readonly BehaviorSubject<Rect> _regionChangeRequested = new(default);
    public Observable<Rect> RegionChangeRequested => _regionChangeRequested;

    public TouchControl()
    {
        InitializeComponent();
        InitializeStaticAnimations();

        TouchSubscribe();

        _containerSizeState = this.ObserveParentSize()
            .ToReadOnlyReactiveProperty();
    }

    private readonly ReadOnlyReactiveProperty<Size> _containerSizeState;
    private Size ContainerSize => _containerSizeState.CurrentValue;

    private const int TouchSpacing = Shared.TouchSpacing;

    private TouchControl Container => this;

    private Size CurrentTouchSize => new(Touch.ActualWidth, Touch.ActualHeight);

    private void TouchSubscribe()
    {
        var raiseMouseReleasedSubject = new Subject<MouseEventArgs>();

        var pointerPressedStream =
            Touch.Events().MouseLeftButtonDown
            .Do(_ => Touch.CaptureMouse())
            .Share();
        var pointerMovedStream =
            Touch.Events().MouseMove
            .Share();
        var pointerReleasedStream =
            Touch.Events().MouseLeftButtonUp
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
                ContainerSize,
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
                        ContainerSize,
                        new Rect((Point)touchPos, CurrentTouchSize),
                        TouchSpacing);
            })
            .SelectMany(finalPos =>
                Observable.FromAsync(_ => new(AnimateTouchToEdgeAsync(finalPos, Touch))))
            .Share();

        var touchDockRectUpdateStream =
            Container.Events().SizeChanged
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchDockRect, sizeEvent.NewSize, TouchSpacing))
            .Do(rect => TouchDockRect = rect)
            .Share();

        // 订阅窗口大小改变时自动更新停靠的touch位置
        touchDockRectUpdateStream.Subscribe();

        DefinePublicSignals();

        TouchOpacitySubscription(pointerPressedStream, touchDockedStream);

        void TouchOpacitySubscription(
            Observable<MouseButtonEventArgs> pointerPressedStream,
            Observable<Unit> touchDockedStream)
        {
            // 订阅变透明动画
            Observable.Merge(
                WhenWindowReady,
                WhenTouchVisible,
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
        }

        void DefinePublicSignals()
        {
            RegionResetRequested =
                Touch.Events().MouseLeftButtonDown
                .Select(_ => Unit.Default);

            Observable.Merge(
                    touchDockedStream.Select(_ => TouchDockRect),
                    WhenTouchVisible.Select(_ => TouchDockRect),
                    touchDockRectUpdateStream)
                .Subscribe(_regionChangeRequested.AsObserver());

            Clicked =
                pointerPressedStream
                .SelectMany(press =>
                    pointerReleasedStream
                    .Take(1)
                    .TakeUntil(dragStartedStream))
                // 略微有些 hack 的操作，即锁定了可能的 fade in 动画，保证了双击不会重复 press 的时序问题
                .Do(_ => AnimationRunningSubject.OnNext(true))
                .Delay(OpacityFadeInDuration)
                .ThrottleFirst(OpacityFadeInDuration)
                .ObserveOn(App.UISyncContext)
                .Select(_ => TouchDockRect)
                .Share();
        }
    }

    private Observable<Unit> WhenWindowReady =>
        Container.Events().SizeChanged
            .Where(sizeEvent => sizeEvent.NewSize.Width > CurrentTouchSize.Width)
            .Take(1)
            .Select(_ => Unit.Default);

    private Observable<Unit> WhenTouchVisible =>
        Container.Events().IsVisibleChanged
            .Where(_ => Container.IsVisible)
            .Select(_ => Unit.Default)
            .Share();

    private Rect TouchDockRect
    {
        get => new(MoveTransform.X, MoveTransform.Y, Touch.Width, Touch.Height);
        set => (MoveTransform.X, MoveTransform.Y, Touch.Width) = (value.X, value.Y, value.Width);
    }
}
