# UI Plan

This file tracks the planned configuration UI structure for Disaster Command Center.

## Current State

- `DisasterCommandCenter/` currently exposes a minimal settings group.
- The legacy `Source/` UI is the reference for parity: in-game tabs, disaster rows, progress bars, action buttons, dependency status, localized labels, and debug controls.
- Treat this plan as target design and migration guidance, not a statement of implemented Disaster Command Center UI.

## Dashboard

- Show overall mod status.
- Show migration/import status when applicable.
- Show current disaster generation state.
- Show the highest priority compatibility warning.
- Provide quick access to manual disaster controls and emergency actions.
- Keep advanced diagnostics out of the first view unless there is a warning.

## Disasters Tab

- Group settings by disaster type.
- Cover forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, and meteor strike.
- Show enable/disable, generated intensity, manual intensity, cooldown, and frequency controls where available.
- Use tooltips to explain generated disasters versus manual spawns.
- Show population-scaled intensity state when enabled.
- Avoid exposing internal simulation terms without player-facing explanations.

## Compatibility Tab

- Show detected mods that may affect time flow, disasters, fire behavior, assets, or UI.
- Use status rows with severity: OK, Info, Warning, Conflict.
- Include expected impact and recommended player action.
- Prioritize Real Time, Extended InfoPanel 2, ACME, Tree Fire Control, Game Anarchy, Rain Firefighting, Adjustable Fire, No Fires, and Skyve support context.

## Advanced Tab

- Place high-risk tuning, diagnostics, and recovery actions here.
- Separate destructive or reset actions from routine settings.
- Require clear wording for emergency stop and reset disaster state.
- Keep defaults conservative.
- Do not expose recovery actions until valid rebuild/repair targets can be detected safely.

## Logs And Debug Tab

- Show whether diagnostic logging is enabled.
- Provide short explanations for what gets logged.
- Include recent compatibility and disaster state summaries if feasible.
- Avoid overwhelming players with raw internals unless they opt in.

## Open UX Questions

- Which settings should be part of a Basic mode?
- Which settings should be hidden until Advanced mode is enabled?
- How should Real Time recurrence estimates be worded?
- Should emergency actions require confirmation?
- What minimum compatibility information is useful without causing alarm?
- How should legacy settings migration be surfaced to existing users?
