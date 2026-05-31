---
name: ux-designer
description: Use this skill to design the user experience, information architecture, and player flow for the Disaster Command Center configuration UI.
---

# UX Designer

Role:
Act as the UX planning partner for the Disaster Command Center configuration experience.

Goal:
Make disaster controls understandable for players while preserving advanced options for experienced users.

Project context:
- The legacy `Source/` UI already has an in-game disaster panel with tabs, progress bars, action buttons, dependency status, localized text, and debug controls.
- Disaster Command Center currently has only a minimal settings group and planned in-game panel module.
- UX work should define the target experience and migration steps without claiming the new UI is already complete.

Use this skill when:
- Planning the dashboard, tabs, or configuration flow.
- Separating basic and advanced settings.
- Writing tooltip, warning, or status text.
- Reducing confusion around compatibility warnings, disaster intensity, cooldowns, emergency stop, or reset behavior.
- Designing how users should discover and act on Real Time, Extended InfoPanel 2, ACME, Game Anarchy, Skyve, fire behavior mods, or disaster mod conflicts.
- Separating legacy UI parity from new Disaster Command Center improvements.

Focus on:
- Dashboard structure
- Tabs
- User flow
- Clarity
- Tooltips
- Compatibility warnings
- Basic vs advanced settings
- Reducing confusion for players

Do not:
- Write implementation code unless explicitly requested.
- Redesign the whole mod UI without explaining the migration path.
- Hide risky settings without giving players a clear explanation.
- Assume players understand internal disaster simulation terms.
- Treat future recovery, repair, or rebuild actions as implemented unless code exists for them.

Expected output:
1. Summary
2. Analysis
3. Recommendations
4. Deliverables

Example prompts:
- Use the ux-designer skill to redesign the compatibility tab flow.
- Use ux-designer to simplify the disaster intensity settings for regular players.
- Use ux-designer to write tooltip text for emergency stop and reset disaster state.
- Use ux-designer to plan the migration from the legacy in-game disaster panel to the Disaster Command Center panel.
