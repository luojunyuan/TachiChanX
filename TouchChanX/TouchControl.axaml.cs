using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using R3;
using R3.ObservableEvents;

namespace TestAva;

static class AnimationTool
{
    public static Animation CreateTranslateAnimation(TimeSpan duration) => new()
    {
        Duration = duration,
        FillMode = FillMode.Forward,
        Children =
        {
            CreatePointKeyFrame(0d),
            CreatePointKeyFrame(1d),
        },
    };

    public static void UpdateTranslateAnimationProperties(this Animation animation, Point start, Point end)
    {
        ((Setter)animation.Children[0].Setters[0]).Value = start.X;
        ((Setter)animation.Children[0].Setters[1]).Value = start.Y;
        ((Setter)animation.Children[1].Setters[0]).Value = end.X;
        ((Setter)animation.Children[1].Setters[1]).Value = end.Y;
    }

    private static KeyFrame CreatePointKeyFrame(double timePoint) => new()
    {
        Cue = new Cue(timePoint),
        Setters =
        {
            new Setter(TranslateTransform.XProperty, default),
            new Setter(TranslateTransform.YProperty, default)
        }
    };
}

public partial class TouchControl : UserControl
{
    public TouchControl()
    {
        InitializeComponent();

        // https://github.com/luojunyuan/TouchPerformance/blob/master/TouchAva/TouchControl.axaml.cs
        // https://github.com/luojunyuan/TachiChanNext/blob/master/TouchChan.Ava/TouchControl.axaml.cs

        Touch.RenderTransform = TouchTransform;
        TranslationAnimation = AnimationTool.CreateTranslateAnimation(ReleaseToEdgeDuration);
        TouchSubscribe(this);
    }

    private readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);
    private readonly Animation TranslationAnimation = new();
    private readonly TranslateTransform TouchTransform = new();

    private void TouchSubscribe(Control container)
    {
        var moveAnimationEndedStream = new Subject<Unit>();

        var raisePointerReleasedSubject = new Subject<PointerEventArgs>();
        
        // work-around: 强转 Touch 以触发生成器工作
        var pointerPressedStream =
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

        // Timeline -->
        // |
        // |    Pressed suddenly released
        // | x -*|----->
        // |
        // |    Dragging
        // | x -*--*---------*------->
        // |       Released  Released
        // |      (by raise)
        // |                 ↓
        // |                 DragEnded
        // |                -*---------------------*|-->
        // |                 Start    Animation    End

        // Touch 的拖拽逻辑
        var draggingStream =
            dragStartedStream
            .SelectMany(pressedEvent =>
            {
                // Origin   Element
                // *--------*--*------
                //             Pointer 
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

        // 1. 订阅拖动
        draggingStream
            .Select(item => item.Delta)
            .Subscribe(newPos =>
                (TouchTransform.X, TouchTransform.Y) = (newPos.X, newPos.Y));

        // Touch 拖动边界检测
        var boundaryExceededStream =
            draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                item.Delta, Touch.Width, container.Bounds.Size))
            .Select(item => item.MovedEvent);

        // 2. 订阅边界释放事件
        boundaryExceededStream
            .Subscribe(raisePointerReleasedSubject.OnNext);

        // 别名拖动结束流为动画开始流
        var moveAnimationStartedStream = dragEndedStream;

        // 3. 订阅 Touch 释放停靠动画
        moveAnimationStartedStream
            .Select(pointer =>
            {
                var distanceToOrigin = pointer.GetPosition(container);
                var distanceToElement = pointer.GetPosition(Touch);
                var touchPos = distanceToOrigin - distanceToElement;
                return (touchPos, PositionCalculator.CalculateTouchFinalPosition(container.Bounds.Size, new Rect(touchPos, Touch.Bounds.Size)));
            })
            .SubscribeAwait(async (positionPair, _) =>
            {
                var (startPos, stopPos) = positionPair;
                TranslationAnimation.UpdateTranslateAnimationProperties(startPos, new(stopPos.X, stopPos.Y));
                await TranslationAnimation.RunAsync(Touch, CancellationToken.None);
                moveAnimationEndedStream.OnNext(Unit.Default);
            });

        // 4. 订阅交互限制：释放到边缘动画期间屏蔽操作
        //Observable.Merge(
        //    moveAnimationStartedStream.Select(_ => false),
        //    moveAnimationEndedStream.Select(_ => true))
        //    .Subscribe(canHit => Touch.IsHitTestVisible = canHit);

        //// 回调设置容器窗口的可观察区域
        //Touch.Clicked()
        //    .Merge(dragStartedStream.Select(_ => Unit.Default))
        //    .Select(_ => container.ActualSize.XDpi(DpiScale))
        //    .Subscribe(clientArea => ResetWindowObservable?.Invoke(clientArea));

        //moveAnimationEndedStream.Select(_ => Unit.Default)
        //    .Do(_ => Touch.IsHitTestVisible = true)
        //    //.Merge(OnMenuClosed)
        //    //.Do(_ => RestoreFocus?.Invoke())
        //    //.Select(_ => GetTouchDockRect().XDpi(DpiScale))
        //    .Subscribe();
        //.Subscribe(rect => SetWindowObservable?.Invoke(rect));

        //// 调整按钮透明度
        //AnimationTool.InitializeOpacityAnimations(Touch);
        //pointerPressedStream.Select(_ => Unit.Default)
        //    // 打开 menu 或者 pointerReleasedStream 的时候保持透明度
        //    .Where(_ => !Touch.IsFullyOpaque())
        //    .Subscribe(_ => AnimationTool.FadeInOpacityStoryboard.Begin());

        //OnWindowBounded
        //    .Merge(pointerReleasedStream.Select(_ => Unit.Default))
        //    .Merge(moveAnimationEndedStream.Select(_ => Unit.Default))
        //    .Merge(OnMenuClosed)
        //    .Select(_ =>
        //        Observable.Timer(OpacityFadeDelay)
        //        .TakeUntil(pointerPressedStream))
        //    .Switch()
        //    .ObserveOn(App.UISyncContext)
        //    .Subscribe(_ => AnimationTool.FadeOutOpacityStoryboard.Begin());

        //// 小白点停留时的位置状态
        //dockObservable =
        //    moveAnimationEndedStream.Select(_ =>
        //        PositionCalculator.GetLastTouchDockAnchor(container.ActualSize.ToSize(), GetTouchDockRect()))
        //    .Merge(container.Events().SizeChanged.Select(_ => CurrentDock))
        //    .Select(dock => PositionCalculator.TouchDockCornerRedirect(dock, container.ActualSize.ToSize(), Touch.Width))
        //    .ToProperty(initialValue: new(TouchCorner.Left, 0.5));
    }

    // 备考：苹果的小圆点不仅仅是依赖释放位置来判断动画和停靠，如果拖拽释放速度小于一个值，就是按照边缘动画恢复。
    // 如果拖拽释放速度大于一个值，还有加速度作用在控件上往速度方向飞出去
}