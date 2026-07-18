# Architecture

## System boundary

KeyViz is a Windows-only WPF application. It captures keyboard events through Windows Raw Input, converts them into display tokens, and renders a click-through overlay. It has no backend, database, analytics, or network connection.

Typed content exists only in process memory. Settings are the only application data written to disk.

## Input flow

```text
Windows keyboard event
        ↓ WM_INPUT
RawKeyboardInput
        ↓ KeyboardInputEvent
MainWindow
        ├─ KeyboardTextTranslator → Unicode text from the active layout
        └─ KeyLabelFormatter      → special-key or shortcut label
        ↓
DisplayHistory → ordered Text/Special tokens
        ↓
MainWindow → WPF keystroke bubble
```

## Component ownership

| Component | Responsibility |
| --- | --- |
| `App` | Application lifecycle, shared live settings, ControlWindow creation, and System Tray actions |
| `RawKeyboardInput` | Registers `RIDEV_INPUTSINK` and converts `WM_INPUT` into keyboard events without blocking normal input |
| `KeyboardTextTranslator` | Resolves the focused control's keyboard layout and translates printable keys with `ToUnicodeEx` |
| `KeyLabelFormatter` | Names non-printable keys and formats shortcuts such as `Ctrl+Shift+S` |
| `DisplayHistory` | Stores ordered tokens, applies the Unicode code-point limit, removes text on Backspace, and groups repeated special keys |
| `MainWindow` | Receives input, updates history, and renders the non-activating click-through bubble |
| `ControlWindow` | Hosts the fixed `Keyviz` button and its upward-expanding live property panel |
| `SettingsService` | Loads, normalizes, creates, and saves `settings.json` |

## Display behavior

- Printable input follows the direct keyboard layout of the active application.
- Space is stored as an ordinary blank character.
- Shortcuts have no spaces around `+`, for example `Ctrl+Shift+S`.
- Consecutive identical special keys are grouped by total presses: `Backspace`, `Backspace*2`, `Backspace*3`.
- Text and special tokens stay in event order on one line. Special tokens render in green.
- Backspace removes one Unicode code point from the newest text token and is then recorded as a special token.
- The bubble grows with its content up to the available work-area width, then scrolls to the newest token.
- The bubble stays 32 pixels above the work-area bottom and fades after three seconds without a key-down event.

`MainWindow` uses these extended window styles:

- `WS_EX_TRANSPARENT` — mouse input passes through
- `WS_EX_NOACTIVATE` — the overlay does not take focus
- `WS_EX_TOOLWINDOW` — the overlay does not appear as a normal taskbar window

## Control and tray behavior

The lower-left `Keyviz` button stays in place while its property panel expands upward. The panel applies **Show keystrokes**, **Keyviz button**, **History limit**, and **Position** immediately and saves persistent values automatically.

The System Tray menu provides **Show/Hide keystrokes**, **Show/Hide Keyviz button**, and **Exit**. Double-clicking the tray icon toggles keystroke visibility.

## Invariants

- Never block, modify, inject, log, or transmit keyboard input.
- Keep text and special tokens in their original event order.
- Count Unicode code points, not UTF-16 code units, for the history limit.
- Trim text by code point and remove special tokens atomically.
- Keep overlay and control windows non-activating.

## Known limitations

- Secure Desktop surfaces such as UAC prompts are not captured.
- Exclusive-fullscreen applications may render above the overlay; borderless windowed mode is more reliable.
- Placement uses the primary work area. Monitor selection and vertical position are not configurable.
- Direct keyboard layouts are supported; dead-key and IME composition such as Chinese, Japanese, and Korean are not.
