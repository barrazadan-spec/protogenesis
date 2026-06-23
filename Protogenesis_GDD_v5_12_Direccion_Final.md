# PROTOGENESIS: PRIMORDIA

## Game Design Document v5.12 - Direccion Final

**Biological RTS de cuerpo multicelular, squads libres, Genome Lab y madre germinal latente.**

> No comandas unidades sueltas: programas un linaje vivo, construyes un cuerpo multicelular alrededor de una madre germinal, desprendes squads cuando conviene y sobrevives a la caida del cuerpo con una celula campeona que solo despierta si invertiste en ella.

| Campo | Valor |
| --- | --- |
| Version | v5.12 - Direccion final del juego terminado |
| Fecha | 2026-05-04 |
| Base | v5.11 Cuerpo Multicelular + sistemas ya implementados v5.10/v5.11 |
| Objetivo | Cerrar el GDD ideal de producto antes de redactar la version implementable basica |
| Decision canonica | El juego terminado se centra en tres manifestaciones del linaje: madre/interior, cuerpo adherido y squads libres |
| Regla de corte | Todo sistema que no fortalezca esa triada pasa a expansion, escenario o backlog |

---

# 1. Veredicto de Direccion

v5.12 acepta el pivote de v5.11, pero lo disciplina.

El cuerpo multicelular se queda como mecanica central porque resuelve los tres problemas historicos del proyecto:

- La madre ya no tiene que ser base, guerrera, fabrica y condicion de derrota al mismo tiempo sin mediacion.
- Las hijas ya no son solo tropas clonadas; pueden ser tejido del cuerpo o squads libres.
- La especializacion ya no encierra la partida en una sola ruta pura; el linaje expresa identidad mediante cuerpo, recetas y auxiliares limitadas.

La version final del juego no intenta simular toda la biologia. Intenta entregar una fantasia jugable muy especifica:

**construir un organismo, leer su anatomia, romperlo tacticamente y reconstruirlo bajo presion.**

Ese es el eje. Todo lo demas existe para servirlo.

---

# 2. Fantasia Final del Jugador

La fantasia final es:

> "Soy una colonia viva que evoluciona desde una protocelula hasta un organismo compuesto. Decido que celulas se vuelven cuerpo, cuales salen como squads, que recetas heredan mis hijas y cuando libero mi cuerpo para convertirlo en ataque."

La partida debe producir cuatro emociones repetibles:

| Momento | Fantasia |
| --- | --- |
| Construccion | "Mi organismo empieza a tener forma propia." |
| Lectura tactica | "Entiendo que parte del cuerpo hace que cosa." |
| Ruptura | "Perder capas no es derrota inmediata, es transicion." |
| Reagrupacion | "Sobrevivi, volvi a adherir, y el cuerpo nuevo cuenta otra historia." |

El juego terminado debe sentirse como RTS tactico, pero con una identidad que StarCraft o Company of Heroes no pueden copiar: **la base, el ejercito y el avatar son tejidos de un mismo linaje vivo**.

---

# 3. Pilares Finales

## 3.1 Pilares que se quedan

1. **Division = produccion.** No hay barracas. Toda unidad nace de una celula.

2. **Genome Lab = eleccion con preview.** La produccion se decide desde recetas visuales estilo Dune: ficha detallada antes, sprite simplificado despues.

3. **Cuerpo multicelular = identidad visible.** El jugador construye un cuerpo legible con slots, roles y vinculos.

4. **Squads libres = RTS real.** El cuerpo no reemplaza el micro tactico; lo complementa.

5. **Afinidad = memoria causal.** El juego muestra por que se desbloquean rutas, recetas y configuraciones.

6. **Ecosistema = tercer jugador.** El mapa reacciona, no es fondo.

7. **Campeon latente = ultimo recurso, no plan principal.** La madre expuesta es dramatica, temporal y peligrosa.

8. **Biologia honesta, abstraccion jugable.** Se prioriza lo biologicamente reconocible sobre lo enciclopedico.

## 3.2 Pilares que se cortan o limitan

| Elemento | Decision final |
| --- | --- |
| Catalogo de 66 estructuras como contenido base | Se reduce a catalogo jugable de 28-32 piezas base; el resto queda en expansion/apendice |
| Grilla de 60 posiciones visibles | Se reemplaza por cuerpo activo de 19 slots legibles; cuerpos masivos se abstraen |
| Joints fisicos como base del sistema | Se usan vinculos logicos y visuales; fisica real solo si el prototipo lo justifica |
| Reglas automaticas por unidad ilimitadas | Se reemplazan por doctrinas de receta y modos simples |
| Cinco condiciones de victoria estandar | Skirmish usa 3; el resto queda para escenarios |
| Tardigrado jugable principal | Se mantiene como neutral raro, gen de criptobiosis o evento apex defensivo |
| Campeon como comeback gratis | El campeon requiere inversion previa y tiene temporizador severo |

---

# 4. Estructura Jugable Final

El jugador opera tres capas simultaneas.

## 4.1 Capa 1: Madre Germinal e Interior

La madre es el nucleo del linaje.

Funciones:

- Guarda identidad genetica.
- Instala estructuras internas.
- Produce hijas mediante division.
- Define recetas en el Genome Lab.
- Activa poder campeon solo cuando queda expuesta.

La madre no debe ser una unidad frontline normal mientras tiene cuerpo. En cuerpo completo, su funcion es coordinar, metabolizar y sobrevivir dentro de capas.

## 4.2 Capa 2: Cuerpo Multicelular

El cuerpo es una estructura viva adherida a la madre.

Funciones:

- Protege al nucleo.
- Aporta buffs metabolicos.
- Define silueta y estilo de la colonia.
- Recibe dano por capas.
- Puede desprenderse total o parcialmente.

El cuerpo es el "edificio" del juego, pero se mueve, sangra, se rompe y se recompone.

## 4.3 Capa 3: Squads Libres

Los squads libres son celulas no adheridas.

Funciones:

- Recolectan.
- Exploran.
- Hostigan.
- Colonizan.
- Defienden nodos externos.
- Vuelven a adherirse si sobreviven.

Los squads libres conservan el RTS tactico. El cuerpo da identidad; los squads dan alcance.

---

# 5. Core Loop Final

## 5.1 Loop de 5 segundos

- Seleccionar celula, cuerpo o squad.
- Cambiar modo celular.
- Mover o atacar.
- Reaccionar a dano o threat.
- Reposicionar cuerpo si esta en peligro.

## 5.2 Loop de 30 segundos

- Dividir.
- Elegir receta.
- Decidir destino: adherida, squad libre o auto.
- Colocar adherida en slot valido.
- Ajustar modo inicial.
- Responder al estado del cuerpo.

## 5.3 Loop de 3 minutos

- Instalar estructura de madre.
- Ajustar doctrina de recetas.
- Consolidar ruta por afinidad.
- Cambiar composicion del cuerpo.
- Preparar counter ecologico.
- Decidir timing de ataque, Husk Drop o reagrupacion.

## 5.4 Pregunta central de cada division

Cada division responde:

1. **Que nace?** Receta/fenotipo.
2. **Para que nace?** Modo y rol.
3. **Donde vive?** Cuerpo o squad.
4. **Que historia refuerza?** Afinidad y madurez de receta.

Si una division no responde esas cuatro cosas, el sistema esta perdiendo identidad.

---

# 6. Cuerpo Multicelular Final

## 6.1 Slots activos

La version terminada usa un cuerpo activo de 19 slots:

- Nucleo madre: 1 centro.
- Ring interno: 6 slots.
- Ring externo: 12 slots.

El GDD v5.11 proponia 4 anillos y 60 posiciones potenciales. v5.12 lo corta por legibilidad. El jugador no debe administrar 60 piezas visibles en tiempo real. Si una build representa 20+ organismos, se expresa con microcolonias visuales o extensiones abstractas, no con 60 unidades controlables.

## 6.2 Estados del cuerpo

| Estado | Adheridas | Funcion | Riesgo |
| --- | ---: | --- | --- |
| Nucleo Expuesto | 0-2 | Madre campeona temporal, alta movilidad | Stress acelerado, vulnerable a focus |
| Cuerpo Parcial | 3-5 | Transicion, buffs leves | Puede colapsar rapido |
| Cuerpo Completo | 6-12 | Estado estable ideal | Costo metabolico y counters penetrantes |
| Cuerpo Sobrecargado | 13+ | Late game potente | Mantenimiento alto, lento, vulnerable a quimico |

La meta no debe ser "siempre 19 slots llenos". La meta debe ser **tener el cuerpo correcto para la situacion**.

## 6.3 Roles de slot

| Rol | Slots preferidos | Funcion |
| --- | --- | --- |
| Coraza | Ring externo | Absorbe contacto fisico |
| Motora | Ring externo/lateral | Aumenta velocidad del cuerpo |
| Productora | Ring interno o externo luminoso | ATP, O2, biomasa |
| Boca | Ring externo/frente | Contacto, fagocitosis, control |
| Conectiva | Cualquier ring | Transferencia, adhesion, resiliencia |
| Sensorial | Ring externo | Vision, deteccion de threats |
| Reserva | Ring interno | Reagrupacion, sucesion, reparacion |

## 6.4 Vinculos

Los vinculos se implementan como conexiones logicas con visualizacion clara.

No son cuerpos rigidos de fisica por defecto. La estabilidad del cuerpo se calcula por:

- cantidad de adheridas;
- calidad de receta;
- tipo de adhesion;
- stress;
- dano reciente;
- distancia a madre;
- afinidad de ruta.

Los vinculos comparten recursos parcialmente:

| Recurso | Regla final |
| --- | --- |
| ATP | Redistribucion leve cada pocos segundos |
| Biomasa | Compartida lento, no instantanea |
| HP | Regeneracion de red, no HP comun |
| Stress | Se propaga y se reduce por cohesion |

## 6.5 Movimiento del cuerpo

El cuerpo se mueve como una entidad tactica alrededor de la madre.

Las adheridas no hacen pathfinding independiente mientras estan adheridas. Orbitan slots relativos. Esto protege performance y legibilidad.

Velocidad base:

```
velocidad_cuerpo = velocidad_madre * modificador_estado * aporte_motoras * penalizacion_tamano
```

Reglas:

- cuerpo completo sin motoras es lento;
- motoras aumentan movilidad, pero ocupan slots que podrian ser defensa o produccion;
- cuerpo sobrecargado paga penalizacion fuerte;
- Nucleo Expuesto es rapido, pero se quema por stress.

---

# 7. Division y Genome Lab Final

## 7.1 La division abre una decision corta

Cuando el jugador divide desde la madre:

```
[DIVIDIR]
Receta: Hija plastica / Recolectora / Defensora / Raider / Auxiliar
Destino: Adherir / Squad libre / Auto
Modo inicial: Default / Recolectar / Defender / Cazar / Colonizar / Reparar
```

El jugador puede aceptar un default en un click. La profundidad vive en el Genome Lab, no en cada pop-up.

## 7.2 Defaults canonicos

| Situacion | Default |
| --- | --- |
| Early game sin cuerpo | Hija plastica adherida |
| Cuerpo con menos de 3 adheridas | Defensora/recolectora adherida |
| Cuerpo completo | Nueva hija como squad libre |
| Madre bajo ataque | Defensora adherida o reparar |
| Stress alto | Squad libre o reparadora, no adherir |

## 7.3 Recetas

Las recetas son planes de fenotipo.

Cada receta define:

- codigo;
- cuerpo;
- organelos/estructuras;
- destino preferido;
- modo inicial;
- costo;
- stress;
- madurez por afinidad;
- efecto jugable.

Las recetas pueden estar:

| Estado | Efecto |
| --- | --- |
| Inestable | Se puede usar, cuesta mas stress, bonus reducido |
| Madura | Costo estable, bonus normal/mejorado |
| Bloqueada | Falta estructura, gen o entorno esencial |

Regla final: **la mayoria de recetas no se bloquea por historia; se penaliza si la historia no la sostiene.** Esto permite improvisar sin que el juego parezca arbitrario.

## 7.4 Doctrinas de produccion

El Genome Lab permite guardar doctrinas simples:

- Cuerpo defensivo: prioriza adheridas de coraza/productoras.
- Swarm libre: prioriza squads libres baratos.
- Predador compuesto: prioriza bocas externas y raiders.
- Red territorial: prioriza conectivas, hifas y colonizadoras.
- Auto balanceado: repara deficits del cuerpo.

No se permiten reglas infinitas por celula. Las doctrinas son pocas, legibles y editables.

---

# 8. Modos Celulares Finales

Los modos celulares son player-facing. Las directivas son plumbing interno.

| Modo | Funcion base | En cuerpo | En squad |
| --- | --- | --- | --- |
| Seguir linaje | Cohesion | Mantiene slot/forma | Escolta madre |
| Recolectar | Economia | Aporta absorcion/ATP | Busca recursos |
| Defender | Supervivencia | Refuerza capa/slot | Intercepta threats |
| Explorar | Vision | Sensor externo | Scout rapido |
| Colonizar | Territorio | Huella ambiental | Ancla zona |
| Cazar | Daño | Boca/arma activa | Persigue objetivos |
| Reparar | Recuperacion | Baja stress de red | Vuelve a madre |
| Especial de ruta | Identidad avanzada | Depende ruta | Depende ruta |

Cada modo tiene costo de oportunidad. Ningun modo debe ser estrictamente superior.

---

# 9. Combate Final

## 9.1 Principio

El combate debe ser legible en 30-60 segundos. El jugador debe entender:

- que capa esta recibiendo dano;
- si el cuerpo esta a punto de romperse;
- si el daño rival es contacto, quimico o penetrante;
- si conviene aguantar, desprender o huir.

## 9.2 Tipos de daño

| Tipo | Respeta capas | Uso |
| --- | --- | --- |
| Contacto fisico | Si | Fagocitosis, golpes, mordidas, pseudopodos |
| Quimico ambiental | Parcial | Toxinas, acido, ROS, zonas contaminadas |
| Penetrante | No | Estilete, fago, movimiento helicoidal, citolisina |
| Stress/red | No | Panico quimico, ruptura de vinculos, sobrecarga |

## 9.3 Dano por capas

Contacto fisico:

1. Busca la celula adherida mas cercana al atacante.
2. Aplica daño a esa celula.
3. Si muere, el excedente pasa a la siguiente capa.
4. Solo llega a la madre si no hay capa valida.

Quimico:

- afecta varias celulas del cuerpo;
- no borra todo instantaneamente;
- se mitiga con catalasa, pared, detox y entorno.

Penetrante:

- aplica daño directo parcial a madre o vinculo central;
- es counter del cuerpo grande;
- debe ser visible y anticipable.

## 9.4 Fuga de dano al nucleo

Para evitar invulnerabilidad binaria, todo cuerpo tiene fuga leve:

| Estado | Fuga a madre ante contacto |
| --- | ---: |
| Cuerpo completo | 5-8% |
| Cuerpo parcial | 10-15% |
| Nucleo expuesto | 100% |

Esto mantiene tension sin invalidar las capas.

## 9.5 Cuerpo vs squad

El cuerpo gana por resiliencia y area. El squad gana por velocidad, flanqueo y objetivos.

Si el cuerpo siempre persigue y mata squads, esta mal balanceado.
Si los squads siempre ignoran el cuerpo, tambien.

El diseño correcto:

- cuerpo domina zonas;
- squads dominan mapa;
- daño penetrante obliga a diversificar;
- ecosistema castiga blobs.

---

# 10. Husk Drop y Reagrupacion

## 10.1 Husk Drop

Husk Drop es la jugada de ruptura.

Efecto:

- libera adheridas;
- convierte cuerpo en squads temporales;
- da buff breve a ex-adheridas;
- expone a la madre;
- activa surge de nucleo.

Cooldown base: 90s.

## 10.2 Anti-cheese del campeon

Husk Drop voluntario desde cuerpo sano no debe activar el campeon completo de forma gratuita.

Regla final:

- Si el cuerpo cae por daño o queda bajo cierto umbral: Champion Mode pleno.
- Si el jugador usa Husk Drop voluntario con cuerpo sano: Exposed Surge reducido.
- Si habia estructuras latentes instaladas: se activan parcial o totalmente segun daño previo y stress.

Asi Husk Drop es tactica, no boton para saltar a forma final.

## 10.3 Reagrupacion

Las ex-adheridas pueden volver:

- si estan vivas;
- si estan cerca de la madre;
- si hay slots;
- si el stress no esta en colapso;
- pagando ATP y tiempo.

La reagrupacion debe ser una de las habilidades que diferencia a jugadores buenos de jugadores excelentes.

---

# 11. Madre Campeona Final

La madre campeona no es un heroe permanente. Es un estado de emergencia.

## 11.1 Activacion

Se activa cuando:

- la madre queda con 0-2 adheridas;
- existe inversion previa en estructuras de combate;
- el cuerpo fue roto o liberado bajo presion real.

## 11.2 Efectos

| Eje | Efecto |
| --- | --- |
| Daño | Sube por estructuras latentes |
| Velocidad | Sube temporalmente |
| Stress | Sube acelerado |
| Reparacion | Sube brevemente |
| Riesgo | Colapso si no gana, huye o reagrupa |

## 11.3 Temporizador

| Tiempo | Estado |
| --- | --- |
| 0-30s | Ventana optima |
| 30-60s | Advertencia |
| 60-90s | Stress critico |
| 90s+ | Colapso probable |

El campeon debe generar una pregunta clara:

> "¿Puedo terminar esto ahora, o tengo que reconstruir cuerpo?"

---

# 12. Roster Final

v5.12 separa familias jugables, subrutas, neutrales y eventos. No todo organismo interesante debe ser ruta principal.

## 12.1 Rutas jugables principales

| Ruta | Categoria | Fantasia | Rol de cuerpo |
| --- | --- | --- | --- |
| Bacteria | Procariota swarm | Biofilm, toxinas, division rapida | Microcolonias externas/baratas |
| Arquea | Procariota extrema | pH, calor, supervivencia | Coraza quimica, nucleo resistente |
| Cianobacteria | Procariota fototrofa | Luz, O2, tapetes | Productoras y frentes oxigenados |
| Ameba | Eucariota predadora | Fagocitosis, pseudopodos | Bocas externas, cuerpo predador |
| Flagelado | Eucariota movil | Scouts, raideo, gradientes | Motoras, flancos, persecucion |
| Ciliado | Eucariota controlador | Corrientes, filtracion | Control de zona y anti-swarm |
| Microalga | Eucariota productora | Fotosintesis organelar | Baterias energeticas protegidas |
| Diatomea | Microalga acorazada | Frustula, defensa, luz | Placas defensivas/productoras |
| Hongo | Red fija | Hifas, detritus, defensa | Cuerpo anclado, fortaleza territorial |
| Moho mucilaginoso | Red movil | Memoria quimica, rutas | Cuerpo que reconfigura caminos |
| Rotifero | Microfauna filtradora | Anti-swarm, corona | Apex/late microfauna controladora |
| Nematodo | Microfauna perforadora | Estilete, penetracion | Counter de cuerpos y biofilms |

## 12.2 Subrutas y variantes

| Subruta | Familia | Uso |
| --- | --- | --- |
| Espiroqueta/Espirilo | Bacteria/Flagelado segun abstraccion | Penetrante, atraviesa biofilms |
| Anabaena | Cianobacteria | Cadena con heterocistos, fijacion N |
| Myxococcus | Bacteria | Swarm social coordinado |
| Euglena | Flagelado/Microalga | Switch fotosintesis/heterotrofia |
| Volvox | Microalga | Apex colonial/esfera germinal |
| Suctoria | Ciliado | Torre fagocitica anclada |
| Heliozoo/Radiolaria | Neutral/expansion | Defensa radial, belleza visual |

## 12.3 Neutrales, threats y eventos

| Organismo/evento | Rol |
| --- | --- |
| Bacteriofago T4 | Anti-snowball procariota, daño penetrante |
| Daphnia | Grazer/evento contra blooms |
| Dinoflagelado bloom | Evento toxico/rojo, control de luz/nutrientes |
| Tardigrado | Neutral raro, criptobiosis, no ruta core |
| Hydra | Boss/apex de regeneracion, no unidad comun |
| Actinomicetos | Zona antibiotica natural |
| Lacrymaria | Emboscador neutral |

Regla final: el roster puede ser amplio en el mundo, pero **solo una fraccion debe ser jugable principal**.

---

# 13. Estructuras Finales

v5.11 catalogaba 66 elementos. v5.12 los conserva como universo, pero el juego base shippea menos.

## 13.1 Base game recomendado

| Categoria | Cantidad base | Funcion |
| --- | ---: | --- |
| Organelos madre | 8-10 | Economia, identidad, campeon |
| Membrana/pared | 5-6 | Defensa y counters |
| Locomocion | 4-5 | Movimiento y cuerpo |
| Enzimas/proteinas | 6-8 | Resistencia y especializacion |
| Toxinas/armas | 4-5 | Combate quimico |
| Apex/latentes | 4-5 | Late game |

Total base ideal: **28-32 piezas mecanicas**.

Las 66 quedan como:

- expansion;
- codex;
- escenarios;
- future content;
- no como requisito del core.

## 13.2 Tres grupos narrativos

| Grupo | Momento |
| --- | --- |
| Metabolicas | Construccion y economia |
| Anatomicas | Forma del cuerpo y slots |
| Latentes de campeon | Solo importan al colapsar el cuerpo |

Esta division debe aparecer en UI. El jugador no debe ver una lista plana de biologia.

---

# 14. Ecosistema Final

El ecosistema se mantiene como campo de batalla vivo, pero con una regla nueva:

**un cuerpo grande deja huella ecologica fuerte.**

## 14.1 Huella de cuerpo

| Tipo de cuerpo | Huella |
| --- | --- |
| Fototrofo | O2, luz explotada, riesgo Daphnia/blooms |
| Bacteriano | Biofilm, toxinas, fagos |
| Fungico | Detritus digerido, red viva |
| Arquea | pH extremo, bolsas acidas |
| Predador | Panico quimico, restos, neutrales huyendo |

La IA y el Director Ecologico deben responder a huellas visibles, no a castigos ocultos.

## 14.2 Estados del Living Battlefield

Se mantienen seis:

- Cicatriz toxica.
- Corredor nutritivo.
- Frente oxigenado.
- Bolsa acida.
- Red viva.
- Zona de panico quimico.

v5.12 agrega que cada estado debe tener:

- indicador visual;
- tooltip;
- respuesta sugerida;
- threat asociado posible.

---

# 15. Victoria y Modos Finales

## 15.1 Condiciones de victoria en skirmish

Skirmish estandar usa tres condiciones:

| Condicion | Descripcion |
| --- | --- |
| Dominancia ecologica | Controlar porcentaje del mapa durante tiempo sostenido |
| Colapso del linaje rival | Destruir madre/sucesion y cuerpos viables |
| Apex biologico | Completar forma apex o condicion evolutiva avanzada |

Otras condiciones quedan para escenarios:

- biomasa total;
- supervivencia temporal;
- purificacion de zona;
- boss/evento.

## 15.2 Modos de juego

| Modo | Estado final |
| --- | --- |
| Sandbox | Core |
| Skirmish vs IA | Core |
| Escenarios tutoriales | Core |
| Campaña | Producto completo |
| PvP 1v1 | Vertical slice avanzado |
| Co-op/FFA | Expansion |

---

# 16. UI/UX Final

## 16.1 Regla de oro

La UI debe mostrar anatomia, no menus abstractos.

El jugador debe poder mirar la madre y entender:

- estado del cuerpo;
- slots ocupados;
- roles de las adheridas;
- buffs activos;
- riesgos;
- estructuras latentes;
- boton de Husk Drop;
- recetas disponibles.

## 16.2 Pantallas principales

| Pantalla | Funcion |
| --- | --- |
| HUD superior | Estado madre/cuerpo, recursos principales, amenaza |
| HUD inferior | Seleccion, modos, comandos, receta actual |
| Genome Lab | Recetas, previews, madurez, destino de division |
| Panel de cuerpo | Slots, vinculos, buffs, detach/readhere |
| Evolution Board | Afinidad, historial, unlocks |
| Codex | Biologia, counters, organismos vistos |

## 16.3 Defaults y coach

El juego terminado debe incluir un coach contextual ligero:

- "Tu cuerpo tiene pocas productoras."
- "El rival tiene daño penetrante; no confies solo en capas."
- "Puedes reagrupar ex-adheridas cerca de la madre."
- "Esta receta esta inestable porque falta conducta predatoria."

El coach explica sistemas cuando importan, no como tutorial permanente.

---

# 17. Arte y Feedback Final

## 17.1 Lectura visual

Cada celula adherida debe comunicar:

- ruta;
- modo;
- slot/rol;
- estado de HP/stress;
- si esta vinculada o por romperse.

## 17.2 Siluetas de cuerpo

| Silueta | Asociacion |
| --- | --- |
| Esfera/coraza | Defensa, diatomea, arquea, cianobacteria |
| Torpedo | Flagelado, nematodo, raideo |
| Red | Hongo, moho, bacteria biofilm |
| Boca frontal | Ameba, rotifero, predador |
| Bloom | Cianobacteria, microalga, Volvox |

El cuerpo debe contar la build sin abrir paneles.

## 17.3 Sonido

Sonidos clave:

- division;
- adhesion;
- ruptura de vinculo;
- entrada a cuerpo completo;
- nucleo expuesto;
- activacion de campeon;
- Husk Drop;
- reagrupacion.

Estos momentos son identidad del juego.

---

# 18. Balance Final

## 18.1 Rangos objetivo

| Metrica | Objetivo |
| --- | ---: |
| Combate decisivo | 30-60s |
| Tiempo para primer cuerpo parcial | 5-10 min |
| Tiempo para cuerpo completo | 12-20 min |
| Partidas con nucleo expuesto | 30-50% |
| Nucleos expuestos que sobreviven | 20-35% |
| Husk Drops por partida | 0-3 |
| Rutas expresadas por partida | 1 principal + 1-2 auxiliares |
| Recetas usadas por partida | 3-6 |

## 18.2 Costos de cuerpo

El cuerpo no se limita solo por cap. Se limita por:

- mantenimiento ATP;
- stress;
- vulnerabilidad a counters;
- perdida de movilidad;
- costo de oportunidad de slots.

Esto evita que "mas grande" sea siempre "mejor".

## 18.3 Anti-dominancia

Cada macroestrategia tiene counter:

| Estrategia | Counter |
| --- | --- |
| Cuerpo grande defensivo | Penetrante, quimico, economia de mapa |
| Squads libres swarm | AoE, blob penalty, filtradores |
| Fototrofos | Grazers, toxinas, sombra/eventos |
| Biofilm | Nematodo, espirilo, fagos |
| Campeon | Kite, stress, reagrupacion negada |
| Red fungica | Fuego/quimico/ruptura de nodos |

---

# 19. Campaña y Onboarding Final

El juego necesita tutorial progresivo por escenarios. No debe enseñar todo en una partida.

## 19.1 Arco de campaña

| Capitulo | Enseña |
| --- | --- |
| 1. Gota primordial | Recursos, madre, division simple |
| 2. Primer tejido | Adherir, cuerpo parcial, slots |
| 3. Squads libres | Destino de division, modos |
| 4. Ecosistema hostil | Living Battlefield, Director |
| 5. Cuerpo completo | Buffs, counters, mantenimiento |
| 6. Ruptura | Daño por capas, nucleo expuesto |
| 7. Husk Drop | Liberacion tactica y reagrupacion |
| 8. Apex | Forma final y victoria biologica |

## 19.2 Tutorial invisible

El jugador aprende por necesidad:

- si le falta cuerpo, la UI sugiere adherir;
- si tiene cuerpo lento, sugiere motora;
- si muere por quimico, muestra counter;
- si una receta sale inestable, explica que conducta falta.

---

# 20. Telemetria y Exito

El juego terminado debe medir desde temprano:

- recetas producidas;
- recetas maduras vs inestables;
- tiempo en cada estado de cuerpo;
- cantidad de adheridas promedio;
- muertes por daño penetrante/quimico/contacto;
- usos de Husk Drop;
- reagrupaciones exitosas;
- campeones que ganan/pierden;
- rutas dominantes;
- counters usados;
- momentos de abandono del jugador.

Sin telemetria, este juego es demasiado sistemico para balancear solo por sensacion.

---

# 21. Que Le Meteria

Estas son las adiciones que fortalecen el juego terminado:

1. **Plantillas de cuerpo.** Esfera defensiva, torpedo, red, bloom, predador frontal. El jugador puede usarlas como recetas de adhesion.

2. **Coach contextual biologico.** Explica causas, no solo comandos.

3. **Reporte post-partida.** "Tu linaje fue 42% fototrofo, 28% territorial, 18% predador." Muestra evolucion como historia.

4. **Counters muy visibles.** Si el rival usa penetrante, el jugador debe verlo antes de morir.

5. **Codex vivo.** Cada organismo visto desbloquea ficha corta con rol, counter y biologia real.

6. **Eventos ecologicos con causa.** Fagos por swarm bacteriano, Daphnia por blooms, acidez por arqueas, etc.

7. **Reagrupacion como skill.** Que reconstruir cuerpo despues de una pelea sea satisfactorio.

8. **Siluetas por ruta.** La forma del cuerpo debe cambiar segun receta y ruta.

9. **Apex emergentes de cuerpo.** Volvox, Hydra/boss, red fungica apex, predador compuesto.

10. **Modo practica sin presion.** Necesario para un juego de sistemas.

---

# 22. Que Le Sacaria

Estas piezas dañan claridad o scope si entran al juego base:

1. **60 slots visibles.** Demasiado para leer y controlar.

2. **66 estructuras como base game.** Buen universo, mal primer producto.

3. **Joints fisicos obligatorios.** Riesgo alto de bugs, performance y frustracion.

4. **Campeon demasiado fuerte.** Debe ser ventana dramatica, no estrategia dominante.

5. **Reglas automaticas por unidad estilo programacion profunda.** Se reemplazan por doctrinas.

6. **Tardigrado como ruta jugable normal.** Sigue siendo anti-juego si su fantasia es solo no morir.

7. **Demasiadas condiciones de victoria en skirmish.** Tres bastan.

8. **Terminologia confusa como "stress emocional".** Usar stress quimico/biologico/colonial.

9. **Todo organismo interesante como jugable.** Muchos deben ser neutrales, threats o eventos.

10. **Microgestion de submateriales en HUD principal.** Mantener ATP, Biomasa, Stress arriba; submateriales en interior.

---

# 23. Que Mejoraria de v5.11

## 23.1 Roster

v5.11 mezcla rutas, subrutas y organismos de expansion. v5.12 los separa para balancear.

## 23.2 Cuerpo

v5.11 propone demasiado cuerpo potencial. v5.12 define un cuerpo legible de 19 slots y abstrae el resto.

## 23.3 Champion

v5.11 lo vuelve muy atractivo. v5.12 le pone anti-cheese y temporizador mas severo.

## 23.4 Estructuras

v5.11 tiene un catalogo hermoso, pero demasiado grande. v5.12 distingue base game de universo expandido.

## 23.5 Roadmap

v5.11 salta a 23 scripts V6. v5.12 recomienda validar primero el nucleo:

- adhesion logica;
- slots;
- estados;
- buffs;
- detach/readhere;
- daño simple por capas.

---

# 24. Riesgos Finales

| Riesgo | Mitigacion final |
| --- | --- |
| Sobrecarga UX | Defaults, plantillas, coach, tutorial por capas |
| Cuerpo dominante | Mantenimiento, penetrante, quimico, mapa |
| Campeon roto | Anti-cheese, timer, requiere inversion |
| Performance | Vinculos logicos, 19 slots, microcolonias visuales |
| Roster inflado | Familias jugables vs subrutas/neutrales |
| Balance imposible | Telemetria por combinacion |
| Curva dura | Campaña por mecanica |

---

# 25. GDD Final: Regla de Producto

La version terminada de Protogenesis no se define por tener mas organismos, mas estructuras o mas rutas.

Se define por esta frase:

> Cada celula que produces debe hacerte elegir entre construir cuerpo, crear squad o preparar evolucion.

Si una feature refuerza esa frase, entra.

Si no, pasa a expansion o se corta.

---

# 26. Proximo Documento

Despues de esta direccion final, el siguiente documento debe ser:

**Protogenesis_v5_12_Version_Implementable_Basica**

Ese documento no debe intentar implementar todo este GDD. Debe elegir el menor conjunto que pruebe la fantasia:

1. Hija nace adherida o libre.
2. Cuerpo usa slots logicos alrededor de madre.
3. Estados del cuerpo afectan stats.
4. HUD muestra estado.
5. Desadherir/reagrupar existe.
6. Combate fisico golpea adheridas antes que madre.

Si eso no es divertido, el resto no salva el juego.

Si eso es divertido, Protogenesis tiene columna vertebral.

