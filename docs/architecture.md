# Architecture

## Overview

KeyViz is a WPF desktop application with a transparent overlay and a System Tray icon. It has no backend, database, or network connection.

Main input and rendering flow:

```text
Windows keyboard event
        ↓
Windows Raw Input (`WM_INPUT`)
        ↓
RawKeyboardInput
        ↓
MainWindow
        ├─ KeyboardTextTranslator → foreground keyboard layout → Unicode text
        └─ KeyLabelFormatter → special-key and shortcut labels
        ↓
Ordered display tokens (`Text` / `Special`)
        ↓
WPF overlay (`MainWindow`)
```

## Components

### `RawKeyboardInput`

Registers a keyboard device through Win32 Raw Input with `RIDEV_INPUTSINK`, allowing the window to receive `WM_INPUT` while another application is in the foreground. KeyViz reads virtual keys, scan codes, and key-down/key-up state without modifying or blocking input delivered to other applications.

### `KeyboardTextTranslator`

Resolves the input locale of the caret or keyboard-focused control's GUI thread for each printable key and calls `ToUnicodeEx` with the Raw Input virtual key, scan code, and current modifier/toggle state. The top-level foreground window and KeyViz thread provide fallbacks when Windows does not expose a focused control. Resolving the control thread is required for hosted editors such as modern Notepad, where the top-level window and text control can use different input locales. Translation uses the non-mutating keyboard-state flag available on supported Windows versions. This allows English US, Thai Kedmanee, Thai Pattachote, and other direct keyboard layouts to follow the input language selected in the active application.

### `KeyLabelFormatter`

Converts non-printable virtual keys into readable labels and combines active modifiers into shortcuts such as `Ctrl+Shift+S`. Printable text conversion belongs to `KeyboardTextTranslator`.

### `DisplayHistory`

Stores ordered text and special tokens, appends translated Unicode strings, removes the last Unicode code point from the newest text token on Backspace, and applies the shared history-length limit. Text can be trimmed by code point; special labels are removed atomically.

### `MainWindow`

A transparent WPF overlay that stays above ordinary windows and does not accept mouse input or focus. It uses the following extended window styles:

- `WS_EX_TRANSPARENT`
- `WS_EX_NOACTIVATE`
- `WS_EX_TOOLWINDOW`

Text and special keys are rendered on one line, for example `hello ที่ Ctrl+S`. Pressing Space inserts an ordinary blank character. Consecutive copies of the same special key are collapsed into a total press counter such as `Backspace*2`, `Backspace*3`, and so on. Special tokens use the same typography as ordinary text but are colored green. Thai-capable fallback fonts allow vowels and tone marks to compose in the same text run. `maxHistoryLength` limits the combined Unicode code-point count across both token types, so `ที่` contributes 3, `Shift` contributes 5, and `Ctrl+S` contributes 6. The horizontal position is read from `settings.json`; the bubble's bottom edge stays 32 pixels above the work-area bottom, aligned with the Show/Hide controls. When the content exceeds the bubble width, the view scrolls to the newest token. The panel fades after three seconds without a key-down event.

### `ControlWindow`

A small lower-left control window positioned 32 pixels from the work-area edges. It provides Show and Hide buttons for `MainWindow` without taking foreground focus from the active application.

### `App`

Owns application lifecycle, root dark-theme color resources, shared state between `MainWindow` and `ControlWindow`, and the System Tray icon. The overlay can be shown or hidden through either the on-screen controls or the tray menu. Exit remains available from the tray menu.

### `SettingsService`

Reads `%LocalAppData%\KeyViz\settings.json` at startup and creates it with defaults when it does not exist. Invalid files fall back to defaults, and numeric values outside supported ranges are clamped.

## Data Handling And Limitations

- Key events and display tokens are processed in memory only; typed content is not logged or sent over the network.
- Secure Desktop surfaces such as UAC prompts are not captured or overlaid.
- Exclusive-fullscreen games may appear above the overlay; borderless windowed mode is more reliable.
- The primary work area is used for placement. Monitor selection and vertical positioning are not configurable.
- Direct keyboard layouts use the foreground application's active input locale. Dead-key composition and IME composition such as Chinese, Japanese, and Korean input are not supported.
