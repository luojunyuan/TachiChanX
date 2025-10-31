using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace TouchChanX.Ava;

public static class Shared
{
    public static class Constants
    {
        public const int TouchSpacing = 2;
    }
    
    public static FuncValueConverter<double, CornerRadius> RadiusToCircleConverter { get; } =
        new(num => new CornerRadius(num / 2));

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

        public static TouchDockAnchor NewLeft(double scale) => new Left(scale);
        public static TouchDockAnchor NewTop(double scale) => new Top(scale);
        public static TouchDockAnchor NewRight(double scale) => new Right(scale);
        public static TouchDockAnchor NewBottom(double scale) => new Bottom(scale);
        public static TouchDockAnchor NewTopLeft() => new TopLeft();
        public static TouchDockAnchor NewTopRight() => new TopRight();
        public static TouchDockAnchor NewBottomLeft() => new BottomLeft();
        public static TouchDockAnchor NewBottomRight() => new BottomRight();

        public static TouchDockAnchor Default { get; } = new Left(0.5);
        
        public static TouchDockAnchor FromRect(Size containerSize, Rect touchRect)
        {
            const int spacing = Constants.TouchSpacing;
        
            var right = containerSize.Width - spacing - touchRect.Width;
            var bottom = containerSize.Height - spacing - touchRect.Height;
        
            var x = touchRect.X;
            var y = touchRect.Y;
        
            return (x, y) switch
            {
                (spacing, spacing) => NewTopLeft(),
                (spacing, var py) when IsSnapped(py, bottom) => NewBottomLeft(),
                var (px, py) when IsSnapped(px, right) && IsSnapped(py, spacing) => NewTopRight(),
                var (px, py) when IsSnapped(px, right) && IsSnapped(py, bottom) => NewBottomRight(),

                (spacing, var py) => 
                    NewLeft((py + spacing + touchRect.Height / 2.0) / containerSize.Height),
                (var px, spacing) => 
                    NewTop((px + spacing + touchRect.Width / 2.0) / containerSize.Width),
                var (px, py) when IsSnapped(px, right) => 
                    NewRight((py + spacing + touchRect.Height / 2.0) / containerSize.Height),
                var (px, py) when IsSnapped(py, bottom) => 
                    NewBottom((px + spacing + touchRect.Width / 2.0) / containerSize.Width),

                _ => Default
            };
            
            static bool IsSnapped(double value, double target, double tolerance = 0.01d) => 
                Math.Abs(value - target) <= tolerance;
        }
    }
}

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度
/// </summary>
public class TouchLayerMarginConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // parameter 指示该层应缩放宽度的倍率因子
        if (value is double number && parameter is string factorString && TryParseFraction(factorString, out var factor))
        {
            return new Thickness(number * factor);
        }

        throw new InvalidOperationException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new InvalidOperationException();

    private static bool TryParseFraction(string fraction, out double factor)
    {
        var parts = fraction.Split('/');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], out var numerator) &&
            double.TryParse(parts[1], out var denominator))
        {
            factor = numerator / denominator;
            return true;
        }

        factor = 0;
        return false;
    }
}