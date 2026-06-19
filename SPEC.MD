# Natural Disasters Renewal / Disaster Command Center Spec

Last updated: 2026-06-17

This file is the single starting point for Codex, Claude, or any future agent working in this repository. Read this before changing code, writing release material, planning QA, or porting features.

## Project Identity

- Current working mod: `Natural Disasters Renewal` / `Disaster Command Center` (same Steam Workshop mod, id `2957578256` — in-place upgrade, not a separate listing)
- Current legacy source version: `1.3.0`
- Game target: `Cities: Skylines 1`
- Framework target: `.NET Framework v3.5`
- Single solution: `NaturalDisastersRenewal.sln`, containing two projects — `Source/NaturalDisastersRenewal.csproj` (frozen legacy) and `DisasterCommandCenter/DisasterCommandCenter.csproj` (active project)

Important distinction:

- `Source/` is now **frozen** — it is the working legacy implementation and behavior reference (version `1.3.0`). Do not modify files under `Source/` going forward; it exists only for parity comparison.
- `DisasterCommandCenter/` is the **active project**. It was bootstrapped as a verbatim copy of `Source/` 1.3.0 (same `namespace NaturalDisastersRenewal.*`, same `CommonProperties.cs` constants/Steam id, so save/settings files written by either build stay compatible), and is where all new work happens, starting with extracting domain logic out of UI classes (see `Source/Handlers/DisasterActionsService.cs` equivalent under `DisasterCommandCenter/Handlers/`).
- Compare new `DisasterCommandCenter/` work against `Source/` 1.3.0 for behavior parity, since they started as the same code.

## Current Functional Scope

The mod replaces the base game's random disaster flow with configurable disaster timing, intensity, evacuation, UI, compatibility status, and persistence.

Core gameplay features:

- Natural disaster recurrence based on weather, season, cooldowns, time since last disaster, and population scaling.
- Real Time compatible recurrence paths for forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, and meteor strike.
- Per-disaster enable/disable controls.
- Configurable evacuation modes: manual, automatic evacuate/release, and focused automatic evacuate/release where supported.
- Focused evacuation radius and shelter release behavior.
- Optional pause when disasters start.
- Optional disabling of automatic camera follow.
- Maximum generated intensity controls, including advanced extreme-intensity support.
- Disaster event logging to CSV when enabled.
- Emergency stop and disaster progress reset controls in the in-game panel.

Supported disaster models:

- `Thunderstorm`
- `ForestFire`
- `Sinkhole`
- `Tornado`
- `Earthquake`
- `Tsunami`
- `MeteorStrike`

Compatibility checks currently target:

- Real Time
- Extended InfoPanel 2
- ACME
- Tree Fire Control
- Game Anarchy
- Rain Firefighting
- Adjustable Fire
- No Fires

## Existing Files And Responsibilities

Note: `DisasterCommandCenter/` mirrors the `Source/` file list below 1:1 (same relative paths, same namespaces), except where this spec says otherwise (e.g. `DisasterCommandCenter/Handlers/DisasterActionsService.cs` is new — see Future Improvements Roadmap). When editing active code, edit it under `DisasterCommandCenter/`, not `Source/`.

### Root Documentation

- `README.md`: player-facing overview for Natural Disasters Renewal `1.3.0`, including disaster behavior, evacuation modes, panel usage, emergency button, and logging.
- `SteamDescription.txt`: Steam Workshop-facing text. Currently modified in the worktree.
- `LICENSE.txt`: project license.
- `SPEC.MD`: this agent-facing source of truth.

### Release Documentation

- `docs/release/release-notes-v2.md`: template for future GitHub/Steam release notes, especially Disaster Command Center successor releases.
- `docs/release/steam-workshop-description.md`: draft Workshop description template for Disaster Command Center. Keep claims marked TBD until implemented and tested.
- `Source/Versions/Changelog.txt`: historical release notes from `1.0` through `1.3.0`.
- `Source/Versions/FutureWork/FutureWork.txt`: existing future-work list moved out of inline TODO comments.

### Mod Entry Points

- `Source/Mod.cs`: ICities user mod entry point. Exposes mod name, description, and settings UI.
- `Source/BaseGameExtensions/LoadingExtension.cs`: initializes compatibility checks, in-game panel, settings UI state, unlocks, and max intensity on level load; resets runtime state on unload.
- `Source/BaseGameExtensions/DisasterExtension.cs`: receives game disaster lifecycle events and forwards them to `NaturalDisasterHandler`; logs started disasters.
- `Source/BaseGameExtensions/Threading.cs`: runs per-frame disaster logic and prevents original random disasters from occurring.

### Central Runtime Services

- `Source/Handlers/NaturalDisasterHandler.cs`: central coordinator for settings, Harmony patches, disaster lifecycle routing, UI creation, hotkeys, panel/button positions, and unlock checks.
- `Source/Common/Services.cs`: cached access to Cities: Skylines singletons such as disaster, simulation, building, terrain, water, plugin, game area, weather, and unlock managers.
- `Source/Common/CommonProperties.cs`: mod constants, version, Steam id, file names, local app data paths, and mod description.
- `Source/Common/DisasterSimulationUtils.cs`: shared simulation helper logic and compatibility helper calls.
- `Source/Common/DisasterRecurrenceTuning.cs`: shared recurrence tuning helpers.
- `Source/Common/RealTimeDisasterTiming.cs`: Real Time aware timing utilities.
- `Source/Common/ModCompatibilityService.cs`: reads embedded compatibility metadata and detects active mods.
- `Source/Resources/Files/CompatibleMods.json`: embedded compatibility metadata for known related mods.

### Disaster Models

- `Source/Models/NaturalDisaster/DisasterBaseModel.cs`: base behavior, shared settings, probability/intensity helpers, enabled state, and lifecycle methods.
- `Source/Models/NaturalDisaster/ForestFireModel.cs`: dry-weather and Real Time forest-fire logic, fog/rain effects, evacuation handling.
- `Source/Models/NaturalDisaster/ThunderstormModel.cs`: seasonal/rain-based thunderstorm logic and Real Time frequency presets.
- `Source/Models/NaturalDisaster/SinkholeModel.cs`: groundwater/rain-based sinkhole logic and Real Time wet-interval logic.
- `Source/Models/NaturalDisaster/TornadoModel.cs`: seasonal tornado logic, fog/rain modifiers, destruction settings, and evacuation handling.
- `Source/Models/NaturalDisaster/EarthquakeModel.cs`: earthquake recurrence, aftershocks, crack settings, intensity handling, and evacuation.
- `Source/Models/NaturalDisaster/TsunamiModel.cs`: tsunami recurrence, wave/risk handling, and evacuation behavior.
- `Source/Models/NaturalDisaster/MeteorStrikeModel.cs`: meteor stream and Real Time meteor frequency logic.

### Settings And Persistence

- `Source/Models/Setup/DisasterSetupModel.cs`: in-memory and XML-backed settings model; owns all disaster model instances and general options.
- `Source/Serialization/Setup/LoadedGameSerializableDataExtension.cs`: save/load hook for per-save disaster settings.
- `Source/Serialization/Setup/SerializableDataDisasterSetup.cs`: serializes general setup state.
- `Source/Serialization/NaturalDisaster/*.cs`: serializes per-disaster settings.
- `Source/Common/HotkeyHelper.cs`: hotkey formatting and matching.
- `Source/Common/enums/*.cs`: setting enums for evacuation, cracks, language, occurrence area, probability distribution, and Real Time frequency presets.

### User Interface

- `Source/UI/InGameDisastersPanel.cs`: in-game panel with statistics and controls tabs, disaster rows, dependency status, help, emergency stop, and reset.
- `Source/UI/ModSettingsScreen.cs`: options screen, section navigation, general settings, disaster-specific settings, debug progress controls, hotkey capture, and save/reset actions.
- `Source/UI/ShelterHoverDebugOverlay.cs`: debug overlay for inspecting shelter/disaster behavior.
- `Source/UI/SettingsSections/AboutSettingsSection.cs`: About tab content and links.
- `Source/UI/SettingsSections/DependenciesSettingsSection.cs`: dependency/compatibility section.
- `Source/UI/ComponentHelper/*.cs`: reusable UI helpers for labels, buttons, icons, rows, progress bars, scroll panels, sliders, checkboxes, hotkeys, styles, tabs, and dropdowns.
- `Source/UI/Extensions/UIHelperExtensions.cs`: helper extensions for settings UI construction.
- `Source/Resources/Images/*`: embedded images and SVG icons used by the UI.

### Patches, Logs, And Models

- `Source/Services/HarmonyPatches/*.cs`: Harmony patches for modified disaster helper logic and destruction behavior.
- `Source/Logger/DebugLogger.cs`: mod debug logging.
- `Source/Logger/DisasterLogger.cs`: disaster event CSV logging.
- `Source/Models/Disaster/DisasterInfoModel.cs`: active disaster state wrapper.
- `Source/Models/Disaster/ShelterInfoModel.cs`: shelter-related model data.

### Agent Skills

These files were added to help Codex/Claude specialize work. Skills are intentionally mirrored in both `.codex/skills/` and `.claude/skills/`:

- `.codex/skills/product-manager/SKILL.md`: classify feedback, roadmap items, parity gaps, and feature requests.
- `.codex/skills/qa-tester/SKILL.md`: create manual QA, regression, compatibility, and release validation plans.
- `.codex/skills/release-manager/SKILL.md`: prepare Steam/GitHub release notes, known issues, and user-facing communication.
- `.codex/skills/ui-designer/SKILL.md`: visual design guidance for in-game UI and Workshop presentation.
- `.codex/skills/ux-designer/SKILL.md`: information architecture, player flow, tooltips, and settings clarity.
- `.codex/skills/ui-developer/SKILL.md`: implementation-oriented UI planning for Cities: Skylines 1 C# constraints.

When editing a skill, update the matching file under both `.codex/skills/<skill-name>/SKILL.md` and `.claude/skills/<skill-name>/SKILL.md`. Claude Code should be opened from the repository root so it can discover project skills under `.claude/skills/`.

## Build And Runtime Notes

- Both projects target old Cities: Skylines 1 assemblies from the local Steam install.
- NuGet/package dependencies include CitiesHarmony API/Harmony packages under the shared `packages/` folder at repo root (both `Source/` and `DisasterCommandCenter/` resolve it via `..\packages\`).
- Each `.csproj` generates its own `Common/BuildInfo.cs` before build.
- Both projects live in the same `NaturalDisastersRenewal.sln`, so `$(SolutionName)` resolves identically for both. `Source/NaturalDisastersRenewal.csproj` still deploys via `$(SolutionName)` to `Addons\Mods\NaturalDisastersRenewal\` (unchanged). `DisasterCommandCenter/DisasterCommandCenter.csproj` overrides its `PostBuildEvent` with a hardcoded folder name (`Addons\Mods\DisasterCommandCenter\`) instead of `$(SolutionName)`, specifically to avoid both projects deploying into the same mods folder.
- **Never enable both mods at the same time in the in-game Content Manager.** They share the same namespace, Harmony patches, and Steam id — running both simultaneously double-applies Harmony patches and runs two parallel `NaturalDisasterHandler` singletons. Enable only the one you are currently testing.
- Avoid APIs unavailable to `.NET Framework v3.5` and Unity 5-era Cities: Skylines 1.
- Many runtime behaviors need manual in-game validation; do not assume compile success proves simulation correctness.

## Change Safety Rules For Agents

- Do not modify files under `Source/`. It is frozen as the `1.3.0` behavior reference. Make all new changes under `DisasterCommandCenter/`.
- Treat `Source/` 1.3.0 as the source of truth for *expected* behavior when verifying `DisasterCommandCenter/` changes preserve parity.
- Keep migration or successor claims separate from implemented legacy behavior.
- Preserve save compatibility unless a migration plan and QA checklist exist.
- Update serialization carefully. If adding persisted fields, verify XML defaults and per-save `DataVersion` implications.
- Update `LocalizationService` for all player-facing strings in both English and Spanish when possible.
- Update UI helper usage consistently rather than creating one-off controls.
- Update `Source/Versions/Changelog.txt`, `Source/Versions/FutureWork/FutureWork.txt`, release templates, or this spec when behavior or roadmap changes.
- Use the relevant `.codex/skills/*` file for planning-heavy tasks.

## Future Improvements Roadmap

### High Priority: Migration And Source Hygiene

- Done: `DisasterCommandCenter/` now contains real checked-in source (a 1:1 copy of `Source/` 1.3.0 plus the `DisasterActionsService` extraction below). `Source/` is frozen.
- Done: parity checklist from `Source/` `1.3.0` to `DisasterCommandCenter/` (15 manual test cases covering mod isolation in Content Manager, save/settings migration, log output, in-game panel actions, the shelter debug overlay, and Real Time frequency harmony warnings) — all 15 verified manually in-game by the maintainer. No regressions found from the architecture extraction.
- Confirm whether `SteamDescription.txt` and `docs/release/*` should describe Natural Disasters Renewal, Disaster Command Center, or both.
- Done: `UI/ShelterHoverDebugOverlay.cs` meteor/shelter query logic extracted to `Common/Development/ShelterFloodQueryService.cs`. Done: `UI/ModSettingsScreen.cs`'s Real Time frequency harmony apply/mismatch logic moved to `DisasterSetupModel`. Remaining: the ~80 single-line slider/checkbox property writes in `ModSettingsScreen.cs` still assign directly to `disasterContainer.<Property>` from UI callbacks — assessed as low risk (no business rule involved), not yet extracted to a command layer.

### High Priority: Existing Future Work From Source

- Reconstruction helper panel for collapsed/damaged service buildings and damaged roads.
  - Source note: `Source/UI/InGameDisastersPanel.cs:219`
  - Target: version `2.0`
  - Needs list, camera focus, optional quick rebuild/repair, and safe filtering for reconstructable assets.

- Separate nuclear incident event instead of mixing special behavior into forest fires.
  - Source note: `Source/Models/NaturalDisaster/ForestFireModel.cs:730`
  - Target: version `2.0`
  - Needs reactor targeting, fire/collapse/explosion tuning, evacuation radius, pollution/contamination simulation, and safe fallback when no reactor exists.

- Configurable meteor targeting mode that prefers populated areas.
  - Source note: `Source/Models/NaturalDisaster/MeteorStrikeModel.cs:579`
  - Target: version `2.0`
  - Keep vanilla/random targeting default; add optional populated-area targeting with offset and fallback.

- Coastal shelter evacuation for water-impact meteor strikes.
  - Source note: `Source/Models/NaturalDisaster/MeteorStrikeModel.cs:588`
  - Target: version `2.0`
  - Detect water impact and optionally activate nearby coastal shelters without changing land-impact focused evacuation.

- Split tsunami evacuation into explicit modes.
  - Source note: `Source/Models/NaturalDisaster/TsunamiModel.cs:129`
  - Preserve current auto evacuate/release flow and add auto evacuate only.

### Medium Priority: UI And UX

- Clarify player-facing difference between emergency stop and reset progress.
- Continue separating basic settings from advanced/high-risk settings.
- Add clearer compatibility warning language for mods that alter disaster, fire, camera, or time behavior.
- Consider a dedicated recovery/reconstruction tab only after reconstruction logic exists.
- Review long localized strings for fit in Cities: Skylines 1 UI panels.

### Medium Priority: QA And Compatibility

- Build a manual QA matrix for each disaster with Real Time enabled and disabled.
- Test save/load settings persistence for every disaster and general option.
- Test compatibility status with each known compatible/conflicting mod active and inactive.
- Validate that disabling a disaster terminates active disabled disasters safely.
- Validate panel/button drag persistence at multiple UI scales and resolutions.
- Validate logging output path and CSV content when event logging is enabled.

### Medium Priority: Documentation And Release

- Convert `Source/Versions/FutureWork/FutureWork.txt` into GitHub issues or a structured roadmap when ready.
- Keep Workshop copy honest about what is implemented versus planned.
- Add migration notes only after settings/save import behavior has been implemented and tested.
- Keep changelog entries user-facing but precise.

### Low Priority: Technical Cleanup

- Consider replacing ad hoc JSON parsing in `ModCompatibilityService` only if a safe `.NET 3.5` compatible parser is already available or can be embedded without increasing risk.
- Review comments and stale names that still reference older mod names.
- Review generated/build artifacts and ignore rules for `bin/` and `obj/`.
- Add focused helper abstractions only when repeated UI or simulation logic becomes hard to maintain.

## Agent Workflow

When asked to work on this repo:

1. Read this file first.
2. Check `git status --short` and avoid overwriting user changes.
3. If changing gameplay, inspect the relevant model in `Source/Models/NaturalDisaster/`.
4. If changing settings or persistence, inspect `DisasterSetupModel` and matching serialization files.
5. If changing UI text, inspect `LocalizationService`.
6. If planning, use the matching `.codex/skills/*` file.
7. After implementation, document what changed and what was not verified in-game.
