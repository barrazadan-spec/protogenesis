# Protogenesis: Primordia — V5 Prototype 0.7

Iteración enfocada en profundidad sistémica: microzonas/biomas celulares, mejoras de linaje, amenazas adaptativas y balance runtime editable.

## Nuevos sistemas

### 1. Microzonas / Biomas celulares
Archivo principal:

`Assets/_ProjectV5/Scripts/Biomes/V5BiomeSystem.cs`

Clasifica cada zona del mapa según nutrientes, luz, oxígeno, toxinas, acidez, colonización, temperatura y detritus.

Tipos incluidos:

- NeutralBroth
- PhoticShelf
- OxygenBloom
- ToxicBloom
- AcidFrontier
- DetritusField
- ColonizedMatrix
- ThermalPocket

Las microzonas aplican efectos sistémicos:

- Fotosintéticos rinden más en PhoticShelf.
- Respiradores rinden más en OxygenBloom.
- Arqueas/quimiolitótrofos resisten AcidFrontier.
- ColonizedMatrix reduce stress de células propias.
- ToxicBloom daña células sin resistencia.
- DetritusField favorece recyclers/hongos.

Control:

`B` abre/cierra panel de microzonas.

### 2. Mejoras de linaje
Archivo principal:

`Assets/_ProjectV5/Scripts/Lineages/V5LineageUpgradeSystem.cs`

Convierte roles RTS en ramas semipermanentes:

- FarmerEnzymes: farmers recolectan mejor.
- ScoutChemotaxis: scouts se mueven más rápido.
- DefenderMembrane: defenders resisten más.
- ColonizerMatrix: colonizers colonizan más rápido.
- PredatorLysosomalBurst: predators hacen más daño.
- RecyclerAutolysisLoop: recyclers aprovechan detritus/cadáveres mejor.
- GeneralistHomeostasis: generalistas reciben bonos pequeños globales.

Control:

`X` abre/cierra panel de mejoras.

Las mejoras se guardan con F5/F9.

### 3. Ecología de amenazas adaptativas
Archivo principal:

`Assets/_ProjectV5/Scripts/Threats/V5ThreatEcologySystem.cs`

Las amenazas ahora reciben arquetipos según el ambiente y presión de partida:

- Drifter
- Hunter
- Bloomer
- Grazer
- Acidophile
- PhageCloud

El sistema calcula presión con tiempo, colonización, células del jugador y enemigos activos. Puede crear amenazas adaptativas si la partida se vuelve dominante.

Control:

`Z` abre/cierra panel de amenazas.

### 4. Balance runtime editable
Archivo principal:

`Assets/_ProjectV5/Scripts/Balance/V5BalanceProfileSystem.cs`

Permite tocar valores sin recompilar:

- multiplicador de recursos del jugador;
- multiplicador de costo de división;
- multiplicador de spawn enemigo;
- multiplicador de efecto de biomas;
- multiplicador de costo de mejoras de linaje.

Controles:

- `J`: panel de balance.
- `F6`: exportar JSON.
- `F8`: importar JSON.

Ruta exportada:

`Application.persistentDataPath/protogenesis_v5_balance_0_7.json`

## Sistemas conectados

Se agregaron referencias nuevas en `V5GameManager`:

- `Biomes`
- `LineageUpgrades`
- `ThreatEcology`
- `BalanceProfile`

`V5GameBootstrap` ahora crea estos sistemas automáticamente en la escena generada.

## Cambios de gameplay

- El movimiento puede ser modificado por upgrades de linaje.
- El farmeo puede ser modificado por upgrades de linaje.
- La colonización puede ser modificada por upgrades de linaje.
- El daño hecho y recibido puede ser modificado por upgrades de linaje.
- El costo de división se puede ajustar con balance runtime.
- Las microzonas aplican efectos de productividad/hostilidad a células.
- Las amenazas mutan según microzona y presión ecológica.

## Controles nuevos

- `B`: microzonas / biomas.
- `X`: mejoras de linaje.
- `Z`: ecología de amenazas.
- `J`: balance runtime.
- `F6`: exportar balance JSON.
- `F8`: importar balance JSON.

## Controles previos importantes

- `D`: dividir.
- `E`: panel interior.
- `G`: genes.
- `L`: roles de linaje.
- `N`: minimapa.
- `F10`: selector de escenarios.
- `F5`: quicksave.
- `F9`: quickload.

## Cómo probar

1. Abrir Unity.
2. Usar menú:

`Protogenesis > V5 > Create Full Prototype Scene`

3. Presionar Play.
4. Abrir paneles con `B`, `X`, `Z`, `J`.
5. Probar una run con First Drop y luego cambiar a Oxygen War / Acid Frontier desde `F10`.

## Nota técnica

La 0.7 sigue aislada dentro de:

`Assets/_ProjectV5/`

No reemplaza sistemas legacy del proyecto.
