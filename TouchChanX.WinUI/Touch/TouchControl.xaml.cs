using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using R3;
using R3.ObservableEvents;
using System.Numerics;
using Windows.Foundation;

namespace TouchChanX.WinUI.Touch;

public sealed partial class TouchControl : UserControl
{
    public static CornerRadius CircleCornerRadius(double width) => new(width / 2);

    public Observable<Unit> Clicked { get; }

    private Size ContainerSize => new(ActualWidth, ActualHeight);

    private Rect TouchRect => new(TouchTransform.TranslateX, TouchTransform.TranslateY, TouchBorder.ActualWidth, TouchBorder.ActualHeight);

    public TouchControl()
    {
        InitializeComponent();

        TouchBorder.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        (TouchTransform.TranslateX, TouchTransform.TranslateY) =
            (Shared.TouchSpacing, Shared.TouchSpacing);

        var pressed = TouchBorder.Events().PointerPressed.Share();
        var dragStarted = TouchBorder.Events().ManipulationStarted.Share();
        var draggingStream = TouchBorder.Events().ManipulationDelta.Share();
        var dragEnded = TouchBorder.Events().ManipulationCompleted.Share();

        // 订阅拖动事件，更新位置
        draggingStream
            .Select(item => item.Delta.Translation)
            .Subscribe(delta =>
            {
                TouchTransform.TranslateX += delta.X;
                TouchTransform.TranslateY += delta.Y;
            });

        // 订阅边界检查事件，超出边界则结束拖动
        draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                ContainerSize, TouchRect))
            .Subscribe(e => e.Complete());

        // 订阅拖动结束事件，执行停靠动画
        dragEnded
            .Select(_ => PositionCalculator.CalculateTouchDockedPosition(
                ContainerSize, TouchRect, Shared.TouchSpacing))
            .Subscribe(finalPos =>
                AnimateTouchToEdge(finalPos, TouchTransform));

        // 订阅父容器大小变化事件，动态调整触控位置以保持相对位置不变
        this.ObserveParentSize()
            .Select(sizeEvent => PositionCalculator.CalculateNewDockedPosition(
                sizeEvent.PreviousSize, TouchRect, sizeEvent.NewSize, Shared.TouchSpacing))
            .Subscribe(rect =>
                (TouchTransform.TranslateX, TouchTransform.TranslateY) =
                    (rect.X, rect.Y));

        // 订阅透明度VSM状态变化事件
        this.Events().Loaded.AsUnitObservable()
            .Merge(dragEnded.AsUnitObservable())
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
            .AsUnitObservable()
            .Share();
    }
}

public static class PointerExtesions
{
    extension(PointerRoutedEventArgs pointerEvent)
    {
        public Point GetPosition(UIElement? visual = null) =>
            pointerEvent.GetCurrentPoint(visual).Position;
    }

    extension(Point)
    {
        public static Vector2 operator -(Point p1, Point p2) =>
           p1.ToVector2() - p2.ToVector2();
    }
}
