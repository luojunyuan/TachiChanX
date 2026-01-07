using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Controls;

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度
/// </summary>
public class TouchLayerMarginConverter : MarkupExtension, IValueConverter
{
    public double Numerator { get; init; }

    public double Denominator { get; init; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not double d
            ? Binding.DoNothing
            : d * Numerator / Denominator;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
