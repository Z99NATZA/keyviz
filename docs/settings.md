# Settings

KeyViz reads the following file when the application starts:

```text
%LocalAppData%\KeyViz\settings.json
```

If the file does not exist, KeyViz creates it automatically with default values. Restart KeyViz after editing the file for changes to take effect.

## Format

```json
{
  "maxTextLength": 20,
  "maxSpecialKeys": 5,
  "showControls": true,
  "bubblePosition": "center"
}
```

### `maxTextLength`

The maximum number of recent characters retained across all text tokens. Supported values range from `0` to `200`, and typed spaces count as text characters. Set the value to `0` to hide text tokens.

### `maxSpecialKeys`

The maximum number of recent special tokens. Supported values range from `0` to `20`. Every chip counts as exactly one item, so `[Space]`, `[Shift]`, `[Ctrl]`, `[Enter]`, and `[Ctrl + S]` each count as one. Set the value to `0` to hide special tokens.

Text and special tokens are rendered in event order on one line:

```text
hello [Space] world [Ctrl + S] next
```

When the content exceeds the bubble width, KeyViz scrolls to the newest token without wrapping to another line.

### `showControls`

- `true` shows the lower-left Show and Hide buttons.
- `false` hides both buttons. The overlay and Exit action remain available from the System Tray.

### `bubblePosition`

Controls the horizontal bubble position. Supported values are `"left"`, `"center"`, and `"right"`, which place the bubble center at approximately 25%, 50%, and 75% of the work area. KeyViz keeps at least 32 pixels between the overlay window and the horizontal work-area edges. Unsupported values fall back to `"center"`.

Numeric values outside supported ranges are clamped. If the file cannot be read or contains invalid JSON, KeyViz uses defaults without overwriting the existing file.
