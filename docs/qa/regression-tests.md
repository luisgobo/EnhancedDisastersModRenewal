# Regression Tests

This matrix tracks recurring tests for Disaster Command Center and migration parity against the legacy `Source/` implementation.

| Test ID | Area | Scenario | Steps | Expected result | Priority | Risk |
| --- | --- | --- | --- | --- | --- | --- |
| REG-001 | Startup | New city load | Start a new city with only required dependencies. | Target build initializes without exceptions. | High | Startup regression. |
| REG-002 | Startup | Existing save load | Load a save created with the legacy mod or previous target build. | Save loads; migration behavior matches the milestone expectation. | High | Existing user saves fail. |
| REG-003 | Settings | Persistence | Change implemented settings, save, exit to menu, reload. | Values persist and UI reflects saved values. | High | Configuration loss. |
| REG-004 | Migration parity | Supported disaster list | Compare migrated disasters against `Source/` 1.3.0. | Forest fire, thunderstorm, sinkhole, tornado, tsunami, earthquake, and meteor strike are ported or explicitly listed as pending. | High | Missing core feature. |
| REG-005 | Disaster behavior | Generated disaster limits | Configure intensity and frequency for each migrated disaster, then observe generated disasters. | Generated disasters respect enabled state and configured caps. | High | Core gameplay regression. |
| REG-006 | Manual trigger | Manual intensity | Trigger a migrated manual disaster at low, medium, and high intensity. | Applied intensity matches configured rules or shows a clear cap warning. | High | Manual controls unreliable. |
| REG-007 | Evacuation | Manual/automatic/focused modes | Test each migrated evacuation mode with disasters that support it. | Shelters start and release according to the selected mode. | High | Evacuation regression. |
| REG-008 | Emergency controls | Emergency stop | Start a stoppable disaster and use emergency stop. | Active disaster handling matches documented behavior; tsunami limitation is documented if applicable. | High | Recovery control confusion. |
| REG-009 | Reset controls | Reset disaster progress | Use reset progress after generated or manual activity. | Internal cooldown/progress state resets without changing saved settings. | Medium | Unexpected setting changes. |
| REG-010 | Compatibility | Real Time enabled | Enable Real Time and open compatibility status after migration. | Timing impact is detected and explained. | High | Common mod interaction. |
| REG-011 | Compatibility | Fire behavior mod enabled | Enable a fire behavior mod and test forest fire messaging. | Forest fire impact is detected or documented as unknown. | Medium | Fire disaster reports. |
| REG-012 | Logs | Diagnostic logging | Enable diagnostics and reproduce a compatibility warning after logging is migrated. | Logs include mod state and relevant disaster settings. | Medium | Poor bug reports. |
| REG-013 | Recovery | Future repair/rebuild tools | After recovery tools exist, test valid and invalid repair targets. | Only valid rebuildable buildings or damaged roads can be acted on. | Medium | Unsafe repair actions. |

## Add New Regression Tests When

- A player reports a reproducible bug.
- A release changes disaster timing, intensity, emergency controls, or persistence.
- Compatibility detection changes.
- A release changes UI labels, tabs, warnings, or settings layout.
- A legacy feature is ported into Disaster Command Center.
