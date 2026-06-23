# Changelog v5.15.1 -> v5.15.2

## Objetivo

Hacer que la telemetria de adaptaciones salga tambien en reportes JSON de playtest, para comparar runs y balancear el arbol de Genoma con datos.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | Crea explicitamente `V5RunSummarySystem` y `V5PlaytestReportSystem`. |
| `Assets/_ProjectV5/_Archive/V5PlaytestReportSystem.cs` | Amplia `V5PlaytestReportData` con genoma adaptativo, identidad, cap, fallos, top adaptaciones, recursos, cuerpo, squad, battlefield y loop. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida que el exportador exista y que `CreateReport()` incluya datos adaptativos. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.2. |

## Resultado Jugable

- El boton `Exportar reporte JSON` del resumen de run ya tiene un sistema garantizado detras.
- `Ctrl+F7` exporta reportes que explican que adaptaciones tomo el jugador, que ruta empujo y donde se bloqueo.
- Los reportes sirven para balancear cap, costos, tutorial y caminos del arbol de Genoma.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5152b.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5152.log` termina con `Failures: 0`.
