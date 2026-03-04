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

    private Size ContainerSize => new(ActualWidth, ActualHeight);

    private Rect TouchRect => new(TouchTransform.X, TouchTransform.Y, TouchBorder.ActualWidth, TouchBorder.ActualHeight);

    public TouchControl()
    {
        InitializeComponent();

        TouchBorder.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;

        var draggingStream =
            TouchBorder.Events().ManipulationDelta
            .Select(e => new
            {
                Delta = e.Delta.Translation,
                Args = e,
            })
            .Share();

        draggingStream
            .Select(item => item.Delta)
            .Subscribe(delta =>
            {
                TouchTransform.X += delta.X;
                TouchTransform.Y += delta.Y;
            });

        draggingStream
            .Where(item => PositionCalculator.IsBeyondBoundary(
                ContainerSize, TouchRect))
            .Select(item => item.Args)
            .Subscribe(e => e.Complete());

        TouchBorder.Events().ManipulationCompleted
            .Select(_ => PositionCalculator.CalculateTouchDockedPosition(
                ContainerSize, TouchRect, 2))
            .Subscribe(finalPos =>
                (TouchTransform.X, TouchTransform.Y) = (finalPos.X, finalPos.Y));
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
