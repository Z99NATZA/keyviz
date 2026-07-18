# Development

## Stack

- C# and .NET 10
- WPF for the overlay and control window
- Win32 interop for Raw Input, keyboard-layout translation, and window styles
- Windows Forms `NotifyIcon` for the System Tray
- No external NuGet dependencies

## Run and verify

```powershell
dotnet restore
dotnet format --verify-no-changes
dotnet build -c Release
dotnet run --project Tests\KeyViz.Tests.csproj -c Release
```

Run the application for manual testing with:

```powershell
dotnet run
```

Debug mode remains attached to the terminal and stops with `Ctrl+C`. Release mode is a Windows GUI application and exits through the System Tray. Close an existing KeyViz instance before rebuilding the same configuration because Windows locks the running executable.

Publish only when distribution behavior needs verification:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Project map

| Path | Ownership |
| --- | --- |
| `App.xaml(.cs)` | Lifecycle, shared theme resources, live settings, and System Tray |
| `MainWindow.xaml(.cs)` | Raw-input orchestration and keystroke bubble |
| `ControlWindow.xaml(.cs)` | `Keyviz` launcher and property panel |
| `Models/DisplayToken.cs` | Text/special token model |
| `Models/DisplayHistory.cs` | Unicode-aware history, repeat grouping, and trimming |
| `Native/WindowsApi.cs` | Win32 declarations |
| `Services/RawKeyboardInput.cs` | Raw Input registration and parsing |
| `Services/KeyboardTextTranslator.cs` | Active-layout Unicode translation |
| `Services/KeyLabelFormatter.cs` | Special-key labels and shortcut formatting |
| `Services/SettingsService.cs` | Settings contract and persistence |
| `Services/UnicodeText.cs` | Unicode code-point helpers |
| `Tests/` | Dependency-free automated tests |

## Manual checklist

1. Type English and Thai Kedmanee/Pattachote text in modern Notepad, Chrome, and VS Code; verify the active application's layout is followed.
2. Test modifiers, Caps Lock, arrows, function keys, `Ctrl+Shift+S`, and repeated special keys such as `Backspace*2`.
3. Type `ที่`, then press Backspace; verify the text changes through `ที`, `ท`, and empty without splitting Unicode code points incorrectly.
4. Verify the overlay remains click-through, does not take focus, keeps tokens on one line, grows to the work-area width, and then scrolls to the latest token.
5. Verify **Show keystrokes**, **Keyviz button**, **History limit**, and **Position** apply live and persist to `settings.json`.
6. Hold History limit `−` and `+`; verify adjustment accelerates and never drops below 1.
7. Hide the `Keyviz` button from its panel, restore it from the System Tray, and verify the launcher does not move while expanding.
8. Verify every System Tray action and confirm double-click toggles keystroke visibility.

## Out of scope

- Dead-key and IME composition
- Color, font-size, duration, monitor, or vertical-position settings
- Mouse-button visualization
- Start with Windows
- A project-specific tray icon
- Automated UI tests for click-through behavior and live foreground-layout switching
