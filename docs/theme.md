# Theme

KeyViz has one dark theme. Its palette and surface hierarchy are based on wfchat, but KeyViz does not implement a full theme framework.

Shared colors live in `App.xaml` under semantic resource keys:

| Resource | Value | Purpose |
| --- | --- | --- |
| `Theme.AppBackgroundBrush` | `#282c33` | Base application color |
| `Theme.AppPanelBrush` | `rgb(59 65 77 / 0.88)` | Overlay and control-panel surfaces |
| `Theme.AppSoftBrush` | `rgb(47 52 62 / 0.72)` | Special-key chips and ordinary controls |
| `Theme.AppTextBrush` | `#f2f5f8` | Primary text |
| `Theme.MutedTextBrush` | `#b8c0ca` | Secondary text |
| `Theme.AppBorderBrush` | `rgb(116 126 143 / 0.38)` | Surface borders |
| `Theme.ActionBrush` | `#282c33` | Pressed-button background |
| `Theme.ActionHoverBrush` | `#2f343e` | Hover and selected states |
| `Theme.ActionBorderBrush` | `#5d6675` | Action and selected-button borders |
| `Theme.InactiveControlBrush` | `rgb(47 52 62 / 0.4)` | Inactive-button background |
| `Theme.InactiveBorderBrush` | `rgb(116 126 143 / 0.2)` | Inactive-button border |
| `Theme.InactiveTextBrush` | `rgb(184 192 202 / 0.6)` | Inactive-button text |

Component dimensions, spacing, corner radii, and states are defined directly in each window's XAML because KeyViz has a small UI surface. Add repeated colors to `App.xaml` instead of scattering them across components.

Surface guidelines:

- The overlay uses the app panel, app border, and a dark shadow.
- Special-key chips inserted between text tokens use the app-soft surface and app border.
- Show/Hide uses one local shared button style in `ControlWindow.xaml`.
- The active button uses the action-hover background and action border.
- The inactive button uses lower-opacity background, border, and text colors.
- KeyViz has no light theme or theme toggle.
