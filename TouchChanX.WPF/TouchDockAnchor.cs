using System.Windows;

namespace TouchChanX.WPF;

public abstract record TouchDockAnchor
{
    public record Left(double Scale) : TouchDockAnchor;
    public record Top(double Scale) : TouchDockAnchor;
    public record Right(double Scale) : TouchDockAnchor;
    public record Bottom(double Scale) : TouchDockAnchor;
    public record TopLeft : TouchDockAnchor;
    public record TopRight : TouchDockAnchor;
    public record BottomLeft : TouchDockAnchor;
    public record BottomRight : TouchDockAnchor;

    public static TouchDockAnchor Default { get; } = new Left(0.5);

    public static TouchDockAnchor SnapFromRect(Size containerSize, Rect touchRect)
    {
        const double tolerance = 0.01d;
        double spacing = Shared.TouchSpacing;

        var isAtLeft = IsSnapped(touchRect.X, spacing);
        var isAtTop = IsSnapped(touchRect.Y, spacing);
        var isAtRight = IsSnapped(touchRect.X, containerSize.Width - spacing - touchRect.Width);
        var isAtBottom = IsSnapped(touchRect.Y, containerSize.Height - spacing - touchRect.Height);

        return (isAtLeft, isAtTop, isAtRight, isAtBottom) switch
        {
            (true, true, _, _) => new TopLeft(),
            (true, _, _, true) => new BottomLeft(),
            (_, true, true, _) => new TopRight(),
            (_, _, true, true) => new BottomRight(),

            (true, _, _, _) => new Left(GetVerticalScale()),
            (_, true, _, _) => new Top(GetHorizontalScale()),
            (_, _, true, _) => new Right(GetVerticalScale()),
            (_, _, _, true) => new Bottom(GetHorizontalScale()),

            _ => Default
        };

        double GetVerticalScale() => (touchRect.Y + spacing + touchRect.Height / 2.0) / containerSize.Height;
        double GetHorizontalScale() => (touchRect.X + spacing + touchRect.Width / 2.0) / containerSize.Width;
        bool IsSnapped(double v, double t) => Math.Abs(v - t) <= tolerance;
    }
}
