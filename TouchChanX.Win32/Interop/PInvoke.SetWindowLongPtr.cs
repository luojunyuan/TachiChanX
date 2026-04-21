using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32;

internal static partial class PInvoke // SetWindowLong build for AnyCPU, win-x86
{
    // https://github.com/microsoft/CsWin32/issues/528
    // 如果不指定 64 位 runtime architecture，CsWin32 就不会帮助生成 SetWindowLongPtr
    // 我们不使用 NativeMethods 来为我们生成这个 PInvoke
    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static partial nint SetWindowLong64(nint hWnd, int nIndex, nint dwNewLong);

    public static nint SetWindowLongPtr(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, nint dwNewLong) =>
        Environment.Is64BitProcess
            ? SetWindowLong64(hWnd, (int)nIndex, dwNewLong)
            : unchecked(SetWindowLong(hWnd, nIndex, (int)dwNewLong));
}
