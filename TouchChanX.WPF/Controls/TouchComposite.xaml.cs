using R3;
using R3.ObservableEvents;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Controls;

public partial class TouchComposite : Grid
{
    public TouchComposite()
    {
        InitializeComponent();

        PART_Root.Events().SizeChanged
            .Select(size => size.NewSize.Width)
            .DistinctUntilChanged()
            .Subscribe(width =>
            {
                LayerOne.Margin = new(width * 2 / 16);
                LayerTwo.Margin = new(width * 3 / 16);
                LayerThree.Margin = new(width * 4 / 16);
            });
    }
}

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度，仅用于设计时
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
