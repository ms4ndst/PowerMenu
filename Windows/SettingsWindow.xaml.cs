using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PowerMenu.Models;
using PowerMenu.Services;
using Button = System.Windows.Controls.Button;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Colors = System.Windows.Media.Colors;

namespace PowerMenu.Windows;

public partial class SettingsWindow : Window
{
    private static readonly (string Label, int VK)[] Keys =
    [
        ("A",0x41),("B",0x42),("C",0x43),("D",0x44),("E",0x45),
        ("F",0x46),("G",0x47),("H",0x48),("I",0x49),("J",0x4A),
        ("K",0x4B),("L",0x4C),("M",0x4D),("N",0x4E),("O",0x4F),
        ("P",0x50),("Q",0x51),("R",0x52),("S",0x53),("T",0x54),
        ("U",0x55),("V",0x56),("W",0x57),("X",0x58),("Y",0x59),
        ("Z",0x5A),
        ("F1",0x70),("F2",0x71),("F3",0x72),("F4",0x73),("F5",0x74),
        ("F6",0x75),("F7",0x76),("F8",0x77),("F9",0x78),("F10",0x79),
        ("F11",0x7A),("F12",0x7B),
        ("0",0x30),("1",0x31),("2",0x32),("3",0x33),("4",0x34),
        ("5",0x35),("6",0x36),("7",0x37),("8",0x38),("9",0x39),
    ];

    private readonly SettingsService _settingsService;
    private AppSettings _settings;
    private string _selectedTheme;

    public SettingsWindow()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.Load();
        _selectedTheme = _settings.Theme;

        InitializeComponent();
        PopulateKeyCombo();
        PopulateThemeButtons();
        LoadCurrentValues();
    }

    private void PopulateKeyCombo()
    {
        foreach (var (label, _) in Keys)
            KeyCombo.Items.Add(label);
    }

    private void PopulateThemeButtons()
    {
        foreach (var flavour in CatppuccinFlavour.All)
        {
            var btn = new Button
            {
                Content = flavour.Name,
                Style = (Style)Resources["ThemeButton"],
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(flavour.Base)!),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(flavour.Text)!),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                Margin = new Thickness(4),
                Tag = flavour.Name,
            };

            // Build four swatch colours shown on the button
            var swatchColors = new[]
            {
                flavour.Mauve, flavour.Blue, flavour.Green, flavour.Peach
            };

            btn.Loaded += (_, _) =>
            {
                // Walk the visual tree to find the Swatch rectangles and colour them
                var template = btn.Template;
                for (int i = 1; i <= 4; i++)
                {
                    if (template.FindName($"Swatch{i}", btn) is System.Windows.Shapes.Rectangle rect)
                        rect.Fill = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString(swatchColors[i - 1])!);
                }
            };

            HighlightThemeButton(btn, flavour.Name == _selectedTheme);

            btn.Click += (_, _) =>
            {
                _selectedTheme = flavour.Name;
                App.ThemeService.Apply(_selectedTheme);
                foreach (Button b in ThemeGrid.Children)
                    HighlightThemeButton(b, (string)b.Tag == _selectedTheme);
            };

            ThemeGrid.Children.Add(btn);
        }
    }

    private void HighlightThemeButton(Button btn, bool selected)
    {
        var flavour = CatppuccinFlavour.All.First(f => f.Name == (string)btn.Tag);
        btn.BorderBrush = selected
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(flavour.Mauve)!)
            : new SolidColorBrush(Colors.Transparent);
    }

    private void LoadCurrentValues()
    {
        ModCtrl.IsChecked  = (_settings.HotkeyModifiers & 0x0002) != 0;
        ModAlt.IsChecked   = (_settings.HotkeyModifiers & 0x0001) != 0;
        ModShift.IsChecked = (_settings.HotkeyModifiers & 0x0004) != 0;
        ModWin.IsChecked   = (_settings.HotkeyModifiers & 0x0008) != 0;

        var keyLabel = AppSettings.VkToName(_settings.HotkeyVirtualKey);
        var idx = Array.FindIndex(Keys, k => k.Label == keyLabel);
        KeyCombo.SelectedIndex = idx >= 0 ? idx : 0;

        StartupCheck.IsChecked = _settings.StartWithWindows;
        UpdateHotkeyLabel();

        ModCtrl.Checked   += (_, _) => UpdateHotkeyLabel();
        ModCtrl.Unchecked += (_, _) => UpdateHotkeyLabel();
        ModAlt.Checked    += (_, _) => UpdateHotkeyLabel();
        ModAlt.Unchecked  += (_, _) => UpdateHotkeyLabel();
        ModShift.Checked  += (_, _) => UpdateHotkeyLabel();
        ModShift.Unchecked+= (_, _) => UpdateHotkeyLabel();
        ModWin.Checked    += (_, _) => UpdateHotkeyLabel();
        ModWin.Unchecked  += (_, _) => UpdateHotkeyLabel();
    }

    private void KeyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateHotkeyLabel();

    private void UpdateHotkeyLabel()
    {
        var parts = new List<string>();
        if (ModCtrl.IsChecked  == true) parts.Add("Ctrl");
        if (ModAlt.IsChecked   == true) parts.Add("Alt");
        if (ModWin.IsChecked   == true) parts.Add("Win");
        if (ModShift.IsChecked == true) parts.Add("Shift");
        if (KeyCombo.SelectedIndex >= 0)
            parts.Add(Keys[KeyCombo.SelectedIndex].Label);
        CurrentHotkeyLabel.Text = parts.Count > 1 ? string.Join("+", parts) : "(no shortcut)";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        uint mods = 0;
        if (ModCtrl.IsChecked  == true) mods |= 0x0002;
        if (ModAlt.IsChecked   == true) mods |= 0x0001;
        if (ModShift.IsChecked == true) mods |= 0x0004;
        if (ModWin.IsChecked   == true) mods |= 0x0008;

        _settings.HotkeyModifiers  = mods;
        _settings.HotkeyVirtualKey = KeyCombo.SelectedIndex >= 0
            ? Keys[KeyCombo.SelectedIndex].VK : 0x50;
        _settings.Theme            = _selectedTheme;
        _settings.StartWithWindows = StartupCheck.IsChecked == true;

        _settingsService.Save(_settings);

        ApplyStartupSetting(_settings.StartWithWindows);

        // Re-register the hotkey with new settings
        ((App)App.Current).ApplySettings(_settings);

        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        // Revert theme preview
        App.ThemeService.Apply(_settings.Theme);
        Close();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e) =>
        Cancel_Click(sender, e);

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private static void ApplyStartupSetting(bool enable)
    {
        const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key is null) return;
        if (enable)
            key.SetValue("PowerMenu", $"\"{Environment.ProcessPath}\"");
        else
            key.DeleteValue("PowerMenu", throwOnMissingValue: false);
    }
}
