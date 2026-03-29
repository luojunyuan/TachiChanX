using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using R3;
using R3.ObservableEvents;
using System.Numerics;
using Windows.Foundation;

namespace TouchChanX.WinUI.Touch;

public partial class TouchControl
{
    public static Observable<Unit>? ObservableRegionResetRequested { get; private set; }

    public static Observable<Rect>? ObservableTouchRegionChanged { get; private set; }
}

public sealed partial class TouchControl : UserControl
{
    private static readonly TimeSpan ReleaseToEdgeDuration = TimeSpan.FromMilliseconds(200);

    public Observable<Rect> Clicked { get; }

    private Size ContainerSize => new(ActualWidth, ActualHeight);

    private Rect TouchRect => new(TouchBorder.Translation.X, TouchBorder.Translation.Y, TouchBorder.ActualWidth, TouchBorder.ActualHeight);

    public TouchControl()
    {
        InitializeComponent();
        TouchBorder.Translation = new(Shared.TouchSpacing, Shared.TouchSpacing, 0);

        var pressed = TouchBorder.Events().PointerPressed.Share();
        var dragStarted = TouchBorder.Events().ManipulationStarted.Share();
        var draggingStream = TouchBorder.Events().ManipulationDelta.Share();
        var dragEnded = TouchBorder.Events().ManipulationCompleted.Share();
        var containerSizeChanged = this.Events().SizeChanged.Share();
        var visibled = this.IsVisibleChanged.Where(visible => visible).AsUnitObservable().Share();
        var touchDocked = new Subject<Unit>();

        // 订阅拖动事件，更新位置
        draggingStream
            .Select(item => item.Delta.Translation)
            .Subscribe(delta =>
                TouchBorder.Translation += delta.ToVector3());

        // 订阅边界检查事件，超出边界则结束拖动
        draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                ContainerSize, TouchRect))
            .Subscribe(e => e.Complete());

        // 订阅拖动结束事件，执行停靠动画
        dragEnded
            .Select(_ => PositionCalculator.CalculateTouchDockedPosition(
                ContainerSize, TouchRect, Shared.TouchSpacing))
            .SubscribeAwait(async (finalPos, _) =>
            {
                var startOffset = new Point(TouchBorder.Translation.X - finalPos.X, TouchBorder.Translation.Y - finalPos.Y);
                TouchBorder.Translation = finalPos.ToVector3();

                await AnimationBuilder.Create()
                    .Translation(from: startOffset.ToVector2(), to: Vector2.Zero, duration: ReleaseToEdgeDuration)
                    .StartAsync(TouchBorder, CancellationToken.None);

                touchDocked.OnNext(Unit.Default);
            });

        // 订阅容器大小变化事件，动态调整触控位置以保持相对位置不变
        containerSizeChanged
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchRect, sizeEvent.NewSize, Shared.TouchSpacing))
            .Subscribe(rect =>
                 TouchBorder.Translation = new Point(rect.X, rect.Y).ToVector3());

        // 订阅透明度VSM状态变化事件
        this.Events().Loaded.AsUnitObservable()
            .Merge(visibled)
            .Merge(touchDocked)
            .Subscribe(_ => VisualStateManager.GoToState(this, "Faded", true));
        pressed
            .Subscribe(_ => VisualStateManager.GoToState(this, "Normal", true));

        // 定义对外暴露的 Clicked 流
        Clicked =
            pressed
            .Select(_ =>
                TouchBorder.Events().Tapped
                .TakeUntil(dragStarted))
            .Switch() // 处理拖动取消流和反复点击流
            .Select(_ => TouchRect)
            .Share();

        ObservableRegionResetRequested = pressed.AsUnitObservable();
        ObservableTouchRegionChanged =
            Observable.Merge(
                containerSizeChanged.AsUnitObservable(),
                touchDocked,
                visibled)
            .Select(_ => TouchRect);
    }
}
