# Legacy scripts triage - 2026-04-29

Context: `Assets/Scripts` belongs to an older Protogenesis prototype. It currently blocks the Unity compile because several namespaces and systems referenced by those scripts no longer exist in the V5.2 code path. The folder should stay preserved, but disabled outside `Assets`.

## Useful ideas to port into V5.2 later

### Living battlefield ecology

- `Enemies/Tardigrado_NPC.cs`
  - Keep the cryptobiosis loop: heavy recent damage pushes the tardigrade into a temporary protected state, then a short vulnerable recovery window.
  - Good fit for the V5.2 tardigrade as a neutral apex/survival organism, not as an immune unit.

- `Enemies/Bacteriophage.cs`
  - Keep the staged infection behavior: seek membrane, approach, anchor while vulnerable, inject, retreat.
  - Keep antigenic variation as an adaptation mechanic, but rename it as ecological resistance or viral adaptation if phages become neutral threats.
  - Good future use: battlefield crisis, neutral hazard, or counterplay event.

- `Enemies/Didinium_NPC.cs` and `Enemies/Paramecio_NPC.cs`
  - Keep predator/prey logic: Didinium hunts Paramecium, Paramecium zigzags and flees Didinium.
  - Paramecium trichocyst stun can become a ciliate squad utility.
  - Defensive vesicles at low HP are useful as a soft deterrence mechanic.

- `Enemies/EnemyBase.cs`
  - Keep the layered AI concept: environment, perception, decision, action, survival.
  - Keep relative-size interaction rules: detectability, phagocytosis threshold, and "appears as wall" behavior.
  - Avoid porting the class directly; V5.2 already has its own entity and combat systems.

### Progression and identity

- `Archetypes/ArchetypeResolver.cs`
  - Keep behavior-based scoring over time: ATP economy, fermentation, allies nearby, recent kills, low oxygen tolerance.
  - Best use: an advisor/director that notices player style and suggests routes.

- `Progression/SpecializationTracker.cs`
  - Keep the 3-source identity model: genes, player behavior, installed structures.
  - Update route names to the V5.2 organism roster: prokaryotes, eukaryotes, and ecosystem organisms instead of immune cells.

- `Progression/GeneticRingSystem.cs`
  - Keep the 3-ring idea as a readable player-facing progression layer.
  - Good use: domain choice, identity structure choice, late adaptation choice.

- `Progression/GeneticTreeSystem.cs`
  - Keep adaptation/metabolism/symbiosis branches.
  - Replace immune-specific nodes with ecosystem organism paths: rotifer, nematode, slime mold, tardigrade, colonial/symbiotic forms.

### Presentation and tooling

- `Rendering/MapVisualizer.cs`
  - Keep procedural biome bands and ambient particles.
  - Needs adaptation to the V5.2 living battlefield grid rather than a fixed 3-band map.

- `Rendering/ZoomManager.cs`
  - Keep phenotype/organism-aware camera scale and cinematic zoom transitions.
  - Needs safer scale values for current prototype scenes.

- `Core/PowerMomentSystem.cs`
  - Keep short celebration moments for consolidation, apex emergence, and elimination.
  - Replace old OnGUI implementation with V5 HUD hooks later.

- `Balance/PlaytestManager.cs`
  - Keep the idea of editor-only balance telemetry: session duration, damage, ATP spent, win/loss, and warnings.
  - Rebuild against V5 managers instead of porting directly.

## Do not port directly

- `Online/*`: too early for the current single-player RTS prototype; preserve for later multiplayer thinking only.
- `Core/GameManager.cs`, `Core/SceneBootstrap.cs`, old `EventBus.cs`: replaced by V5 bootstrap/manager code.
- `Slots/*` and `Organelles/*`: old slot/organelle architecture conflicts with the current V5 structures and squad systems.
- Empty or obsolete folders under the old tree: keep archived, but do not compile.

## Decision

Move `Assets/Scripts` and `Assets/Scripts.meta` outside `Assets` into `_LegacyDisabled/Assets_Scripts_2026-04-29`. This preserves the old prototype while removing it from Unity compilation.

## Verification

- Archive location: `_LegacyDisabled/Assets_Scripts_2026-04-29`
- Unity compile log: `Logs/CodexCompile_AfterLegacyDisable.log`
- Result: script compilation succeeds after disabling the legacy folder.
- Remaining issues are warnings in V5 code, mostly obsolete Unity API calls such as `FindObjectOfType`, plus `V5EnvironmentGrid.light` hiding `Component.light`.
