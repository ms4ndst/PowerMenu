using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PowerMenu.Services;

public static class PowerService
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool LockWorkStation();

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private const uint WM_SYSCOMMAND = 0x0112;
    private const int SC_SCREENSAVE  = 0xF140;

    [DllImport("Powrprof.dll", SetLastError = true)]
    private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

    public static void Lock()
    {
        SendMessage(GetDesktopWindow(), WM_SYSCOMMAND, (IntPtr)SC_SCREENSAVE, IntPtr.Zero);
        LockWorkStation();
    }

    public static void Sleep() => SetSuspendState(false, false, false);

    public static void Hibernate() => SetSuspendState(true, false, false);

    public static void Shutdown() =>
        Process.Start(new ProcessStartInfo("shutdown", "/s /t 5 /c \"PowerMenu shutdown\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });

    public static void CancelShutdown() =>
        Process.Start(new ProcessStartInfo("shutdown", "/a")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });
}
