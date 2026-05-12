using System.Windows;
using PowerMenu.Models;
using Application = System.Windows.Application;

namespace PowerMenu.Services;

public class ThemeService
{
    private static readonly Dictionary<string, Uri> ThemeUris = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Mocha"]     = new Uri("Resources/Themes/Mocha.xaml",     UriKind.Relative),
        ["Macchiato"] = new Uri("Resources/Themes/Macchiato.xaml", UriKind.Relative),
        ["Frappe"]    = new Uri("Resources/Themes/Frappe.xaml",    UriKind.Relative),
        ["Latte"]     = new Uri("Resources/Themes/Latte.xaml",     UriKind.Relative),
    };

    public static IReadOnlyList<string> AvailableThemes => [.. ThemeUris.Keys];

    public void Apply(string themeName)
    {
        if (!ThemeUris.TryGetValue(themeName, out var uri))
            uri = ThemeUris["Mocha"];

        var merged = Application.Current.Resources.MergedDictionaries;

        // Remove any existing theme dictionary
        var existing = merged.FirstOrDefault(d => d.Source != null &&
            d.Source.OriginalString.Contains("Themes/"));
        if (existing != null)
            merged.Remove(existing);

        merged.Add(new ResourceDictionary { Source = uri });
    }
}
