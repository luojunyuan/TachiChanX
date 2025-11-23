using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Converter;

public class CornerRadiusToCircleConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double width)
            throw new InvalidOperationException();

        return new CornerRadius(width / 2.0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new InvalidOperationException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
