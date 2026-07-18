# Agent Work Guide

## Current priority

No active priority. The repository is ready for the next scoped task.

## Start here

1. Read `docs/architecture.md` for system boundaries, ownership, and invariants.
2. Read `docs/development.md` for commands, project paths, and verification.
3. Inspect the owning implementation before changing behavior.
4. Read only the task-specific references below.

## Task routing

| Task | Read and inspect |
| --- | --- |
| Settings or history | `docs/settings.md`, `settings.example.json`, `Services/SettingsService.cs`, `Models/DisplayHistory.cs`, `Services/UnicodeText.cs` |
| Overlay layout or theme | `docs/theme.md`, `App.xaml`, `MainWindow.xaml(.cs)` |
| Keyboard capture or translation | `Native/WindowsApi.cs`, `Services/RawKeyboardInput.cs`, `Services/KeyboardTextTranslator.cs`, `Services/KeyLabelFormatter.cs`, `MainWindow.xaml.cs` |
| Keyviz controls or System Tray | `App.xaml.cs`, `ControlWindow.xaml(.cs)` |
| Tests or release verification | `docs/development.md`, `Tests/`, `KeyViz.csproj` |

## Non-negotiable invariants

- Keyboard input must not be blocked, modified, injected, logged, persisted, or transmitted.
- Overlay and control windows must not take focus; the overlay must remain click-through.
- Text and special tokens must remain in event order.
- History length counts Unicode code points across both token types.
- Text trims by code point; special tokens trim atomically.
- Space remains ordinary text. Special tokens use the normal typography and green foreground.
- Existing user changes in the worktree must be preserved.

## Definition of done

For code changes:

```powershell
dotnet format --verify-no-changes
dotnet build -c Release
dotnet run --project Tests\KeyViz.Tests.csproj -c Release
```

Also run the relevant manual checks from `docs/development.md`. Run the publish command only when distribution behavior changes.

For documentation changes, run `git diff --check` and verify every documented path, command, setting, and UI label against the implementation.

## Documentation policy

- Treat implementation as the source of truth.
- Keep `architecture.md`, `development.md`, `settings.md`, and `theme.md` limited to current behavior.
- Keep task history out of current documentation.
- Never edit `docs/release/` while cleaning current documentation; release files are historical records.
- Update `settings.example.json` with settings-contract changes and `theme.md` with shared visual-resource changes.
- Do not add a backend, network communication, persistent key logging, or external dependency unless explicitly requested.
