# Changelog v5.15.0 -> v5.15.1

## Objetivo

Convertir la telemetria en una herramienta util para validar el nuevo sistema de Adaptaciones durante playtest.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Systems/V5TelemetrySystem.cs` | Registra adaptaciones instaladas, hitos, tier, ruta empujada, fallos por categoria y expone panel `Telemetria`. |
| `Assets/_ProjectV5/Scripts/Evolution/V5AdaptationSystem.cs` | Emite eventos de telemetria al instalar o bloquear adaptaciones. |
| `Assets/_ProjectV5/Scripts/Release/V5RunSummarySystem.cs` | El resumen final reporta `Adaptaciones` y agrega resumen de genoma cuando el sistema nuevo esta activo. |
| `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | Reinicia telemetria al construir una run nueva. |
| `Assets/_ProjectV5/Scripts/Systems/V5RunResetSystem.cs` | Reinicia telemetria al reiniciar escenario. |
| `Assets/_ProjectV5/Scripts/Core/V5GameManager.cs` | Limpia telemetria al limpiar celdas/carga. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida telemetria instalada, conteo de adaptaciones y bloqueos duplicados. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.1. |

## Resultado Jugable

- El playtest ahora permite ver que rasgos se estan eligiendo realmente.
- Los bloqueos del arbol de Genoma quedan clasificados: cap, recursos, requisitos, duplicados u otros.
- El resumen de run ya no queda pegado al sistema viejo de genes cuando las Adaptaciones son la fuente de verdad.
- El panel `Telemetria` sirve como una camara interna para balancear rutas, caps y tutorial.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5151b.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5151b.log` termina con `Failures: 0`.
- Nota conocida: Unity batchmode sigue registrando un warning externo intermitente de Burst JIT cache; no proviene de los cambios de gameplay y no fallo el smoke.
