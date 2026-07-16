# Development

## Technology

- C# and .NET 10
- WPF for the user interface
- Win32 interop for Raw Input, foreground keyboard-layout translation, and overlay window styles
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
├─ Models/DisplayHistory.cs      Unicode-aware token history and trimming
├─ Native/WindowsApi.cs          Win32 declarations
├─ Services/RawKeyboardInput.cs
├─ Services/KeyboardTextTranslator.cs
├─ Services/KeyLabelFormatter.cs
├─ Services/SettingsService.cs
├─ Services/UnicodeText.cs
├─ Tests/                        dependency-free automated verification
├─ settings.example.json
└─ docs/
```

## Verification

The following commands work in Command Prompt, PowerShell, or any terminal that can invoke `dotnet`:

```text
dotnet format --verify-no-changes
dotnet build -c Release
dotnet run --project Tests\KeyViz.Tests.csproj -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Manual checks:

1. Select English US and type `test` in modern Notepad, Chrome, and VS Code.
2. Select Thai Kedmanee and type `ที่` in all three applications; repeat with Thai Pattachote. This verifies both hosted Notepad text controls and conventional foreground-window layout resolution.
3. Type `hello ที่` and confirm that Thai vowels and tone marks compose without clipping.
4. Switch layouts with `Win + Space` and `Alt + Shift`; confirm that the next printable key uses the newly selected layout.
5. Test Shift, Caps Lock, repeated keys, arrow keys, function keys, and `Ctrl + Shift + S`.
6. Press Backspace after `ที่` and confirm the text portion changes through `ที`, `ท`, and empty while Backspace remains a special token.
7. Confirm that the overlay does not take focus and remains click-through.
8. Confirm that text and special tokens appear in event order on one line.
9. Confirm that the combined Unicode code-point count follows `maxHistoryLength`, Space counts as one, and special tokens remain complete when old content is trimmed.
10. Set `showControls` to both `true` and `false` and verify the Show/Hide controls.
11. Set `bubblePosition` to `left`, `center`, and `right` and verify horizontal placement.
12. Confirm that the bubble and Show/Hide controls share the same 32-pixel bottom work-area margin.
13. Verify Show, Hide, and Exit from the System Tray.

## Current Scope

The MVP provides active-layout Unicode text for direct English and Thai keyboard layouts, inline text/special-token history, virtual-key labels with modifiers, and configurable horizontal overlay placement. The following features are not supported:

- Dead-key composition and IME composition for languages such as Chinese, Japanese, and Korean
- A settings UI for position, color, size, duration, and monitor selection
- Mouse-button visualization
- Start with Windows
- A project-specific tray icon
- Automated UI tests for the click-through overlay and live foreground-layout switching
