# Changelog v5.14.8 -> v5.14.9

## Tema

UI de Genoma y consejeria alineadas con el sistema canonico de adaptaciones.

## Cambios principales

| Sistema | Cambio |
| --- | --- |
| Genoma | `V5GenomePanelIMGUI` ahora muestra un arbol compacto por tiers, no una lista vertical larga de botones. |
| Rutas | Las adaptaciones canonicas de la identidad/ruta actual se resaltan y pueden filtrarse con `Solo ruta`. |
| Progresion | Se dibujan conectores de prerequisitos entre adaptaciones para que el camino evolutivo se lea visualmente. |
| Detalle | Cada adaptacion muestra estado, costo, prerequisitos, efectos, counters, ruta afin y bonus Champion cuando corresponde. |
| Consejo | `V5AdvisorSystem` fue reescrito para separar modo adaptativo y modo legacy; en modo nuevo recomienda adaptaciones concretas. |
| Feedback | `V5FeedbackSystem` deja de presentar estructuras legacy como sistema principal cuando una adaptacion actualiza el fenotipo. |
| Comando | `V5ColonyCommandCenterIMGUI` limpia `DEBUG` y etiqueta el acceso a estructuras como rasgo legacy si hay adaptaciones. |

## Archivos modificados

- `Assets/_ProjectV5/Scripts/UI/V5GenomePanelIMGUI.cs`
- `Assets/_ProjectV5/Scripts/UX/V5AdvisorSystem.cs`
- `Assets/_ProjectV5/Scripts/UX/V5EvolutionPlannerIMGUI.cs`
- `Assets/_ProjectV5/Scripts/UX/V5FeedbackSystem.cs`
- `Assets/_ProjectV5/Scripts/Commands/V5ColonyCommandCenterIMGUI.cs`
- `Lista_Impacto_Codigo_v5_14_Adaptaciones.md`

## Validacion

- Compile Unity batch: OK sin errores C# en `Logs/codex_unity_compile_v5149b.log`.
- Smoke test completo: OK en `Logs/codex_unity_smoke_v5149_rerun.log`, termina con `Failures: 0`.
- Nota: el primer intento encontro el fallo externo recurrente de Burst JIT cache; se limpio `Library/BurstCache/JIT` y el rerun paso completo.

## Siguiente capa sugerida

1. Hacer que el HUD interior oculte el catalogo completo de estructuras por defecto y lo trate como modo avanzado.
2. Agregar telemetria de adaptaciones para ver pickrate, tiempo de instalacion y rutas dominantes.
3. Revisar visuales de morfologia para que lean adaptaciones directamente y no solo estructuras legacy.
