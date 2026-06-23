# Changelog v5.14.1 -> v5.14.2

## Implementado

- Se agrego `V5AdaptationSystem` como sistema principal de progreso biologico.
- Se agrego `V5AdaptationLibrary` con catalogo P0 instalable y mapeo legacy a estructuras/metabolismos.
- Se agrego `V5IdentityRecognizer` para consolidar identidad emergente desde adaptaciones.
- Se agrego `V5GenomePanelIMGUI`; la tecla `G` abre Genoma/Adaptaciones.
- El HUD superior muestra identidad y resumen de adaptaciones.
- El panel Interior deriva progreso biologico hacia Genoma y deja de competir con metabolismo.
- El cuerpo multicelular ahora requiere `BasicAdhesin`, `ColonialAdhesin` o `PersistentAdhesion`.
- `V5PlayableLoopSystem`, `V5AdvisorSystem`, `V5EvolutionPlannerIMGUI`, `V5FeedbackSystem` y `V5EcologySpawnPolicy` leen adaptaciones.
- Quicksave guarda/restaura adaptaciones.
- Smoke test actualizado para el flujo nuevo de Pared/Flagelo/Adesina.

## Validacion

- Compilacion Unity batch OK sin errores C#.
- Smoke test completo pendiente por lock de instancia de Unity.

## Pendiente

- Migrar `V5BiologyCanon` a adaptaciones/identidades.
- Agregar prerequisitos por adaptacion a recetas germinales.
- Mejorar el panel Genoma con arbol visual compacto y conectores.
- Agregar entradas de Codex para adaptaciones e identidades.
