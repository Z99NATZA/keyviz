# Settings

## Location and lifecycle

```text
%LocalAppData%\KeyViz\settings.json
```

KeyViz creates this file with defaults when it does not exist. Changes made through the `Keyviz` property panel or System Tray are applied immediately and saved automatically. Changes made directly in JSON require an application restart.

If the file is unreadable or contains invalid JSON, KeyViz uses defaults without overwriting it.

## Format

```json
{
  "maxHistoryLength": 20,
  "showControls": true,
  "bubblePosition": "center"
}
```

| JSON field | UI control | Default | Accepted values |
| --- | --- | --- | --- |
| `maxHistoryLength` | **History limit** | `20` | Integer from `1` to `2,147,483,647` |
| `showControls` | **Keyviz button** | `true` | `true` or `false` |
| `bubblePosition` | **Position** | `"center"` | `"left"`, `"center"`, or `"right"` |

## `maxHistoryLength`

This is the combined Unicode code-point count retained across displayed text and special-token labels. Examples:

| Display value | Count |
| --- | ---: |
| `ที่` | 3 |
| `Shift` | 5 |
| `Ctrl+S` | 6 |
| Space | 1 |

Text is trimmed one code point at a time. Special tokens are removed whole. Very large limits can increase memory use and rendering work.

The History limit buttons accelerate while held:

- Under 1 second: step 1
- 1–2.5 seconds: step 5
- 2.5–5 seconds: step 25
- Over 5 seconds: step 100

Values below 1 are normalized to 1.

## `showControls`

- `true` shows the lower-left `Keyviz` button.
- `false` hides the button without disabling the keystroke overlay or System Tray.

Use **Show Keyviz button** in the System Tray to restore a hidden button.

## `bubblePosition`

The value places the bubble center at approximately 25% (`left`), 50% (`center`), or 75% (`right`) of the primary work area. The window remains at least 32 pixels from horizontal work-area edges. Unsupported values normalize to `center`.

The bubble grows to the available work-area width before scrolling to its newest content.
