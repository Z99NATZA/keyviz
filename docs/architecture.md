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
        ↓
KeyLabelFormatter
        ↓
Ordered display tokens (`Text` / `Special`)
        ↓
WPF overlay (`MainWindow`)
```

## Components

### `RawKeyboardInput`

Registers a keyboard device through Win32 Raw Input with `RIDEV_INPUTSINK`, allowing the window to receive `WM_INPUT` while another application is in the foreground. KeyViz reads key-down and key-up data without modifying or blocking input delivered to other applications.

### `KeyLabelFormatter`

Converts virtual-key codes into readable labels, combines active modifiers into shortcuts such as `Ctrl + Shift + S`, and converts letters, numbers, and common punctuation into text. Printable-character conversion currently follows an English keyboard layout rather than the active Windows keyboard layout.

### `MainWindow`

A transparent WPF overlay that stays above ordinary windows and does not accept mouse input or focus. It uses the following extended window styles:

- `WS_EX_TRANSPARENT`
- `WS_EX_NOACTIVATE`
- `WS_EX_TOOLWINDOW`

Text and special keys are stored as ordered tokens and rendered on one line, for example `text [Space] text [Ctrl + S]`. Total text characters, special-token count, and the left/center/right position are read from `settings.json`. Every special token counts as one item regardless of label length. When the content exceeds the bubble width, the view scrolls to the newest token. The panel fades after three seconds without a key-down event.

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
- Printable-character conversion follows English virtual-key positions and may not match Thai or other keyboard layouts, dead keys, or input method editors.
