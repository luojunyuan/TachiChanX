using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace TouchChanX.Win32.Interop;

/// <summary>
/// 对 Win32 Api 的业务逻辑封装
/// </summary>
public static partial class OsPlatformApi
{
    /// <summary>
    /// 发送 Alt + Enter 消息
    /// </summary>
    public static void SendAltEnter(IntPtr hwnd)
    {
        const uint WM_SYSKEYDOWN = 0x0104;
        const uint WM_SYSKEYUP = 0x0105;
        const int VK_RETURN = 0x0D;
        const int VK_MENU = 0x12; // Alt key

        PInvoke.SendMessage(new(hwnd), WM_SYSKEYDOWN, VK_MENU, nint.Zero);
        PInvoke.SendMessage(new(hwnd), WM_SYSKEYDOWN, VK_RETURN, 0x20000000);
        PInvoke.SendMessage(new(hwnd), WM_SYSKEYUP, VK_RETURN, 0x20000000);
        PInvoke.SendMessage(new(hwnd), WM_SYSKEYUP, VK_MENU, nint.Zero);
    }

    /// <summary>
    /// 判断进程对象是否对 DPI 不感知
    /// </summary>
    [SupportedOSPlatform("windows8.1")]
    public static bool IsDpiUnaware(Process process)
    {
        var result = PInvoke.GetProcessDpiAwareness(process.SafeHandle, out var awareType);

        return result == 0 && (awareType == 0 || awareType == PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE);
    }

    /// <summary>
    /// 获取窗口所在显示器的 raw DPI
    /// </summary>
    [SupportedOSPlatform("windows8.1")]
    public static uint GetDpiForWindowsMonitor(nint hwnd)
    {
        var monitorHandle = PInvoke.MonitorFromWindow(new(hwnd), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
        PInvoke.GetDpiForMonitor(monitorHandle, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _);
        return dpiX;
    }
}
