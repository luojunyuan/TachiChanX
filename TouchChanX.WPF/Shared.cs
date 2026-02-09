using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using R3.ObservableEvents;

namespace TouchChanX.WPF;

public static class Shared
{
    public const int TouchSpacing = 2;

    public const double TouchSize = 80.0;
    public const double MenuSize = TouchSize * 4;
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