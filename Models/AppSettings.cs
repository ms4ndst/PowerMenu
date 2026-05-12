namespace PowerMenu.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Mocha";

    /// <summary>
    /// Win32 modifier flags: MOD_ALT=0x0001 MOD_CTRL=0x0002 MOD_SHIFT=0x0004 MOD_WIN=0x0008
    /// Default: Ctrl+Alt
    /// </summary>
    public uint HotkeyModifiers { get; set; } = 0x0003;

    /// <summary>Virtual-key code. Default: P (0x50)</summary>
    public int HotkeyVirtualKey { get; set; } = 0x50;

    public bool StartWithWindows { get; set; } = false;

    public string GetHotkeyDisplayString()
    {
        var parts = new List<string>();
        if ((HotkeyModifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((HotkeyModifiers & 0x0001) != 0) parts.Add("Alt");
        if ((HotkeyModifiers & 0x0008) != 0) parts.Add("Win");
        if ((HotkeyModifiers & 0x0004) != 0) parts.Add("Shift");
        parts.Add(VkToName(HotkeyVirtualKey));
        return string.Join("+", parts);
    }

    public static string VkToName(int vk) => vk switch
    {
        0x30 => "0", 0x31 => "1", 0x32 => "2", 0x33 => "3", 0x34 => "4",
        0x35 => "5", 0x36 => "6", 0x37 => "7", 0x38 => "8", 0x39 => "9",
        0x41 => "A", 0x42 => "B", 0x43 => "C", 0x44 => "D", 0x45 => "E",
        0x46 => "F", 0x47 => "G", 0x48 => "H", 0x49 => "I", 0x4A => "J",
        0x4B => "K", 0x4C => "L", 0x4D => "M", 0x4E => "N", 0x4F => "O",
        0x50 => "P", 0x51 => "Q", 0x52 => "R", 0x53 => "S", 0x54 => "T",
        0x55 => "U", 0x56 => "V", 0x57 => "W", 0x58 => "X", 0x59 => "Y",
        0x5A => "Z",
        0x70 => "F1",  0x71 => "F2",  0x72 => "F3",  0x73 => "F4",
        0x74 => "F5",  0x75 => "F6",  0x76 => "F7",  0x77 => "F8",
        0x78 => "F9",  0x79 => "F10", 0x7A => "F11", 0x7B => "F12",
        _ => $"VK_0x{vk:X2}"
    };
}
