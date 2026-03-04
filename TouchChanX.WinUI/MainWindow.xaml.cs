using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using R3;
using R3.ObservableEvents;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TouchChanX.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //Touch.Clicked.Subscribe(_ => Debug.WriteLine("Tapped"));
    }
}

public static class Shared
{
    public const int TouchSpacing = 2;

    public const double TouchSize = 80.0;
    public const double MenuSize = TouchSize * 4;

    extension(FrameworkElement element)
    {
        public Observable<SizeChangedEventArgs> ObserveParentSize() =>
            element.Events().Loaded
            .Select(_ => VisualTreeHelper.GetParent(element).Required<FrameworkElement>())
            .SelectMany(p => p.Events().SizeChanged);
    }

    [StackTraceHidden]
    public static T Required<T>(this object? obj,
       string? errorMessage = null,
       [CallerArgumentExpression(nameof(obj))]
        string? paramName = null)
       => (T)(obj ?? throw new ArgumentNullException(paramName, errorMessage));
}
