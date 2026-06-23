# V5 2.4 - Control Cap And Microcolonies

This update replaces the old "20 cells max" rule with a control-based cap model.

## Canon

- The player has a maximum of 24 controllable entities.
- The colony has a soft population load of 14 and a hard population load of 30.
- A controllable entity is not always one organism.
- Swarm routes can represent visual microcolonies while remaining one tactical object.
- Local density still matters: blob penalty, phages, toxins, filters and competition punish packed swarms.

## Route Scale

| Route | Entity cap | Visual organism scale |
| --- | ---: | --- |
| Bacteria | 24 | microcolony, 6-12 organisms |
| Cyanobacteria | 24 | mat/bloom, 5-10 organisms |
| Archaea | 22 | resistant group, 2-6 organisms |
| Microalga | 20 | colony, 2-6 organisms |
| Flagellate | 18 | 1-3 organisms |
| Ciliate | 16 | 1-2 organisms |
| Amoeba | 12 | 1 organism |
| Fungus | 12 | node/network, 1-3 organisms |
| Slime Mold | 14 | node/network, 1-3 organisms |
| Rotifer | 8 | 1 organism |
| Nematode | 8 | 1 organism |
| Tardigrade | 3 | 1 rare organism |

## Code Notes

- `V5Balance.HardControllableEntityCap` is the tactical entity cap.
- `V5Balance.HardPopulationLoad` is the biological load cap.
- `V5RosterBalance` now stores visual organism ranges per route.
- `V5CellEntity` renders microcolony satellite dots for routes whose entity represents multiple organisms.
- HUD now labels the count as entities and shows selected-unit scale.
