using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TouchChanX.WinUI.Controls;

public sealed partial class TouchGlyph : Grid
{
    private Thickness TouchLayerMargin(double numerator, double denominator) =>
        new(this.Width * numerator / denominator);

    public TouchGlyph()
    {
        InitializeComponent();
    }
}
