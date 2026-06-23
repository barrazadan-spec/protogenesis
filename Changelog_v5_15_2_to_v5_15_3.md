# Changelog v5.15.2 -> v5.15.3

## Objetivo

Agregar un diagnostico de run que traduzca telemetria y estado de partida en alertas accionables para playtest.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Systems/V5RunDiagnosticsSystem.cs` | Nuevo sistema con score, alertas, consejo prioritario, panel `Diagnostico` y export JSON. |
| `Assets/_ProjectV5/Scripts/Core/V5GameManager.cs` | Agrega referencia runtime a `V5RunDiagnosticsSystem`. |
| `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | Crea el diagnostico en toda escena V5 y lo reinicia al iniciar run. |
| `Assets/_ProjectV5/Scripts/Systems/V5RunResetSystem.cs` | Reinicia diagnostico al reiniciar escenario. |
| `Assets/_ProjectV5/_Archive/V5PlaytestReportSystem.cs` | Reporte JSON sube a `playtest_report_v5_15_3` e incluye score, estado, consejo y reporte de diagnostico. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida diagnostico y campos nuevos del reporte JSON. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.3. |

## Resultado Jugable

- El panel `Diagnostico` muestra si una run se ve legible, si esta jugable con alertas o si conviene revisar balance.
- Las alertas apuntan a problemas concretos: cap, bloqueos del Genoma, identidad confusa, cuerpo sin uso, poblacion baja, stress, toxinas o presion enemiga.
- Cada reporte JSON incluye una recomendacion prioritaria para revisar despues del playtest.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5153.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5153.log` termina con `Failures: 0`.
- Nota conocida: Unity batchmode aun puede registrar warning externo de Burst JIT cache; no proviene de estos cambios ni fallo el smoke.
