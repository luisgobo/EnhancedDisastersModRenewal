# Disaster Command Center migration plan

## Goal

Create Disaster Command Center as the visible successor to Natural Disasters Renewal while migrating behavior incrementally and preserving existing user saves and options where practical.

## Project boundaries

Disaster Command Center owns the new architecture, naming, settings model, UI structure, compatibility layer, disaster simulation modules, evacuation behavior, and future recovery tools.

Natural Disasters Renewal remains the stable reference implementation until each area is migrated and verified.

## Proposed migration order

1. Establish project shell, module boundaries, build output, and visible mod identity.
2. Port common properties, logging, localization, and compatibility detection.
3. Port settings models and option persistence with legacy import support.
4. Port recurrence and Real Time timing helpers.
5. Port disasters one by one: forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, meteor strike.
6. Port evacuation and shelter release behavior.
7. Rebuild the in-game command center panel against the new module structure.
8. Add recovery tools for collapsed buildings and damaged roads.
9. Update Steam description, README, screenshots, icon, changelog, and migration notes.

## Compatibility requirements

Keep the legacy identifiers documented in `Migration/LegacyModIdentity.cs`.

Before replacing the Workshop-facing mod, decide whether Disaster Command Center should import from:

- `NaturalDisastersRenewalModOptions.xml`
- `NaturalDisastersRenewalMod` save data
- existing disaster-specific settings serialized by Natural Disasters Renewal

## Naming

Visible name: Disaster Command Center

Assembly and namespace: DisasterCommandCenter

Internal data id: DisasterCommandCenter
