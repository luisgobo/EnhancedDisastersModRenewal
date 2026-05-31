# Compatibility Tab Plan

The compatibility tab should help players understand whether other mods may affect Disaster Command Center behavior.

## Current State

- Legacy `Source/` 1.3.0 already contains compatibility metadata for Real Time, Extended InfoPanel 2, ACME, Tree Fire Control, Game Anarchy, Rain Firefighting, Adjustable Fire, and No Fires.
- `DisasterCommandCenter/Compatibility/CompatibilityModule.cs` is currently a placeholder for the migrated compatibility layer.
- This plan describes the target migrated behavior.

## Goals

- Detect relevant mods when possible.
- Explain the likely impact in plain language.
- Separate informational notes from real conflicts.
- Give players a recommended action.
- Avoid blaming other mods when the interaction is uncertain.

## Priority Mods

| Mod | Area | Possible impact | Suggested severity |
| --- | --- | --- | --- |
| Real Time | Time flow | Changes effective disaster recurrence and timing assumptions. | Info or Warning |
| Extended InfoPanel 2 | UI/information panels | May overlap with or affect expectations around in-game information panels. | Info |
| ACME | Camera/tools | May affect camera workflow and some behavior assumptions already handled partially in the legacy mod. | Info or Warning |
| Game Anarchy | Simulation/game rules | May alter unlocks, restrictions, or simulation assumptions. | Info |
| Skyve | Mod management | Helps users manage compatibility and load order. | Info |
| Tree Fire Control | Fire behavior | May alter forest fire availability or spread. | Warning |
| Rain Firefighting | Fire behavior | May suppress or change fire disaster outcomes. | Warning |
| Adjustable Fire | Fire behavior | May alter fire intensity or spread behavior. | Warning |
| No Fires | Fire behavior | May prevent fire-related disasters from behaving as expected. | Conflict |

## Real Time Starter Plan

- Detect whether Real Time is enabled.
- Port legacy Real Time detection before changing recurrence behavior.
- Show whether Disaster Command Center has adjusted recurrence assumptions after the feature is migrated.
- Show per-disaster effective recurrence if available.
- Warn when configured frequencies may feel too frequent or too rare in real time.
- Keep the message specific to time-flow effects rather than general incompatibility.

## Warning Message Examples

### Real Time Detected

Real Time is active. Disaster timing may feel different because the game clock progresses differently. Disaster Command Center will show adjusted recurrence estimates where available.

### Real Time Support Pending Migration

Real Time is active, but this Disaster Command Center build has not migrated Real Time recurrence support yet. Use a tested build or keep the legacy mod active until migration is complete.

### Fire Behavior Mod Detected

A fire behavior mod is active. Forest fires may be limited, suppressed, or changed by that mod. If forest fires do not start as expected, check fire-related settings in both mods.

### No Fires Detected

No Fires is active. Fire-related disasters may not behave as expected because fire behavior is being suppressed. Disable No Fires or avoid fire disaster features if you want normal fire behavior.

### Unknown Disaster Mod Detected

Another disaster-related mod may be active. If disaster timing, intensity, or spawning behaves unexpectedly, test with only Disaster Command Center enabled and include logs when reporting the issue.

## Open Questions

- Which detection methods are reliable in the current codebase?
- Which warnings should block actions versus only inform the player?
- Should compatibility rows link to logs or diagnostics?
- Which warnings belong on the dashboard as top-level alerts?
- Which legacy compatibility messages should be copied exactly for continuity?
