# Settings

KeyViz reads the following file when the application starts:

```text
%LocalAppData%\KeyViz\settings.json
```

If the file does not exist, KeyViz creates it automatically with default values. Restart KeyViz after editing the file directly for changes to take effect. When `showControls` is enabled, the compact `Keyviz` button provides live controls for history length, bubble position, and keystroke visibility. Changes made there are saved automatically.

## Format

```json
{
  "maxHistoryLength": 20,
  "showControls": true,
  "bubblePosition": "center"
}
```

### `maxHistoryLength`

The maximum combined Unicode code-point count retained across text and special tokens. The minimum is `1`; there is no practical UI maximum beyond the underlying 32-bit integer limit. Very large histories can increase memory usage and rendering work. The property panel's `−` and `+` buttons accelerate while held. `ที่` counts as 3, `Shift` counts as 5, `Ctrl+S` counts as 6, and a blank inserted by Space counts as 1. Text is trimmed by code point, while special tokens are removed as complete labels.

Text and special tokens are rendered in event order on one line:

```text
hello ที่ Ctrl+S next
```

Special tokens use the same typography as ordinary text and are distinguished by green text. Token type affects presentation only, not history-length accounting.

When the content exceeds the bubble width, KeyViz scrolls to the newest token without wrapping to another line.

### `showControls`

- `true` shows the lower-left `Keyviz` button. Pressing it expands the property panel upward while keeping the button in place.
- `false` hides the button. The overlay and Exit action remain available from the System Tray.

The property panel can hide the controls immediately. Use **Show Keyviz button** in the System Tray menu to restore it. Both the property panel and System Tray actions update this setting automatically.

### `bubblePosition`

Controls the horizontal bubble position. Supported values are `"left"`, `"center"`, and `"right"`, which place the bubble center at approximately 25%, 50%, and 75% of the work area. KeyViz keeps at least 32 pixels between the overlay window and the horizontal work-area edges. Unsupported values fall back to `"center"`.

Numeric values below the supported minimum are clamped. If the file cannot be read or contains invalid JSON, KeyViz uses defaults without overwriting the existing file.
