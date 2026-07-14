using System.ComponentModel;
using System.Runtime.InteropServices;
using KeyViz.Native;

namespace KeyViz.Services;

internal sealed class RawKeyboardInput : IDisposable
{
    private const ushort GenericDesktopUsagePage = 0x01;
    private const ushort KeyboardUsage = 0x06;
    private const uint MapVirtualKeyScanCodeToVirtualKeyEx = 3;
    private const uint Error = uint.MaxValue;

    private bool _disposed;

    internal RawKeyboardInput(IntPtr windowHandle)
    {
        var devices = new[]
        {
            new WindowsApi.RawInputDevice
            {
                UsagePage = GenericDesktopUsagePage,
                Usage = KeyboardUsage,
                Flags = WindowsApi.RidevInputSink,
                Target = windowHandle
            }
        };

        if (!WindowsApi.RegisterRawInputDevices(
                devices,
                (uint)devices.Length,
                (uint)Marshal.SizeOf<WindowsApi.RawInputDevice>()))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    internal bool TryRead(IntPtr rawInputHandle, out KeyboardInputEvent keyboardEvent)
    {
        keyboardEvent = default!;
        var headerSize = (uint)Marshal.SizeOf<WindowsApi.RawInputHeader>();
        uint bufferSize = 0;

        if (WindowsApi.GetRawInputData(
                rawInputHandle,
                WindowsApi.RidInput,
                IntPtr.Zero,
                ref bufferSize,
                headerSize) == Error
            || bufferSize < headerSize + Marshal.SizeOf<WindowsApi.RawKeyboard>())
        {
            return false;
        }

        var buffer = Marshal.AllocHGlobal((int)bufferSize);

        try
        {
            var bytesRead = WindowsApi.GetRawInputData(
                rawInputHandle,
                WindowsApi.RidInput,
                buffer,
                ref bufferSize,
                headerSize);

            if (bytesRead == Error || bytesRead != bufferSize)
            {
                return false;
            }

            var header = Marshal.PtrToStructure<WindowsApi.RawInputHeader>(buffer);
            if (header.Type != WindowsApi.RimTypeKeyboard)
            {
                return false;
            }

            var keyboardPointer = IntPtr.Add(buffer, (int)headerSize);
            var keyboard = Marshal.PtrToStructure<WindowsApi.RawKeyboard>(keyboardPointer);
            if (keyboard.VirtualKey == 0x00FF)
            {
                return false;
            }

            var isKeyDown = keyboard.Message is WindowsApi.WmKeyDown or WindowsApi.WmSysKeyDown;
            var isKeyUp = keyboard.Message is WindowsApi.WmKeyUp or WindowsApi.WmSysKeyUp;

            if (!isKeyDown && !isKeyUp)
            {
                return false;
            }

            keyboardEvent = new KeyboardInputEvent(
                NormalizeVirtualKey(keyboard),
                keyboard.MakeCode,
                isKeyDown,
                (keyboard.Flags & WindowsApi.RiKeyE0) != 0);
            return true;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var devices = new[]
        {
            new WindowsApi.RawInputDevice
            {
                UsagePage = GenericDesktopUsagePage,
                Usage = KeyboardUsage,
                Flags = WindowsApi.RidevRemove,
                Target = IntPtr.Zero
            }
        };

        WindowsApi.RegisterRawInputDevices(
            devices,
            (uint)devices.Length,
            (uint)Marshal.SizeOf<WindowsApi.RawInputDevice>());
        _disposed = true;
    }

    private static int NormalizeVirtualKey(WindowsApi.RawKeyboard keyboard)
    {
        var isExtended = (keyboard.Flags & WindowsApi.RiKeyE0) != 0;

        return keyboard.VirtualKey switch
        {
            0x10 => (int)WindowsApi.MapVirtualKey(
                keyboard.MakeCode,
                MapVirtualKeyScanCodeToVirtualKeyEx),
            0x11 => isExtended ? 0xA3 : 0xA2,
            0x12 => isExtended ? 0xA5 : 0xA4,
            _ => keyboard.VirtualKey
        };
    }
}

internal sealed record KeyboardInputEvent(
    int VirtualKey,
    int ScanCode,
    bool IsKeyDown,
    bool IsExtended);
