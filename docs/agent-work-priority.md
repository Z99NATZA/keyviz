# Agent Work Priority

Read this file before starting a scoped task in KeyViz. It identifies the active
priority and routes work to the current documentation and owning code.

## Active Scope

No active priority.

Status: Ready for the next scoped task.

## Required Read Order

For every task:

1. Read `docs/architecture.md` for the current input, token, overlay, and
   application boundaries.
2. Read `docs/development.md` for the project structure and verification steps.
3. Inspect the owning implementation before changing behavior.

Then read only the documents relevant to the task:

- Settings or history limits: `docs/settings.md`, `settings.example.json`, and
  `Services/SettingsService.cs`.
- Overlay rendering or colors: `docs/theme.md`, `App.xaml`, and
  `MainWindow.xaml`.
- Keyboard capture or key translation: `Native/WindowsApi.cs`,
  `Services/RawKeyboardInput.cs`, `Services/KeyLabelFormatter.cs`, and
  `MainWindow.xaml.cs`.
- Application lifecycle, controls, or tray behavior: `App.xaml.cs` and
  `ControlWindow.xaml(.cs)`.

## Required Outcome

- Add the concrete, observable outcome for the next task here.
- Include the relevant automated and manual verification required to consider
  the task complete.

## Notes

- This file is a reusable task template. Do not delete, collapse, or rewrite the
  template structure when a priority is completed.
- When a priority is completed, clear only task-specific content from
  `Active Scope`, `Required Read Order`, and `Required Outcome`; keep the
  headings, default placeholder text, and these notes for the next agent.
- Keep this file short and task-focused.
- Treat the implementation as the source of truth. Inspect the owning code
  before adding or changing a behavior claim in `docs/`.
- Keep `docs/architecture.md`, `docs/settings.md`, `docs/theme.md`, and
  `docs/development.md` limited to current behavior. Do not add migration
  narrative, replacement history, or superseded behavior to these documents.
- Treat files under `docs/release/` as release records, not as sources of current
  behavior.
- Preserve the overlay guarantees: keyboard input must not be blocked or
  modified, and overlay windows must not take focus or intercept mouse input.
- Preserve the privacy boundary: keyboard events and display tokens stay in
  process memory and are not logged, persisted, or sent over a network.
- Keep text and special tokens in event order. `IsSpecial` controls presentation
  and atomic trimming; `maxHistoryLength` counts the displayed characters of
  both token types under one limit.
- Plain Space is an ordinary blank character. Special tokens use the same
  typography as text, render in green, and are removed as complete labels when
  history is trimmed.
- Update `settings.example.json` and `docs/settings.md` whenever the settings
  contract changes. Update `docs/theme.md` whenever shared visual resources or
  rendering rules change.
- The project currently has no automated test project. Run
  `dotnet format --verify-no-changes` and `dotnet build -c Release` for every code
  change, then perform the relevant manual checks from `docs/development.md`.
  Run the documented publish command when distribution behavior changes.
- Do not introduce a backend, network communication, persistent key logging, or
  an external dependency unless the active scope explicitly requires it.
- Clear completed task details and restore the default placeholders so this
  file is ready for the next scoped task.
