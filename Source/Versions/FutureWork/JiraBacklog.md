# Jira Backlog

Generated from `JiraBacklog.csv`.

## Summary

- Total issues: 40
- Epics: 6
- Stories: 34

## Priority Breakdown

| Priority | Count |
| --- | ---: |
| Highest | 9 |
| High | 3 |
| Medium | 17 |
| Low | 11 |

## Epics

### [1] Separate UI From Core Disaster Logic

- Priority: Highest
- Labels: `ndr;future-work;architecture`
- Source: FutureWork.md; UiCoreSeparationTODO.md
- Description: Make the UI render prepared state and dispatch commands only. Move disaster state mutation, simulation actions, settings persistence, diagnostics, and view-model preparation into application/core services.
- Acceptance: UI renders prepared state only; commands and mutations live in application/core services; future testing strategy can target stable boundaries.

#### [2] Extract Disaster Commands From In-Game Panel

- Priority: Highest
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 1
- Description: As a maintainer, I want InGameDisastersPanel to call command services instead of processing disaster state directly, so UI actions do not contain simulation logic.
- Acceptance criteria: DisasterCommandService exists; emergency stop is behind StopAllDisasters; reset progress is behind ResetAllDisasterProgress; enable/disable is behind ToggleDisasterEnabled; InGameDisastersPanel no longer directly uses Vehicles, Water, Terrain, or Disasters services; existing stop/reset/toggle behavior still works.

#### [3] Introduce Disaster Panel View Models

- Priority: Highest
- Depends on: NDR-ARCH-001
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 2
- Description: As a UI maintainer, I want the in-game disaster panel to render from view models, so it does not pull domain data directly from global services.
- Acceptance criteria: DisasterPanelViewModel exists; DisasterRowViewModel exists; DisasterPanelController.BuildViewModel exists; InGameDisastersPanel builds rows from view model rows; DisasterRowHelper renders from DisasterRowViewModel; InGameDisastersPanel no longer reads Services.DisasterHandler.container.AllDisasters directly.

#### [4] Move Panel And Overlay Creation Out Of NaturalDisasterHandler

- Priority: Highest
- Depends on: NDR-ARCH-001
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 3
- Description: As a maintainer, I want NaturalDisasterHandler to stop creating concrete UI components, so core orchestration stays independent from UI implementation.
- Acceptance criteria: DisasterPanelUiManager exists; UI manager owns InGameDisastersPanel, toggle UIButton, and ShelterHoverDebugOverlay creation; NaturalDisasterHandler no longer stores UI components; NaturalDisasterHandler has no NaturalDisastersRenewal.UI using; LoadingExtension does not call ModSettingsScreen.UpdateUISettingsOptions directly.

#### [5] Move Settings Mutations Behind A Settings Controller

- Priority: High
- Depends on: NDR-ARCH-003
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 4
- Description: As a maintainer, I want settings UI callbacks to dispatch to a controller, so persistence and simulation side effects are not embedded in ModSettingsScreen.
- Acceptance criteria: DisasterSettingsController exists; SettingsViewModel exists; ModSettingsScreen no longer directly calls DisasterSetup.Save, DisasterHandler.ReadValuesFromFile, DisasterHandler.ResetToDefaultValues, or DisasterExtension.SetDisableDisasterFocus; UI callbacks mostly dispatch commands and refresh visuals.

#### [6] Extract Shelter Debug Data Provider

- Priority: Medium
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 5
- Description: As a maintainer, I want shelter debug calculations outside the overlay UI, so the overlay only renders prepared debug data.
- Acceptance criteria: ShelterDebugInfoProvider exists; ShelterDebugInfoViewModel exists; ShelterHoverDebugOverlay asks provider for data; overlay no longer directly reads Buildings, Disasters, Simulation, or Terrain services; most TryGet calculation methods move out of UI.

#### [7] Add A UI Refresh Boundary

- Priority: High
- Depends on: NDR-ARCH-003; NDR-ARCH-004
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 6
- Description: As a maintainer, I want core/application code to refresh UI through a narrow boundary, so core does not call concrete UI screens.
- Acceptance criteria: IUiRefreshService or equivalent exists; application/core code does not call ModSettingsScreen.UpdateUISettingsOptions directly; application/core code does not call InGameDisastersPanel.Refresh directly; settings, localization, and panel changes refresh through the boundary.

#### [8] Remove Remaining Global Services Usage From UI

- Priority: High
- Depends on: NDR-ARCH-001; NDR-ARCH-002; NDR-ARCH-004; NDR-ARCH-005
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 7
- Description: As a maintainer, I want Source/UI to stop reaching into global game services directly, so UI remains a rendering layer.
- Acceptance criteria: `rg -n Services\\. Source\\UI` returns zero or documented exceptions; any exception explains why direct UI access is required; domain/simulation reads move to command services, view-model builders, providers, or UI managers.

#### [9] Remove Cross-UI Static Coordination

- Priority: Medium
- Depends on: NDR-ARCH-006
- Labels: `ndr;future-work;architecture`
- Source: UiCoreSeparationTODO.md Stage 8
- Description: As a maintainer, I want UI components to avoid coordinating through static methods, so ownership is explicit.
- Acceptance criteria: ModSettingsScreen.IsCapturingHotkey is replaced by HotkeyCaptureState or equivalent; InGameDisastersPanel no longer calls ModSettingsScreen directly; NaturalDisasterHandler no longer checks ModSettingsScreen; ModSettingsScreen static references are UI-owned or documented exceptions.

#### [10] Define Testing Strategy After Architecture Stabilizes

- Priority: Medium
- Depends on: NDR-ARCH-001 through NDR-ARCH-008
- Labels: `ndr;future-work;architecture;testing`
- Source: UiCoreSeparationTODO.md Stage 9
- Description: As a maintainer, I want a testing strategy after the architecture boundaries are stable, so tests do not lock in the temporary mixed design.
- Acceptance criteria: TestingStrategyTODO.md exists; plan distinguishes automated tests, manual in-game checks, and compatibility smoke tests; plan identifies what can be tested without Cities: Skylines runtime; regression flows include generated disasters, manual spawns, emergency stop, reset state, settings save/load, level load/unload, and Real Time behavior.

### [11] Stability And Compatibility Requests

- Priority: Highest
- Labels: `ndr;future-work;stability;compatibility`
- Source: FutureWork.md
- Description: Address high-priority community requests around reliability, compatibility, diagnostics, and disaster state control.
- Acceptance: High-priority community stability items are triaged, implemented or documented, and validated through release checks.

#### [12] Improve Forest Fire Compatibility Diagnostics

- Priority: Highest
- Labels: `ndr;future-work;stability;compatibility`
- Source: FutureWork.md Forest Fire Compatibility Follow-Up
- Description: As a player, I want clear diagnostics when forest fires are blocked or altered by other mods, so I understand why forest fires are not spawning.
- Acceptance criteria: Diagnostics distinguish enabled state, tree fire spreading availability, external mod conflicts, and weather/fog/rain blocking conditions; detected fire behavior mods show explanatory warning text; Allow tree fire spreading feasibility is documented; Force forest fire availability feasibility is documented or implemented.

#### [13] Split Disaster Destruction Settings By Target Type

- Priority: Highest
- Labels: `ndr;future-work;stability;compatibility`
- Source: FutureWork.md Disaster Destruction Consistency
- Description: As a player, I want separate destruction controls for buildings, roads, rail/metro, and terrain deformation, so I can control disaster damage predictably.
- Acceptance criteria: Road destruction behavior is verified; rail/metro destruction support is evaluated; terrain deformation side effects are investigated; destruction settings are split or infeasibility is documented; skipped destruction logs include cause when possible.

#### [14] Redesign Emergency And Disaster State Controls

- Priority: Highest
- Depends on: NDR-ARCH-001
- Labels: `ndr;future-work;stability`
- Source: FutureWork.md Emergency And Disaster State Controls
- Description: As a player, I want emergency stop, disable toggles, and reset state to be separate actions, so I can recover from disaster issues without confusing saved settings.
- Acceptance criteria: Reported re-enable issue is verified; controls separate disable mod disasters, disable vanilla disasters, emergency stop, and reset state/progress; reset disaster state clears cooldown/progress/active-disaster tracking without changing saved settings; tsunami stop limitations are documented.

#### [15] Verify Manual Spawn Intensity Reliability

- Priority: Highest
- Labels: `ndr;future-work;stability`
- Source: FutureWork.md Manual Spawn Intensity Reliability
- Description: As a player, I want manually spawned disasters to respect configured intensity rules, so manual testing and gameplay are predictable.
- Acceptance criteria: Manual spawn behavior is tested for all disaster types; logs include requested and final applied intensity; logs include configured and population-scaled caps when relevant; behavior above active cap is defined; follow-up implementation stories are created if defects are found.

#### [16] Add Force Unlock All Disasters Action

- Priority: Medium
- Labels: `ndr;future-work;stability`
- Source: FutureWork.md Unlock Disaster Controls
- Description: As a player or tester, I want a force unlock action, so I can recover from unlock bugs or validate disaster behavior in new cities/scenarios.
- Acceptance criteria: Current unlock state is logged after level load; current milestone is logged after level load; Force unlock all disasters action exists in a suitable debug/settings area; new city, loaded city, and scenario unlock behavior is verified.

#### [17] Expand Compatibility Monitor

- Priority: Medium
- Labels: `ndr;future-work;compatibility`
- Source: FutureWork.md Compatibility Monitor Expansion
- Description: As a player, I want compatibility warnings to explain impact and recommended action, so I can resolve mod conflicts.
- Acceptance criteria: Compatibility definitions include Ragnarok and Natural Disasters Overhaul or similar mods; 81 Tiles 2 compatibility is documented only if confirmed; dependency rows show mod name, impact, recommended action, and severity; rows distinguish info, warning, and conflict.

#### [18] Audit Load And NullReference Safety

- Priority: Medium
- Labels: `ndr;future-work;stability;compatibility`
- Source: FutureWork.md NullReference And Load-Safety Audit
- Description: As a player, I want level load/unload paths to be robust, so the mod does not fail during new games, saves, scenarios, or unload-to-menu.
- Acceptance criteria: New game, load game, scenario, and unload-to-menu paths are verified; optional game panels/managers are guarded where needed; NullReference risks are fixed or logged as stories; 81 Tiles 2 is smoke-tested only if a reproducible issue/API interaction is identified.

### [19] Advanced Disaster Control

- Priority: Medium
- Labels: `ndr;future-work;advanced-control`
- Source: FutureWork.md
- Description: Add clearer advanced tuning while preserving stable defaults.
- Acceptance: Advanced controls are introduced with safe defaults, clear tooltips, and migration handling.

#### [20] Split Intensity Caps By Usage

- Priority: Medium
- Labels: `ndr;future-work;advanced-control`
- Source: FutureWork.md Per-Disaster Intensity Caps
- Description: As a player, I want separate random, manual, and absolute intensity caps, so generated and manually spawned disasters are controlled independently.
- Acceptance criteria: Design defines random/generated max, manual spawn max, and absolute safety cap; UI labels/tooltips explain each cap; defaults preserve 1.3.0 behavior; serialization migration is defined.

#### [21] Add Behavior Profiles

- Priority: Medium
- Labels: `ndr;future-work;advanced-control`
- Source: FutureWork.md Behavior Profiles
- Description: As a player, I want behavior profiles, so I can quickly choose a simple, Ragnarok-like, full NDR, or custom experience.
- Acceptance criteria: Profiles are defined for Lite, Ragnarok, Full NDR, and Custom modes; each profile lists owned settings; switching previews impact or asks confirmation before overwriting custom settings.

#### [22] Add Advanced Frequency Controls

- Priority: Medium
- Labels: `ndr;future-work;advanced-control`
- Source: FutureWork.md Advanced Frequency Control
- Description: As an advanced player, I want numeric frequency and cooldown controls, so I can tune disaster occurrence more precisely.
- Acceptance criteria: Per-disaster disasters-per-year controls are designed or implemented; cooldown overrides are designed or implemented; max active disasters setting is designed or implemented; overlapping disaster behavior is configurable or documented unsupported; Real Time estimates remain understandable.

#### [23] Add Advanced Real Time Tuning

- Priority: Medium
- Labels: `ndr;future-work;advanced-control;compatibility`
- Source: FutureWork.md Real Time Advanced Integration
- Description: As a Real Time player, I want more precise Real Time tuning, so disaster recurrence feels balanced in slower timelines.
- Acceptance criteria: Existing per-disaster Real Time presets remain; optional numeric frequency scaling per disaster is designed or implemented; meteor period customization for Real Time is designed or implemented; UI shows expected recurrence in game-time and real-time terms.

#### [24] Add Settings Presets

- Priority: Medium
- Labels: `ndr;future-work;advanced-control`
- Source: FutureWork.md Presets System
- Description: As a player, I want ready-made presets, so I can quickly apply a known disaster balance.
- Acceptance criteria: Presets are defined for Vanilla-like, Ragnarok-like, Safe coastal city, Apocalypse, and Real Time balanced; applying a preset previews or summarizes changes; custom settings are preserved unless replacement is confirmed.

### [25] Evacuation And Shelter Improvements

- Priority: Medium
- Labels: `ndr;future-work;evacuation`
- Source: FutureWork.md
- Description: Improve evacuation clarity and shelter-related control without breaking current focused evacuation behavior.
- Acceptance: Evacuation and shelter features are clearer, safer, and do not regress focused evacuation/release flows.

#### [26] Expand Evacuation Behavior Options

- Priority: Medium
- Labels: `ndr;future-work;evacuation`
- Source: FutureWork.md Evacuation Behavior Expansion; FutureWork.md Split Tsunami Evacuation Behavior
- Description: As a player, I want clearer evacuation behavior options, so I can choose between vanilla AI, nearest shelter behavior, warning-based evacuation, and auto release behavior.
- Acceptance criteria: Current evacuation modes are reviewed and documented; tsunami behavior is split into auto evacuate/release and auto evacuate only; focused evacuation/release remains intact; unsupported behavior is documented before implementation.

#### [27] Evaluate Shelter Stock And Consumption Controls

- Priority: Low
- Labels: `ndr;future-work;evacuation`
- Source: FutureWork.md Shelter Improvements
- Description: As a player, I want shelter stock and consumption options, so shelters remain useful during disaster-heavy games.
- Acceptance criteria: Feasibility of keeping shelters stocked is documented; feasibility of ignoring consumption is documented; risks to balance or save integrity are documented; implementation stories are created if feasible.

#### [28] Add Shelter Monitoring Panel

- Priority: Low
- Depends on: NDR-EVAC-002
- Labels: `ndr;future-work;evacuation`
- Source: FutureWork.md Shelter Improvements
- Description: As a player, I want a shelter monitoring panel, so I can see capacity, stock, evacuation state, and active disaster risks.
- Acceptance criteria: Panel lists capacity and stock/resource status where available; panel shows active evacuation state; panel shows disasters affecting each shelter; panel refreshes without embedding shelter calculations directly in UI.

#### [29] Evaluate Alarm Customization

- Priority: Low
- Labels: `ndr;future-work;evacuation`
- Source: FutureWork.md Alarm Customization
- Description: As a player, I want disaster alarm controls, so I can adjust or disable alarm behavior.
- Acceptance criteria: Base-game API support for alarm volume is evaluated; enable/disable alarm setting feasibility is evaluated; custom alarm sound support and asset-loading constraints are documented; implementation stories are created if feasible.

### [30] UX Accessibility And Documentation

- Priority: Medium
- Labels: `ndr;future-work;ux;documentation`
- Source: FutureWork.md
- Description: Reduce player confusion through clearer controls, startup guidance, and documentation.
- Acceptance: User-facing controls and docs clearly explain setup, compatibility, reset actions, and common issues.

#### [31] Add Open Panel Action In Settings

- Priority: Low
- Labels: `ndr;future-work;ux`
- Source: FutureWork.md UX Follow-Up
- Description: As a player, I want an Open panel button in settings, so I can find the in-game disaster panel easily.
- Acceptance criteria: Settings screen includes open/show panel action; action works if panel was hidden; action does not overwrite saved panel position.

#### [32] Add Optional Startup Notification

- Priority: Low
- Labels: `ndr;future-work;ux`
- Source: FutureWork.md UX Follow-Up
- Description: As a new player, I want a small startup hint, so I know how to open the NDR panel.
- Acceptance criteria: Startup notification copy is defined; notification is optional or non-intrusive; default hotkey text reflects configured hotkey when possible.

#### [33] Clarify Reset Actions In UI

- Priority: Medium
- Labels: `ndr;future-work;ux`
- Source: FutureWork.md UX Follow-Up
- Description: As a player, I want reset actions to use precise names, so I do not confuse settings reset, UI reset, and disaster-state reset.
- Acceptance criteria: UI distinguishes reset saved settings, reset defaults, reset panel/button position, and reset disaster state; tooltips explain impact; existing behavior remains unchanged unless separately planned.

#### [34] Add DLC And FAQ Documentation

- Priority: Low
- Labels: `ndr;future-work;documentation`
- Source: FutureWork.md Documentation Follow-Up
- Description: As a player, I want clear setup and compatibility documentation, so I can install and troubleshoot the mod.
- Acceptance criteria: DLC requirements are documented; FAQ includes Skyve visibility, Real Time compatibility, fire behavior conflicts, disaster overhaul conflicts, and why disasters may be too frequent or too rare.

#### [35] Verify Scenario Support

- Priority: Low
- Labels: `ndr;future-work;ux;stability`
- Source: FutureWork.md Scenario Support
- Description: As a scenario player, I want NDR behavior to work predictably in scenarios, so scenario rules do not silently break disasters.
- Acceptance criteria: Scenario start behavior is verified; disaster settings and unlock logic are verified in scenarios; scenario restrictions are documented; separate setting is proposed only if scenario rules require it.

### [36] Future Gameplay Features

- Priority: Low
- Labels: `ndr;future-work;feature`
- Source: FutureWork.md
- Description: Track larger gameplay enhancements that should remain separate from the UI/core refactor.
- Acceptance: Exploratory gameplay features are evaluated independently from structural refactor work.

#### [37] Add Reconstruction Helper Panel

- Priority: Medium
- Labels: `ndr;future-work;feature`
- Source: FutureWork.md Add Reconstruction Helper Panel
- Description: As a player, I want a reconstruction helper panel, so I can find and repair disaster-damaged public/service assets faster.
- Acceptance criteria: Rebuildable buildings are scanned safely; listed buildings include name/type and district or position context; camera can move to selected building; quick rebuild is added only if game API allows it safely; disaster-damaged roads are detected and repair feasibility is evaluated.

#### [38] Evaluate Separate Nuclear Incident Event

- Priority: Low
- Labels: `ndr;future-work;feature`
- Source: FutureWork.md Evaluate Separate Nuclear Incident Event
- Description: As a player, I want nuclear incidents evaluated as a distinct event, so reactor failures are not mixed into normal forest fires.
- Acceptance criteria: Valid reactor/power plant target detection is evaluated; trigger options are documented; contamination simulation feasibility is documented; tuning options are proposed; safe fallback behavior is defined when no valid reactor exists.

#### [39] Evaluate Populated-Area Meteor Targeting

- Priority: Low
- Labels: `ndr;future-work;feature`
- Source: FutureWork.md Evaluate Populated-Area Meteor Targeting
- Description: As a player, I want an optional meteor targeting mode near populated areas, so meteor strikes can feel more threatening without replacing vanilla/random targeting.
- Acceptance criteria: Vanilla/random targeting remains default; optional populated-area targeting is designed; offset targeting is evaluated; safe fallback is defined when no populated target exists; Real Time affects timing only, not target selection.

#### [40] Evaluate Coastal Shelter Evacuation For Water Meteor Impacts

- Priority: Low
- Labels: `ndr;future-work;feature`
- Source: FutureWork.md Evaluate Coastal Shelter Evacuation For Water Meteor Impacts
- Description: As a coastal-city player, I want shelters near water meteor impacts to evacuate when appropriate, so water impacts have believable coastal risk.
- Acceptance criteria: Water impact detection is evaluated using terrain and water height; optional setting is disabled by default; coastal shelter inference is defined by water proximity/elevation; inland shelters are not evacuated unnecessarily; normal focused evacuation behavior remains unchanged for land impacts.
