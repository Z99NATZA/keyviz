using KeyViz.Native;
using System.Runtime.InteropServices;

namespace KeyViz.Services;

internal static class KeyboardTextTranslator
{
    private const int KeyboardStateLength = 256;
    private const int TranslationBufferLength = 16;
    private const byte KeyDownMask = 0x80;
    private const byte ToggledMask = 0x01;
    private const uint PreserveKeyboardState = 0x00000004;

    private const int VirtualShift = 0x10;
    private const int VirtualControl = 0x11;
    private const int VirtualAlt = 0x12;
    private const int VirtualCapsLock = 0x14;
    private const int VirtualNumLock = 0x90;
    private const int VirtualScrollLock = 0x91;

    private static readonly int[] ShiftKeys = [VirtualShift, 0xA0, 0xA1];
    private static readonly int[] ControlKeys = [VirtualControl, 0xA2, 0xA3];
    private static readonly int[] AltKeys = [VirtualAlt, 0xA4, 0xA5];

    internal static bool TryTranslate(
        KeyboardInputEvent keyboardEvent,
        IReadOnlySet<int> pressedKeys,
        out string text)
    {
        return TryTranslate(
            keyboardEvent,
            pressedKeys,
            GetForegroundKeyboardLayout(),
            out text);
    }

    internal static bool TryTranslate(
        KeyboardInputEvent keyboardEvent,
        IReadOnlySet<int> pressedKeys,
        IntPtr keyboardLayout,
        out string text)
    {
        text = string.Empty;
        var keyboardState = CreateKeyboardState(pressedKeys);
        var buffer = new char[TranslationBufferLength];
        var translatedLength = WindowsApi.ToUnicodeEx(
            (uint)keyboardEvent.VirtualKey,
            (uint)keyboardEvent.ScanCode,
            keyboardState,
            buffer,
            buffer.Length,
            PreserveKeyboardState,
            keyboardLayout);

        if (translatedLength <= 0)
        {
            return false;
        }

        text = new string(buffer, 0, Math.Min(translatedLength, buffer.Length));
        if (text.Any(char.IsControl))
        {
            text = string.Empty;
            return false;
        }

        return true;
    }

    private static byte[] CreateKeyboardState(IReadOnlySet<int> pressedKeys)
    {
        var keyboardState = new byte[KeyboardStateLength];

        foreach (var key in pressedKeys)
        {
            if (key is >= 0 and < KeyboardStateLength)
            {
                keyboardState[key] |= KeyDownMask;
            }
        }

        SetAggregateModifier(keyboardState, pressedKeys, VirtualShift, ShiftKeys);
        SetAggregateModifier(keyboardState, pressedKeys, VirtualControl, ControlKeys);
        SetAggregateModifier(keyboardState, pressedKeys, VirtualAlt, AltKeys);
        SetToggleState(keyboardState, VirtualCapsLock);
        SetToggleState(keyboardState, VirtualNumLock);
        SetToggleState(keyboardState, VirtualScrollLock);

        return keyboardState;
    }

    private static void SetAggregateModifier(
        byte[] keyboardState,
        IReadOnlySet<int> pressedKeys,
        int aggregateKey,
        IEnumerable<int> modifierKeys)
    {
        if (modifierKeys.Any(pressedKeys.Contains))
        {
            keyboardState[aggregateKey] |= KeyDownMask;
        }
    }

    private static void SetToggleState(byte[] keyboardState, int virtualKey)
    {
        if ((WindowsApi.GetKeyState(virtualKey) & ToggledMask) != 0)
        {
            keyboardState[virtualKey] |= ToggledMask;
        }
    }

    private static IntPtr GetForegroundKeyboardLayout()
    {
        var foregroundWindow = WindowsApi.GetForegroundWindow();
        var threadInfo = new WindowsApi.GuiThreadInfo
        {
            Size = (uint)Marshal.SizeOf<WindowsApi.GuiThreadInfo>()
        };

        if (WindowsApi.GetGUIThreadInfo(0, ref threadInfo))
        {
            var inputWindow = SelectInputWindow(threadInfo, foregroundWindow);
            var inputLayout = GetKeyboardLayoutForWindow(inputWindow);
            if (inputLayout != IntPtr.Zero)
            {
                return inputLayout;
            }
        }

        var foregroundLayout = GetKeyboardLayoutForWindow(foregroundWindow);
        return foregroundLayout != IntPtr.Zero
            ? foregroundLayout
            : WindowsApi.GetKeyboardLayout(0);
    }

    internal static IntPtr SelectInputWindow(
        WindowsApi.GuiThreadInfo threadInfo,
        IntPtr fallbackWindow)
    {
        if (threadInfo.CaretWindow != IntPtr.Zero)
        {
            return threadInfo.CaretWindow;
        }

        if (threadInfo.FocusedWindow != IntPtr.Zero)
        {
            return threadInfo.FocusedWindow;
        }

        if (threadInfo.ActiveWindow != IntPtr.Zero)
        {
            return threadInfo.ActiveWindow;
        }

        return fallbackWindow;
    }

    private static IntPtr GetKeyboardLayoutForWindow(IntPtr window)
    {
        if (window == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        var thread = WindowsApi.GetWindowThreadProcessId(window, IntPtr.Zero);
        return thread == 0
            ? IntPtr.Zero
            : WindowsApi.GetKeyboardLayout(thread);
    }
}
