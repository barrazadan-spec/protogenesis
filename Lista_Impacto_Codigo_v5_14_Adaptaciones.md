# Lista de Impacto Codigo v5.14 - Sistema de Adaptaciones

## Objetivo

Implementar el sistema canonico de Adaptaciones sin reescribir todo el proyecto. La estrategia es incremental:

```text
V5AdaptationSystem = fuente de verdad nueva
Sistemas legacy = efectos, visuales y compatibilidad temporal
```

---

## 1. Scripts Nuevos

| Script | Prioridad | Responsabilidad |
| --- | --- | --- |
| `V5AdaptationTypes.cs` | Alta | enums de AdaptationId, tags, transicion, estado |
| `V5AdaptationDefinition.cs` | Alta | datos de costo, prerequisitos, efectos, conflictos |
| `V5AdaptationLibrary.cs` | Alta | catalogo inicial de adaptaciones |
| `V5AdaptationSystem.cs` | Alta | instalar, bloquear, aplicar efectos legacy |
| `V5IdentityRecognizer.cs` | Alta | calcular identidad, subruta y apex cercano |
| `V5GenomePanelIMGUI.cs` | Alta | reemplazo contextual del panel G |
| `V5AdaptationEventLog.cs` | Media | historial legible para HUD/Codex/debug |

---

## 2. Scripts A Modificar

| Script | Cambio |
| --- | --- |
| `V5HudIMGUI.cs` | G debe abrir GenomePanel. HUD debe mostrar identidad emergente y adaptacion sugerida. |
| `V5GeneSystem.cs` | Mantener temporalmente como fachada legacy o debug. No debe ser UI principal. |
| `V5CellEntity.cs` | Agregar lista de adaptaciones instaladas o lectura via AdaptationSystem. |
| `V5GerminalProductionSystem.cs` | Desbloquear recetas segun identidad/adaptaciones, no solo ruta. |
| `V5MulticellularBodySystem.cs` | Separar Adesina basica de Adhesion Persistente. |
| `V5BodyPanelIMGUI.cs` | Mostrar requisitos de cuerpo desde adaptaciones. |
| `V5ThreatEcologySystem.cs` | Director debe leer senales e identidad emergente. |
| `V5EvolutionAffinitySystem.cs` | Mantener solo debug o retirar de HUD principal. |
| `V5BiologyCanon.cs` | Migrar tablas de rutas/canon a adaptaciones e identidades. |
| `V5CodexSystem.cs` | Entradas nuevas para adaptaciones, identidades y counters. |
| `V5AdvisorSystem.cs` | Recomendaciones basadas en adaptaciones disponibles. |
| `V5PlayableLoopSystem.cs` | Tutorial: Motor/Adesina/Adaptaciones, no anillos de genes. |

---

## 3. Primer Paquete Implementable

No implementar las 39+ adaptaciones de una vez. Primer corte:

| Grupo | Adaptaciones |
| --- | --- |
| T1 | Pared bacteriana, Flagelo bacteriano, Capsule, Pili/Fimbrias, Adesina basica, Tilacoide, Membrana extremofila, Bomba protones |
| T2 | Nucleo, Mitocondria, Cloroplasto |
| T3 | Lisosoma, Pseudopodos, Cilios, Vacuola, Frustula, Hifa, Plasmodio, Memoria quimica |
| T4 | Adhesion Persistente, Diferenciacion, Comunicacion por senales |

Identidades iniciales:

```text
Bacteria swarm
Biofilm
Arquea
Cianobacteria
Ameba
Ciliado
Microalga
Diatomea
Hongo
Moho mucilaginoso
Volvox temprano
```

---

## 4. Orden De Trabajo

1. Crear tipos y definiciones de adaptaciones.
2. Crear catalogo inicial.
3. Crear sistema de instalacion.
4. Mapear adaptaciones a estructuras/effects legacy.
5. Crear IdentityRecognizer.
6. Crear GenomePanelIMGUI contextual.
7. Cambiar G para abrir GenomePanel.
8. Ocultar o degradar arbol genico viejo.
9. Cambiar produccion germinal para leer identidad.
10. Cambiar cuerpo para usar Adesina basica y Adhesion Persistente.
11. Cambiar HUD/Advisor/Codex.
12. Cambiar Director Ecologico.

---

## 5. Regresion A Evitar

- No borrar `V5GeneSystem` hasta que GenomePanel y AdaptationSystem sean estables.
- No romper `V5StructureId`; muchas estadisticas y visuales ya dependen de estructuras.
- No volver a duplicar metabolismo en E y G.
- No hacer un arbol visual gigante como UI principal.
- No tratar Tardigrado como ruta core.
- No hacer locks ambientales duros para adaptaciones basicas.

---

## 6. Criterios De Validacion

| Criterio | Meta |
| --- | --- |
| El jugador entiende que G es progreso principal | 90% de sesiones internas |
| E deja de competir con G | no hay botones de metabolismo/progreso principal en E |
| Adesina basica aparece temprano | antes de formar cuerpo completo |
| Identidad cambia de forma explicable | HUD muestra "por que soy X" |
| Produccion cambia con adaptaciones | cada adaptacion core desbloquea/mejora receta |
| Director explica amenazas | amenaza muestra causa biologica |

---

## 7. Estado Implementado - Fase 1

Fecha: 2026-05-07.

Ya implementado en codigo:

| Area | Estado |
| --- | --- |
| Tipos canonicos | `V5AdaptationId`, `V5AdaptationTier`, `V5AdaptationKind`, `V5IdentityId`. |
| Catalogo P0 | `V5AdaptationLibrary` con T1-T5, costos, prerrequisitos, efectos y mapeo legacy. |
| Instalacion | `V5AdaptationSystem` instala adaptaciones, paga recursos, aplica estructuras/metabolismos legacy y stats directas. |
| Identidad emergente | `V5IdentityRecognizer` consolida BacteriaSwarm, Biofilm, Arquea, Cianobacteria, Protista, Ameba, Ciliado, Microalga, Diatomea, Hongo, Moho y Volvox temprano. |
| Panel G | `V5GenomePanelIMGUI` reemplaza G como panel principal de Genoma/Adaptaciones. El arbol genico viejo queda como fallback si no existe el panel nuevo. |
| HUD | HUD superior muestra identidad y resumen de adaptaciones. El panel interior deriva progreso a Genoma. |
| Adesina | `V5MulticellularBodySystem` requiere `BasicAdhesin`, `ColonialAdhesin` o `PersistentAdhesion` para pegar hijas. |
| Loop/tutorial | `V5PlayableLoopSystem` ya no exige Motor + gen; empuja primera adaptacion y luego Adesina basica. |
| Advisor/planner | Recomendaciones principales leen adaptaciones e identidad. |
| Produccion germinal | Cupo auxiliar y simbiosis leen `CellDifferentiation` / `SignalingCommunication`. |
| Director ecologico | Awakening usa conteo de adaptaciones como señal de progreso. |
| Feedback | Floating feedback dice "Adaptacion activada" cuando sube el conteo. |
| Save/load | Quicksave guarda/restaura lista de adaptaciones y recalcula identidad. |
| Smoke test | `V5PrototypeSmokeRunner` actualizado para validar adaptaciones + Adesina basica. |

Validacion tecnica:

- Compilacion Unity batch: OK sin errores C# en `Logs/codex_unity_compile_adaptaciones_4.log`.
- Smoke test completo: pendiente. El intento sin `-quit` fue bloqueado por Unity indicando otra instancia del proyecto abierta; el intento con `-quit` compila pero no ejecuta play mode.

Siguiente capa recomendada:

1. Migrar `V5BiologyCanon` de genes/rutas a adaptaciones/identidades.
2. Agregar vista visual tipo arbol en `V5GenomePanelIMGUI` con nodos compactos y conectores.
3. Hacer que recetas germinales tengan requisitos explicitos por adaptacion, no por estructura legacy.
4. Actualizar Codex con entradas de adaptaciones e identidades.
5. Reintentar smoke test cuando Unity no tenga lock de instancia.

---

## 8. Correccion v5.14.3

Aplicado:

| Correccion | Estado |
| --- | --- |
| Cap base 14 / apex 16 | Implementado en `V5AdaptationSystem`. |
| Hongo requiere Enzimas | Implementado en `V5IdentityRecognizer`. |
| Enzimas a P0 | Implementado en catalogo y codigo como `ExtracellularEnzymes`. |
| Quitina queda P1 | Documentado. |
| Espirilo/Bdellovibrio P2 | Documentado como subrutas futuras con adaptaciones pendientes. |
| Campos Champion/visual/counter/telemetria | Agregados a definicion y mostrados en panel Genoma. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5143.log`.

---

## 9. Correccion v5.14.4 - Canon Adaptativo En Produccion

Aplicado:

| Correccion | Estado |
| --- | --- |
| `V5BiologyCanon` lee adaptaciones | Agregado canon por ruta e identidad: `AdaptationsForRoute`, `AdaptationsForIdentity`, score por ruta y texto compacto. |
| Costos de estructuras migrados | `StructureInstallCostMultiplier` prioriza licencias por adaptacion cuando existe `V5AdaptationSystem`; genes quedan como fallback. |
| Recetas germinales maduran por adaptaciones | `V5PhenotypeRecipeLibrary` combina afinidad legacy con senales nuevas: cilios, lisosoma, pseudopodos, plastidos, hifa/enzimas, memoria quimica, etc. |
| Castas auxiliares con locks biologicos claros | Amoeboide requiere `Lysosome + Pseudopods`; Ciliada requiere `Cilia`; Simbionte bacteriano requiere pared/pili/adesina; Microalga requiere tilacoide/cloroplasto. |
| Castas naturales desbloqueadas | Si una casta auxiliar coincide con la ruta primaria, se trata como casta natural del linaje y no como mezcla externa bloqueada. |
| Panel germinal actualizado | La placa muestra requisitos de adaptacion junto a canon, ruta, madurez y destino. |
| HUD interior actualizado | Etiquetas de estructuras dicen `adaptacion` cuando la licencia viene del catalogo nuevo. |
| Smoke test ampliado | Valida canon adaptativo de Bacteria y que Hongo incluya `ExtracellularEnzymes`. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5144b.log`.
- Smoke test completo pendiente: `Logs/codex_unity_smoke_v5144b.log` carga la escena y recompila, pero no alcanza `EnteredPlayMode`; aparece un error de Burst JIT/cache fuera del cambio de gameplay.

---

## 10. Correccion v5.14.5 - Explicacion Ecologica Adaptativa

Aplicado:

| Correccion | Estado |
| --- | --- |
| Codex adaptativo | `V5CodexSystem` ahora registra adaptaciones e identidades con efectos, ruta afin, counters y canon. |
| Identidad al Codex | `V5IdentityRecognizer` desbloquea entrada de identidad cuando cambia el patron dominante. |
| Adaptaciones al Codex | `V5AdaptationSystem` llama `ObserveAdaptation` en vez de registrar solo texto generico. |
| Despertar ecologico | `V5EcologySpawnPolicy` calcula presion desde adaptaciones activas, identidad y senales canonicas, con fallback legacy. |
| Amenazas con causa | `V5ThreatEcologySystem` elige respuestas por fotosintesis, red territorial, depredacion, movilidad, biofilm o metabolismo extremo, y muestra la causa en el mensaje. |
| Planner actualizado | `V5EvolutionPlannerIMGUI` muestra adaptaciones canonicas por ruta y progreso adaptativo cuando existe `V5AdaptationSystem`; estructuras quedan como fallback. |
| Smoke determinista | `V5PlayableLoopSystem.RefreshNow()` permite refrescar el estado del loop antes de validaciones automatizadas. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5145b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5145b.log` termina con `Failures: 0`.
- Se limpio `Library/BurstCache/JIT` para resolver el fallo externo de Burst JIT/cache.

---

## 11. Correccion v5.14.6 - Conductas De Combate Por Adaptacion

Aplicado:

| Correccion | Estado |
| --- | --- |
| Panel bioactivo renombrado | `V5AbilitySystem` pasa de "habilidades experimentales" a "conductas bioactivas": Impulso, Senal y Reparar. |
| Pulso adaptativo | `ApplyPulseForCell` decide entre bloom O2, bolsa extrema, digestion radial, puncion, corriente ciliada, toxina/red u homeostasis usando adaptaciones e identidad. |
| Combate adaptativo | `V5CombatSystem` reconoce redes, cilios y seudopodos/lisosoma desde adaptaciones, no solo desde estructuras legacy. |
| HUD interior coherente | `V5HudIMGUI.PulseMode` predice la conducta K usando adaptaciones canonicas. |
| Fallback legacy | Estructuras y flags antiguos siguen funcionando para NPC, amenazas y saves previos. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5146.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5146_final.log` termina con `Failures: 0`.

---

## 12. Correccion v5.14.7 - Crisis Y Sucesion Por Adaptacion

Aplicado:

| Correccion | Estado |
| --- | --- |
| Deuda adaptativa canonica | `V5CrisisAndStabilitySystem` suma deuda por complejidad real de adaptaciones, identidad, stress, poblacion y monocultivo. |
| Homeostasis por adaptaciones | CatalasaROS, Vacuola contractil, Comunicacion y Champion reducen deuda o dano de crisis. |
| Crisis contextuales | Las crisis se eligen por causa biologica: fotosintesis -> UV, redes -> crash de recursos, depredacion -> migracion, extremofilia -> acidez, biofilm -> shock osmotico. |
| Mutaciones negativas contextuales | Fragilidad, fuga metabolica, ruido hereditario, hambre y drift dependen de adaptaciones instaladas o ausentes. |
| Sucesion ecologica funcional | `V5EcosystemSuccessionSystem` calcula diversidad y productividad desde adaptaciones, identidad y presion ecologica. |
| Continuidad colonial adaptativa | `V5ColonialContinuitySystem` suma adhesion, redes, memoria, cubiertas, senales, diferenciacion y Champion al puntaje de sucesion. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5147.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5147.log` termina con `Failures: 0`.

---

## 13. Correccion v5.14.8 - Economia Territorial Por Adaptacion

Aplicado:

| Correccion | Estado |
| --- | --- |
| Relaciones ecologicas adaptativas | `V5EcologicalRelationsSystem` calcula mutualismo, competencia y predacion desde diversidad de adaptaciones, redes, metabolismo, adhesion y depredacion real. |
| Biofilm canonico | La matriz territorial ahora reconoce `BasicAdhesin`, `PiliFimbriae`, `ColonialAdhesin` y `PersistentAdhesion`, no solo flags legacy. |
| Distritos por identidad biologica | `V5ColonyDistrictSystem` recomienda Photic Shelf, Detox Matrix, Hunting Ground, Metabolic Reactor o Defensive Bastion segun ambiente y adaptaciones instaladas. |
| Rendimiento territorial con sinergia | Cada tipo de distrito recibe multiplicadores por adaptaciones coherentes: fotosintesis, enzimas, catalasa, paredes, adhesiones, lisosomas, cilios o memoria quimica. |
| Supply adaptativo | Adhesion, comunicacion, diferenciacion y memoria aumentan capacidad colonial, haciendo que el cap se apoye en biologia y no solo en cantidad de distritos. |
| Objetivos iniciales actualizados | `V5MissionSystem` deja de pedir motor/anillos legacy y guia por primera adaptacion, Adesina basica, division, roles, colonizacion e identidad emergente. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5148.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5148.log` termina con `Failures: 0`.

---

## 14. Correccion v5.14.9 - UI De Genoma Y Consejeria Adaptativa

Aplicado:

| Correccion | Estado |
| --- | --- |
| Arbol compacto de Genoma | `V5GenomePanelIMGUI` deja la lista vertical larga y pasa a 5 columnas por tier: T1 supervivencia, T2 eucariota, T3 especializacion, T4 cuerpo y T5 apex. |
| Conectores de prerequisitos | El panel dibuja lineas entre adaptaciones dependientes para que el jugador lea caminos, no botones aislados. |
| Foco de ruta canonica | Las adaptaciones del canon actual se marcan con franja amarilla; el jugador puede alternar entre ver todo o solo la ruta/requisitos. |
| Detalle lateral | La adaptacion seleccionada muestra costo, estado, efecto, prerequisitos, counters, ruta afin y efecto Champion si existe. |
| Accion sugerida | El header muestra la adaptacion sugerida por identidad y permite instalarla directamente si esta disponible. |
| Consejero adaptativo | `V5AdvisorSystem` separa consejo adaptativo vs legacy, prioriza Adesina, Catalasa/ROS, Nucleo, territorio, presion enemiga e identidad sugerida. |
| Feedback coherente | `V5FeedbackSystem` muestra `Fenotipo actualizado` o `Rasgo corporal actualizado` en modo adaptaciones, evitando que la UI vuelva a hablar de estructuras como sistema principal. |
| Comando colonial | `V5ColonyCommandCenterIMGUI` elimina etiqueta DEBUG y marca el boton de estructuras como legacy cuando hay adaptaciones. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5149b.log`.
- Smoke test completo OK tras limpiar cache Burst JIT: `Logs/codex_unity_smoke_v5149_rerun.log` termina con `Failures: 0`.

---

## 15. Correccion v5.15.0 - Interior Como Fenotipo Y Morfologia Adaptativa

Aplicado:

| Correccion | Estado |
| --- | --- |
| HUD build actualizado | `V5HudIMGUI` sube a build `2.18`. |
| Panel Interior reorientado | `DrawInteriorPanel` deja de abrir el catalogo de estructuras por defecto y pasa a mostrar fenotipo, identidad, canon, adaptaciones, recursos y rasgos corporales generados. |
| Estructuras como modo avanzado | El catalogo manual de estructuras queda oculto tras `Modo avanzado`; se mantiene para debug, saves antiguos y pruebas legacy. |
| Flujo principal claro | El panel Interior empuja a `Abrir Genoma`, `Sugerida` y lectura de identidad, reforzando el loop `Genoma -> Adaptaciones -> Identidad -> Produccion`. |
| Lenguaje de herencia actualizado | La barra inferior habla de `rasgos corporales` cuando existe `V5AdaptationSystem`; `estructuras` queda solo para fallback legacy. |
| Morfologia por adaptacion | `V5CellMorphologyRenderer` ahora dibuja rasgos desde adaptaciones activas: paredes/capsulas, tilacoide/cloroplasto, flagelos, pili/adhesinas, cilios, hifas, plasmodio y seudopodos. |
| Fallback visual preservado | NPCs y saves antiguos siguen usando estructuras legacy como fuente visual. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5150b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5150_rerun.log` termina con `Failures: 0`.
- Nota: Unity sigue registrando un aviso externo intermitente de Burst JIT cache al cerrar batchmode; no genero fallos del smoke ni errores C#.

---

## 16. Correccion v5.15.1 - Telemetria De Adaptaciones Para Playtest

Aplicado:

| Correccion | Estado |
| --- | --- |
| Telemetria adaptativa | `V5TelemetrySystem` deja de medir solo estructuras legacy y ahora registra instalaciones, hitos, fallos, tier, ruta empujada y ultimo evento de adaptacion. |
| Fallos clasificables | Los intentos bloqueados se agrupan por cap, recursos, requisitos, duplicado u otro para detectar friccion real del panel Genoma. |
| Panel de playtest | Se agrega panel `Telemetria` al router de paneles, con resumen de genoma, top adaptaciones, ruta dominante y fallos. |
| Resumen de run actualizado | `V5RunSummarySystem` puntua y reporta `Adaptaciones` cuando existe el sistema nuevo; `Genes` queda como fallback legacy. |
| Reset consistente | Bootstrap, reinicio de escenario y limpieza de celdas reinician tambien la telemetria para evitar datos contaminados entre runs. |
| Eventos desde la fuente de verdad | `V5AdaptationSystem` emite telemetria tanto en instalaciones exitosas como en bloqueos de instalacion. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que la telemetria exista, registre instalaciones, bloquee duplicados y conserve el conteo hasta el final. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5151b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5151b.log` termina con `Failures: 0`.
- Nota: Unity sigue registrando un aviso externo intermitente de Burst JIT cache durante batchmode; el smoke completo se ejecuto y no genero fallos.

---

## 17. Correccion v5.15.2 - Reportes JSON Con Genoma Adaptativo

Aplicado:

| Correccion | Estado |
| --- | --- |
| Exportador reactivado | `V5GameBootstrap` ahora crea `V5RunSummarySystem` y `V5PlaytestReportSystem` de forma explicita para que F7/Ctrl+F7 funcionen en cualquier escena V5. |
| Reporte con esquema nuevo | `V5PlaytestReportData` sube a `playtest_report_v5_15_1` e incluye modo de genoma, identidad, adaptaciones, cap activo, fallos y ruta empujada. |
| Telemetria exportable | El JSON guarda `AdaptationSummary`, ruta dominante, fallos por categoria y top adaptaciones, ademas del resumen general. |
| Contexto de run ampliado | El reporte agrega recursos de la madre, cuerpo, squad, battlefield y loop jugable. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que el exportador exista y que `CreateReport()` produzca datos adaptativos coherentes. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5152b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5152.log` termina con `Failures: 0`.

---

## 18. Correccion v5.15.3 - Diagnostico De Run Para Playtest

Aplicado:

| Correccion | Estado |
| --- | --- |
| Sistema de diagnostico | Nuevo `V5RunDiagnosticsSystem` que convierte telemetria y estado de partida en score, alertas y consejo prioritario. |
| Panel de diagnostico | Se registra panel `Diagnostico` en `V5PanelRouter`, con reporte legible, recalculo manual y export JSON. |
| Checks accionables | Detecta cap apretado, bloqueos excesivos, identidad poco clara, Adesina sin cuerpo, baja poblacion, stress, toxinas y presion enemiga. |
| Runtime conectado | `V5GameManager`, `V5GameBootstrap` y `V5RunResetSystem` ahora conocen y reinician el diagnostico. |
| JSON enriquecido | `V5PlaytestReportData` sube a `playtest_report_v5_15_3` e incluye score, estado, consejo y reporte completo de diagnostico. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que el diagnostico exista, produzca score/reporte y exporte datos en el JSON. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5153.log` termina con `Tundra build success` tras una pasada extra de grafo.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5153.log` termina con `Failures: 0`.
- Nota: Unity sigue registrando un aviso externo intermitente de Burst JIT cache; el smoke completo se ejecuto sin fallos.

---

## 19. Correccion v5.15.4 - Consejero Vivo Desde Diagnostico

Aplicado:

| Correccion | Estado |
| --- | --- |
| Coach desde diagnostico | `V5RunDiagnosticsSystem` ahora separa prioridad tecnica de `CoachAdvice` y `CoachAction` en lenguaje jugable. |
| Consejero conectado | `V5AdvisorSystem` consulta el diagnostico cuando hay alertas reales y muestra una accion contextual en vez de quedarse en consejo generico. |
| HUD de consejo ampliado | `V5HudIMGUI` agranda el panel `Consejo` cuando la fuente es diagnostico y muestra score/estado de run. |
| JSON actualizado | `V5PlaytestReportData` sube a `playtest_report_v5_15_4` e incluye `diagnosticsCoachAdvice` y `diagnosticsCoachAction`. |
| Smoke protegido | `V5PrototypeSmokeRunner` fuerza stress alto de prueba y valida que el Consejero use diagnostico cuando la run necesita coaching. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5154b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5154_final.log` termina con `Failures: 0`.

---

## 20. Correccion v5.15.5 - Live Coach No Invasivo

Aplicado:

| Correccion | Estado |
| --- | --- |
| Coach vivo | Nuevo `V5LiveCoachSystem` que decide cuando una alerta del diagnostico merece toast contextual. |
| Cooldown anti-spam | El coach solo interrumpe si sube severidad, cambia la accion, aumentan alertas o se fuerza desde test/debug. |
| Runtime conectado | `V5GameManager`, `V5GameBootstrap` y `V5RunResetSystem` ahora crean, exponen y reinician `LiveCoach`. |
| Reporte JSON actualizado | `V5PlaytestReportData` sube a `playtest_report_v5_15_5` e incluye intervenciones, resumen y ultima accion del live coach. |
| Smoke protegido | `V5PrototypeSmokeRunner` fuerza stress alto, valida toast del live coach, conteo de intervenciones y export JSON. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5155.log` termina con `Tundra build success` tras pasada extra de grafo.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5155.log` termina con `Failures: 0`.

---

## 21. Correccion v5.15.6 - Coach Con Adaptacion Especifica

Aplicado:

| Correccion | Estado |
| --- | --- |
| Adaptacion sugerida | `V5RunDiagnosticsSystem` ahora expone `CoachAdaptation`, nombre y estado de instalacion para recomendaciones de Genoma. |
| Mapeo de sintomas | Stress alto recomienda Catalasa/ROS o homeostasis; toxinas recomiendan Catalasa/ROS; baja poblacion recomienda Division rapida; presion enemiga busca rasgos de combate. |
| Accion de Genoma concreta | `CoachAction` puede decir `Abre Genoma (G) e instala X` y aclarar si esta lista o bloqueada por requisito/recurso. |
| HUD de Consejo ampliado | `V5HudIMGUI` muestra una linea `Genoma: adaptacion | estado` cuando el consejo viene desde diagnostico. |
| Live coach conserva sugerencia | `V5LiveCoachSystem` guarda ultima adaptacion sugerida y la agrega a su resumen. |
| JSON actualizado | `V5PlaytestReportData` sube a `playtest_report_v5_15_6` e incluye sugerencia de adaptacion para diagnostico y live coach. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que stress alto recomiende Catalasa/ROS y que la sugerencia llegue al reporte. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5156.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5156.log` termina con `Failures: 0`.

---

## 22. Correccion v5.15.7 - Coach Accionable En Genoma

Aplicado:

| Correccion | Estado |
| --- | --- |
| Apertura enfocada | `V5GenomePanelIMGUI` expone `OpenFocused()` y `FocusAdaptation()` para abrir el arbol directamente en la adaptacion recomendada. |
| Coach sobre identidad | El header de Genoma prioriza la sugerencia del diagnostico cuando existe, y mantiene la sugerencia de identidad como fallback. |
| Nodo marcado | La adaptacion recomendada por el coach queda resaltada en el arbol y el detalle muestra la accion concreta del diagnostico. |
| Consejo accionable | `V5HudIMGUI` build `2.19` agrega botones `Abrir Genoma` e `Instalar` en el panel Consejo cuando hay `CoachAdaptation`. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que el Genoma abra desde el coach y seleccione Catalasa/ROS cuando el stress alto la recomienda. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5157.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5157.log` termina con `Failures: 0`.

---

## 23. Correccion v5.15.8 - Genoma Explicable

Aplicado:

| Correccion | Estado |
| --- | --- |
| Explicador de instalacion | `V5AdaptationSystem` expone requisito faltante, checklists de prerequisitos/recursos, recursos faltantes, siguiente paso y explicacion completa. |
| Detalle accionable | `V5GenomePanelIMGUI` muestra `Siguiente`, prerequisitos `[x]/[ ]`, recursos `actual/requerido` y boton `Ver requisito` cuando una adaptacion esta encadenada. |
| Bloqueos visuales | Los nodos bloqueados tienen barra inferior por causa: prerequisito, recursos, cap u otro. |
| Conectores informativos | La linea hacia la adaptacion seleccionada se marca como faltante si el prerequisito directo no esta instalado. |
| Build HUD | `V5HudIMGUI` sube a build visible `2.20`. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que Mitocondria explique Nucleo como requisito faltante y proponga el siguiente paso correcto. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5158b.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5158.log` termina con `Failures: 0`.

---

## 24. Correccion v5.15.9 - Adhesion Corporal Compacta

Aplicado:

| Correccion | Estado |
| --- | --- |
| Slot sin orbita artificial | `V5MulticellularBodySystem` elimina el radio fijo `1.35f` de los slots corporales y calcula contacto desde radios reales. |
| Pegado visual | Los slots usan un solapamiento biologico leve (`AttachedContactOverlap = 0.88f`) para que la hija se vea adherida a la madre. |
| Snap al adherir | `TryAttach` posiciona la celula inmediatamente en el slot usando `SnapAttachedToBodySlot`. |
| Seguimiento mas firme | Las celulas adheridas siguen al cuerpo con velocidad de seguimiento mayor para no separarse al mover la madre. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida que la hija adherida quede en distancia de contacto visual y no flotando separada. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5159.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5159.log` termina con `Failures: 0`.

---

## 25. Implementacion v5.16.0 - Castas Funcionales Base

Aplicado:

| Sistema | Estado |
| --- | --- |
| Catalogo de castas | Nuevo `V5CasteLibrary` con Hibrida, Recolectora, Atacante, Defensora, Productora, Sensora y Estructural. |
| Casta persistente | `V5CellEntity` guarda `FunctionalCaste`, la expone en HUD y la conserva al dividir hijas diferenciadas. |
| Efecto mecanico | Las castas modifican dano, velocidad, recoleccion, dano recibido, colonizacion y reparacion sobre el modo celular activo. |
| Visual por rol | Las celulas hijas muestran color/halo/marker segun casta para lectura estilo Cell Lab. |
| Camara germinal | Las recetas germinales ahora mapean a una casta funcional y la placa de preview muestra ese rol. |
| Cuerpo y HUD | HUD build `2.21` muestra composicion de castas; panel de cuerpo muestra casta de cada ocupante. |
| Persistencia | `V5SaveSystem` guarda/restaura `functionalCaste`; saves antiguos caen en `Hybrid`. |
| Smoke protegido | `V5PrototypeSmokeRunner` valida biblioteca, mapeo `LineageRaider -> Attacker`, produccion germinal y bonus de combate. |

Validacion:

- Unity batch compile OK: `Logs/codex_unity_compile_v5160.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5160.log` termina con `Failures: 0`.
