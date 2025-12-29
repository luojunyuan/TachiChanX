using R3;
using R3.ObservableEvents;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace TouchChanX.WPF.Controls;

public record LayerConfig(double Numerator, double Denominator)
{
    public double Ratio => Numerator / Denominator;
}

public partial class TouchComposite : Grid
{
    public static readonly IReadOnlyList<LayerConfig> Layers =
    [
        new LayerConfig(2, 16),
        new LayerConfig(3, 16),
        new LayerConfig(4, 16),
    ];

    public TouchComposite()
    {
        InitializeComponent();

        PART_Root.Events().SizeChanged
            .Select(size => size.NewSize.Width)
            .DistinctUntilChanged()
            .Subscribe(width =>
            {
                LayerOne.Margin = new(width * Layers[0].Ratio);
                LayerTwo.Margin = new(width * Layers[1].Ratio);
                LayerThree.Margin = new(width * Layers[2].Ratio);
            });
    }
}

/// <summary>
/// 自动计算 Touch 每层圆点所占据宽度，仅用于设计时
/// </summary>
public class TouchLayerMarginConverter : MarkupExtension, IValueConverter
{
    public int LayerIndex { get; init; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double d)
            return Binding.DoNothing;

        return d * TouchComposite.Layers[LayerIndex].Ratio;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
