using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PowerMenu.Services;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using Point = System.Windows.Point;

namespace PowerMenu.Windows;

public partial class PopupWindow : Window
{
    public PopupWindow()
    {
        InitializeComponent();

        // Set scale transform origin for the card animation
        Card.RenderTransformOrigin = new Point(0.5, 0.5);
        Card.RenderTransform = new ScaleTransform(1, 1);
        
        // Ensure focus is set when window is activated
        Activated += Window_Activated;
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        // Set focus to Lock button when window becomes active
        LockButton.Focus();
    }

    public void SetHotkeyHint(string hint) =>
        HotkeyHint.Text = hint;

    public void SetSubtitle(string text) =>
        SubtitleText.Text = text;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Pre-select Lock so the user can Tab through or press Enter immediately
        LockButton.Focus();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            CloseWithAnimation();
    }

    private void Backdrop_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Only close when clicking the backdrop, not the card
        if (e.Source == Backdrop)
            CloseWithAnimation();
    }

    private void Card_MouseDown(object sender, MouseButtonEventArgs e) =>
        e.Handled = true;

    private void Close_Click(object sender, RoutedEventArgs e) =>
        CloseWithAnimation();

    private void Lock_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
        PowerService.Lock();
    }

    private void Sleep_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
        PowerService.Sleep();
    }

    private void Hibernate_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
        PowerService.Hibernate();
    }

    private void Shutdown_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
        PowerService.Shutdown();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        CloseWithAnimation();
        var settings = new SettingsWindow();
        settings.Show();
    }

    private void CloseWithAnimation()
    {
        // Simple fade-out before closing
        var anim = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(100));
        anim.Completed += (_, _) => Close();
        Card.BeginAnimation(OpacityProperty, anim);
    }
}
