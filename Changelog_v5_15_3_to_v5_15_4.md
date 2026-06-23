# Changelog v5.15.3 -> v5.15.4

## Objetivo

Conectar el diagnostico de run con el Consejero en vivo, para que las alertas de playtest tambien puedan guiar al jugador dentro de la partida.

## Cambios De Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Systems/V5RunDiagnosticsSystem.cs` | Agrega `CoachAdvice` y `CoachAction` en lenguaje jugable, separados de la prioridad tecnica. |
| `Assets/_ProjectV5/Scripts/UX/V5AdvisorSystem.cs` | Usa diagnostico cuando hay alertas reales y expone `UsingDiagnosticAdvice`/`DiagnosticStatus`. |
| `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs` | El panel `Consejo` se expande cuando usa diagnostico y muestra score/estado. |
| `Assets/_ProjectV5/_Archive/V5PlaytestReportSystem.cs` | Reporte JSON sube a `playtest_report_v5_15_4` e incluye consejo/accion de coach. |
| `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs` | Valida que el Consejero tome diagnostico bajo stress alto y que el JSON exporte accion de coach. |
| `Lista_Impacto_Codigo_v5_14_Adaptaciones.md` | Documenta la capa v5.15.4. |

## Resultado Jugable

- El Consejero ya no depende solo de reglas fijas; puede leer sintomas de la run.
- Si la madre entra en stress alto, el Consejo cambia a una accion concreta de supervivencia.
- Los reportes JSON ahora guardan tanto diagnostico tecnico como accion jugable recomendada.

## Validacion

- Compile OK: `Logs/codex_unity_compile_v5154b.log`.
- Smoke OK: `Logs/codex_unity_smoke_v5154_final.log` termina con `Failures: 0`.
