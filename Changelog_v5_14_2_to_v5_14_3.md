# Changelog v5.14.2 -> v5.14.3

## Correcciones de diseno

- El cap de adaptaciones activas sube de 12 a 14.
- Champion/apex eleva el cap activo a 16.
- `Enzimas extracelulares` sube de P1 a P0 para que Hongo tenga fantasia saprofita completa.
- `Hongo` ya no se reconoce con Hifa sola; requiere Hifa + Enzimas.
- `Quitina` queda P1 como defensa avanzada del Hongo.
- `Espirilo` y `Bdellovibrio` se degradan a subrutas P2 hasta tener adaptaciones reales.
- Se agregan campos canonicos para fichas: Tier visual, Latente Champion, Efecto Champion, Counter natural y Telemetria.

## Implementado en codigo

- Agregado `V5AdaptationId.ExtracellularEnzymes`.
- Agregados metadatos de visual/counter/telemetria/champion en `V5AdaptationDefinition`.
- `V5AdaptationSystem` usa cap dinamico 14/16.
- `V5IdentityRecognizer` reconoce Hongo solo con `FungalHypha + ExtracellularEnzymes`.
- El plan rapido de Hongo en `V5ColonyCommandCenterIMGUI` incluye Enzimas.
- `V5GenomePanelIMGUI` muestra tier visual, counters y efectos Champion.

## Validacion

- Compilacion Unity batch OK sin errores C# en `Logs/codex_unity_compile_v5143.log`.
