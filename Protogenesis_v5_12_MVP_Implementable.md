# PROTOGENESIS: PRIMORDIA

## Version Implementable Basica v5.12

Vertical slice tecnico del pivote Madre / Cuerpo / Squads.

> Este documento no reemplaza al GDD v5.12 Direccion Final. Lo aterriza en una version programable, testeable y cortada para entrar a codigo sin abrir todos los sistemas del juego terminado al mismo tiempo.

| Campo | Valor |
| --- | --- |
| Version | v5.12 MVP Implementable |
| Fecha | 2026-05-04 |
| Base | Protogenesis_GDD_v5_12_Direccion_Final.md |
| Objetivo | Definir el primer slice jugable del nuevo pivote |
| Resultado esperado | Una run de 12-18 minutos donde el jugador produce hijas, decide cuerpo vs squad, protege la madre y gana por estabilidad/dominancia |
| Regla de produccion | Si un sistema no ayuda a probar Madre / Cuerpo / Squads, queda fuera del MVP |

---

# 1. Declaracion del MVP

El MVP implementable debe responder una sola pregunta:

La fantasia de construir un cuerpo vivo alrededor de una madre y desprender squads libres es divertida, legible y programable?

Para responder eso, el MVP no necesita:

- 12 rutas balanceadas.
- todos los organismos del roster final.
- campaña.
- fisica real de tejidos.
- estructuras externas complejas.
- UI final.
- apex forms completas.

Si necesita:

- una madre germinal funcional;
- produccion por division desde Genome Lab;
- hijas con receta, modo y destino;
- cuerpo multicelular con slots logicos;
- squads libres con control RTS;
- combate donde el cuerpo proteja capas;
- colapso del cuerpo sin derrota inmediata;
- sucesion colonial si la madre muere;
- HUD que explique que esta pasando.

La primera version buena de Protogenesis no debe sentirse completa. Debe sentirse verdadera.

---

# 2. Alcance Jugable

## 2.1 Duracion objetivo

| Tipo de run | Duracion |
| --- | ---: |
| Tutorial tecnico | 6-8 min |
| Run MVP normal | 12-18 min |
| Run extendida de balance | 20-25 min |

La run MVP termina antes de que el sistema pida late game. El objetivo es validar el loop, no llenar el mapa de contenido.

## 2.2 Modo principal

Skirmish biologico contra Director Ecologico simple.

El jugador empieza con una madre LUCA, recursos basicos y mapa con nutrientes, luz, oxigeno, toxinas y acidez. El ecosistema genera presion visible mediante amenazas simples.

## 2.3 Condiciones de victoria MVP

Usar solo dos condiciones.

| Condicion | Regla |
| --- | --- |
| Victoria por Dominancia Colonial | Mantener 70%+ de estabilidad colonial durante 90s con 8+ celulas vivas y al menos 6 slots corporales ocupados |
| Victoria por Control Ecologico | Colonizar 3 nodos nutritivos y sostenerlos durante 120s |

No implementar todavia victoria por apex. La forma apex debe quedar para una fase posterior.

## 2.4 Condiciones de derrota MVP

| Condicion | Regla |
| --- | --- |
| Colapso de Linaje | Madre muere y Colonial Continuity no encuentra sucesor viable |
| Colapso Metabolico | ATP y biomasa bajo minimo durante 60s, sin unidades recolectoras funcionales |
| Colapso Ecologico | Stress de madre llega a 100 y no baja durante 30s |

La muerte de la madre no es derrota automatica. La sucesion ya existe en codigo y debe seguir siendo parte del MVP.

---

# 3. Rutas MVP

El MVP usa las rutas que ya estan marcadas como MVP o mas cerca del balance actual:

| Ruta | Estado | Fantasia MVP | Por que entra |
| --- | --- | --- | --- |
| Bacteria | MVP | swarm, biofilm, bajo peso biologico | prueba microcolonias y caps biologicos |
| Archaea | MVP | supervivencia extrema, control de pH | prueba ecosistema hostil |
| Cyanobacteria | MVP | fotosintesis, oxigenacion, economia territorial | prueba luz/oxigeno y cuerpo productor |
| Amoeba | MVP | cuerpo grande, fagocitosis, tanque organico | prueba unidades pesadas y dano por contacto |

Rutas que quedan visibles en UI como bloqueadas/experimental, pero no balanceadas:

| Ruta | Estado MVP |
| --- | --- |
| Flagellate | Bloqueada hasta Sprint 4 o playtest extendido |
| Ciliate | Bloqueada |
| Microalga | Bloqueada, salvo como auxiliar luminica |
| Fungus | Bloqueada |
| Slime Mold | Bloqueada |
| Rotifer | Threat/experimental |
| Nematode | Threat/experimental |
| Tardigrade | Neutral/gen, no jugable |

La idea no es negar el roster final. Es evitar que el nuevo cuerpo multicelular se rompa mientras todavia no sabemos si el loop base respira.

---

# 4. Loop MVP

## 4.1 Loop corto: 5 segundos

1. Seleccionar madre, cuerpo o squad.
2. Mover, atacar o cambiar modo.
3. Leer dano en capas corporales.
4. Reposicionar o retirar.

## 4.2 Loop medio: 30 segundos

1. Abrir Genome Lab.
2. Elegir receta.
3. Elegir destino: cuerpo, squad libre o automatico.
4. Si va al cuerpo, ocupar slot valido.
5. Si va a squad, sale con modo y directiva inicial.

## 4.3 Loop largo: 3 minutos

1. Consolidar afinidad hacia una ruta.
2. Ajustar composicion del cuerpo.
3. Desprender squads para recursos o amenazas.
4. Reagrupar cuerpo si hay dano.
5. Sostener nodos o dominancia.

---

# 5. Modelo de Madre MVP

La madre deja de ser una guerrera normal.

## 5.1 Estados de la madre

| Estado | Cuando ocurre | Control | Riesgo |
| --- | --- | --- | --- |
| Nucleo interno | 3+ celulas adheridas | se mueve como centro del cuerpo | dano filtrado, baja movilidad |
| Nucleo expuesto | 0-2 celulas adheridas | se mueve directamente | recibe dano alto, stress alto |
| Madre en crisis | HP critica o cuerpo colapsado | posible champion si cumple requisitos | temporizador severo |
| Sucesion activa | madre muere con sucesor viable | nueva madre temporal | debe estabilizar colonia |

## 5.2 Reglas basicas

- Mientras tenga cuerpo parcial o completo, la madre no debe ser el mejor atacante.
- El dano de combate a la madre se reduce cuando hay capas, pero nunca desaparece totalmente.
- La madre es banco principal de recursos y fuente de division.
- Las hijas pueden operar como cuerpo o como squads libres.
- Las hijas no necesitan tener todas la misma ruta si son auxiliares; el linaje primario si debe seguir siendo claro.

## 5.3 Movimiento

El jugador mueve el cuerpo seleccionando la madre/cuerpo. Las celulas adheridas no hacen pathfinding independiente. Orbitan sus slots alrededor del nucleo.

Formula inicial:

```text
velocidad_cuerpo =
  velocidad_madre
  * modificador_estado_cuerpo
  * bono_motoras
  * penalizacion_tamano
  * penalizacion_stress
```

Valores iniciales:

| Estado | Modificador |
| --- | ---: |
| Nucleo expuesto | 1.15 |
| Cuerpo parcial | 0.95 |
| Cuerpo completo | 0.78 |
| Cuerpo sobrecargado | 0.62 |

---

# 6. Cuerpo Multicelular MVP

## 6.1 Implementacion por etapas

El GDD final define 19 slots. El MVP los implementa en dos pasos:

| Paso | Slots activos | Uso |
| --- | ---: | --- |
| Sprint 1 | 1 nucleo + 6 ring interno | validar adhesion, orbitas, buffs y dano |
| Sprint 2 | 1 nucleo + 6 interno + 12 externo | validar cuerpo final legible |

Aunque Sprint 1 use solo 6 slots, el modelo de datos debe estar preparado para 19. Asi no reescribimos la arquitectura dos veces.

## 6.2 Slot definition

Cada slot debe tener:

| Campo | Tipo | Uso |
| --- | --- | --- |
| slotIndex | int | identificador estable |
| ring | enum | Nucleus, Inner, Outer |
| angleDegrees | float | posicion relativa |
| radius | float | distancia al nucleo |
| allowedRoles | flags | que recetas encajan mejor |
| occupiedCell | V5CellEntity | celula adherida actual |
| integrity01 | float | salud del vinculo |

## 6.3 Roles de slot MVP

Usar solo cinco roles al inicio.

| Rol | Funcion | Recetas compatibles |
| --- | --- | --- |
| Coraza | absorbe contacto | Lineage Defender, Amoeboid Guard |
| Motora | mejora velocidad | Lineage Scout, Lineage Raider |
| Productora | genera energia o recursos | Lineage Gatherer, Microalga Support |
| Boca | dano/contacto/control | Lineage Raider, Amoeboid Guard |
| Conectiva | reduce stress y mejora cohesion | Plastic Daughter, Bacterial Symbiont |

Sensorial y Reserva quedan para Sprint 3.

## 6.4 Estados del cuerpo

| Estado | Slots ocupados | Efecto MVP |
| --- | ---: | --- |
| Expuesto | 0-2 | madre rapida, stress +, dano entrante alto |
| Parcial | 3-5 | buffs leves, dano por capas parcial |
| Completo | 6-12 | estabilidad alta, dano filtra por capas |
| Sobrecargado | 13+ | solo Sprint 2+, potente pero lento |

## 6.5 Adhesion

Reglas:

- Una celula se puede adherir si esta a menos de 2.5u de la madre y hay slot valido.
- Al adherirse, pierde directiva independiente.
- Mientras esta adherida, su transform orbita el slot relativo.
- La celula adherida sigue existiendo como entidad y puede morir.
- Si muere, libera el slot y puede disparar stress en la madre.
- Si se desprende voluntariamente, recibe 2s de invulnerabilidad parcial para evitar muerte instantanea por solapamiento.

## 6.6 Buffs del cuerpo

Cada celula adherida aporta un pequeno efecto.

| Rol | Efecto inicial |
| --- | --- |
| Coraza | +8% reduccion de dano fisico por pieza, max 30% |
| Motora | +7% velocidad por pieza, max 21% |
| Productora | +0.18 ATP/s o +0.10 biomasa/s segun ruta |
| Boca | +0.15 dano contacto del cuerpo |
| Conectiva | -0.12 stress/s y +5% velocidad de reparacion |

El MVP no debe tener builds perfectas. Si una composicion obvia gana siempre, los valores deben bajar.

---

# 7. Produccion y Genome Lab MVP

## 7.1 Decision de produccion

Cada division debe elegir:

| Pregunta | Opciones MVP |
| --- | --- |
| Que nace? | receta/casta |
| Para que nace? | modo celular default |
| Donde vive? | cuerpo, squad libre, automatico |
| Que refuerza? | afinidad y madurez |

## 7.2 Destinos

| Destino | Comportamiento |
| --- | --- |
| Cuerpo | intenta adherirse al mejor slot valido |
| Squad libre | nace cerca de madre con directiva segun receta |
| Automatico | si hay slot critico vacio va al cuerpo; si no, squad |

## 7.3 Recetas MVP

Usar las recetas ya existentes, pero reinterpretarlas con destino corporal.

| Receta | Codigo | Destino recomendado | Rol corporal |
| --- | --- | --- | --- |
| Plastic Daughter | LIN-PLS | Auto | Conectiva |
| Lineage Gatherer | LIN-GAT | Squad o Cuerpo | Productora |
| Lineage Scout | LIN-SCT | Squad | Motora/Sensorial |
| Lineage Defender | LIN-DEF | Cuerpo | Coraza |
| Lineage Raider | LIN-HNT | Squad o Boca | Boca |
| Amoeboid Guard | AUX-AMG | Cuerpo | Coraza/Boca |
| Bacterial Symbiont | AUX-BIO | Cuerpo | Conectiva/Productora |
| Microalga Support | AUX-LUX | Cuerpo | Productora |

Ciliate Controller puede quedar oculto en MVP si el control por corrientes no esta listo.

## 7.4 Maturity

La madurez de receta ya existe. En el MVP debe afectar:

| Estado | Efecto |
| --- | --- |
| Madura | aplica stats completos, costo de stress reducido |
| Inestable | aplica 65-75% del efecto, stress mayor |

La UI debe mostrar `*` para madura y `~` para inestable, como ya hace el sistema actual.

## 7.5 Produccion fallida

Si el jugador intenta producir algo que no puede:

- mostrar razon corta;
- sugerir una receta viable;
- no castigar con costo;
- no cerrar el panel.

---

# 8. Squads Libres MVP

Los squads libres son el lado RTS tradicional del juego.

## 8.1 Modos disponibles

Usar los modos ya implementados:

| Tecla | Modo | Uso |
| --- | --- | --- |
| 1 | Follow Lineage | volver a madre |
| 2 | Gather | recolectar |
| 3 | Defend | proteger zona/madre |
| 4 | Scout | explorar |
| 5 | Colonize | tomar nodo |
| 6 | Hunt | atacar/cazar |

## 8.2 Reglas

- Las unidades libres mantienen pathfinding simple actual.
- Las unidades adheridas no entran en squads seleccionables por drag, salvo si el jugador activa modo de edicion corporal.
- `Shift+S` para reagrupar squad se mantiene.
- El blob penalty se mantiene para celulas libres, pero no debe castigar slots adheridos del cuerpo.

## 8.3 Retorno al cuerpo

Una celula libre puede volver a adherirse si:

- esta cerca de madre;
- hay slot compatible;
- no esta en cooldown de desprendimiento;
- no esta en combate activo fuerte.

---

# 9. Combate MVP

## 9.1 Principio

El combate debe demostrar que el cuerpo importa.

Sin cuerpo, la madre cae rapido. Con cuerpo, la colonia aguanta, pero pierde piezas visibles. El jugador debe leer el combate como dano anatomico, no solo barras de HP.

## 9.2 Dano por capas

Regla inicial:

1. Si el atacante golpea a una celula libre, usa combate actual.
2. Si golpea el cuerpo, el dano busca primero slot externo ocupado.
3. Si no hay externo, golpea ring interno.
4. Una fraccion pequena filtra a la madre.
5. Toxinas y dano ambiental pueden afectar varias piezas a la vez.

Valores iniciales:

| Caso | Fuga a madre |
| --- | ---: |
| Cuerpo completo | 6% |
| Cuerpo parcial | 12% |
| Nucleo expuesto | 100% |
| Dano piercing | +10 puntos de fuga |
| Dano quimico | afecta slot y madre con 50% de fuga normal |

## 9.3 Destruccion de slots

Cuando una celula adherida llega a 0 HP:

- muere como entidad normal;
- libera slot;
- genera stress +3 en madre;
- si era conectiva, genera stress +5;
- si el cuerpo baja de completo a parcial, HUD avisa "Cuerpo fracturado".

## 9.4 Champion MVP

El champion no debe entrar en Sprint 1. En Sprint 2 o 3:

Activacion solo si:

- colapso fue forzado por dano enemigo o ambiental;
- la colonia tuvo 6+ adheridas durante al menos 45s antes del colapso;
- madre tiene HP > 20%;
- stress < 95;
- no hubo Husk Drop voluntario en los ultimos 30s.

Efecto:

- 20-30s de movilidad y supervivencia;
- dano decente, no dominante;
- objetivo es escapar y reconstruir, no ganar la pelea solo.

---

# 10. Ecosistema MVP

El ecosistema MVP debe presionar, no saturar.

## 10.1 Estados visibles

Implementar o reutilizar solo tres estados al inicio:

| Estado | Trigger | Efecto |
| --- | --- | --- |
| Corredor nutritivo | nutrientes altos y bajo peligro | atrae recoleccion y colonizacion |
| Cicatriz toxica | toxinas altas por combate/metabolismo | castiga quedarse quieto |
| Frente oxigenado | oxigeno alto + luz | favorece Cyanobacteria, amenaza anaerobios |

Los seis estados finales quedan documentados, pero el MVP prueba tres.

## 10.2 Threats MVP

Usar amenazas simples:

| Threat | Trigger | Funcion |
| --- | --- | --- |
| Bacteriophage patch | demasiada densidad bacteriana | anti-snowball de swarm |
| Predator neutral small | exceso de recursos sin defensa | obliga a crear defensores |
| Toxic bloom local | luz + nutrientes + tiempo | obliga a retirarse |

No implementar Daphnia, Hydra ni Dinoflagellate bloom completo todavia.

---

# 11. UI MVP

## 11.1 HUD principal

Debe mostrar:

- ATP;
- Biomasa;
- Stress;
- poblacion actual / carga biologica;
- estado del cuerpo;
- slots ocupados;
- fase de run;
- mensaje corto de afinidad reciente.

Los recursos secundarios siguen existiendo internamente, pero no deben dominar el HUD principal.

## 11.2 Panel de cuerpo

Panel IMGUI inicial aceptable.

Debe mostrar:

```text
CUERPO
Estado: Completo
Slots: 8/19
Ring interno: [D][P][C][-][P][D]
Ring externo: [B][-][-][M]...
Buffs: -18% dano fisico | +14% velocidad | +0.36 ATP/s
Riesgo: fuga 6%
```

## 11.3 Genome Lab

Mantener inspiracion Dune:

- lista de recetas a la izquierda;
- preview/placa visual a la derecha;
- costo y afinidad visible;
- boton o selector de destino;
- boton producir.

La placa puede seguir siendo procedural en MVP. No necesita arte final.

## 11.4 Feedback obligatorio

Eventos que deben dar feedback:

| Evento | Feedback |
| --- | --- |
| celula adherida | linea/pulso hacia slot |
| celula desprendida | pulso hacia afuera |
| slot destruido | flash rojo + texto corto |
| cuerpo cambia estado | toast + HUD |
| fuga a madre | pequeno marcador sobre nucleo |
| sucesion activa | overlay de crisis |

---

# 12. Datos y Codigo Nuevos

## 12.1 Nuevos enums sugeridos

Agregar en `V5Types.cs`:

```csharp
public enum V5BodyRing { Nucleus, Inner, Outer }
public enum V5BodySlotRole { None, Armor, Motor, Producer, Mouth, Connector, Sensor, Reserve }
public enum V5BodyState { Exposed, Partial, Complete, Overloaded }
public enum V5CellDeploymentMode { Auto, FreeSquad, AttachedBody }
public enum V5AttachmentState { Free, Attached, Detaching, Cooldown }
```

## 12.2 Nuevos scripts

| Script | Responsabilidad |
| --- | --- |
| V5MulticellularBodySystem.cs | estado del cuerpo, slots, buffs, attach/detach, damage routing |
| V5BodySlotDefinition.cs | datos de slot, ring, angulo, rol preferido |
| V5BodyPanelIMGUI.cs | panel radial/ textual de cuerpo |
| V5BodyDebugOverlay.cs | gizmos de slots, vinculos y dano |

## 12.3 Scripts existentes a modificar

| Script | Cambio |
| --- | --- |
| V5Types.cs | enums nuevos |
| V5GameManager.cs | referencia a BodySystem |
| V5GameBootstrap.cs | crear BodySystem y BodyPanel |
| V5CellEntity.cs | attachment state, slot index, bloqueos de AI cuando adherida |
| V5CellFactory.cs | spawn con deployment mode |
| V5GerminalProductionSystem.cs | selector destino y auto-attach |
| V5PhenotypeRecipeLibrary.cs | rol corporal recomendado por receta |
| V5CombatSystem.cs | redirigir dano al cuerpo cuando corresponde |
| V5SelectionSystem.cs | comandos attach/detach y evitar seleccionar organos por error |
| V5SquadTacticsSystem.cs | blob penalty ignora adheridas |
| V5HudIMGUI.cs | estado cuerpo, slots y fuga |
| V5ColonialContinuitySystem.cs | continuidad suma cuerpo estable y sucesor protegido |
| V5SaveSystem.cs | persistir slots si el guardado queda activo |

## 12.4 No tocar todavia

- AI enemiga avanzada.
- campaña.
- apex forms.
- 12 rutas finales.
- fisica de articulaciones.
- UI definitiva no-IMGUI.
- save completo si bloquea el avance.

---

# 13. Orden de Implementacion

## Sprint 1: Cuerpo logico minimo

Objetivo: una madre con 6 slots internos y hijas adheribles.

Tareas:

1. Agregar enums de cuerpo.
2. Crear `V5MulticellularBodySystem`.
3. Definir 6 slots internos.
4. Permitir attach/detach manual.
5. Hacer que celulas adheridas orbiten slots.
6. Bloquear AI/pathfinding de adheridas.
7. Mostrar panel simple de cuerpo.

Aceptacion:

- puedo producir 6 hijas;
- puedo adherirlas;
- se mueven con la madre;
- no se separan por steering;
- si una muere, slot queda libre;
- HUD muestra slots.

## Sprint 2: Produccion con destino

Objetivo: Genome Lab decide si una hija nace como cuerpo o squad.

Tareas:

1. Agregar `V5CellDeploymentMode`.
2. Agregar selector de destino al Genome Lab.
3. Mapear recetas a roles corporales.
4. Auto-asignar mejor slot.
5. Aplicar buffs del cuerpo.
6. Ajustar caps para contar adheridas correctamente.

Aceptacion:

- LIN-DEF puede nacer adherida como coraza;
- LIN-GAT puede nacer libre o productora;
- Auto elige cuerpo si hay hueco critico;
- UI explica destino.

## Sprint 3: Combate por capas

Objetivo: el cuerpo recibe dano antes que la madre.

Tareas:

1. Redirigir dano de ataques contra madre/cuerpo.
2. Implementar fuga a nucleo.
3. Implementar destruccion de slots.
4. Ignorar blob penalty para adheridas.
5. Ajustar feedback visual.
6. Medir duracion de combate 6v6.

Aceptacion:

- cuerpo completo protege;
- cuerpo parcial se siente vulnerable;
- focus piercing amenaza madre;
- perder slots cambia estado;
- combate decisivo dura 30-60s.

## Sprint 4: Crisis y reconstruccion

Objetivo: colapsar, sobrevivir y reconstruir.

Tareas:

1. Integrar body state con Colonial Continuity.
2. Implementar Husk Drop basico.
3. Implementar champion de emergencia si cumple requisitos.
4. Agregar cooldown anti-cheese.
5. Ajustar stress por colapso.

Aceptacion:

- perder cuerpo no termina partida;
- usar Husk Drop voluntario no regala champion;
- sucesor viable puede mantener run;
- reconstruir cuerpo es una decision clara.

## Sprint 5: Ecosistema MVP y victoria

Objetivo: cerrar una run.

Tareas:

1. Estados visibles: corredor nutritivo, cicatriz toxica, frente oxigenado.
2. Threats simples.
3. Condiciones de victoria MVP.
4. Resumen de run.
5. Telemetria minima.

Aceptacion:

- la run termina por victoria o derrota;
- el ecosistema presiona con causas visibles;
- el resumen muestra cuerpo, squads y afinidad.

---

# 14. Balance Inicial

## 14.1 Caps

Mantener los caps actuales como base:

| Medida | Valor |
| --- | ---: |
| Entidades controlables hard cap | 24 |
| Carga biologica hard cap | 30 |
| Auxiliares maximo | 30% de carga |
| Slots corporales finales | 19 |
| Slots Sprint 1 | 6 |

Las celulas adheridas cuentan para carga biologica. Tambien cuentan para hard cap, salvo que mas adelante se cree un sistema de microtejido abstracto.

## 14.2 Costos iniciales

| Accion | Costo |
| --- | ---: |
| Producir hija basica | costo actual de receta |
| Adherir a slot | 0 ATP, +1 stress |
| Desprender voluntario | +2 stress |
| Husk Drop | +12 stress, cooldown 30s, bloquea champion 30s |
| Reparacion de slot | pasiva por conectivas/productoras |

## 14.3 Buff limits

| Buff | Maximo MVP |
| --- | ---: |
| reduccion dano fisico | 30% |
| velocidad por motoras | 21% |
| ATP por productoras | +0.54/s |
| reduccion stress | -0.36/s |
| fuga minima a madre | 6% |

---

# 15. Metricas de Exito

El MVP esta funcionando si:

| Metrica | Objetivo |
| --- | ---: |
| El jugador produce al menos 8 hijas en una run normal | 80% de runs |
| El jugador usa cuerpo y squad libre en la misma run | 70% |
| El jugador entiende por que una hija fue al cuerpo | 80% en feedback |
| Al menos un combate destruye slots sin matar inmediatamente a la madre | 60% |
| Muerte de madre con sucesor viable no termina la run | 100% |
| Combate decisivo dura 30-60s | 70% |
| La run termina en 12-18 min | 70% |
| El cuerpo completo no es siempre mejor que cuerpo parcial + squads | meta no dominante |

Estas metricas importan mas que agregar contenido.

---

# 16. Riesgos del MVP

| Riesgo | Sintoma | Respuesta |
| --- | --- | --- |
| El cuerpo se siente como mochila pasiva | jugador lo llena y se olvida | agregar dano por capas y roles mas contrastados |
| El cuerpo complica demasiado | jugador no entiende slots | reducir a 6 slots mas tiempo |
| Squads pierden importancia | todo se queda adherido | nodos lejos y threats que exigen salida |
| Madre sigue siendo guerrera | jugador pelea solo con ella | bajar dano madre y subir stress expuesta |
| Produccion abruma | demasiadas recetas | ocultar auxiliares hasta afinidad |
| UI IMGUI se vuelve ilegible | paneles tapan accion | modo compacto y hotkeys simples |

---

# 17. Decision de Producto

La version implementable basica no intenta mostrar todo Protogenesis. Intenta mostrar la razon por la que Protogenesis merece existir:

un RTS donde producir unidades tambien es construir anatomia.

Si el MVP logra que el jugador piense:

> "Esta hija la quiero como coraza, esta otra la quiero libre, y si pierdo este lado del cuerpo tengo que retirarme"

entonces el pivote funciona.

Si el jugador solo piensa:

> "Estoy sacando unidades con nombres raros"

entonces falta legibilidad biologica.

---

# 18. Proximo Paso

Despues de este documento, el siguiente paso es entrar a codigo con el Sprint 1:

Cuerpo logico minimo: 6 slots internos, attach/detach, orbitas, HUD simple.

Ese sprint debe terminar antes de tocar champion, ecosistema nuevo o rutas extra.
