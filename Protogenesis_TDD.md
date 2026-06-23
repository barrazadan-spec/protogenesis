# Protogénesis: Caldo Primigenio — Especificación Técnica (TDD)

> Documento de implementación. Objetivo: que al programar no quede ambigüedad en el **núcleo de simulación**.
> Todos los números son valores de partida (`tunables`), marcados como tales. Identificadores en inglés para mapear directo a C#.
> Alcance de este documento: **Módulo 1 — Motor de simulación**. Networking (Módulo 2) y Presentación/UI (Módulo 3) se especifican aparte; aquí solo se dejan las interfaces que el núcleo expone.

---

## 0. Principios de implementación

1. **Simulación determinista por tick fijo**, separada del render. La lógica corre a `SIM_HZ` (ej. 10 Hz, `dt = 0.1s`); el render interpola.
2. **Datos orientados a estructuras** (SoA o arrays de structs) para que escale y sea portable a DOTS/ECS y a netcode determinista más adelante.
3. **Una sola fuente de verdad para constantes**: todas viven en `SimConfig` (sección 11). Nada de números mágicos en sistemas.
4. **El orden de los sistemas dentro del tick es parte del contrato** (sección 3). Cambiarlo cambia el comportamiento.
5. **Sin azar no sembrado**: todo RNG usa un `DeterministicRandom(seed)` por partida. Imprescindible para repeticiones y futuro lockstep.

---

## 1. Arquitectura de sistemas

El mundo es un `SimWorld` que contiene el estado y una lista ordenada de sistemas. Cada sistema implementa `Tick(SimWorld world, float dt)`.

| Sistema | Responsabilidad | Lee | Escribe |
|---|---|---|---|
| `MediumSystem` | Difusión y decaimiento de campos del medio | grid | grid |
| `AbsorptionSystem` | Cada célula extrae sustrato del medio | cells, grid | cells.atpInput, grid.substrate |
| `MetabolismEngine` | Convierte sustrato→ATP según metabolismo; emite desechos | cells | cells.atp, grid (O2/acid/CO2) |
| `UpkeepSystem` | Cobra mantenimiento; acumula biomasa; inanición | cells | cells.biomass, cells.alive |
| `PhenotypeSystem` | Recalcula stats derivadas (mass, integrity, defense, speed) desde el genoma | cells.genome | cells.stats |
| `ActionSystem` | Movimiento, emisión de toxina, progreso de canalizados | cells, grid | cells.pos, grid.toxin |
| `CombatSystem` | Resuelve fagocitosis, daño por toxina, biofilm, lisis | cells, grid | cells, grid.substrate |
| `DivisionSystem` | Procesa solicitudes de división (clonar/especializar) | cells | cells (+nuevas) |
| `ConjugationSystem` | Robo de genes entre células adyacentes | cells | cells.genome |
| `EraSystem` | Máquina de estados de eras: triggers, telegrafiado, modificadores globales | grid, world | world.era, world.modifiers |
| `ScoringSystem` | Acumula Fitness por jugador | cells, world | players.fitness |
| `NucleusSystem` | Estado de mando: detecta pérdida de núcleo, modo feral | cells, players | players.feral |

El orden de ejecución es el de la sección 3, **no** el de esta tabla.

---

## 2. Modelos de datos

```csharp
struct Cell {
    int       id;
    int       ownerId;        // jugador
    bool      alive;
    bool      isNucleus;
    Vector2   pos;            // coordenadas continuas dentro de la placa
    int       gridIndex;      // celda del medio que ocupa (cache de pos)

    // recursos
    float     atp;            // pool de energía (gastable este tick)
    float     biomass;        // acumulado; al umbral, divide

    // genoma: hasta `slots` genes expresados
    GeneInstance[] genome;    // longitud <= activeSlots
    int       activeSlots;

    // stats derivadas (las recalcula PhenotypeSystem; nunca a mano)
    CellStats stats;

    // temporizadores / estado
    float     refractory;     // s restantes antes de poder dividir
    EngulfState engulf;       // canalizado de fagocitosis en curso (o null)
    DivisionRequest pending;  // solicitud de división emitida por IA/jugador
}

struct CellStats {
    float mass;
    float integrity;          // PV actuales
    float maxIntegrity;
    float defense;            // 0..1, reduce toxina y dificulta engullir
    float speed;
    float uptakeRate;
    float upkeep;
}

enum MetabolismType { Photosynthesis, Fermentation, AerobicRespiration, Chemolithotrophy }

struct GeneInstance {
    GeneId id;                // ver catálogo sección 5
    byte   level;             // 1 o 2
}

struct MediumCell {           // una celda de la rejilla ~32x32
    float substrate;          // 0..SUBSTRATE_MAX
    float o2;                 // 0..1
    float co2;                // 0..1
    float acidity;            // pH invertido: 0 = neutro, 1 = muy ácido
    float toxin;              // 0..1
    float temperature;        // 0..1 (1 = cálido)
    float light;              // 0..1 (estático por celda, define por posición)
    float inorganic;          // sustrato para quimiolitotrofía (estático/lento)
}

struct Player {
    int   id;
    float fitness;
    int   nucleusCellId;      // -1 si feral
    bool  feral;
    float newNucleusTimer;    // cooldown para redesignar núcleo
    StrainId strain;
}

struct EraState {
    EraType current;
    EraType pending;          // None si no hay transición en curso
    float   telegraphTimer;   // s restantes de aviso; 0 = ya transicionó
    GlobalModifiers mods;     // ver sección 9
}
```

---

## 3. Bucle de simulación (orden de contrato)

```
SimWorld.Tick(dt):                 // dt = 1 / SIM_HZ
  1. MediumSystem        // difusión + decaimiento de O2, CO2, acidez, toxina, temperatura
  2. AbsorptionSystem    // cada célula resta sustrato del medio local -> atpInput crudo
  3. MetabolismEngine    // sustrato -> ATP segun metabolismo; emite desechos al medio
  4. UpkeepSystem        // ATP -= upkeep; excedente -> biomass; si biomass<=0 -> starve
  5. PhenotypeSystem     // recalcula stats desde genome (tras posibles cambios)
  6. ActionSystem        // mover, emitir toxina, avanzar canalizados de engulf
  7. CombatSystem        // completar engulfs, aplicar daño de toxina/biofilm, lisis
  8. ConjugationSystem   // robo de genes
  9. DivisionSystem      // procesa Cell.pending -> nuevas células
 10. NucleusSystem       // detecta núcleos muertos -> feral
 11. EraSystem           // triggers, telegraph, aplica modificadores globales
 12. ScoringSystem       // fitness += producción del tick + bonos
```

Notas: PhenotypeSystem va tras combate y antes de acciones del siguiente tick; tras división y conjugación se marca `dirty` el genoma y se recalcula al inicio del tick siguiente (o inmediatamente dentro de DivisionSystem para las hijas nuevas).

---

## 4. Economía y metabolismo (fórmulas exactas)

### 4.1 Absorción (`AbsorptionSystem`)

```
surface      = sqrt(cell.stats.mass)          // superficie ~ raíz de la masa
want         = cell.stats.uptakeRate * surface * dt
got          = min(want, medium[cell.gridIndex].substrate)
medium[idx].substrate -= got
cell._rawSubstrate = got                       // insumo para el metabolismo este tick
```

### 4.2 Conversión (`MetabolismEngine`)

Cada metabolismo es un motor I/O. `eff` = ATP por unidad de sustrato. Salida de desecho al medio local.

```
switch metabolism:

  Photosynthesis:                              // luz-dependiente, poco sustrato
    atpGain  = PHOTO_RATE * medium.light * dt + cell._rawSubstrate * 0.2
    medium.o2 += atpGain * O2_PER_ATP
    if !world.mods.aerobicEnabled: atpGain *= 1.0   // sin penalización
  Fermentation:                                // barato y rápido, anaerobio
    atpGain  = cell._rawSubstrate * FERMENT_EFF        // FERMENT_EFF ~ 0.4
    medium.acidity += atpGain * ACID_PER_ATP
    if hasResistance(cell): selfAcidDamage = 0
  AerobicRespiration:                          // alta energía, requiere O2
    o2avail  = medium.o2
    if o2avail <= 0: atpGain = 0
    else:
      used    = min(cell._rawSubstrate, o2avail / O2_PER_RESP)
      atpGain = used * RESP_EFF                  // RESP_EFF ~ 1.0 (el más alto)
      medium.o2  -= used * O2_PER_RESP
      medium.co2 += used * CO2_PER_USED
  Chemolithotrophy:                            // estable, techo bajo, nichos extremos
    atpGain  = min(medium.inorganic, CHEMO_RATE * dt) * CHEMO_EFF
    medium.inorganic -= atpGain / CHEMO_EFF

cell.atp += atpGain
cell._producedThisTick = atpGain               // lo usa ScoringSystem
```

Penalizaciones de era (aplicadas como `world.mods`, sección 9): p. ej. en Gran Oxidación, una célula anaerobia sin resistencia recibe `o2Toxicity = medium.o2 * O2_TOX_DMG * dt` a la integridad.

### 4.3 Mantenimiento y biomasa (`UpkeepSystem`)

```
upkeep = (BASE_UPKEEP
          + cell.stats.mass * UPKEEP_PER_MASS
          + sum(geneUpkeep[g] for g in cell.genome)) * dt

cell.atp -= upkeep
if cell.atp < 0:
    cell.biomass += cell.atp            // déficit consume biomasa (inanición)
    cell.atp = 0
else:
    atpCap = ATP_CAP_BASE + cell.stats.mass * ATP_CAP_PER_MASS   // [Q1]
    if cell.atp > atpCap:               // el excedente sobre el tope -> biomasa
        cell.biomass += (cell.atp - atpCap) * ATP_TO_BIOMASS
        cell.atp = atpCap               // el resto se conserva para acciones (toxina/engulf)

if cell.biomass <= 0:
    cell.alive = false                  // muere de hambre -> lisis en CombatSystem
```

> [Q1 cerrada] El ATP se conserva entre ticks hasta `atpCap`; el excedente pasa a biomasa. Las acciones (toxina/engulf) descuentan de `cell.atp`, de modo que pelear compite con crecer: gastar energía en atacar es renunciar a biomasa.

---

## 5. Genoma y fenotipo

### 5.1 Catálogo de genes (efectos como modificadores de stats)

| GeneId | Anillo | Nivel I | Nivel II | upkeep |
|---|---|---|---|---|
| `Photo` | 1 | metabolism=Photosynthesis | +PHOTO_RATE×1.5 | bajo |
| `Ferment` | 1 | metabolism=Fermentation | acidResist=true, acid arma×1.5 | bajo |
| `Chemo` | 1 | metabolism=Chemolithotrophy | acidImmune, +estabilidad | bajo |
| `Aerobic` | 1* | metabolism=AerobicRespiration | RESP_EFF×1.2 | medio |
| `Wall` | 2 | defense+0.3 | defense+0.5, indigesto (reflect) | medio |
| `Flagellum` | 2 | speed+SPEED_BONUS | +quimiotaxis (auto-pathing) | medio |
| `Toxin` | 2 | emite toxina r=1 | r=2, potencia×1.5 | alto |
| `Phago` | 2 | engulfRatio 1.25, +mass | ratio 1.1, canal×0.7 | medio |
| `Conjugation` | 2 | roba gen nivel 1 | roba con nivel, −exposición | bajo |
| `Swarm` | 3-prok | refractory×0.6 | refractory×0.3 | bajo |
| `Spore` | 3-prok | dormancia 1 era | esporas instantáneas | nulo (dormido) |
| `Resistance` | 3-prok | toxinResist 0.5 | toxinResist 1.0, reflect | medio |
| `Endosymbiont` | 3-euk | +orgánulo (mito/cloro) | optimiza; doble simbionte | alto |
| `Nucleus` | 3-euk | activeSlots+2 | activeSlots+3, mutación dirigible | alto |
| `Cytoskeleton` | 3-euk | mass+50%, +phago | forma/motilidad avanzada | alto |
| `Differentiation` | 4-euk | roles de célula | tejidos con sinergia | alto |
| `Integration` | 4-euk | colonia coordinada | organismo pluricelular | muy alto |

`*Aerobic` no está disponible para expresar hasta que `world.mods.aerobicEnabled == true`.

### 5.2 PhenotypeSystem (derivación de stats)

```
stats = BaseStats()                    // valores base de célula simple
for g in cell.genome:
    apply(stats, geneEffect[g.id][g.level])
stats.maxIntegrity = BASE_INTEGRITY + stats.mass * INTEGRITY_PER_MASS + wallIntegrityBonus
clamp(stats.defense, 0, 1)
cell.stats = stats
// integridad actual no se resetea aquí; solo el techo
```

### 5.3 Puertas (gating)

- Genes de Anillo 1 (excepto `Aerobic`): disponibles desde el inicio.
- `Aerobic`: requiere `aerobicEnabled`.
- Anillos 3/4 (la bifurcación): requieren `world.era >= GranOxidacion completada` **y** `cell.activeSlots >= FORK_SLOT_THRESHOLD`. Al elegir vía, se marca `cell.lineage = Prokaryote | Eukaryote` (excluyente).
- `slots`: empieza en `START_SLOTS`, +1 por era avanzada, +`Nucleus` bonus. Tope `MAX_SLOTS`.

---

## 6. División (`DivisionSystem`)

Una solicitud `DivisionRequest { type: Clone|Specialize, geneToExpress?, dir? }` la emite el jugador (UI) o la IA de colonia.

```
process(cell, req):
  if cell.refractory > 0: return
  if playerCellCount(cell.ownerId) >= MAX_CELLS_PER_PLAYER: return   // [Q3] cap duro de seguridad
  if req.type == Clone:
      if cell.biomass < DIV_THRESHOLD: return
      child = spawnAdjacent(cell)
      child.biomass = cell.biomass * 0.5
      cell.biomass *= 0.5
      child.genome = clone(cell.genome)
      applyDrift(child)                  // prob DRIFT_CHANCE
  if req.type == Specialize:
      if cell.biomass < DIV_THRESHOLD + SPECIALIZE_COST: return
      child = spawnAdjacent(cell)
      child.biomass = (cell.biomass - SPECIALIZE_COST) * 0.5
      cell.biomass  = (cell.biomass - SPECIALIZE_COST) * 0.5
      child.genome = clone(cell.genome)
      expressOrSwap(child, req.geneToExpress)   // respeta activeSlots y gating
  child.refractory = cell.refractory = refractoryFor(cell.lineage)  // PROK ~3s, EUK ~8s
  child.isNucleus = false
  PhenotypeSystem.recalc(child)
```

`spawnAdjacent`: elige la `MediumCell` vecina con menor ocupación; si la placa local está llena, la división falla (presión de espacio = razón natural para expandir o pelear).

`applyDrift`: con prob `DRIFT_CHANCE` (~5%), sube/baja en 1 el nivel de un gen aleatorio o aplica un micro-modificador de stat. Usa `DeterministicRandom`.

---

## 7. Combate (`CombatSystem`) — algoritmos

### 7.1 Fagocitosis (engulf)

```
// iniciar (en ActionSystem cuando jugador/IA ordena atacar a target adyacente)
if attacker.stats.mass >= engulfRatio(attacker) * target.stats.mass:
    attacker.engulf = { targetId, progress: 0 }

// avanzar (ActionSystem) y completar (CombatSystem)
channelTime = ENGULF_CHANNEL * phagoSpeedMod(attacker)
if target moved out of contact OR target.alive == false: abort engulf
attacker.engulf.progress += dt
attacker.atp -= ENGULF_ATP_COST * dt
if targetHasWall(target):
    channelTime *= WALL_SLOW_FACTOR
    attacker.stats.integrity -= wallReflect(target) * dt    // indigesto
if attacker.engulf.progress >= channelTime:
    attacker.biomass += target.biomass * ABSORB_EFFICIENCY   // ~0.7
    target.alive = false
    target._lysisDump = false        // absorbido: no recicla al medio
    attacker.engulf = null
```

### 7.2 Toxina

```
// emisión (ActionSystem) por células con gen Toxin
medium[cellArea(cell, radius)].toxin += TOXIN_EMIT * potency(cell) * dt
cell.atp -= TOXIN_ATP_COST * dt

// daño (CombatSystem) a TODA célula en celda con toxina (incluidas propias)
conc = medium[cell.gridIndex].toxin
mitigation = cell.stats.defense + toxinResist(cell)        // 0..1+
dmg = conc * TOXIN_DMG * (1 - clamp(mitigation,0,1)) * dt
cell.stats.integrity -= dmg
```

La toxina difunde y decae en `MediumSystem` (sección 8).

### 7.3 Biofilm (atrincherar)

Entidad `Biofilm { ownerId, cells[], hp }` que ocupa celdas del grid.
- Dentro de sus celdas: enemigos no pueden iniciar engulf, toxina entrante ×`BIOFILM_TOXIN_MULT` (<1), velocidad enemiga ×`BIOFILM_SLOW`.
- Se construye gastando biomasa por celda durante `BIOFILM_BUILD_TIME`. Tiene HP; se degrada con enzimas (gen futuro) o daño sostenido.

### 7.4 Lisis y reciclaje

```
onDeath(cell):
    if cell._lysisDump != false:
        medium[cell.gridIndex].substrate += cell.biomass * RECYCLE_FRACTION  // ~0.6
    remove(cell)
```

### 7.5 Pérdida de núcleo (`NucleusSystem`)

```
if !player.feral and !alive(player.nucleusCellId):
    player.feral = true
    player.newNucleusTimer = NEW_NUCLEUS_COOLDOWN
// feral: la IA de colonia solo ejecuta defaults (cosechar/replegar); sin órdenes ofensivas
// el jugador puede designar nuevo núcleo si tiene una célula viva y paga NEW_NUCLEUS_BIOMASS
```

---

## 8. El medio (`MediumSystem`)

Rejilla `GRID_W x GRID_H` (~32×32). Cada tick, para los campos difusibles (`o2, co2, acidity, toxin, temperature`):

```
// difusión discreta (Laplaciano de 4 vecinos)
for each field F with coeff D_F:
    newF[i] = F[i] + D_F * (sum(F[neighbors]) - 4*F[i])
// decaimiento
toxin[i]   *= (1 - TOXIN_DECAY * dt)
acidity[i] *= (1 - ACID_DECAY * dt)
// [Q2 cerrada] substrate NO difunde, pero recarga hacia su riqueza basal (no es difusión, es recarga):
substrate[i] += SUBSTRATE_REGEN * (baseline[i] - substrate[i]) * dt
// baseline[i] se fija al generar la placa; los picos de lisis decaen hacia él. inorganic se regenera igual.
```

Doble buffer (`F` y `newF`) para que la difusión sea simultánea, no secuencial (determinismo).

`light` e `inorganic` se inicializan por posición al generar la placa (define los nichos de spawn asimétricos).

### 8.1 Disparadores de era (calculados aquí, consumidos por EraSystem)

```
globalO2        = mean(medium.o2)
globalSubstrate = mean(medium.substrate)
world.signals.o2Crossed       = globalO2 > O2_ERA_THRESHOLD
world.signals.scarcityCrossed = globalSubstrate < SCARCITY_THRESHOLD
```

---

## 9. Eras (`EraSystem`) — máquina de estados

```
enum EraType { Anoxic, GreatOxidation, Glaciation, RadiationStorm, Scarcity, EukaryoticThreshold }

tick(world, dt):
  if era.pending != None:
      era.telegraphTimer -= dt
      if era.telegraphTimer <= 0:
          era.current = era.pending; era.pending = None
          applyModifiers(era.current)        // setea world.mods
      return
  // sin transición en curso: evaluar triggers
  next = evaluateTriggers(world)             // ver tabla
  if next != None and next != era.current:
      era.pending = next
      era.telegraphTimer = ERA_TELEGRAPH     // ~25s de aviso
      broadcastWarning(next)                 // UI/audio
```

| Era | Condición de trigger | `mods` que aplica |
|---|---|---|
| `Anoxic` | inicial | aerobicEnabled=false; sustrato alto |
| `GreatOxidation` | `o2Crossed && era==Anoxic` | aerobicEnabled=true; anaerobios sufren `O2_TOX_DMG` |
| `Glaciation` | cíclica (timer) | metabolismRate×COLD_MULT; difusión×0.5 |
| `RadiationStorm` | cíclica (timer) | DRIFT_CHANCE×RAD_MULT (mutación global) |
| `Scarcity` | `scarcityCrossed` | regeneración de sustrato↓; engullir da +bonus |
| `EukaryoticThreshold` | `o2 establecido` un tiempo | habilita bifurcación/anillos 3-4 |

`applyModifiers` setea un struct `GlobalModifiers` que leen MetabolismEngine, DivisionSystem, etc. Las eras cíclicas se programan a intervalos `ERA_INTERVAL` eligiendo con `DeterministicRandom` entre Glaciation/RadiationStorm. Avanzar de era hace `player.maxSlots++` y dispara el aviso.

---

## 10. Cepas, puntuación, fin de partida

### 10.1 Cepas (data-driven)

```
StrainDef { id, startMetabolism, startGenes[], passive }
// passive es un hook: ej. Cianos -> O2_PER_ATP×1.3; Cazadores -> ENGULF_CHANNEL×0.8; etc.
```

Las 6 cepas (sección 7 del GDD) se definen como `StrainDef` en datos, no en código. El `passive` es un modificador aplicado al crear el inóculo y/o un override de constante para ese jugador.

### 10.2 Fitness (`ScoringSystem`)

```
player.fitness += sum(cell._producedThisTick for cell in player.cells) * FIT_PRODUCTION
on milestone (firstEukaryote, firstMulticellular): player.fitness += FIT_MILESTONE
on engulf completed: player.fitness += absorbedBiomass * FIT_ENGULF
```

### 10.3 Condiciones de fin

- **Eliminación**: un jugador sin células vivas queda fuera; último en pie gana.
- **Fin por eras**: al completar la última era (`ERAS_PER_MATCH`, 4–6), gana la mayor `fitness`.
- Empate por `fitness` se rompe por biomasa total actual.

---

## 11. `SimConfig` — constantes (fuente única de verdad)

```
SIM_HZ              = 10           // dt = 0.1s
GRID_W = GRID_H     = 32
ERAS_PER_MATCH      = 5            // 4–6
ERA_TELEGRAPH       = 25           // s
ERA_INTERVAL        = 180          // s entre eras cíclicas (tunable)

// economía
SUBSTRATE_MAX       = 100
PHOTO_RATE          = 3.0
FERMENT_EFF         = 0.4
RESP_EFF            = 1.0
CHEMO_RATE          = 2.0
CHEMO_EFF           = 0.7
O2_PER_ATP          = 0.5
O2_PER_RESP         = 0.4
CO2_PER_USED        = 0.3
ACID_PER_ATP        = 0.2
ATP_TO_BIOMASS      = 1.0
ATP_CAP_BASE        = 20           // [Q1] tope base del pool de ATP
ATP_CAP_PER_MASS    = 2.0          // [Q1] tope adicional por unidad de masa
BASE_UPKEEP         = 0.5
UPKEEP_PER_MASS     = 0.1
SUBSTRATE_REGEN     = 0.02         // [Q2] recarga del sustrato hacia baseline (lenta)
MAX_CELLS_PER_PLAYER= 200          // [Q3] cap duro de seguridad por jugador

// división
DIV_THRESHOLD       = 100
SPECIALIZE_COST     = 40
REFRACTORY_PROK     = 3.0
REFRACTORY_EUK      = 8.0
DRIFT_CHANCE        = 0.05

// genoma
START_SLOTS         = 3
MAX_SLOTS           = 8
FORK_SLOT_THRESHOLD = 4

// combate
ENGULF_RATIO        = 1.25
ENGULF_CHANNEL      = 1.5          // s
ENGULF_ATP_COST     = 2.0         // /s
ABSORB_EFFICIENCY   = 0.7
WALL_SLOW_FACTOR    = 1.8
TOXIN_EMIT          = 0.5
TOXIN_ATP_COST      = 1.5         // /s
TOXIN_DMG           = 4.0         // /s a conc=1
BASE_INTEGRITY      = 50
INTEGRITY_PER_MASS  = 5
SPEED_BONUS         = 1.5
RECYCLE_FRACTION    = 0.6
NEW_NUCLEUS_BIOMASS = 60
NEW_NUCLEUS_COOLDOWN= 4

// medio (coeficientes de difusión por tick)
D_O2 = 0.12; D_CO2 = 0.12; D_ACID = 0.08; D_TOXIN = 0.10; D_TEMP = 0.05
TOXIN_DECAY = 0.15; ACID_DECAY = 0.05
O2_ERA_THRESHOLD    = 0.4
SCARCITY_THRESHOLD  = 0.15
O2_TOX_DMG          = 3.0
COLD_MULT           = 0.5
RAD_MULT            = 4.0

// fitness
FIT_PRODUCTION = 1.0; FIT_MILESTONE = 500; FIT_ENGULF = 1.5
```

---

## 12. Interfaces hacia otros módulos

- **Hacia Networking (Módulo 2)**: el `SimWorld` es serializable; el servidor autoritativo corre el `Tick` y emite snapshots delta. Las entradas del cliente son `Command { playerId, type, params }` (ej. `SetStance`, `OrderDivision`, `ExpressGene`, `OrderEngulf`, `DesignateNucleus`). Tick de red 5–10 Hz, interpolación en cliente.
- **Hacia Presentación (Módulo 3)**: render solo lee el `SimWorld` (no escribe). Overlays = visualización directa de los campos de `MediumCell`. La cámara/zoom no toca la simulación.
- **IA de colonia**: traduce posturas (`Expand/Hold/Harvest/Retreat`) en `DivisionRequest` y órdenes de movimiento por célula. Es un sistema aparte que produce `Command`s, de modo que IA y jugador usan el mismo canal (clave para que feral funcione).

---

## 13. Decisiones cerradas (registro)

Las siete preguntas abiertas quedaron resueltas y ya están reflejadas en las secciones correspondientes:

1. **ATP entre ticks** → pool con tope. Se acumula hasta `atpCap = ATP_CAP_BASE + masa·ATP_CAP_PER_MASS`; el excedente pasa a biomasa; las acciones descuentan del pool, así pelear compite con crecer. (§4.3, §11)
2. **¿El sustrato difunde?** → no difunde, pero recarga hacia su `baseline` con `SUBSTRATE_REGEN`. Los cementerios de lisis decaen lentamente hacia ese baseline; no hay parches muertos permanentes. (§8, §11)
3. **Tope de células** → ambos límites: upkeep creciente (blando) + `MAX_CELLS_PER_PLAYER = 200` (cap duro). `DivisionSystem` aborta al alcanzar el cap. (§6, §11)
4. **Multicelularidad** → híbrido. `Differentiation` (N1) asigna `CellRole` (cosechadora / guerrera / estructural) a células que siguen siendo entidades sueltas; `Integration` (N2) las fusiona en una entidad `Organism` controlable como unidad, lo que además baja el conteo de entidades. Pendiente de añadir `enum CellRole` y la entidad `Organism` al modelo de datos (§2).
5. **Representación para red** → la simulación es siempre individual, protegida por el cap de Q3. La agregación por clusters queda diferida al Módulo 2 y solo se construye si el perfilado lo exige; no se pre-optimiza. (§12)
6. **Quimiotaxis (Flagelo N2)** → ascenso de gradiente sobre el medio (sube por sustrato, baja por toxina). Barato, determinista, sin A*. Se implementa en `ActionSystem`.
7. **Esporulación** → la espora se queda en el grid como estado `dormant`: `defense` muy alta y `upkeep` nulo, sobrevive las presiones de era, pero un ataque directo todavía la destruye. Escape ambiental, no escudo de combate.


---

## 14. Orden de implementación sugerido

1. `SimWorld` + modelos de datos + `SimConfig` + `DeterministicRandom`.
2. `MediumSystem` (grid + difusión) — testeable solo.
3. `AbsorptionSystem` + `MetabolismEngine` + `UpkeepSystem` — economía de una célula que crece.
4. `PhenotypeSystem` + catálogo de genes (sin combate aún).
5. `DivisionSystem` — una célula se vuelve colonia.
6. `CombatSystem` (engulf → toxina → biofilm → lisis).
7. `EraSystem` + modificadores globales.
8. `ScoringSystem`, `NucleusSystem`, `ConjugationSystem`, cepas.
9. IA de colonia (posturas → comandos).
10. Recién entonces: Networking (Módulo 2) y Presentación (Módulo 3).

Cada paso es jugable/testeable en aislamiento contra bots o en sandbox — que es exactamente el “single-player primero” del plan técnico.
