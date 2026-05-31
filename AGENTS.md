# Codex Instructions

This repository contains Disaster Command Center, a Cities: Skylines 1 mod project and visible successor to the legacy Natural Disasters Renewal mod.

## Current Repository State

- `DisasterCommandCenter/` is the new project shell. Current version is `0.1.0-dev`.
- The new project currently contains module boundaries for migration, localization, compatibility, settings, serialization, disaster simulation, evacuation, recovery, and in-game UI.
- The new project does not yet contain the full disaster behavior from the legacy implementation.
- `Source/` contains the current functional reference implementation, version `1.3.0`, still under the legacy namespace and assembly.
- `DisasterCommandCenter/Docs/MigrationPlan.md` is the migration guide. Treat it as the source of truth for the intended transition.
- `DisasterCommandCenter/Migration/LegacyModIdentity.cs` documents the legacy identifiers that must be preserved for import and save/settings migration planning.

## Product Scope

Disaster Command Center is intended to control, tune, respond to, and eventually recover from disasters in Cities: Skylines 1. The migrated feature set is expected to cover:

- Weather, season, time, and elapsed-time based disaster occurrence.
- Manual disaster triggering from low to maximum supported intensity.
- Per-disaster enable/disable controls.
- Population-scaled maximum generated intensity.
- Manual, automatic, and focused evacuation behavior.
- Pause-before-disaster and camera-focus options.
- Emergency stop and disaster progress reset flows.
- Real Time aware recurrence and timing.
- Compatibility detection and warnings.
- Save-specific settings and migration from legacy settings where practical.
- Future recovery tooling for rebuildable buildings and damaged roads.

## Existing Legacy Behavior To Preserve During Migration

When planning Disaster Command Center work, compare against `Source/` before changing behavior. The legacy mod currently supports:

- Disasters: forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, and meteor strike.
- In-game disaster panel with tabs, progress bars, action buttons, dependency status, localized text, and debug controls.
- Section-based settings screen with About and Dependencies sections.
- English and Spanish localization for panel text, settings labels, tooltips, dependency warnings, and about content.
- Real Time compatible recurrence for all supported disasters.
- Compatibility metadata and checks for Real Time, Extended InfoPanel 2, ACME, Tree Fire Control, Game Anarchy, Rain Firefighting, Adjustable Fire, and No Fires.
- CSV disaster event logging in the Cities: Skylines data directory when enabled.
- Harmony patches for disaster destruction behavior.

## Technical Constraints

- Target Cities: Skylines 1 and its Unity 5-era API surface.
- Both projects target `.NET Framework v3.5`.
- Avoid modern C# syntax and APIs that may not work with the current target.
- Prefer simple C# patterns compatible with older Unity and ICities APIs.
- Be careful with `Assembly-CSharp`, `ColossalManaged`, `ICities`, `UnityEngine`, and CitiesHarmony references.
- Do not introduce runtime dependencies without explaining compatibility and deployment risk.

## Working Rules

- Prefer small, reviewable changes that are easy for the project owner to inspect.
- Do not modify source code, project files, solution files, build scripts, or runtime files unless the user explicitly asks for implementation work.
- Keep product recommendations separate from code changes. Label them as recommendations.
- Do not make large architectural changes without explaining the reason, expected benefit, risk, and migration path.
- Do not claim a Disaster Command Center feature is implemented unless it exists in `DisasterCommandCenter/`; otherwise call it planned, legacy, or pending migration.
- Treat documentation, QA checklists, release notes, and agent skill files as development workflow assets only. They must not be included as runtime assets for the mod.
- Do not add secrets, credentials, API keys, tokens, Steam credentials, or machine-specific paths.

## Compatibility Expectations

When proposing or implementing behavior changes, consider interaction with mods that may affect simulation, disasters, time flow, fire behavior, assets, or UI, including:

- Real Time
- Extended InfoPanel 2
- ACME
- Tree Fire Control
- Game Anarchy
- Rain Firefighting
- Adjustable Fire
- No Fires
- Skyve, as a mod manager and user support context
- Other disaster, time, asset, or UI mods

Compatibility work should explain the current implementation state, expected player impact, detection method, warning severity, and recommended player action.

## QA And Release Expectations

- Produce or update QA checklists when implementing or proposing behavior changes.
- Separate QA for the legacy `Source/` implementation from QA for the new `DisasterCommandCenter/` migration.
- Include manual test coverage for startup, settings persistence, disaster behavior, manual triggers, evacuation behavior, save/load, compatibility, logs, and migration/import behavior.
- When preparing releases, update the files under `docs/release/`.
- Keep Steam Workshop communication clear, professional, and player-facing.
