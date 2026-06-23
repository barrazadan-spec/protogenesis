# V5 2.5 - Programmable Systems Pass

This pass turns the v5.5 review into an implementation queue. The goal is not more playable routes yet; it is to close rules that must be programmatic, measurable and visible.

## Accepted Decisions

- Tardigrade is not a core playable route.
- Tardigrade should live as:
  - `Cryptobiosis` gene/adaptation,
  - rare neutral in extreme zones,
  - optional defensive apex/event.
- HUD-visible resources are ATP, Biomass and Stress.
- Amino acids, lipids, nucleotides and minerals remain internal submaterials shown in the interior/details panel.
- Microcolonies are one controllable entity, one agent and one HP pool with multiple non-simulated visual bodies.
- Colonial Continuity needs numeric scoring before implementation.
- Fungus and Slime Mold must diverge mechanically:
  - Fungus: fixed defensive digestion territory.
  - Slime Mold: mobile network with chemical memory and route optimization.
- Living Battlefield states must be visible, named and detected.

## Implementation Queue

| Priority | Task | Output |
| ---: | --- | --- |
| 1 | Tardigrade final status | Implemented: removed from playable core, kept gene/neutral/apex hooks |
| 2 | Resource visibility | Implemented: HUD = ATP/Biomass/Stress; interior panel = submaterials |
| 3 | Microcolony contract | One entity/agent/HP pool, visual satellites only |
| 4 | Continuity scoring | Implemented: numeric succession score and outcomes |
| 5 | Fungus vs Slime Mold | Fixed hifal digestion vs mobile memory network |
| 6 | Living Battlefield recognizer | Implemented: toxic scar, nutrient corridor, oxygen front, acid pocket, living network, chemical panic |
| 7 | Ecological Crisis trigger | Activate when at least 2 of 3 thresholds are true |
| 8 | Run metrics | 60/40 time split, decisive combat, tracker clarity, succession recovery |
| 9 | Bacteriophage T4 threat | First new organism after systems are closed |

## Implemented in Code

- `V5HudIMGUI` now shows only ATP, Biomass and Stress in the main HUD.
- `V5HudIMGUI` shows amino acids, lipids, nucleotides and minerals inside the interior panel.
- `V5RosterBalance` marks Tardigrade as `NeutralOnly` with player hard cap 0.
- `V5EvolutionRoster` removes Tardigrade from primary playable routes.
- `V5CellEntity` no longer resolves player evolution into Tardigrade from cryptobiosis structures.
- `V5ColonyCommandCenterIMGUI`, `V5BuildOrderPlannerIMGUI` and `V5EvolutionRouteBoardSystem` no longer offer Tardigrade as a playable target path.
- `V5ColonialContinuitySystem` handles mother death with numeric succession scoring.
- `V5GameBootstrap`, `V5GameManager` and `V5RunResetSystem` register/reset the continuity system.
- `V5BattlefieldStateRecognizer` detects and labels the six Living Battlefield states.
- `V5GameBootstrap`, `V5GameManager`, `V5RunResetSystem` and `V5HudIMGUI` register/reset/display battlefield state summaries.

## V5 2.6 - Germinal Nexus Pivot

The central gameplay architecture pivots from "mother as everything" to:

- Mother = mobile germinal nexus, genetic identity and vulnerable command center.
- Lineage phenotypes = tactical roles inside the dominant route.
- Auxiliary castes = limited off-route support, capped by biological load.
- Germinal Chamber = the production UI where units are selected and previewed in detail.

Implemented systems:

- `V5GerminalProductionSystem`
  - Adds the Germinal panel under `Paneles > Germinal`.
  - Provides lineage phenotypes: Plastic Daughter, Gatherer, Scout, Defender and Raider.
  - Provides auxiliary castes: Amoeboid Guard, Ciliate Controller, Bacterial Symbiont and Microalga Support.
  - Enforces auxiliary biological load cap.
  - Shows a Dune-style biological plate/preview for the selected unit.
- `V5CellFactory.SpawnGerminalCell`
  - Produces daughters with phenotype labels, path overrides, role directives, stat modifiers and granted structures.
- `V5SelectionSystem`
  - Pressing `D` on the mother now routes through germinal production and creates a Plastic Daughter.
- `V5CombatSystem` and `V5CellEntity`
  - Mother combat damage is reduced.
  - Mother receives more combat stress and is riskier to use as frontline.
- `V5Balance`
  - `MotherCombatDamageMultiplier = 0.35`
  - `MotherCombatIncomingDamageMultiplier = 1.18`
  - `MotherCombatStressMultiplier = 1.55`
  - `MaxAuxiliaryPopulationLoadRatio = 0.30`
  - `SymbiosisAuxiliaryPopulationLoadRatio = 0.40`

Canon rule:

> The mother defines identity. The environment and genes open possibilities. Biological load limits the mix.

## V5 2.7 - Affinity-Gated Germinal Production

This pass connects route identity, evolution UI and germinal production through one shared affinity reading.

Implemented systems:

- `V5EvolutionAffinitySystem`
  - Scores each playable route from 0-100 using domain, key structures, metabolism, genes and local environment.
  - Returns a readable reason string such as structure, gene or environmental signals.
  - Provides the 60% consolidation check used by the mother.
- `V5CellEntity`
  - Player mother no longer consolidates routes from a hidden structure count.
  - Player mother consolidates only when the best route reaches `RouteConsolidationAffinityThreshold`.
- `V5EvolutionPlannerIMGUI` and `V5EvolutionRouteBoardSystem`
  - Show route affinity and the reasons behind it.
  - Route consolidation button now respects the 60% threshold.
- `V5GerminalProductionSystem`
  - Auxiliary castes can open from compatible lineage biology, Symbiosis or sufficient affinity toward the target route.
  - The selected Dune-style plate now shows affinity and unlock reasons for the caste.

New balance constants:

- `RouteConsolidationAffinityThreshold = 0.60`
- `GerminalAuxiliaryAffinityThreshold = 0.32`

## V5 2.8 - Persistent Affinity Event Memory

This pass makes affinity remember what the player actually did during the run.

Implemented systems:

- `V5AffinityEventLog`
  - Stores recent affinity events by route, points, reason, source and time.
  - Contributes a decaying but persistent history bonus into `V5EvolutionAffinitySystem`.
  - Keeps readable summaries for route cards and the Germinal plate.
- Event hooks
  - Structure installation logs route-specific biological signals.
  - Metabolism changes log fermentation, chemolithotrophy, photosynthesis or respiration pressure.
  - Gene unlocks log route intentions.
  - Germinal production logs produced phenotype/caste.
  - Colonization, environmental exposure and combat behavior add smaller repeated signals with throttling.
- UI
  - Evolution Route Board shows `Historial` per route.
  - Evolution Planner shows recent route events.
  - Germinal Chamber shows history for the selected target path.

New balance constants:

- `AffinityEventCapacity = 96`
- `AffinityEventMemorySeconds = 720`
- `AffinityEventMinimumRetention = 0.35`
- `AffinityEventScoreCap = 28`
- `AffinityEventMinInterval = 5`

## V5 2.9 - Cell Modes Over Directives

This pass starts the controlled Cell Lab pivot in code without replacing the RTS layer yet.

Implemented systems:

- `V5CellModeLibrary`
  - Defines canonical cell modes: Follow Lineage, Gather, Defend, Scout, Colonize, Hunt, Recover and Route Special.
  - Maps old RTS directives to biological modes for compatibility.
  - Gives each mode real multipliers for movement, synthesis, damage, damage taken, colonization, repair and stress drift.
- `V5CellEntity`
  - Stores `CellMode` and automatically syncs when older systems change `Directive`.
  - Applies mode modifiers in movement, harvesting, colonization, repair, combat damage and incoming damage.
  - Daughters inherit the parent mode unless born directly from the mother, where they start in Follow Lineage.
  - Role markers now use mode color, making phenotype behavior visible at a glance.
- `V5SelectionSystem`
  - Number keys now assign biological modes instead of raw directives:
    - 1 Follow Lineage
    - 2 Gather
    - 3 Defend
    - 4 Scout
    - 5 Colonize
    - 6 Hunt
- UI integration
  - HUD shows selected cell mode and mode effect summary.
  - Germinal Chamber shows the default mode for each phenotype plate.
  - Colony command and automation panels now speak in modes rather than directives.

Canon rule:

> Directives are now implementation plumbing. Modes are the player-facing biological behavior layer.

## V5 2.10 - Germinal Phenotype Recipes

This pass makes the Germinal Chamber read like a Cell Lab-style phenotype designer instead of a conventional unit list.

Implemented systems:

- `V5PhenotypeRecipeLibrary`
  - Adds canonical recipe cards for every germinal phenotype and auxiliary caste.
  - Each recipe defines a code, body plan, organelle plan, inheritance rule, default cell mode and effect summary.
  - Recipes apply small biological stat adjustments on spawn: radius, repair, toxin resistance, attack range, biomass load and division efficiency.
- `V5CellFactory`
  - Germinal daughters now receive recipe identity and recipe stat adjustments at birth.
- `V5CellEntity`
  - Stores `PhenotypeRecipeCode` and `PhenotypeRecipeSummary` for UI and future save expansion.
- UI integration
  - Germinal Chamber list shows recipe code next to each phenotype.
  - The selected plate shows body plan, organelles, inheritance, mode and effect.
  - HUD selected-unit readout shows the active recipe code and summary.

Canon rule:

> A germinal phenotype is no longer just a role. It is a readable biological recipe: body, organelles, inheritance and behavior.

## V5 2.11 - Affinity-Matured Phenotype Recipes

This pass connects recipe quality to what the colony has actually done during the run.

Implemented systems:

- `V5AffinityEventLog`
  - Records cell mode usage as affinity history.
  - Gather, Defend, Scout, Colonize, Hunt and Recover now add small route signals with throttling.
- `V5PhenotypeRecipeLibrary`
  - Adds recipe maturity evaluation.
  - Basic recipes are usable early, but can be `inestable` when the colony has not generated enough matching biological signals.
  - Mature recipes reduce germinal stress and apply recipe stat adjustments more cleanly.
  - Unstable recipes add stress and dampen recipe stat adjustments instead of becoming hard-locked.
- `V5GerminalProductionSystem`
  - Shows maturity marker in the phenotype list:
    - `*` mature recipe
    - `~` unstable recipe
  - Shows maturity score/reason on the selected germinal plate.
  - Uses recipe maturity to adjust displayed and paid stress cost.
- `V5CellEntity`
  - Produced cells display recipe code with `*` or `~` depending on maturity at birth.

Canon rule:

> The player can improvise biology early, but repeated behavior stabilizes better recipes.

## Living Battlefield Recognizer

Detected states:

| State | Signal | Player response |
| --- | --- | --- |
| Toxic Scar | High toxins, detritus and/or low oxygen | Avoid, use catalase or toxin-resistant routes |
| Nutrient Corridor | High nutrients with low toxicity | Farm, colonize or contest |
| Oxygen Front | High oxygen or sharp oxygen gradient | Use aerobes, pressure anaerobes |
| Acid Pocket | Extreme local pH | Use Archaea/cuticle or route around |
| Living Network | Strong colonization/biofilm/hyphal signal | Defend nodes or cut connections |
| Chemical Panic | Stress cluster, threats and hostile chemistry | Retreat, repair, cohere or adapt |

## Ecological Crisis Trigger

Crisis Ecologica starts when at least 2 of 3 are true:

- Dominant route affinity is above 75%.
- Ecological Director pressure is above 0.60 for at least 45 seconds.
- Run time is above 18 minutes.

## Continuity Score Bands

| Score | Outcome |
| ---: | --- |
| 70+ | Fast succession |
| 40-69 | Recoverable continuity crisis |
| 20-39 | Fragmentary survival |
| 0-19 | Lineage collapse |

## Backlog Organisms

Hold these until MVP + core routes are playable and measurable:

- Bacteriophage T4 as the first priority threat.
- Anabaena as cyanobacteria subroute/expansion.
- Myxococcus as social bacteria subroute.
- Suctoria as anchored neutral or ciliate expansion.
- Daphnia as grazer event.
- Hydra as apex/boss only.
- Volvox remains apex/neutral until Microalga and Cyanobacteria are stable.
