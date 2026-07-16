using System.Runtime.InteropServices;
using KeyViz.Models;
using KeyViz.Services;

var tests = new (string Name, Action Run)[]
{
    ("counts Unicode code points", CountUnicodeCodePoints),
    ("removes Thai code points in key order", RemoveThaiCodePoints),
    ("preserves surrogate pairs", PreserveSurrogatePairs),
    ("counts text and special labels under one limit", CountCombinedHistory),
    ("trims special labels atomically", TrimSpecialLabelsAtomically),
    ("replaces modifier previews", ReplaceModifierPreviews),
    ("translates English US through Windows", TranslateEnglishUs),
    ("translates Shift state through Windows", TranslateShiftState),
    ("translates Thai Kedmanee through Windows", TranslateThaiKedmanee),
    ("translates Thai Pattachote through Windows", TranslateThaiPattachote),
    ("prefers the caret thread for hosted text controls", PreferCaretWindow),
    ("falls back from caret to focus and active windows", FallBackThroughGuiWindows)
};

foreach (var test in tests)
{
    test.Run();
    Console.WriteLine($"PASS {test.Name}");
}

Console.WriteLine($"{tests.Length} KeyViz tests passed.");

static void CountUnicodeCodePoints()
{
    Equal(3, UnicodeText.CountCodePoints("ที่"));
    Equal(3, UnicodeText.CountCodePoints("A😀B"));
}

static void RemoveThaiCodePoints()
{
    var history = new DisplayHistory(20);
    history.AppendText("ที่");

    Equal(3, history.Length);
    Equal("ที่", history.Tokens.Single().Value);

    history.RemoveLastTextCodePoint();
    Equal("ที", history.Tokens.Single().Value);

    history.RemoveLastTextCodePoint();
    Equal("ท", history.Tokens.Single().Value);

    history.RemoveLastTextCodePoint();
    Equal(0, history.Tokens.Count);
}

static void PreserveSurrogatePairs()
{
    var history = new DisplayHistory(10);
    history.AppendText("A😀");

    Equal(2, history.Length);
    history.RemoveLastTextCodePoint();
    Equal("A", history.Tokens.Single().Value);
}

static void CountCombinedHistory()
{
    var history = new DisplayHistory(8);
    history.AppendText("abc");
    history.AddSpecial("Shift");

    Equal(8, history.Length);

    history.AppendText("x");
    Equal(8, history.Length);
    Equal("bc", history.Tokens[0].Value);
    Equal("Shift", history.Tokens[1].Value);
    Equal("x", history.Tokens[2].Value);
}

static void TrimSpecialLabelsAtomically()
{
    var history = new DisplayHistory(6);
    history.AddSpecial("Shift");
    history.AppendText("ab");

    Equal(2, history.Length);
    Equal(1, history.Tokens.Count);
    Equal("ab", history.Tokens.Single().Value);
}

static void ReplaceModifierPreviews()
{
    var history = new DisplayHistory(20);
    history.AddSpecial("Ctrl");
    history.AddSpecial("Ctrl + S", replaceLastSpecial: true);

    Equal(1, history.Tokens.Count);
    Equal("Ctrl + S", history.Tokens.Single().Value);
    Equal(8, history.Length);
}

static void TranslateEnglishUs()
{
    var layout = NativeMethods.LoadKeyboardLayout("00000409", NativeMethods.DoNotNotifyShell);
    NotZero(layout, "English US keyboard layout");

    var keyboardEvent = new KeyboardInputEvent(0x31, 0x02, IsKeyDown: true, IsExtended: false);
    var translated = KeyboardTextTranslator.TryTranslate(
        keyboardEvent,
        new HashSet<int> { keyboardEvent.VirtualKey },
        layout,
        out var text);

    Equal(true, translated);
    Equal("1", text);
}

static void TranslateShiftState()
{
    var layout = NativeMethods.LoadKeyboardLayout("00000409", NativeMethods.DoNotNotifyShell);
    NotZero(layout, "English US keyboard layout");

    var keyboardEvent = new KeyboardInputEvent(0x31, 0x02, IsKeyDown: true, IsExtended: false);
    var translated = KeyboardTextTranslator.TryTranslate(
        keyboardEvent,
        new HashSet<int> { keyboardEvent.VirtualKey, 0xA0 },
        layout,
        out var text);

    Equal(true, translated);
    Equal("!", text);
}

static void TranslateThaiKedmanee()
{
    var layout = NativeMethods.LoadKeyboardLayout("0000041E", NativeMethods.DoNotNotifyShell);
    NotZero(layout, "Thai Kedmanee keyboard layout");

    var keys = new[]
    {
        new KeyboardInputEvent(0x4D, 0x32, IsKeyDown: true, IsExtended: false),
        new KeyboardInputEvent(0x55, 0x16, IsKeyDown: true, IsExtended: false),
        new KeyboardInputEvent(0x4A, 0x24, IsKeyDown: true, IsExtended: false)
    };
    var translatedText = string.Empty;

    foreach (var keyboardEvent in keys)
    {
        var translated = KeyboardTextTranslator.TryTranslate(
            keyboardEvent,
            new HashSet<int> { keyboardEvent.VirtualKey },
            layout,
            out var text);

        Equal(true, translated);
        translatedText += text;
    }

    Equal("ที่", translatedText);
}

static void TranslateThaiPattachote()
{
    var layout = NativeMethods.LoadKeyboardLayout("0001041E", NativeMethods.DoNotNotifyShell);
    NotZero(layout, "Thai Pattachote keyboard layout");

    var keyboardEvent = new KeyboardInputEvent(0x4D, 0x32, IsKeyDown: true, IsExtended: false);
    var translated = KeyboardTextTranslator.TryTranslate(
        keyboardEvent,
        new HashSet<int> { keyboardEvent.VirtualKey },
        layout,
        out var text);

    Equal(true, translated);
    if (!text.Any(character => character is >= '\u0E00' and <= '\u0E7F'))
    {
        throw new InvalidOperationException($"Expected Thai Pattachote text, got '{text}'.");
    }
}

static void PreferCaretWindow()
{
    var threadInfo = new KeyViz.Native.WindowsApi.GuiThreadInfo
    {
        ActiveWindow = new IntPtr(1),
        FocusedWindow = new IntPtr(2),
        CaretWindow = new IntPtr(3)
    };

    Equal(new IntPtr(3), KeyboardTextTranslator.SelectInputWindow(threadInfo, new IntPtr(4)));
}

static void FallBackThroughGuiWindows()
{
    var focusedInfo = new KeyViz.Native.WindowsApi.GuiThreadInfo
    {
        ActiveWindow = new IntPtr(1),
        FocusedWindow = new IntPtr(2)
    };
    Equal(new IntPtr(2), KeyboardTextTranslator.SelectInputWindow(focusedInfo, new IntPtr(4)));

    var activeInfo = new KeyViz.Native.WindowsApi.GuiThreadInfo
    {
        ActiveWindow = new IntPtr(1)
    };
    Equal(new IntPtr(1), KeyboardTextTranslator.SelectInputWindow(activeInfo, new IntPtr(4)));

    Equal(
        new IntPtr(4),
        KeyboardTextTranslator.SelectInputWindow(
            new KeyViz.Native.WindowsApi.GuiThreadInfo(),
            new IntPtr(4)));
}

static void NotZero(IntPtr value, string description)
{
    if (value == IntPtr.Zero)
    {
        throw new InvalidOperationException($"Could not load {description}.");
    }
}

static void Equal<T>(T expected, T actual)
    where T : notnull
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

internal static class NativeMethods
{
    internal const uint DoNotNotifyShell = 0x00000080;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadKeyboardLayout(string keyboardLayoutId, uint flags);
}
