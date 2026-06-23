# Changelog v5.14.5 -> v5.14.6

Fecha: 2026-05-08

## Objetivo

Mover combate y conductas tacticas al lenguaje canonico de adaptaciones, reduciendo la dependencia de estructuras legacy.

## Cambios En Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Abilities/V5AbilitySystem.cs` | El panel se presenta como conductas bioactivas. El pulso ecologico elige efecto por adaptaciones: fotosintesis, extremofilia, digestion, puncion, cilios, toxina/red u homeostasis. |
| `Assets/_ProjectV5/Scripts/Combat/V5CombatSystem.cs` | Especiales de red, filtro ciliado y seudopodos digestivos reconocen adaptaciones del jugador ademas de estructuras legacy. |
| `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs` | El panel interior predice la conducta de pulso usando adaptaciones canonicas. |

## Resultado

- Q/W/R siguen sin ser hotkeys globales por defecto.
- Las acciones activas se sienten mas como conducta biologica contextual que como poderes MOBA.
- Las rutas nuevas pueden tener combate reconocible aunque no dependan de botones/estructuras viejas.

## Validacion

- Compilacion Unity batch: OK.
- Log: `Logs/codex_unity_compile_v5146.log`.
- Smoke test completo: OK.
- Log: `Logs/codex_unity_smoke_v5146_final.log`, `Failures: 0`.
