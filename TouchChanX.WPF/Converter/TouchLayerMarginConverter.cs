using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Converter;

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度
/// </summary>
public class TouchLayerMarginConverter : MarkupExtension, IValueConverter
{
    public double Numerator { get; init; }

    public double Denominator { get; init; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double d)
            throw new InvalidOperationException();

        return d * Numerator / Denominator;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new InvalidOperationException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
