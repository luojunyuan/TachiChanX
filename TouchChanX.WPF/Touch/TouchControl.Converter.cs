using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Touch;

/// <summary>
/// 将宽度转换为圆形的 CornerRadius
/// </summary>
public class CornerRadiusToCircleConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        => value is not double width 
            ? Binding.DoNothing
            : new CornerRadius(width / 2.0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
