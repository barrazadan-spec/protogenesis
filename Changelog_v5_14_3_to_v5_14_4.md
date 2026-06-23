# Changelog v5.14.3 -> v5.14.4

Fecha: 2026-05-07

## Objetivo

Migrar la primera capa de codigo que todavia pensaba en "genes/estructuras" como canon principal hacia el nuevo sistema de adaptaciones e identidades.

## Cambios En Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Evolution/V5BiologyCanon.cs` | Agrega canon de adaptaciones por ruta/identidad, scoring adaptativo y licencias de estructuras por adaptacion. |
| `Assets/_ProjectV5/Scripts/Colony/V5PhenotypeRecipeLibrary.cs` | Las recetas germinales maduran por adaptaciones concretas ademas de afinidad legacy. |
| `Assets/_ProjectV5/Scripts/Colony/V5GerminalProductionSystem.cs` | Las castas auxiliares tienen requisitos biologicos por adaptacion, el panel muestra esos locks y las castas que coinciden con la ruta primaria se tratan como naturales del linaje. |
| `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs` | El HUD interior muestra canon adaptativo y etiquetas `adaptacion` cuando corresponde. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Smoke test valida canon adaptativo para Bacteria y Hongo. |

## Decisiones De Diseno Implementadas

- El canon nuevo es `Adaptaciones + Identidad`; genes y estructuras quedan como compatibilidad.
- Las hijas basicas de linaje siguen disponibles temprano, para no ahogar el tutorial.
- Las castas auxiliares si tienen locks duros por adaptacion, porque representan mezcla tactica fuera de la ruta primaria.
- Las estructuras legacy todavia existen, pero instalar una estructura fuera de una adaptacion afin cuesta mas y queda marcada como exploratoria.

## Validacion

- Compilacion Unity batch: OK, sin errores C#.
- Log: `Logs/codex_unity_compile_v5144b.log`.
- Smoke test completo: pendiente. `Logs/codex_unity_smoke_v5144b.log` carga la escena y recompila, pero no alcanza `EnteredPlayMode`; Unity reporta un error de Burst JIT/cache no relacionado con los cambios de gameplay.
