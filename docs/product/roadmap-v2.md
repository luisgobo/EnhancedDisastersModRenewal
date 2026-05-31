# Roadmap V2

This roadmap tracks Disaster Command Center work. Keep items small enough to review and test.

## Current State

- `DisasterCommandCenter/` is a `0.1.0-dev` successor shell.
- `Source/` version `1.3.0` is the current functional reference.
- Migration parity should be prioritized before broad new gameplay features.

## High Priority

| Item | Type | User impact | Suggested action | Acceptance criteria |
| --- | --- | --- | --- | --- |
| Migration parity baseline | Feature Request | Players should not lose core disaster behavior when moving to Disaster Command Center. | Define parity checklist against `Source/` 1.3.0. | Supported disasters, settings, UI, serialization, logging, compatibility, and evacuation behavior are mapped to migration tasks. |
| Legacy settings and save migration | Compatibility Issue | Existing users may have per-save settings and legacy option files. | Design importer for legacy identifiers and option files. | Migration behavior is documented, tested, and safe when legacy files are missing or malformed. |
| Real Time recurrence migration | Compatibility Issue | Real Time users depend on adjusted disaster timing. | Port timing helpers and per-disaster presets after compatibility module is ready. | Real Time behavior matches or intentionally improves on `Source/` 1.3.0 with QA coverage. |
| In-game command center panel parity | UX/UI Improvement | Players need disaster status, controls, dependency status, and debug tools in-game. | Rebuild the legacy panel against the new module structure. | Panel shows disaster rows, progress, actions, compatibility status, localized text, and safe debug controls. |

## Medium Priority

| Item | Type | User impact | Suggested action | Acceptance criteria |
| --- | --- | --- | --- | --- |
| Disaster module migration | Feature Request | Core gameplay depends on all existing disasters being available. | Port forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, and meteor strike one at a time. | Each disaster has parity tests for occurrence, intensity, evacuation, Real Time behavior, and settings. |
| Compatibility monitor expansion | Compatibility Issue | Players need clearer conflict explanations for disaster, time, and fire mods. | Port legacy detection and add actionable compatibility rows. | Each detected mod shows expected impact and recommended action. |
| Emergency stop and reset clarity | UX/UI Improvement | Players may confuse stopping active disasters with resetting progress. | Keep actions distinct in UI and docs. | Each action has wording, tooltip text, and QA coverage. |
| Recovery tools planning | Feature Request | Players want help finding and repairing disaster damage. | Keep as a 2.0+ feature after core migration. | Scope covers valid rebuildable buildings, damaged roads, camera navigation, and safe API checks. |

## Low Priority

| Item | Type | User impact | Suggested action | Acceptance criteria |
| --- | --- | --- | --- | --- |
| Steam Workshop screenshot refresh | Documentation Need | Workshop page may not show current UI clearly. | Plan updated screenshots after UI changes. | Screenshots match current release and show compatibility status. |
| Behavior profiles | Feature Request | Players may want simpler presets. | Revisit after migrated settings are stable. | Profiles do not overwrite custom settings without confirmation. |

## Out Of Scope

| Item | Reason |
| --- | --- |
| Cities: Skylines 2 support | Disaster Command Center is for Cities: Skylines 1. |
| Unrelated traffic, zoning, or economy features | Outside disaster control and compatibility scope. |
| Claims of full successor parity before migration is complete | Misleads players and testers. |

## Later Ideas

- Behavior profiles for Lite, Ragnarok-style, Full Command Center, and Custom modes.
- Per-disaster generated/manual/absolute intensity caps if technically feasible.
- Advanced numeric recurrence controls with Real Time estimates.
- More detailed diagnostics for weather, fog, rain, and disaster start blockers.
- Nuclear incident event separate from normal forest fires.
- Meteor targeting mode that can prefer populated areas.
- Coastal shelter evacuation for water-impact meteor strikes.
- Beta tester checklist and structured feedback form.
