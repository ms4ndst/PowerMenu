using System.Windows;
using System.Windows.Forms;
using PowerMenu.Models;
using PowerMenu.Services;
using PowerMenu.Windows;

namespace PowerMenu;

public partial class App : System.Windows.Application
{
    public static readonly ThemeService ThemeService = new();
    private readonly SettingsService _settingsService = new();
    private readonly HotkeyService _hotkeyService = new();
    private NotifyIcon? _tray;
    private HiddenWindow? _hotkeyHost;
    private AppSettings _settings = new();
    private PopupWindow? _popup;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settings = _settingsService.Load();
        ThemeService.Apply(_settings.Theme);

        _hotkeyHost = new HiddenWindow();
        _hotkeyHost.Show();
        _hotkeyHost.Hide();

        _hotkeyService.Initialize(_hotkeyHost);
        RegisterHotkey();

        SetupTray();
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        ThemeService.Apply(_settings.Theme);
        RegisterHotkey();
    }

    private void RegisterHotkey()
    {
        bool ok = _hotkeyService.Register(
            _settings.HotkeyModifiers,
            _settings.HotkeyVirtualKey,
            ShowPopup);

        if (!ok)
        {
            System.Windows.MessageBox.Show(
                $"Could not register hotkey {_settings.GetHotkeyDisplayString()}.\n" +
                "Another application may be using it. Open Settings to choose a different shortcut.",
                "PowerMenu – Hotkey Conflict",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ShowPopup()
    {
        if (_popup != null)
        {
            _popup.Close();
            _popup = null;
            return;
        }

        _popup = new PopupWindow();
        _popup.SetHotkeyHint(_settings.GetHotkeyDisplayString());
        _popup.SetSubtitle("Choose a power action or press Esc to cancel.");
        _popup.Closed += (_, _) => _popup = null;
        _popup.Show();
        _popup.Activate();
    }

    private void SetupTray()
    {
        _tray = new NotifyIcon
        {
            Icon = GetAppIcon(),
            Text = "PowerMenu",
            Visible = true,
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Menu", null, (_, _) => ShowPopup());
        menu.Items.Add("Settings",  null, (_, _) => new SettingsWindow().Show());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _tray.ContextMenuStrip = menu;
        _tray.DoubleClick += (_, _) => ShowPopup();
    }

    private static System.Drawing.Icon GetAppIcon()
    {
        // Generate a simple power-symbol icon programmatically
        var bmp = new System.Drawing.Bitmap(32, 32);
        using var g = System.Drawing.Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(System.Drawing.Color.Transparent);

        // Circle background
        using var bgBrush = new System.Drawing.SolidBrush(
            System.Drawing.Color.FromArgb(255, 30, 30, 46));
        g.FillEllipse(bgBrush, 0, 0, 31, 31);

        // Power arc
        using var arcPen = new System.Drawing.Pen(
            System.Drawing.Color.FromArgb(255, 203, 166, 247), 2.5f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap   = System.Drawing.Drawing2D.LineCap.Round
        };
        g.DrawArc(arcPen, 7, 7, 18, 18, -60, 300);

        // Vertical line
        g.DrawLine(arcPen, 16f, 6f, 16f, 14f);

        return System.Drawing.Icon.FromHandle(bmp.GetHicon());
    }

    private void ExitApp()
    {
        _hotkeyService.Dispose();
        _tray?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeyService.Dispose();
        _tray?.Dispose();
        base.OnExit(e);
    }
}

/// <summary>Invisible WPF window used solely to receive WM_HOTKEY messages.</summary>
internal sealed class HiddenWindow : Window
{
    public HiddenWindow()
    {
        Width = 0; Height = 0;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        Opacity = 0;
        IsHitTestVisible = false;
    }
}
