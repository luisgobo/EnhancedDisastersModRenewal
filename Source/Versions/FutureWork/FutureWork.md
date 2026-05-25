# Future Work

These items were moved out of inline TODO comments so future work can be tracked
from one place while preserving the original code reference.

## Community Requests Pending After 1.3.0

These requests were reviewed against version 1.3.0. Items already covered by 1.3.0 are not listed
as standalone work unless the current implementation is only partial.

### Stability And Compatibility

- Status context:
  - Real Time detection, timing adjustments, per-disaster Real Time frequency presets, mismatch warnings,
    and dependency status UI are already present in 1.3.0.
  - Compatibility detection already includes Real Time, Extended InfoPanel 2, ACME, Tree Fire Control,
    Game Anarchy, Rain Firefighting, Adjustable Fire, and No Fires.
  - ACME warnings and forest-fire behavior-mod status indicators are already partially present.

#### Forest Fire Compatibility Follow-Up

- Priority: high
- Status: partial in 1.3.0

Requirements:

- Verify forest fires can spawn correctly when fire-related mods are enabled.
- Add explicit warning text for detected forest-fire behavior mods explaining likely impact, not only active/inactive state.
- Add a setting to force forest fire availability when external mods suppress tree-fire behavior.
- Evaluate whether an "Allow tree fire spreading" setting is feasible through the base game/mod APIs.
- Add diagnostics that distinguish:
  - Forest fire disaster enabled/disabled.
  - Tree fire spreading available/suppressed.
  - External mod conflict detected.
  - Weather/fog/rain condition blocking fire start.

#### Disaster Destruction Consistency

- Priority: high
- Status: partial in 1.3.0

Requirements:

- Verify and fix cases where disaster destruction does not affect roads.
- Evaluate support for rail and metro destruction.
- Investigate whether terrain deformation can break later destruction checks.
- Split destruction settings by target type:
  - Buildings.
  - Roads.
  - Rail/Metro.
  - Terrain deformation.
- Keep existing tornado destruction threshold behavior, but do not limit future destruction settings to tornadoes only.
- Add diagnostic logging when destruction is skipped because of intensity, terrain state, unsupported network type, or compatibility issues.

#### Emergency And Disaster State Controls

- Priority: high
- Status: partial in 1.3.0

Requirements:

- Verify the reported issue where disasters cannot be re-enabled after emergency actions.
- Separate user-facing controls:
  - Disable mod-generated disasters.
  - Disable vanilla disasters.
  - Emergency stop currently active disasters.
  - Reset disaster state/progress.
- Preserve the existing emergency stop behavior, but move it behind clearer command semantics.
- Add a dedicated "Reset disaster state" action that clears cooldown/progress/active-disaster tracking without changing saved settings.
- Document which disasters cannot be stopped cleanly, especially tsunami waves.

#### Manual Spawn Intensity Reliability

- Priority: high
- Status: unknown/needs verification in 1.3.0

Requirements:

- Verify manually spawned disasters respect the configured/generated intensity limits.
- Add logging for:
  - Requested manual intensity.
  - Configured maximum generated intensity.
  - Population-scaled intensity, if enabled.
  - Final applied base-game intensity.
- Add validation when requested intensity is above the active cap.
- Decide whether manual spawns should use:
  - Random max cap.
  - Manual max cap.
  - Absolute cap.

#### Unlock Disaster Controls

- Priority: medium
- Status: partial in 1.3.0

Requirements:

- Keep current milestone unlock checks.
- Verify all disasters unlock correctly in new cities, loaded cities, and scenarios.
- Add a manual "Force unlock all disasters" button for recovery/debugging.
- Log current milestone and final unlock state after level load.

#### Compatibility Monitor Expansion

- Priority: medium
- Status: partial in 1.3.0

Requirements:

- Add detection/warnings for known disaster-overhaul conflicts:
  - Ragnarok.
  - Natural Disasters Overhaul or similar ND behavior mods.
  - 81 Tiles 2 compatibility notes if a specific issue is confirmed.
- Convert the dependencies panel from active/inactive labels into actionable warning rows.
- For each detected compatibility issue, show:
  - Mod name.
  - Expected impact.
  - Recommended action.
  - Whether the issue is informational, warning, or conflict.

#### NullReference And Load-Safety Audit

- Priority: medium
- Status: partially improved in 1.3.0, needs targeted audit

Requirements:

- Audit level load/unload paths for `NullReferenceException` risks.
- Add guarded fallbacks around optional game panels/managers.
- Verify behavior with new game, load game, scenario start, and unload-to-menu.
- Verify compatibility with 81 Tiles 2 only after a reproducible issue or API interaction is identified.

### Advanced Control

#### Per-Disaster Intensity Caps

- Priority: medium
- Status: partial in 1.3.0

Context:

- 1.3.0 already has `MaxGeneratedIntensity` per disaster plus global `AllowExtremeIntensities`.
- The community request asks for clearer separation of random/manual/absolute limits.

Requirements:

- Split intensity limits into:
  - Random/generated disaster max.
  - Manual spawn max.
  - Absolute safety cap.
- Display each cap clearly in settings and diagnostics.
- Ensure tooltip text explains which cap affects which flow.
- Keep conservative defaults compatible with current 1.3.0 behavior.

#### Behavior Profiles

- Priority: medium
- Status: pending

Requirements:

- Add mode presets:
  - Lite Mode: intensity/frequency controls only.
  - Ragnarok Mode: behavior closer to Ragnarok-style control.
  - Full NDR Mode: current advanced behavior.
  - Custom Mode: unlocked individual settings.
- Define exactly which settings each profile owns.
- Prevent profile switches from silently overwriting custom settings without confirmation.

#### Advanced Frequency Control

- Priority: medium
- Status: partial in 1.3.0

Context:

- 1.3.0 includes per-disaster Real Time frequency presets and recurrence tuning.
- The community request asks for more explicit numeric controls.

Requirements:

- Add per-disaster "disasters per year" controls where feasible.
- Add cooldown override controls.
- Add max active disasters setting.
- Add option to allow/prevent overlapping disasters.
- Evaluate event duration multipliers per disaster type.
- Keep Real Time mode understandable by showing converted real-time estimates where applicable.

#### Real Time Advanced Integration

- Priority: medium
- Status: partial in 1.3.0

Requirements:

- Keep current per-disaster Real Time presets.
- Add optional advanced numeric frequency scaling per disaster.
- Add deeper meteor period customization for Real Time players.
- Show effective expected recurrence in both game-time and real-time terms.

#### Presets System

- Priority: medium
- Status: pending

Requirements:

- Add setting presets:
  - Vanilla-like.
  - Ragnarok-like.
  - Safe coastal city.
  - Apocalypse.
  - Real Time balanced.
- Provide preview/diff before applying a preset.
- Preserve user custom settings unless the player confirms replacement.

### Evacuation And Shelter Systems

#### Evacuation Behavior Expansion

- Priority: medium
- Status: partial in 1.3.0

Context:

- 1.3.0 already has manual, automatic, focused evacuation/release behavior and tsunami coastal shelter risk work.

Requirements:

- Clarify and expose behavior choices:
  - Use vanilla AI.
  - Prefer nearest shelter.
  - Increase evacuation time.
  - Auto-evacuate on warning.
- Split tsunami behavior into:
  - Auto evacuate/release.
  - Auto evacuate only.
- Keep focused evacuation/release behavior intact.

#### Shelter Improvements

- Priority: low
- Status: pending

Requirements:

- Evaluate settings to keep shelters stocked.
- Evaluate whether shelter consumption can be ignored safely.
- Add a shelter monitoring panel showing:
  - Shelter capacity.
  - Food/resource stock.
  - Active evacuation state.
  - Disasters currently affecting each shelter.

#### Alarm Customization

- Priority: low
- Status: pending

Requirements:

- Add alarm volume control if the base game API allows it.
- Add enable/disable alarm setting.
- Evaluate custom alarm sound support and asset-loading constraints.

### UX, Accessibility, And Documentation

#### UX Follow-Up

- Priority: low
- Status: partial in 1.3.0

Context:

- 1.3.0 already has reset button/panel position controls and hotkey customization.

Requirements:

- Add "Open panel" button in settings.
- Add optional startup notification such as "Press Shift+D to open Natural Disasters Renewal".
- Improve reset UI wording so players can distinguish:
  - Reset saved settings.
  - Reset defaults.
  - Reset panel/button position.
  - Reset disaster state.

#### Documentation Follow-Up

- Priority: low
- Status: partial in 1.3.0

Requirements:

- Clearly state DLC requirements.
- Add FAQ entries:
  - Mod not appearing in Skyve.
  - Conflicts with Real Time.
  - Conflicts with Game Anarchy / Tree Fire Control / fire behavior mods.
  - Conflicts with Ragnarok or other disaster overhaul mods.
  - Why disasters may appear too frequent or too rare.

#### Scenario Support

- Priority: low
- Status: partial/needs verification in 1.3.0

Context:

- `LoadingExtension` currently handles `NewGameFromScenario`, but the request asks for explicit disaster support in scenarios.

Requirements:

- Verify all disaster settings and unlock logic work in scenarios.
- Decide whether scenario support needs its own setting.
- Add scenario-specific diagnostics if base-game scenario rules block disasters.

## Separate UI Responsibilities From Core Disaster Logic

- Source: [UiCoreSeparationTODO.md](UiCoreSeparationTODO.md)
- Version target: 2.0

### Goal

- UI should render state and dispatch user commands only.
- Core/application services should own disaster state changes, simulation actions, persistence,
  settings mutation, hotkey behavior, and data preparation for views.
- See the linked TODO for the detailed staged refactor plan.

## Add Reconstruction Helper Panel

- Source: `Source/UI/InGameDisastersPanel.cs:219`
- Version target: 2.0

### Requirements

- Scan buildings that the base game allows to reconstruct after disasters, such as museums,
  schools, hospitals, and other supported public/service buildings.
- List rebuildable buildings in a dedicated menu with building name/type and district/position context.
- Add a button to move the camera to the selected building so the player does not need to search the map.
- If the game API allows it safely, add a quick rebuild button for each listed building.
- Detect disaster-damaged road segments and, if the game API allows it safely, repair them in one click.
- Keep the list updated after disasters, manual rebuilds, demolitions, and map reloads.
- Avoid touching buildings that are not in a valid reconstructable state.

## Evaluate Separate Nuclear Incident Event

- Source: `Source/Models/NaturalDisaster/ForestFireModel.cs:730`
- Version target: 2.0

### Requirements To Consider

- Target only specific buildings, such as nuclear reactor/power plant prefabs.
- Trigger fire on the selected building through `BuildingAI.BurnBuilding`.
- Support triggers from nearby disasters, random failure, or configured annual frequency.
- Expose tuning for incident duration, fire intensity, collapse/explosion chance, and evacuation radius.
- Simulate ground contamination through `NaturalResourceManager` pollution resources.
- Optionally simulate water contamination near the reactor through water/sewage pollution APIs.
- Treat air/radiation as a custom temporary radius effect, since there is no clear native air pollution map.
- Keep fallback behavior safe when no valid reactor building exists.

## Evaluate Populated-Area Meteor Targeting

- Source: `Source/Models/NaturalDisaster/MeteorStrikeModel.cs:579`
- Version target: 2.0

### Requirements

- Keep the current vanilla/random targeting as the default behavior.
- Add an optional "near populated areas" mode that selects a populated building or dense area.
- Prefer an offset around the populated target instead of always hitting the building directly.
- Use a safe fallback to vanilla/random when no valid populated target is found.
- Apply the same target selection regardless of Real Time being active; Real Time only changes when meteors occur.
- Consider a future setting for direct-hit risk or offset radius before implementing.

## Evaluate Coastal Shelter Evacuation For Water Meteor Impacts

- Source: `Source/Models/NaturalDisaster/MeteorStrikeModel.cs:588`
- Version target: 2.0

### Requirements

- Detect water impacts by comparing terrain height against terrain-with-water height at the meteor target.
- Add an optional setting, disabled by default, to activate coastal shelters when a detected meteor will hit water.
- Infer coastal shelters by water proximity/elevation because the game does not expose a dedicated coastal shelter type.
- Limit activation by distance from the impact and/or unlocked areas so inland shelters are not evacuated unnecessarily.
- Keep normal focused evacuation behavior unchanged for land impacts.

## Split Tsunami Evacuation Behavior

- Source: `Source/Models/NaturalDisaster/TsunamiModel.cs:129`

### Notes

- Current tsunami automatic evacuation behaves like "auto evacuate/release": it starts evacuation
  on detection and releases shelters when the tsunami finishes.
- Preserve this functional flow as the future auto-evacuate/release option.
- Add a separate auto-evacuate-only option that starts the selected shelters but leaves citizen
  release under manual/player control.
