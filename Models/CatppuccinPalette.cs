namespace PowerMenu.Models;

public record CatppuccinColor(string Name, string Hex);

public record CatppuccinFlavour(
    string Name,
    string Rosewater, string Flamingo, string Pink, string Mauve,
    string Red, string Maroon, string Peach, string Yellow,
    string Green, string Teal, string Sky, string Sapphire, string Blue, string Lavender,
    string Text, string Subtext1, string Subtext0,
    string Overlay2, string Overlay1, string Overlay0,
    string Surface2, string Surface1, string Surface0,
    string Base, string Mantle, string Crust)
{
    public static readonly CatppuccinFlavour Mocha = new(
        "Mocha",
        "#f5e0dc", "#f2cdcd", "#f5c2e7", "#cba6f7",
        "#f38ba8", "#eba0ac", "#fab387", "#f9e2af",
        "#a6e3a1", "#94e2d5", "#89dceb", "#74c7ec", "#89b4fa", "#b4befe",
        "#cdd6f4", "#bac2de", "#a6adc8",
        "#9399b2", "#7f849c", "#6c7086",
        "#585b70", "#45475a", "#313244",
        "#1e1e2e", "#181825", "#11111b");

    public static readonly CatppuccinFlavour Macchiato = new(
        "Macchiato",
        "#f4dbd6", "#f0c6c6", "#f5bde6", "#c6a0f6",
        "#ed8796", "#ee99a0", "#f5a97f", "#eed49f",
        "#a6da95", "#8bd5ca", "#91d7e3", "#7dc4e4", "#8aadf4", "#b7bdf8",
        "#cad3f5", "#b8c0e0", "#a5adcb",
        "#939ab7", "#8087a2", "#6e738d",
        "#5b6078", "#494d64", "#363a4f",
        "#24273a", "#1e2030", "#181926");

    public static readonly CatppuccinFlavour Frappe = new(
        "Frappé",
        "#f2d5cf", "#eebebe", "#f4b8e4", "#ca9ee6",
        "#e78284", "#ea999c", "#ef9f76", "#e5c890",
        "#a6d189", "#81c8be", "#99d1db", "#85c1dc", "#8caaee", "#babbf1",
        "#c6d0f5", "#b5bfe2", "#a5adce",
        "#949cbb", "#838ba7", "#737994",
        "#626880", "#51576d", "#414559",
        "#303446", "#292c3c", "#232634");

    public static readonly CatppuccinFlavour Latte = new(
        "Latte",
        "#dc8a78", "#dd7878", "#ea76cb", "#8839ef",
        "#d20f39", "#e64553", "#fe640b", "#df8e1d",
        "#40a02b", "#179299", "#04a5e5", "#209fb5", "#1e66f5", "#7287fd",
        "#4c4f69", "#5c5f77", "#6c6f85",
        "#7c7f93", "#8c8fa1", "#9ca0b0",
        "#acb0be", "#bcc0cc", "#ccd0da",
        "#eff1f5", "#e6e9ef", "#dce0e8");

    public static IReadOnlyList<CatppuccinFlavour> All { get; } =
        [Mocha, Macchiato, Frappe, Latte];
}
