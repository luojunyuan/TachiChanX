using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace TouchChanX;

public static class Converters
{
    public static FuncValueConverter<double, CornerRadius> RadiusToCircleConverter { get; } =
        new FuncValueConverter<double, CornerRadius>(num => new CornerRadius(num / 2));
}

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度
/// </summary>
partial class TouchLayerMarginConverter : IValueConverter
{
    /// <param name="parameter">指示该层应缩放宽度的倍率因子</param>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double number && parameter is string factorString && TryParseFraction(factorString, out double factor))
        {
            return new Thickness(number * factor);
        }

        throw new InvalidOperationException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new InvalidOperationException();

    private static bool TryParseFraction(string fraction, out double factor)
    {
        string[] parts = fraction.Split('/');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], out double numerator) &&
            double.TryParse(parts[1], out double denominator))
        {
            factor = numerator / denominator;
            return true;
        }

        factor = default;
        return false;
    }
}
