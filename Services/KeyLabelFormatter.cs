namespace KeyViz.Services;

internal static class KeyLabelFormatter
{
    private static readonly int[] ControlKeys = [0x11, 0xA2, 0xA3];
    private static readonly int[] ShiftKeys = [0x10, 0xA0, 0xA1];
    private static readonly int[] AltKeys = [0x12, 0xA4, 0xA5];
    private static readonly int[] WindowsKeys = [0x5B, 0x5C];

    internal static string FormatChord(IReadOnlySet<int> pressedKeys, int currentKey)
    {
        var labels = new List<string>(5);

        AddModifier(labels, pressedKeys, ControlKeys, "Ctrl");
        AddModifier(labels, pressedKeys, ShiftKeys, "Shift");
        AddModifier(labels, pressedKeys, AltKeys, "Alt");
        AddModifier(labels, pressedKeys, WindowsKeys, "Win");

        if (!IsModifier(currentKey))
        {
            labels.Add(GetKeyLabel(currentKey));
        }

        return labels.Count > 0 ? string.Join(" + ", labels) : GetKeyLabel(currentKey);
    }

    private static void AddModifier(
        ICollection<string> labels,
        IReadOnlySet<int> pressedKeys,
        IEnumerable<int> keys,
        string label)
    {
        if (keys.Any(pressedKeys.Contains))
        {
            labels.Add(label);
        }
    }

    internal static bool IsModifier(int key)
    {
        return ControlKeys.Contains(key)
            || ShiftKeys.Contains(key)
            || AltKeys.Contains(key)
            || WindowsKeys.Contains(key);
    }

    internal static bool HasShortcutModifier(IReadOnlySet<int> pressedKeys)
    {
        return ControlKeys.Any(pressedKeys.Contains)
            || AltKeys.Any(pressedKeys.Contains)
            || WindowsKeys.Any(pressedKeys.Contains);
    }

    internal static string GetKeyLabel(int key)
    {
        if (key is >= 0x41 and <= 0x5A)
        {
            return ((char)key).ToString();
        }

        if (key is >= 0x30 and <= 0x39)
        {
            return ((char)key).ToString();
        }

        if (key is >= 0x70 and <= 0x87)
        {
            return $"F{key - 0x6F}";
        }

        if (key is >= 0x60 and <= 0x69)
        {
            return $"Num {key - 0x60}";
        }

        return key switch
        {
            0x08 => "Backspace",
            0x09 => "Tab",
            0x0D => "Enter",
            0x13 => "Pause",
            0x14 => "Caps Lock",
            0x1B => "Esc",
            0x20 => "Space",
            0x21 => "Page Up",
            0x22 => "Page Down",
            0x23 => "End",
            0x24 => "Home",
            0x25 => "←",
            0x26 => "↑",
            0x27 => "→",
            0x28 => "↓",
            0x2C => "Print Screen",
            0x2D => "Insert",
            0x2E => "Delete",
            0x5D => "Menu",
            0x6A => "Num *",
            0x6B => "Num +",
            0x6D => "Num -",
            0x6E => "Num .",
            0x6F => "Num /",
            0x90 => "Num Lock",
            0x91 => "Scroll Lock",
            0xBA => ";",
            0xBB => "=",
            0xBC => ",",
            0xBD => "-",
            0xBE => ".",
            0xBF => "/",
            0xC0 => "`",
            0xDB => "[",
            0xDC => "\\",
            0xDD => "]",
            0xDE => "'",
            _ => $"Key 0x{key:X2}"
        };
    }
}
