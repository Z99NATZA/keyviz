# Settings

KeyViz reads the following file when the application starts:

```text
%LocalAppData%\KeyViz\settings.json
```

If the file does not exist, KeyViz creates it automatically with default values. Restart KeyViz after editing the file for changes to take effect.

## Format

```json
{
  "maxHistoryLength": 20,
  "showControls": true,
  "bubblePosition": "center"
}
```

### `maxHistoryLength`

The maximum combined Unicode code-point count retained across text and special tokens. Supported values range from `0` to `200`. `ที่` counts as 3, `Shift` counts as 5, `Ctrl+S` counts as 6, and a blank inserted by Space counts as 1. Text is trimmed by code point, while special tokens are removed as complete labels. Set the value to `0` to hide all tokens.

Text and special tokens are rendered in event order on one line:

```text
hello ที่ Ctrl+S next
```

Special tokens use the same typography as ordinary text and are distinguished by green text. Token type affects presentation only, not history-length accounting.

When the content exceeds the bubble width, KeyViz scrolls to the newest token without wrapping to another line.

### `showControls`

- `true` shows the lower-left Show and Hide buttons.
- `false` hides both buttons. The overlay and Exit action remain available from the System Tray.

### `bubblePosition`

Controls the horizontal bubble position. Supported values are `"left"`, `"center"`, and `"right"`, which place the bubble center at approximately 25%, 50%, and 75% of the work area. KeyViz keeps at least 32 pixels between the overlay window and the horizontal work-area edges. Unsupported values fall back to `"center"`.

Numeric values outside supported ranges are clamped. If the file cannot be read or contains invalid JSON, KeyViz uses defaults without overwriting the existing file.
