# Protogenesis: Primordia - V5 Prototype 2.3

## Focus
This pass consolidates four design decisions that were pulling against each other:

- Combat should feel biological and tactical, not like a hero hotbar.
- The colony cap should allow up to 20 units, but only for small/light organisms.
- The roster can grow, but it must stay coherent with real micro-ecology.
- Tardigrade should remain experimental until it has a proactive win pattern.

## Review Of The Comments

The critique is mostly correct. The strongest points to accept are:

- The old immune-cell routes do not fit the core fantasy. Neutrophil, macrophage, NK and B cell should remain retired/legacy, not literal playable routes.
- A flat cap breaks the biology. Bacteria can reach 20, but large unicellular predators and microfauna need lower practical caps.
- Q/W/E/R powers risk making the game feel like an ability-brawler. Combat should come from organism traits, positioning, directives, environment and stress.
- Tardigrade is biologically iconic but mechanically passive. It is better as experimental, neutral, apex support or a cryptobiosis gene than as a primary route.
- The specialization tracker needs explicit route weights before it can be trusted by players.

The useful corrections:

- Catalase is valid as a broad enzymatic structure; peroxisome should stay eukaryotic if added later.
- Chloroplast/endosymbiosis should be treated as an abstract evolution unlock during a match, not as literal instant endosymbiosis unless a campaign scenario explains it.
- Blob penalty is accepted, but the fiction is diffusion, local nutrient competition, toxin buildup and pathogen vulnerability.
- Adaptive memory should start around x2 to x3, not x5, unless heavily gated.

## Combat Decision

Core combat is not Q/W/E/R. Core combat is:

1. Selection and directives.
2. Contact geometry.
3. Organism traits.
4. Local ecology.
5. Stress and attrition.

The ability panel remains as an experimental bioactive panel opened with `K`. Direct Q/W/R casting is disabled by default unless `EnableDirectAbilityHotkeys` is explicitly enabled on `V5AbilitySystem`.

### Combat Timing Target

- Small skirmish: 8 to 20 seconds.
- Even squad fight: 30 to 60 seconds.
- Territorial/boss/ecology fight: 60 to 120 seconds.

If an even 6v6 fight ends under 20 seconds, there is not enough room for micro. If it takes longer than 60 seconds without terrain/ecology changing, it becomes mushy.

### Damage Roles

| Damage/Pressure | Main use | Good against | Weak against |
|---|---|---|---|
| Physical contact | baseline duels, engulfing | fragile cells | cuticle, spacing |
| Chemical/toxin | area denial, biofilm pressure | dense colonies | detox/catalase |
| Piercing | linear breach, stylet attacks | biofilms, large targets | fast dispersal |
| Filtration/current | anti-swarm control | small dense units | large predators |
| Osmotic/acid/thermal/oxidative | environment and crisis pressure | unadapted monoculture | route-specific tolerance |

### Route Combat Identities

| Route | Main combat verb | Counterplay |
|---|---|---|
| Bacteria | swarm, cling, poison, biofilm | area toxins, filtration, avoid density |
| Archaea | endure hostile chemistry, hold zones | physical predators, low-resource pressure |
| Cyanobacteria | oxygenate, bloom, win terrain | shade, grazers, toxin bursts |
| Amoeba | chase, engulf, convert prey to biomass | speed, spacing, piercing, swarms |
| Flagellate | raid, flank, kite | nets, cilia, prediction |
| Ciliate | pull particles/prey with currents | heavy armor, burst predators |
| Microalga | economy plus shell/wall defense | grazers, light denial |
| Fungus | static network digestion | mobility, firebreaks, piercing |
| Slime Mold | mobile network and chemical memory | severing routes, drying zones |
| Rotifer | filter swarms, stabilize particles | piercing, large predators |
| Nematode | linear penetration and biofilm breach | lateral movement, toxins |
| Tardigrade | survive collapse | should not be main win condition yet |

## Population Cap Decision

The player-facing maximum is `20` cells, but the real limiter is biological load.

- `V5Balance.HardCellCap = 20`
- `V5Balance.HardPopulationLoad = 20`
- `V5Balance.SoftPopulationLoad = 10`
- Each route has a population weight in `V5RosterBalance`.

This keeps the promise "20 units max" while preventing 20 amebas, rotifers or tardigrades from breaking the game.

| Route | Size class | Weight | Practical hard cap | Good squad size |
|---|---:|---:|---:|---:|
| Bacteria | pico | 0.65 | 20 | 8-14 |
| Archaea | small | 0.80 | 18 | 6-10 |
| Cyanobacteria | small | 0.75 | 18 | 7-12 |
| Flagellate | medium | 1.05 | 14 | 4-8 |
| Ciliate | medium | 1.20 | 12 | 4-7 |
| Microalga | medium | 1.00 | 14 | 5-9 |
| Amoeba | large | 1.55 | 10 | 3-6 |
| Fungus | large | 1.35 | 10 | 3-6 |
| Slime Mold | large | 1.25 | 12 | 3-7 |
| Rotifer | microfauna | 1.80 | 8 | 2-5 |
| Nematode | microfauna | 1.90 | 8 | 2-4 |
| Tardigrade | microfauna | 2.20 | 6 | 1-3 |

## Roster Expansion

Roster expansion should be organized by ecological scale, not just by cool names.

### Pico / Small Prokaryotes

These support high-density play and clear visual silhouettes.

| Candidate | Suggested role | Why it works |
|---|---|---|
| Spirochete | fast infiltrator through biofilm | corkscrew movement is iconic and readable |
| Magnetotactic bacteria | scout/navigation specialist | magnetosomes create a real biological hook |
| Bdellovibrio | bacterial predator/parasitic breach | unique "enter and consume" fantasy |
| Mycoplasma | stealth/infiltration | tiny, wall-less, fragile but slippery |
| Anabaena | filament economy/differentiation | heterocysts create real procaryotic specialization |
| Actinomycete mat | neutral hazard/resource zone | antibiotic territory, good map ecology |

### Small / Medium Eukaryotes

These are the best near-term roster additions because they add mechanics without jumping scale too hard.

| Candidate | Suggested role | Why it works |
|---|---|---|
| Diatom | defensive photosynthetic shell | silica glass box, very visual |
| Euglena | metabolic switcher | photosynthesis/heterotrophy hybrid |
| Dinoflagellate | toxic bloom and stress light | red tide/bioluminescence identity |
| Vorticella | anchored ambush/filter | retractable stalk gives strong behavior |
| Lacrymaria | predator ambush | long extendable neck is visually excellent |
| Stentor | giant ciliate tank | contraction under threat is readable |
| Heliozoan | neutral radial defender | sun-like axopodia make a clear hazard |

### Large Unicellular / Colonial

These should be late-game, neutral, apex or special-route material.

| Candidate | Suggested role | Why it works |
|---|---|---|
| Volvox | colonial photosynthetic apex/NPC | colony sphere with internal daughter colonies |
| Foraminifer | shelled tank | chambered shell plus pseudopodia |
| Radiolarian | passive piercing aura | geometric silica skeleton |
| Physarum | mobile network route | best fit for slime mold fantasy |
| Dictyostelium | starvation aggregate event | good crisis/transition organism |

### Microfauna

Microfauna should be powerful but low-count.

| Candidate | Suggested role | Why it works |
|---|---|---|
| Rotifer | anti-swarm filter | already fits current direction |
| Nematode | piercing raider | linear breach role |
| Tardigrade | neutral/gen/apex candidate | survival fantasy is too passive as a core route |
| Copepod | burst-mobility predator | explosive jumps, good event enemy |
| Daphnia | environmental reproduction switch | parthenogenesis vs stress mode |
| Hydra | apex/neutral boss only | regenerative fantasy is strong but scale is high |
| Planaria | apex/neutral boss only | regeneration can become anti-game if normal route |

## Recommended Add Order

1. Diatom as the next playable route or subroute.
2. Spirochete as a bacteria variant or tactical mutation.
3. Euglena as a flexible metabolic route if the light system becomes central.
4. Bacteriophage, heliozoan, lacrymaria and vorticella as neutral ecosystem actors.
5. Dinoflagellate, Bdellovibrio and magnetotactic bacteria as post-MVP specialties.

## Implementation Notes

Implemented in this pass:

- `V5RosterBalance` adds size class, play status, population weight, practical cap and squad size per route.
- `V5Balance.HardCellCap` is now `20`.
- Player division now checks weighted population load, not only raw cell count.
- Population stress now uses weighted load.
- HUD shows both raw cell count and biological load.
- Difficulty and ecological competition now use load instead of raw count.
- `V5AbilitySystem` treats Q/W/R as panel-only experimental actions by default.

Still pending:

- Explicit Specialization Tracker weight table.
- Tardigrade demotion from primary route to neutral/gen/apex once dependent systems are updated.
- Economy consolidation from six visible resources to three visible plus internal submaterials.
- Formal combat benchmark tests for 6v6, 10v10 swarm and microfauna encounters.
