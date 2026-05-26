# Jira Backlog Proposal

Source documents:

- [FutureWork.md](FutureWork.md)
- [UiCoreSeparationTODO.md](UiCoreSeparationTODO.md)

This backlog separates the current future-work notes into Jira-friendly epics and stories. Story IDs are proposed
temporary keys for planning only; replace them with real Jira keys after import.

## Epic NDR-ARCH: Separate UI From Core Disaster Logic

Goal:

- Make the UI render prepared state and dispatch commands only.
- Move disaster state mutation, simulation actions, settings persistence, diagnostics, and view-model preparation
  into application/core services.

Business value:

- Reduces regression risk when changing UI.
- Makes future testing strategy feasible.
- Avoids duplicating work by stabilizing ownership boundaries before broad automated tests are designed.

### Story NDR-ARCH-001: Extract Disaster Commands From In-Game Panel

- Type: Story
- Priority: Highest
- Source: `UiCoreSeparationTODO.md` Stage 1

User story:

- As a maintainer, I want `InGameDisastersPanel` to call command services instead of processing disaster state directly,
  so UI actions do not contain simulation logic.

Acceptance criteria:

- `DisasterCommandService` exists under `Source/Application`.
- Emergency stop behavior is moved or wrapped behind `DisasterCommandService.StopAllDisasters()`.
- Reset progress behavior is moved or wrapped behind `DisasterCommandService.ResetAllDisasterProgress()`.
- Enable/disable disaster behavior is moved or wrapped behind `DisasterCommandService.ToggleDisasterEnabled(...)`.
- `InGameDisastersPanel` no longer directly uses `Services.Vehicles`, `Services.Water`, `Services.Terrain`,
  or `Services.Disasters`.
- Existing stop/reset/toggle behavior still works in game.

### Story NDR-ARCH-002: Introduce Disaster Panel View Models

- Type: Story
- Priority: Highest
- Depends on: NDR-ARCH-001
- Source: `UiCoreSeparationTODO.md` Stage 2

User story:

- As a UI maintainer, I want the in-game disaster panel to render from view models, so it does not pull domain data
  directly from global services.

Acceptance criteria:

- `DisasterPanelViewModel` exists under `Source/Application/ViewModels`.
- `DisasterRowViewModel` exists under `Source/Application/ViewModels`.
- `DisasterPanelController.BuildViewModel()` exists.
- `InGameDisastersPanel` builds rows from `DisasterPanelViewModel.Rows`.
- `DisasterRowHelper` renders from `DisasterRowViewModel` instead of `DisasterBaseModel`.
- `InGameDisastersPanel` no longer reads `Services.DisasterHandler.container.AllDisasters` directly.

### Story NDR-ARCH-003: Move Panel And Overlay Creation Out Of NaturalDisasterHandler

- Type: Story
- Priority: Highest
- Depends on: NDR-ARCH-001
- Source: `UiCoreSeparationTODO.md` Stage 3

User story:

- As a maintainer, I want `NaturalDisasterHandler` to stop creating concrete UI components, so core orchestration
  stays independent from UI implementation.

Acceptance criteria:

- `DisasterPanelUiManager` exists under `Source/UI`.
- `DisasterPanelUiManager` owns creation of:
  - `InGameDisastersPanel`
  - panel toggle `UIButton`
  - `ShelterHoverDebugOverlay`
- `NaturalDisasterHandler` no longer stores `InGameDisastersPanel`, `ShelterHoverDebugOverlay`, or `UIButton`.
- `NaturalDisasterHandler.cs` has no `using NaturalDisastersRenewal.UI`.
- `LoadingExtension.cs` does not call `ModSettingsScreen.UpdateUISettingsOptions()` directly.

### Story NDR-ARCH-004: Move Settings Mutations Behind A Settings Controller

- Type: Story
- Priority: High
- Depends on: NDR-ARCH-003
- Source: `UiCoreSeparationTODO.md` Stage 4

User story:

- As a maintainer, I want settings UI callbacks to dispatch to a controller, so persistence and simulation side effects
  are not embedded in `ModSettingsScreen`.

Acceptance criteria:

- `DisasterSettingsController` exists under `Source/Application`.
- `SettingsViewModel` exists under `Source/Application/ViewModels`.
- `ModSettingsScreen` no longer calls `Services.DisasterSetup.Save()` directly.
- `ModSettingsScreen` no longer calls `Services.DisasterHandler.ReadValuesFromFile()` directly.
- `ModSettingsScreen` no longer calls `Services.DisasterHandler.ResetToDefaultValues()` directly.
- `ModSettingsScreen` no longer calls `DisasterExtension.SetDisableDisasterFocus()` directly.
- UI callback bodies are reduced to command dispatch and visual refresh.

### Story NDR-ARCH-005: Extract Shelter Debug Data Provider

- Type: Story
- Priority: Medium
- Source: `UiCoreSeparationTODO.md` Stage 5

User story:

- As a maintainer, I want shelter debug calculations outside the overlay UI, so the overlay only renders prepared debug data.

Acceptance criteria:

- `ShelterDebugInfoProvider` exists under `Source/Application`.
- `ShelterDebugInfoViewModel` exists under `Source/Application/ViewModels`.
- `ShelterHoverDebugOverlay` asks the provider for current debug data.
- `ShelterHoverDebugOverlay` no longer directly reads `Services.Buildings`, `Services.Disasters`,
  `Services.Simulation`, or `Services.Terrain`.
- Most `TryGet...` calculation methods are moved out of the UI class.

### Story NDR-ARCH-006: Add A UI Refresh Boundary

- Type: Story
- Priority: High
- Depends on: NDR-ARCH-003, NDR-ARCH-004
- Source: `UiCoreSeparationTODO.md` Stage 6

User story:

- As a maintainer, I want core/application code to refresh UI through a narrow boundary, so core does not call concrete
  UI screens.

Acceptance criteria:

- `IUiRefreshService` or equivalent application event boundary exists.
- Application/core code does not call `ModSettingsScreen.UpdateUISettingsOptions()` directly.
- Application/core code does not call `InGameDisastersPanel.Refresh()` directly.
- Settings, localization, and panel state changes can request refresh through the boundary.

### Story NDR-ARCH-007: Remove Remaining Global Services Usage From UI

- Type: Story
- Priority: High
- Depends on: NDR-ARCH-001, NDR-ARCH-002, NDR-ARCH-004, NDR-ARCH-005
- Source: `UiCoreSeparationTODO.md` Stage 7

User story:

- As a maintainer, I want `Source/UI` to stop reaching into global game services directly, so UI remains a rendering layer.

Acceptance criteria:

- `rg -n "Services\\." Source\\UI` returns zero or only documented exceptions.
- Any exception has a short code comment explaining why direct UI access is necessary.
- Domain/simulation reads are moved to command services, view-model builders, providers, or UI managers.

### Story NDR-ARCH-008: Remove Cross-UI Static Coordination

- Type: Story
- Priority: Medium
- Depends on: NDR-ARCH-006
- Source: `UiCoreSeparationTODO.md` Stage 8

User story:

- As a maintainer, I want UI components to avoid coordinating through static methods, so ownership is explicit.

Acceptance criteria:

- `ModSettingsScreen.IsCapturingHotkey` is replaced by a `HotkeyCaptureState` or equivalent boundary.
- `InGameDisastersPanel` no longer calls `ModSettingsScreen` directly.
- `NaturalDisasterHandler` no longer checks `ModSettingsScreen`.
- `rg -n "ModSettingsScreen\\." Source` shows only UI-owned creation/update or documented exceptions.

### Story NDR-ARCH-009: Define Testing Strategy After Architecture Stabilizes

- Type: Story
- Priority: Medium
- Depends on: NDR-ARCH-001 through NDR-ARCH-008
- Source: `UiCoreSeparationTODO.md` Stage 9

User story:

- As a maintainer, I want a testing strategy after the architecture boundaries are stable, so tests do not lock in the
  temporary mixed design.

Acceptance criteria:

- `Source/Versions/FutureWork/TestingStrategyTODO.md` exists.
- The testing plan distinguishes automated tests, manual in-game checks, and compatibility smoke tests.
- The plan identifies what can be tested without Cities: Skylines runtime.
- The plan includes regression flows for generated disasters, manual spawns, emergency stop, reset state, settings
  save/load, level load/unload, and Real Time behavior.

## Epic NDR-STAB: Stability And Compatibility Requests

Goal:

- Address high-priority community requests around reliability, compatibility, diagnostics, and disaster state control.

### Story NDR-STAB-001: Improve Forest Fire Compatibility Diagnostics

- Type: Story
- Priority: Highest
- Source: `FutureWork.md` Forest Fire Compatibility Follow-Up

User story:

- As a player, I want clear diagnostics when forest fires are blocked or altered by other mods, so I understand why
  forest fires are not spawning.

Acceptance criteria:

- Forest-fire diagnostics distinguish:
  - disaster enabled/disabled,
  - tree fire spreading available/suppressed,
  - external mod conflict detected,
  - weather/fog/rain blocking conditions.
- Detected forest-fire behavior mods show explanatory warning text, not only active/inactive state.
- The feasibility of an "Allow tree fire spreading" setting is documented.
- The feasibility of a "Force forest fire availability" setting is documented or implemented.

### Story NDR-STAB-002: Split Disaster Destruction Settings By Target Type

- Type: Story
- Priority: Highest
- Source: `FutureWork.md` Disaster Destruction Consistency

User story:

- As a player, I want separate destruction controls for buildings, roads, rail/metro, and terrain deformation, so I can
  control disaster damage predictably.

Acceptance criteria:

- Current road destruction behavior is verified.
- Rail/metro destruction support is evaluated.
- Terrain deformation side effects on later destruction are investigated.
- Destruction settings are split or a design note explains why a target type is not feasible.
- Skipped destruction logs include cause when possible.

### Story NDR-STAB-003: Redesign Emergency And Disaster State Controls

- Type: Story
- Priority: Highest
- Depends on: NDR-ARCH-001
- Source: `FutureWork.md` Emergency And Disaster State Controls

User story:

- As a player, I want emergency stop, disable toggles, and reset state to be separate actions, so I can recover from
  disaster issues without confusing saved settings.

Acceptance criteria:

- Reported re-enable issue after emergency actions is verified.
- Controls are separated into:
  - disable mod-generated disasters,
  - disable vanilla disasters,
  - emergency stop active disasters,
  - reset disaster state/progress.
- Reset disaster state clears cooldown/progress/active-disaster tracking without changing saved settings.
- Tsunami stop limitations are documented in UI or tooltip text.

### Story NDR-STAB-004: Verify Manual Spawn Intensity Reliability

- Type: Story
- Priority: Highest
- Source: `FutureWork.md` Manual Spawn Intensity Reliability

User story:

- As a player, I want manually spawned disasters to respect configured intensity rules, so manual testing and gameplay
  are predictable.

Acceptance criteria:

- Manual spawn behavior is tested for all disaster types.
- Logs include requested manual intensity and final applied base-game intensity.
- Logs include configured cap and population-scaled cap when relevant.
- Behavior is defined for requests above active cap.
- Follow-up implementation stories are created if defects are found.

### Story NDR-STAB-005: Add Force Unlock All Disasters Action

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Unlock Disaster Controls

User story:

- As a player or tester, I want a force unlock action, so I can recover from unlock bugs or validate disaster behavior
  in new cities/scenarios.

Acceptance criteria:

- Current unlock state is logged after level load.
- Current milestone is logged after level load.
- A "Force unlock all disasters" action is available in a suitable debug/settings area.
- New city, loaded city, and scenario unlock behavior is verified.

### Story NDR-STAB-006: Expand Compatibility Monitor

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Compatibility Monitor Expansion

User story:

- As a player, I want compatibility warnings to explain impact and recommended action, so I can resolve mod conflicts.

Acceptance criteria:

- Compatibility definitions include Ragnarok and Natural Disasters Overhaul or similar ND behavior mods.
- 81 Tiles 2 compatibility is documented only if a specific issue is confirmed.
- Dependency rows show mod name, expected impact, recommended action, and severity.
- Rows distinguish informational status, warning, and conflict.

### Story NDR-STAB-007: Audit Load And NullReference Safety

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` NullReference And Load-Safety Audit

User story:

- As a player, I want level load/unload paths to be robust, so the mod does not fail during new games, saves, scenarios,
  or unload-to-menu.

Acceptance criteria:

- New game, load game, new scenario, and unload-to-menu paths are manually verified.
- Optional game panels/managers are guarded where needed.
- NullReference risks found during audit are fixed or logged as separate stories.
- 81 Tiles 2 is smoke-tested only if a reproducible issue/API interaction is identified.

## Epic NDR-ADV: Advanced Disaster Control

Goal:

- Add clearer advanced tuning while preserving stable defaults.

### Story NDR-ADV-001: Split Intensity Caps By Usage

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Per-Disaster Intensity Caps

User story:

- As a player, I want separate random, manual, and absolute intensity caps, so generated and manually spawned disasters
  are controlled independently.

Acceptance criteria:

- Design defines random/generated max, manual spawn max, and absolute safety cap.
- UI labels/tooltips explain which cap affects which flow.
- Defaults preserve current 1.3.0 conservative behavior.
- Serialization migration is defined for existing saves/settings.

### Story NDR-ADV-002: Add Behavior Profiles

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Behavior Profiles

User story:

- As a player, I want behavior profiles, so I can quickly choose a simple, Ragnarok-like, full NDR, or custom experience.

Acceptance criteria:

- Profiles are defined:
  - Lite Mode,
  - Ragnarok Mode,
  - Full NDR Mode,
  - Custom Mode.
- Each profile lists owned settings.
- Switching profiles previews impact or asks confirmation before overwriting custom settings.

### Story NDR-ADV-003: Add Advanced Frequency Controls

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Advanced Frequency Control

User story:

- As an advanced player, I want numeric frequency and cooldown controls, so I can tune disaster occurrence more precisely.

Acceptance criteria:

- Per-disaster "disasters per year" controls are designed or implemented.
- Cooldown override controls are designed or implemented.
- Max active disasters setting is designed or implemented.
- Overlapping-disasters behavior is configurable or explicitly documented as unsupported.
- Real Time estimates remain understandable.

### Story NDR-ADV-004: Add Advanced Real Time Tuning

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Real Time Advanced Integration

User story:

- As a Real Time player, I want more precise Real Time tuning, so disaster recurrence feels balanced in slower timelines.

Acceptance criteria:

- Existing per-disaster Real Time presets remain available.
- Optional numeric frequency scaling per disaster is designed or implemented.
- Meteor period customization for Real Time is designed or implemented.
- UI shows effective expected recurrence in game-time and real-time terms.

### Story NDR-ADV-005: Add Settings Presets

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Presets System

User story:

- As a player, I want ready-made presets, so I can quickly apply a known disaster balance.

Acceptance criteria:

- Presets are defined:
  - Vanilla-like,
  - Ragnarok-like,
  - Safe coastal city,
  - Apocalypse,
  - Real Time balanced.
- Applying a preset previews or summarizes changes.
- User custom settings are preserved unless replacement is confirmed.

## Epic NDR-EVAC: Evacuation And Shelter Improvements

Goal:

- Improve evacuation clarity and shelter-related control without breaking current focused evacuation behavior.

### Story NDR-EVAC-001: Expand Evacuation Behavior Options

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Evacuation Behavior Expansion; `FutureWork.md` Split Tsunami Evacuation Behavior

User story:

- As a player, I want clearer evacuation behavior options, so I can choose between vanilla AI, nearest shelter behavior,
  warning-based evacuation, and auto release behavior.

Acceptance criteria:

- Current evacuation modes are reviewed and documented.
- Tsunami behavior is split into:
  - auto evacuate/release,
  - auto evacuate only.
- Existing focused evacuation/release behavior remains intact.
- Any unsupported behavior is documented before implementation.

### Story NDR-EVAC-002: Evaluate Shelter Stock And Consumption Controls

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Shelter Improvements

User story:

- As a player, I want shelter stock and consumption options, so shelters remain useful during disaster-heavy games.

Acceptance criteria:

- Feasibility of keeping shelters stocked is documented.
- Feasibility of ignoring shelter consumption is documented.
- Risks to base-game balance or save integrity are documented.
- Implementation stories are created if feasible.

### Story NDR-EVAC-003: Add Shelter Monitoring Panel

- Type: Story
- Priority: Low
- Depends on: NDR-EVAC-002
- Source: `FutureWork.md` Shelter Improvements

User story:

- As a player, I want a shelter monitoring panel, so I can see capacity, stock, evacuation state, and active disaster risks.

Acceptance criteria:

- Panel lists shelter capacity and stock/resource status where available.
- Panel shows active evacuation state.
- Panel shows disasters currently affecting each shelter.
- Panel refreshes without embedding shelter calculations directly in UI.

### Story NDR-EVAC-004: Evaluate Alarm Customization

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Alarm Customization

User story:

- As a player, I want disaster alarm controls, so I can adjust or disable alarm behavior.

Acceptance criteria:

- Base-game API support for alarm volume is evaluated.
- Enable/disable alarm setting feasibility is evaluated.
- Custom alarm sound support and asset-loading constraints are documented.
- Implementation stories are created if feasible.

## Epic NDR-UXDOC: UX, Accessibility, And Documentation

Goal:

- Reduce player confusion through clearer controls, startup guidance, and documentation.

### Story NDR-UXDOC-001: Add Open Panel Action In Settings

- Type: Story
- Priority: Low
- Source: `FutureWork.md` UX Follow-Up

User story:

- As a player, I want an "Open panel" button in settings, so I can find the in-game disaster panel easily.

Acceptance criteria:

- Settings screen includes an open/show panel action.
- Action works even if the panel was hidden.
- Action does not overwrite saved panel position.

### Story NDR-UXDOC-002: Add Optional Startup Notification

- Type: Story
- Priority: Low
- Source: `FutureWork.md` UX Follow-Up

User story:

- As a new player, I want a small startup hint, so I know how to open the NDR panel.

Acceptance criteria:

- Startup notification copy is defined.
- Notification can be disabled or is non-intrusive.
- Default hotkey text reflects the actual configured hotkey when possible.

### Story NDR-UXDOC-003: Clarify Reset Actions In UI

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` UX Follow-Up

User story:

- As a player, I want reset actions to use precise names, so I do not confuse settings reset, UI reset, and disaster-state reset.

Acceptance criteria:

- UI distinguishes:
  - reset saved settings,
  - reset defaults,
  - reset panel/button position,
  - reset disaster state.
- Tooltips explain impact of each reset action.
- Existing reset behavior remains unchanged unless separately planned.

### Story NDR-UXDOC-004: Add DLC And FAQ Documentation

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Documentation Follow-Up

User story:

- As a player, I want clear setup and compatibility documentation, so I can install and troubleshoot the mod.

Acceptance criteria:

- DLC requirements are clearly documented.
- FAQ includes:
  - mod not appearing in Skyve,
  - Real Time compatibility,
  - Game Anarchy / Tree Fire Control / fire behavior conflicts,
  - Ragnarok or disaster overhaul conflicts,
  - reasons disasters may appear too frequent or too rare.

### Story NDR-UXDOC-005: Verify Scenario Support

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Scenario Support

User story:

- As a scenario player, I want NDR behavior to work predictably in scenarios, so scenario rules do not silently break disasters.

Acceptance criteria:

- Scenario start behavior is verified.
- Disaster settings and unlock logic are verified in scenarios.
- Scenario-specific restrictions are documented.
- A separate setting is proposed only if scenario rules require it.

## Epic NDR-FEATURE: Future Gameplay Features

Goal:

- Track larger gameplay enhancements that should remain separate from the UI/core refactor.

### Story NDR-FEATURE-001: Add Reconstruction Helper Panel

- Type: Story
- Priority: Medium
- Source: `FutureWork.md` Add Reconstruction Helper Panel

User story:

- As a player, I want a reconstruction helper panel, so I can find and repair disaster-damaged public/service assets faster.

Acceptance criteria:

- Rebuildable buildings are scanned safely.
- Listed buildings include name/type and district or position context.
- Camera can move to the selected building.
- Quick rebuild is added only if the game API allows it safely.
- Disaster-damaged roads are detected and repair feasibility is evaluated.

### Story NDR-FEATURE-002: Evaluate Separate Nuclear Incident Event

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Evaluate Separate Nuclear Incident Event

User story:

- As a player, I want nuclear incidents evaluated as a distinct event, so reactor failures are not mixed into normal forest fires.

Acceptance criteria:

- Valid reactor/power plant target detection is evaluated.
- Trigger options are documented.
- Contamination simulation feasibility is documented.
- Tuning options are proposed.
- Safe fallback behavior is defined when no valid reactor exists.

### Story NDR-FEATURE-003: Evaluate Populated-Area Meteor Targeting

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Evaluate Populated-Area Meteor Targeting

User story:

- As a player, I want an optional meteor targeting mode near populated areas, so meteor strikes can feel more threatening
  without replacing vanilla/random targeting.

Acceptance criteria:

- Vanilla/random targeting remains default.
- Optional populated-area targeting is designed.
- Offset targeting is evaluated to avoid always hitting buildings directly.
- Safe fallback behavior is defined when no populated target exists.
- Real Time affects timing only, not target selection.

### Story NDR-FEATURE-004: Evaluate Coastal Shelter Evacuation For Water Meteor Impacts

- Type: Story
- Priority: Low
- Source: `FutureWork.md` Evaluate Coastal Shelter Evacuation For Water Meteor Impacts

User story:

- As a coastal-city player, I want shelters near water meteor impacts to evacuate when appropriate, so water impacts have
  believable coastal risk.

Acceptance criteria:

- Water impact detection is evaluated using terrain and water height.
- Optional setting is disabled by default.
- Coastal shelter inference is defined by water proximity/elevation.
- Inland shelters are not evacuated unnecessarily.
- Normal focused evacuation behavior remains unchanged for land impacts.

## Suggested Jira Import Columns

For manual import, map each entry to:

- Issue Type: Epic or Story
- Summary: story heading without proposed key
- Epic Link: parent epic key
- Priority: listed priority
- Description: user story + source
- Acceptance Criteria: acceptance criteria list
- Dependencies: listed `Depends on`
- Labels:
  - `ndr`
  - `future-work`
  - `architecture`, `stability`, `compatibility`, `advanced-control`, `evacuation`, `ux`, or `feature`

## Recommended Delivery Order

1. NDR-ARCH stories through NDR-ARCH-008.
2. NDR-STAB high-priority stories.
3. NDR-ARCH-009 testing strategy.
4. NDR-STAB medium-priority stories.
5. NDR-ADV and NDR-EVAC medium-priority stories.
6. NDR-UXDOC low/medium documentation and UX stories.
7. NDR-FEATURE exploratory gameplay stories.
