# Changelog v5.15.6 -> v5.15.7

## Objetivo

Cerrar el circuito entre diagnostico, consejo y Genoma: cuando el coach recomienda una adaptacion concreta, el jugador puede abrir el arbol ya enfocado en ese nodo y actuar sin buscar manualmente.

## Cambios principales

| Area | Cambio |
| --- | --- |
| Panel Genoma | `V5GenomePanelIMGUI` agrega `OpenFocused`, `FocusAdaptation` y `SelectedAdaptation` para abrir el arbol en una adaptacion especifica. |
| Sugerencia priorizada | El header del Genoma ahora prioriza `CoachAdaptation` sobre la sugerencia generica de identidad cuando el diagnostico trae una accion concreta. |
| Feedback visual | El nodo recomendado por el coach se marca con borde ambar y el detalle muestra la accion sugerida por diagnostico. |
| Panel Consejo | `V5HudIMGUI` sube a build `2.19` y agrega botones `Abrir Genoma` e `Instalar` cuando existe una sugerencia de adaptacion. |
| Smoke test | `V5PrototypeSmokeRunner` valida que el panel Genoma abra desde el coach y seleccione la adaptacion diagnosticada. |

## Archivos tocados

- `Assets/_ProjectV5/Scripts/UI/V5GenomePanelIMGUI.cs`
- `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs`
- `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs`

## Validacion

- Unity batch compile OK: `Logs/codex_unity_compile_v5157.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5157.log` termina con `Failures: 0`.

