# Changelog v5.15.4 -> v5.15.5

## Objetivo

Agregar un coach vivo no invasivo que muestre sugerencias contextuales cuando el diagnostico detecta una alerta real.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/UX/V5LiveCoachSystem.cs` | Nuevo sistema con cooldown, severidad, ultima accion y contador de intervenciones. |
| `Assets/_ProjectV5/Scripts/Core/V5GameManager.cs` | Agrega referencia runtime a `V5LiveCoachSystem`. |
| `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | Crea y reinicia el live coach en toda escena V5. |
| `Assets/_ProjectV5/Scripts/Systems/V5RunResetSystem.cs` | Reinicia live coach al reiniciar escenario. |
| `Assets/_ProjectV5/_Archive/V5PlaytestReportSystem.cs` | Reporte JSON sube a `playtest_report_v5_15_5` e incluye intervenciones, resumen y ultima accion del live coach. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida toast forzado, conteo de intervenciones y export JSON del live coach. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.5. |

## Resultado Jugable

- El juego puede avisar con un toast cuando el jugador entra en una alerta importante.
- El coach no spamea: respeta cooldown y solo interviene ante cambios relevantes.
- Los reportes de playtest registran cuantas veces el juego tuvo que intervenir.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5155.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5155.log` termina con `Failures: 0`.
