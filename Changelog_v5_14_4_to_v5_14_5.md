# Changelog v5.14.4 -> v5.14.5

Fecha: 2026-05-08

## Objetivo

Hacer que Codex, Director Ecologico y Planner expliquen el juego desde el sistema canonico de adaptaciones e identidades.

## Cambios En Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/UX/V5CodexSystem.cs` | Reescrito como Codex adaptativo: registra adaptaciones, identidades, efectos, canon, ruta afin y counters. |
| `Assets/_ProjectV5/Scripts/Evolution/V5AdaptationSystem.cs` | Al instalar una adaptacion, registra entrada rica en Codex. |
| `Assets/_ProjectV5/Scripts/Evolution/V5IdentityRecognizer.cs` | Al cambiar identidad, registra entrada de identidad en Codex. |
| `Assets/_ProjectV5/Scripts/World/V5EcologySpawnPolicy.cs` | Despertar ecologico usa adaptaciones activas, identidad y senales canonicas como fuente primaria. |
| `Assets/_ProjectV5/Scripts/Threats/V5ThreatEcologySystem.cs` | Las amenazas adaptativas responden a causa biologica visible: fotosintesis, red territorial, depredacion, movilidad, biofilm o metabolismo extremo. |
| `Assets/_ProjectV5/Scripts/UX/V5EvolutionPlannerIMGUI.cs` | El planner muestra adaptaciones canonicas por ruta y progreso adaptativo; estructuras quedan como fallback. |
| `Assets/_ProjectV5/Scripts/Systems/V5PlayableLoopSystem.cs` | Agrega `RefreshNow()` para refrescar el estado del loop sin esperar al tick. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Usa `RefreshNow()` antes de validar avance del loop, evitando falsos negativos por timing. |

## Resultado

- El juego explica mejor por que aparece una amenaza.
- El jugador ve rutas como listas de adaptaciones concretas, no como estructuras legacy.
- El Codex ya puede servir como memoria de diseno dentro del juego.

## Validacion

- Compilacion Unity batch: OK.
- Log: `Logs/codex_unity_compile_v5145b.log`.
- Smoke test completo: OK en `Logs/codex_unity_smoke_v5145b.log`, `Failures: 0`.
- Se limpio `Library/BurstCache/JIT` para resolver el fallo externo de Burst JIT/cache.
