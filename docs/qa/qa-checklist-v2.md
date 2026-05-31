# QA Checklist V2

Use this checklist before release candidates and Steam Workshop updates.

## Current State

- Legacy `Source/` 1.3.0 is the behavior reference.
- Disaster Command Center `0.1.0-dev` is a migration shell.
- QA must state whether it is validating legacy behavior, migrated Disaster Command Center behavior, or migration/import behavior.

## Release Checklist

| Test ID | Area | Steps | Expected result | Priority | Risk |
| --- | --- | --- | --- | --- | --- |
| QA-START-001 | Startup | Start a new city with the target build enabled. | Mod loads without errors and UI expected for that build is accessible. | High | Startup failure blocks all users. |
| QA-START-002 | Startup | Load an existing save with the target build enabled. | Save loads; expected settings behavior is documented for that build. | High | Existing players may lose access to saves. |
| QA-MIG-001 | Migration | Start with legacy options/save data present, then load Disaster Command Center after importer is implemented. | Legacy settings are imported or skipped safely with clear logs. | High | Migration can corrupt or lose user configuration. |
| QA-UI-001 | UI | Open each implemented configuration or in-game tab. | Implemented tabs render without overlap, missing labels, or broken controls. | High | Players cannot configure the mod. |
| QA-UI-002 | UI | Hover major controls and warnings. | Tooltips explain behavior clearly and do not claim unimplemented features. | Medium | Confusing settings cause bad reports. |
| QA-SET-001 | Settings | Change implemented settings, save, exit, and reload. | Settings persist correctly for the target build. | High | Lost settings break trust. |
| QA-DIS-001 | Disaster behavior | For each migrated disaster, enable generated disasters and observe expected behavior. | Migrated disasters respect enabled state, recurrence, environmental rules, and configured limits. | High | Core feature regression. |
| QA-DIS-002 | Manual disaster triggering | Trigger each migrated supported disaster manually. | Manual trigger starts the requested disaster or explains why it cannot. | High | Manual controls are user-facing. |
| QA-EVAC-001 | Evacuation | Test manual, automatic, and focused evacuation modes where migrated. | Shelter behavior matches the documented mode and active-disaster lifecycle. | High | Evacuation mistakes can damage saves. |
| QA-COMP-001 | Compatibility | Run with Real Time enabled after compatibility migration. | Compatibility tab detects Real Time and explains timing impact. | High | Common compatibility scenario. |
| QA-COMP-002 | Compatibility | Run with Extended InfoPanel 2, ACME, Tree Fire Control, Game Anarchy, Rain Firefighting, Adjustable Fire, No Fires, and Skyve where available. | Detected mods show clear status and recommended action. | Medium | Compatibility confusion. |
| QA-SAVE-001 | Save/load | Trigger or configure migrated disasters, save, reload, and continue. | Disaster state and settings remain stable. | High | Save/load regressions are severe. |
| QA-LOG-001 | Logs | Enable diagnostic logging and reproduce a warning state after logging is migrated. | Logs include useful context without excessive noise. | Medium | Bug reports need evidence. |
| QA-WS-001 | Steam Workshop update validation | Review release notes, Workshop description, preview images, and known issues. | User-facing release material matches the build. | High | Incorrect release communication. |

## Notes

- Capture logs for any failed test.
- Include enabled mod list when testing compatibility issues.
- Record game version, mod version, and whether a save is new or existing.
- Retest high-priority items after any late release fix.
- Do not mark an unported Disaster Command Center feature as failed unless it was expected in the milestone.
