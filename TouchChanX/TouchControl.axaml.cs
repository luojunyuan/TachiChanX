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
        
        // work-around: ǿת Touch �Դ�������������
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
        // |                 ��
        // |                 DragEnded
        // |                -*---------------------*|-->
        // |                 Start    Animation    End

        // Touch ����ק�߼�
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

        // 1. �����϶�
        draggingStream
            .Select(item => item.Delta)
            .Subscribe(newPos =>
                (TouchTransform.X, TouchTransform.Y) = (newPos.X, newPos.Y));

        // Touch �϶��߽���
        var boundaryExceededStream =
            draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                item.Delta, Touch.Width, container.Bounds.Size))
            .Select(item => item.MovedEvent);

        // 2. ���ı߽��ͷ��¼�
        boundaryExceededStream
            .Subscribe(raisePointerReleasedSubject.OnNext);

        // �����϶�������Ϊ������ʼ��
        var moveAnimationStartedStream = dragEndedStream;

        // 3. ���� Touch �ͷ�ͣ������
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

        // 4. ���Ľ������ƣ��ͷŵ���Ե�����ڼ����β���
        //Observable.Merge(
        //    moveAnimationStartedStream.Select(_ => false),
        //    moveAnimationEndedStream.Select(_ => true))
        //    .Subscribe(canHit => Touch.IsHitTestVisible = canHit);

        //// �ص������������ڵĿɹ۲�����
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

        //// ������ť͸����
        //AnimationTool.InitializeOpacityAnimations(Touch);
        //pointerPressedStream.Select(_ => Unit.Default)
        //    // �� menu ���� pointerReleasedStream ��ʱ�򱣳�͸����
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

        //// С�׵�ͣ��ʱ��λ��״̬
        //dockObservable =
        //    moveAnimationEndedStream.Select(_ =>
        //        PositionCalculator.GetLastTouchDockAnchor(container.ActualSize.ToSize(), GetTouchDockRect()))
        //    .Merge(container.Events().SizeChanged.Select(_ => CurrentDock))
        //    .Select(dock => PositionCalculator.TouchDockCornerRedirect(dock, container.ActualSize.ToSize(), Touch.Width))
        //    .ToProperty(initialValue: new(TouchCorner.Left, 0.5));
    }

    // ������ƻ����СԲ�㲻�����������ͷ�λ�����ж϶�����ͣ���������ק�ͷ��ٶ�С��һ��ֵ�����ǰ��ձ�Ե�����ָ���
    // �����ק�ͷ��ٶȴ���һ��ֵ�����м��ٶ������ڿؼ������ٶȷ���ɳ�ȥ
}