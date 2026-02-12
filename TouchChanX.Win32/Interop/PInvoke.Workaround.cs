using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32;

internal static partial class PInvoke
{
    // https://github.com/microsoft/CsWin32/issues/528

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static partial int SetWindowLong32(nint hWnd, int nIndex, int dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static partial nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    /// <inheritdoc cref="SetWindowLong(HWND, WINDOW_LONG_PTR_INDEX, int)" />
    public static nint SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong) =>
        nint.Size == 8
            ? SetWindowLongPtr64(hWnd, (int)nIndex, dwNewLong)
            : unchecked(SetWindowLong32(hWnd, (int)nIndex, (int)dwNewLong));
}
