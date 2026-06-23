# Protogénesis: Primordia — Consolidación de Reglas y Mecánicas (Single-Player MVP) v1

> Fuente única de verdad del **MVP single-player**. Reconcilia el canon V5 (Vision Bible + código implementado) con la dirección de simulación explorada — que resultó ser **el mismo juego**, ya presente en tus enums. Poda el scope y difiere lo competitivo a v2.
> Decisión de dirección: **single-player primero (validar), multiplayer a año 2.**

---

# PARTE A — La poda

## A.1 Se mantiene (la columna vertebral)

Sistemas activos alineados con el canon. No se tocan salvo para pulir:

- **Evolution/** — rutas, ramas, doctrinas, adaptaciones, canon (el alma del juego).
- **Cells/** — `V5CellEntity`, dominios, metabolismos.
- **Colony/** — producción germinal, cuerpo multicelular, continuidad colonial.
- **World/** — `V5EnvironmentGrid` (medio vivo), eventos, reconocedor de campo de batalla.
- **Threats/** — counter-pressure, ecología de amenazas (el ecosistema antagonista).
- **Combat/**, **Abilities/**, **Core/**, **UI/**, **RTS/** (selección), **Visual/**.
- **Crisis/**, **Biomes/**, **Ecology/** — son tu Pilar 7 ("el ecosistema decide tanto como el jugador"). Canon, no creep.

## A.2 Se archiva (creep que choca con tu propio canon)

- **Districts/** (`Nursery, MetabolicReactor, DetoxMatrix, DefensiveBastion`) → es construcción de base. **Choca con el Pilar 1** ("no construye una base"). Mover a `_Archive`.
- **Lineages/** (`V5LineageUpgradeSystem`, upgrades persistentes) → **está en tu lista de descarte** ("stat persistente entre partidas — rompe la fantasía"). Mover a `_Archive`.
- **Automation/** (`V5ColonyAutomationSystem`) y **Commands/** (`V5ColonyCommandCenterIMGUI`) → capa de macro-RTS. Revisar: si no sirven a "construir un organismo", a `_Archive`.
- **`V5EvolutionPath` superset** (Archaea, Ciliate, Fungus, Macrophage, Microalga, BCell, Flagellate…) → el MVP usa solo `V5MvpRoute` (4 rutas). Las paths extra quedan como datos latentes para DLC/v2; **no se exponen en el MVP**.

## A.3 Se borra (cáscaras vacías)

Directorios sin scripts, restos de sistemas ya archivados: `Tutorial/, Strategy/, Research/, Operations/, Objectives/, Mutation/, Milestone/, Meta/, Intel/, Infrastructure/, Habitat/, Formations/, Forecast/`. Borrar los directorios.

## A.4 No tocar todavía

`V5RouteBranchSystem` (2090 líneas) y demás sistemas grandes que funcionan con smoke verde. **Regla de tu propio MVP doc: no refactorizar antes de validar el loop.** La limpieza nominal a V7 también espera.

## A.5 Al backlog v2 (no al MVP)

Todo lo competitivo diseñado esta semana: multiplayer (FFA/2v2/ranked), MMR/Fitness scoring, selección de cepa, eras como evento de mundo compartido. Va al archivo "Ideas / v2". Es buena visión — pero es año 2.

---

# PARTE B — Reglas y mecánicas consolidadas (single-player)

Cada sistema, con la regla unificada. La nota `[canon + sim]` señala dónde tu canon y la dirección de simulación convergen (y ya viven en tus enums).

## B.1 La tríada de control

`[canon + sim]` — Madre / Cuerpo / Squads = núcleo + colonia + especialistas.

- **Madre** (`V5CellRole.Mother`): ancla de mando, personaje con historia. Si el cuerpo cae, sobrevive como **Apex** (champion latente).
- **Cuerpo**: hijas adheridas en anillos (`V5BodyRing { Nucleus, Inner, Outer }`), con estados `Exposed / Partial / Complete / Overloaded`.
- **Squads**: hijas libres operando lejos del cuerpo.
- **Control en dos capas:** macro vía directivas de grupo (`V5Directive`: Farm, Defend, Explore, Colonize, Attack…) + micro directo sobre squads/especialistas. No se microea célula por célula.

## B.2 Dominios y progresión evolutiva

`[canon + sim]` — `V5CellDomain { LUCA → Prokaryote → Eukaryote → Multicellular }`.

La progresión se cruza con los **3 hitos** (decisiones irreversibles, Pilar 3):

1. **Núcleo** → transición a Eucariota. Cierra el camino procariota.
2. **Adhesión Persistente** → habilita Multicelular (el cuerpo).
3. **Diferenciación Celular** → castas/tejidos (Volvox).

## B.3 Metabolismo y economía

`[canon + sim]` — `V5MetabolismType { Respiration, Photosynthesis, Fermentation, Chemolithotrophy }`.

- Cadena: **Sustrato (del medio) → ATP (según metabolismo) → Biomasa → producción germinal (división)**.
- **3 recursos visibles: ATP, Biomasa, Stress.** Ni uno más (canon).
- El desecho de cada metabolismo modifica el medio (O₂, ácido, etc.) — esto alimenta el ecosistema antagonista (B.6).

## B.4 Adaptaciones e identidad emergente

`[canon + sim]` — las **22 adaptaciones**; la ruta **emerge**, no se elige (Pilar 5). Esto reemplaza la "selección de cepa" del diseño competitivo.

- Las 4 rutas MVP (`V5MvpRoute`): **Bacteria, Ameba, Productor, Volvox**.
- Cada ruta tiene **2 ramas**; cada rama, **2 doctrinas** (`Anclar` / `Radicalizar`). 16 fantasías doctrinales.
- El sistema reconoce tu ruta por las adaptaciones instaladas ("por cómo jugaste, tu linaje se está convirtiendo en esto").

## B.5 El cuerpo multicelular

`[canon]` — madre + adheridas en anillos; slots con roles (`V5BodySlotRole`: Armor, Motor, Producer, Mouth, Connector, Sensor, Reserve). El **estado Exposed** activa el **Champion Mode** (núcleo expuesto, estructuras latentes).

## B.6 El ecosistema antagonista (reemplaza las "eras competitivas")

`[canon + sim]` — Director Ecológico + counter-pressure + Crisis + Biomes.

- En single-player el antagonista es **el ecosistema, no otros jugadores**.
- Las presiones evolutivas que diseñé como "eras" (Gran Oxidación, glaciación, etc.) se reinterpretan como **escalada ecológica y crisis solo**, disparadas por tu propia ruta (un Productor oxigena su mundo → presión sobre anaerobios).
- Crisis (`OsmoticShock, ToxicRunoff, UVPulse, ResourceCrash, PredatorMigration, AcidStorm`) + liabilities internas por sobre-especialización = el "precio" de tu poder.

## B.7 Combate

`[canon + sim]` — verbos legibles mapeados a rutas/abilities: engullir (Ameba/fagocitosis), negación de área/toxina (Bacteria/Productor), atrincherar/biofilm (Bacteria defensiva). La habilidad de ruta (`V5AbilitySystem`) muta según doctrina.

## B.8 La columna narrativa (intacta — el alma)

`[canon]` — **Ruta → Rama → Doctrina → Objetivo doctrinal → Clímax.** No se toca; es lo que diferencia el juego. El Clímax nombra la historia del linaje; el **Diario de la Colonia** la resume post-partida.

## B.9 Victoria (single-player)

`[canon]` — **Dominancia Ecológica** (controlar 60% por 60 s) o **Supervivencia Apex** (180 s como champion), más el cierre del Clímax. Sin Fitness/MMR (eso es v2).

## B.10 Parámetros del MVP

| Parámetro | Valor |
|---|---|
| Recursos visibles | 3 (ATP, Biomasa, Stress) |
| Rutas / ramas / doctrinas | 4 / 2 por ruta / 2 por rama |
| Adaptaciones | 22 |
| Hitos irreversibles | 3 |
| Cap de entidades | 24 |
| Duración de partida | 15–30 min |
| Modo | Single-player vs ecosistema |

---

# PARTE C — Qué de "mi dirección" entra y qué no

**Entra (porque ya está en tu código o refuerza un pilar):**
- La cadena económica metabolismo → ATP → biomasa → división (ya tienes ATP/Biomasa + producción germinal).
- El medio vivo con difusión (ya tienes `V5EnvironmentGrid`).
- Las presiones evolutivas reinterpretadas como ecosistema antagonista (ya tienes Director/Crisis/counter-pressure).
- El control en dos capas (ya tienes directivas + squads).
- Los dominios procariota/eucariota/multicelular y la bifurcación (ya tienes `V5CellDomain` + los 3 hitos).

**No entra al MVP (va a v2):**
- Multiplayer, ranked, MMR, Fitness scoring.
- Selección de cepa (tu identidad **emerge**, no se elige).
- Eras como evento de mundo compartido entre jugadores.
- FFA / 2v2 / duelo.

---

# Próximo paso

Con esto, la poda y el ruleset están consolidados en un solo documento. El siguiente movimiento real ya no es diseñar: es **ejecutar la poda en Unity** (mover A.2 a `_Archive`, borrar A.3) y volver al loop de validación con smoke verde. Si quieres, te preparo el listado exacto de comandos/movimientos de archivo para la poda, o un prompt para Claude Code que la ejecute de forma segura.
