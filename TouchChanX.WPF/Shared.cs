using R3;
using R3.ObservableEvents;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace TouchChanX.WPF;

public static class Shared
{
    public const int TouchSpacing = 2;

    public const double TouchSize = 80.0;
    public const double MenuSize = TouchSize * 4;

    extension(FrameworkElement element)
    {
        public Observable<Size> ObserveParentSize() => 
            element.Events().Loaded
            .Select(_ => VisualTreeHelper.GetParent(element).Required<FrameworkElement>())
            .SelectMany(p =>
                p.Events().SizeChanged
                .Select(e => e.NewSize)
                .Prepend(new Size(p.ActualWidth, p.ActualHeight)));
    }
}

public static class ThrowHelper
{
    [StackTraceHidden]
    public static T Required<T>(this object? obj,
        string? errorMessage = null,
        [CallerArgumentExpression(nameof(obj))]
        string? paramName = null)
        => (T)(obj ?? throw new ArgumentNullException(paramName, errorMessage));
}

public static class DummyHelper
{
    private static void DummyFunc() => _ = ((FrameworkElement)null!).Events();
}