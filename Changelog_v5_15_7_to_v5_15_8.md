# Changelog v5.15.7 -> v5.15.8

## Objetivo

Hacer que el arbol de Genoma explique por que una adaptacion esta bloqueada y cual es el siguiente paso jugable, especialmente para prerequisitos y recursos.

## Cambios principales

| Area | Cambio |
| --- | --- |
| Sistema de adaptaciones | `V5AdaptationSystem` agrega helpers publicos para explicar instalacion: requisito faltante, checklist de prerequisitos, checklist de recursos, recursos faltantes, siguiente paso y explicacion completa. |
| Panel Genoma | `V5GenomePanelIMGUI` muestra `Siguiente`, prerequisitos con `[x]/[ ]`, recursos con `actual/requerido` y boton `Ver requisito` cuando una adaptacion depende de otra. |
| Arbol visual | Los nodos bloqueados ahora tienen una barra inferior por tipo de bloqueo: prerequisito, recursos, cap u otro. |
| Conectores | La linea entre requisito y adaptacion seleccionada se marca como faltante cuando el padre requerido no esta instalado. |
| HUD | Build visible sube a `2.20`. |
| Smoke test | `V5PrototypeSmokeRunner` valida que Mitocondria detecte Nucleo como requisito faltante y que el sistema entregue el siguiente paso correcto. |

## Archivos tocados

- `Assets/_ProjectV5/Scripts/Evolution/V5AdaptationSystem.cs`
- `Assets/_ProjectV5/Scripts/UI/V5GenomePanelIMGUI.cs`
- `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs`
- `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs`

## Validacion

- Unity batch compile OK: `Logs/codex_unity_compile_v5158b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5158.log` termina con `Failures: 0`.

