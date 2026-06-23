# Protogenesis: Primordia — V5 Prototype 1.3

## Objetivo de la iteración

La 1.3 transforma el prototipo en una demo interna más clara: ya no solo hay sistemas sueltos, sino una progresión legible desde LUCA hasta biosfera estable.

## Sistemas agregados

### 1. Campaña interna 1.3
Archivo: `Scripts/Meta/V5CampaignEpisodeSystem.cs`

Tecla: `PageUp`

Episodios:

1. LucaAwakening
2. MetabolicFork
3. FirstColony
4. TerritorialMatrix
5. EcologicalCrisis
6. ApexEmergence
7. StableBiosphere

Cada episodio tiene objetivo, progreso y recompensa. Esto ordena la demo para playtest sin depender de PvP.

### 2. Sucesión ecológica
Archivo: `Scripts/Systems/V5EcosystemSuccessionSystem.cs`

Tecla: `PageDown`

Etapas:

- PrimordialSoup
- FirstBiofilm
- MetabolicBloom
- EngineeredMicrobiome
- PredatorWeb
- StableBiosphere
- CollapseRisk

La sucesión lee colonización, oxígeno, toxinas, acidez, diversidad, células y amenazas. También puede disparar victoria por biosfera estable.

### 3. Demo Snapshot Export
Archivo: `Scripts/Release/V5DemoSnapshotExportSystem.cs`

Controles:

- `Insert`: panel de snapshot.
- `Ctrl + Enter`: exportar JSON.

Exporta a `Application.persistentDataPath/protogenesis_v5_demo_snapshot_1_3.json` con estado de run, madre, genes, ambiente, sucesión y campaña.

### 4. Auto-instalador 1.3
Archivo: `Scripts/Core/V5Prototype13AutoInstaller.cs`

Instala automáticamente:

- `V5EcosystemSuccessionSystem`
- `V5CampaignEpisodeSystem`
- `V5DemoSnapshotExportSystem`

No requiere editar escenas manualmente.

## Cambios menores

- `V5Balance.SaveVersion` actualizado a 1.3.
- HUD actualizado a build 1.3.
- Overlay de hotkeys actualizado.

## Cómo probar

1. Abre Unity.
2. Menú: `Protogenesis > V5 > Create Full Prototype Scene`.
3. Presiona Play.
4. Usa:
   - `PageUp` para campaña 1.3.
   - `PageDown` para sucesión ecológica.
   - `Insert` para snapshot.
   - `Ctrl + Enter` para exportar snapshot JSON.

## Qué buscar en playtest

- ¿Se entiende qué hacer sin leer el GDD?
- ¿La sucesión ecológica hace que el mundo parezca vivo?
- ¿La campaña ordena el paso de célula sola a colonia?
- ¿La victoria por biosfera estable se siente distinta a matar enemigos?
- ¿El snapshot JSON sirve para comparar runs?

## Nota

Sigue siendo prototipo. Los sistemas están aislados en `_ProjectV5` y no reemplazan el proyecto viejo.
