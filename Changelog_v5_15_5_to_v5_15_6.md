# Changelog v5.15.5 -> v5.15.6

## Objetivo

Hacer que el coach no solo recomiende una conducta general, sino una adaptacion concreta del Genoma cuando la alerta tiene un counter biologico claro.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Systems/V5RunDiagnosticsSystem.cs` | Agrega `CoachAdaptation`, etiqueta, estado y mapeos de sintomas a adaptaciones sugeridas. |
| `Assets/_ProjectV5/Scripts/UX/V5LiveCoachSystem.cs` | Conserva la ultima adaptacion sugerida y la agrega al resumen del live coach. |
| `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs` | Muestra `Genoma: adaptacion | estado` dentro del panel Consejo cuando aplica. |
| `Assets/_ProjectV5/_Archive/V5PlaytestReportSystem.cs` | Reporte JSON sube a `playtest_report_v5_15_6` e incluye sugerencia de adaptacion para diagnostico y live coach. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida que stress alto recomiende Catalasa/ROS y que la sugerencia llegue a LiveCoach/JSON. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.6. |

## Resultado Jugable

- El consejo puede decir `Abre Genoma (G) e instala Catalasa / ROS` en vez de una instruccion generica de homeostasis.
- El jugador ve si la adaptacion esta lista o bloqueada por recursos/requisitos.
- Los reportes de playtest registran que adaptacion especifica intento sugerir el coach.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5156.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5156.log` termina con `Failures: 0`.
