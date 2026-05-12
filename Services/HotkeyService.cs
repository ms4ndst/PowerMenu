using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PowerMenu.Services;

public class HotkeyService : IDisposable
{
    private const int HotkeyId = 0xBEEF;
    private const int WmHotkey = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private HwndSource? _source;
    private IntPtr _hwnd = IntPtr.Zero;
    private Action? _onActivated;

    public bool IsRegistered { get; private set; }

    public void Initialize(Window helperWindow)
    {
        _source = HwndSource.FromHwnd(new WindowInteropHelper(helperWindow).EnsureHandle());
        _hwnd = _source.Handle;
        _source.AddHook(WndProc);
    }

    public bool Register(uint modifiers, int virtualKey, Action onActivated)
    {
        if (_hwnd == IntPtr.Zero) return false;

        // Unregister previous binding
        if (IsRegistered)
            UnregisterHotKey(_hwnd, HotkeyId);

        _onActivated = onActivated;
        IsRegistered = RegisterHotKey(_hwnd, HotkeyId, modifiers, (uint)virtualKey);
        return IsRegistered;
    }

    public void Unregister()
    {
        if (IsRegistered && _hwnd != IntPtr.Zero)
        {
            UnregisterHotKey(_hwnd, HotkeyId);
            IsRegistered = false;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            _onActivated?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Unregister();
        _source?.RemoveHook(WndProc);
        _source?.Dispose();
    }
}
