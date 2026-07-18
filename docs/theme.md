# Theme

KeyViz has one dark theme. Shared colors live in `App.xaml`; component dimensions and local control styles live with their window XAML.

## Shared resources

| Resource | Value | Use |
| --- | --- | --- |
| `Theme.AppBackgroundBrush` | `#282c33` | Base color |
| `Theme.AppPanelBrush` | `rgb(59 65 77 / 0.88)` | Bubble and property-panel surfaces |
| `Theme.AppSoftBrush` | `rgb(47 52 62 / 0.72)` | Ordinary controls |
| `Theme.AppTextBrush` | `#f2f5f8` | Primary text |
| `Theme.SpecialTextBrush` | `#4ade80` | Special-key text |
| `Theme.MutedTextBrush` | `#b8c0ca` | Labels and secondary text |
| `Theme.AppBorderBrush` | `rgb(116 126 143 / 0.38)` | Surface borders |
| `Theme.ActionBrush` | `#282c33` | Pressed controls |
| `Theme.ActionHoverBrush` | `#2f343e` | Hovered and selected controls |
| `Theme.ActionBorderBrush` | `#5d6675` | Hovered and selected borders |
| `Theme.InactiveControlBrush` | `rgb(47 52 62 / 0.4)` | Inactive controls |
| `Theme.InactiveBorderBrush` | `rgb(116 126 143 / 0.2)` | Inactive borders |
| `Theme.InactiveTextBrush` | `rgb(184 192 202 / 0.6)` | Inactive text |
| `Theme.ShadowColor` | `#0f172a` | Bubble and panel shadows |

## Rules

- Add reusable colors to `App.xaml`; do not duplicate color literals across windows.
- Keep spacing, sizes, corner radii, templates, and component-only styles in the owning XAML file.
- Use the monospaced-first overlay font stack with Thai-capable fallbacks: `Leelawadee UI`, `Tahoma`, and `Segoe UI`.
- Render special tokens with the same typography as text and `Theme.SpecialTextBrush`.
- Use action resources for hover, pressed, and selected states; use inactive resources for unselected states.
- Keep the `Keyviz` launcher visually fixed while the property panel expands upward.
- There is no light theme or theme toggle.
