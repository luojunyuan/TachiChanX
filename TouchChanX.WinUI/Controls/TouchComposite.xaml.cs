using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TouchChanX.WinUI.Controls;

public sealed partial class TouchComposite : Grid
{
    private Thickness TouchLayerMargin(double numerator, double denominator) =>
        new(this.Width * numerator / denominator);

    public TouchComposite()
    {
        InitializeComponent();
    }
}
