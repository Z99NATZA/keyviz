# Development

## Technology

- C# and .NET 10
- WPF for the user interface
- Win32 interop for Raw Input and overlay window styles
- Windows Forms `NotifyIcon` for the System Tray

The project has no external NuGet dependencies.

`dotnet run` uses the Debug configuration, which attaches to the terminal and stops with `Ctrl+C` while the terminal has focus. The Release configuration is a Windows GUI application and exits through the System Tray.

KeyViz has one dark theme. Reused colors are declared as root resources in `App.xaml`, while component styling remains in each window's XAML. See `docs/theme.md` for details.

## Structure

```text
keyviz/
├─ App.xaml(.cs)                 application lifecycle and System Tray
├─ MainWindow.xaml(.cs)          overlay and animation
├─ ControlWindow.xaml(.cs)       lower-left Show/Hide controls
├─ Models/DisplayToken.cs        text/special token for inline history
├─ Native/WindowsApi.cs          Win32 declarations
├─ Services/RawKeyboardInput.cs
├─ Services/KeyLabelFormatter.cs
├─ Services/SettingsService.cs
├─ settings.example.json
└─ docs/
```

## Verification

The following commands work in Command Prompt, PowerShell, or any terminal that can invoke `dotnet`:

```text
dotnet format --verify-no-changes
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Manual checks:

1. Press letters, numbers, arrow keys, and function keys.
2. Press shortcuts with multiple modifiers, such as `Ctrl + Shift + S`.
3. Confirm that the overlay does not take focus and remains click-through.
4. Confirm that text and special tokens appear in event order on one line.
5. Confirm that the combined character length of text and special tokens follows `maxHistoryLength`.
6. Confirm that Space appears as an ordinary blank character and special tokens remain complete when old content is trimmed.
7. Set `showControls` to both `true` and `false` and verify the Show/Hide controls.
8. Set `bubblePosition` to `left`, `center`, and `right` and verify horizontal placement.
9. Verify Show, Hide, and Exit from the System Tray.
10. Open several applications and confirm that keyboard events remain visible.

## Current Scope

The MVP provides an inline text/special-token history, virtual-key labels with modifiers, and configurable horizontal overlay placement. The following features are not yet supported:

- Character conversion through the active keyboard layout, including Thai and dead keys
- A settings UI for position, color, size, duration, and monitor selection
- Mouse-button visualization
- Start with Windows
- A project-specific tray icon
- Unit tests for key formatting and token-history behavior
