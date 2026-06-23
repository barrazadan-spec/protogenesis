# PROTOGENESIS: PRIMORDIA

## V5.7 Canon Pivot - AAA compacto

| Campo | Valor |
| --- | --- |
| Version | V5.7 Canon Pivot |
| Fecha | 2026-05-11 |
| Tipo | Documento operativo corto |
| Estado | Fuente de direccion inmediata |
| Base | Vision Completa v1.0 + codigo V5.16.0 |

---

# 1. Decision de direccion

Protogenesis no debe pivotear a "mas sistemas". Debe pivotear a una experiencia que se sienta AAA por claridad, feedback, identidad y momento dramatico.

La meta no es competir con AAA por volumen. La meta es que una partida corta tenga una fantasia tan legible y memorable que parezca producida con intencion de AAA:

```text
nacer -> adaptarse -> producir hijas -> formar cuerpo/squads -> chocar con el ecosistema -> revelar identidad -> ganar o caer en Champion Mode
```

La direccion queda asi:

- Mantener 4 rutas MVP.
- No crear una arquitectura V7 paralela.
- Canonizar y afilar los sistemas V5 existentes.
- Convertir el MVP en una partida libre excelente antes de hacer campana completa.
- Medir exito por sensacion de ruta, lectura visual y tension del arco madre/cuerpo/squads.

---

# 2. Filosofia mecanica canonica

## 2.1 Menos menu, mas metamorfosis

El jugador no debe sentir que compra upgrades. Debe sentir que el organismo cambia de naturaleza.

Regla practica: cada adaptacion importante debe producir al menos uno de estos efectos visibles:

- cambia silueta;
- cambia movimiento;
- cambia forma de producir hijas;
- cambia relacion con ambiente;
- cambia condicion de combate;
- cambia identidad emergente.

## 2.2 Menos simulacion invisible, mas consecuencia visible

Si el sistema calcula algo importante, el jugador debe verlo. Oxigeno, stress, colonizacion, adhesion, casta y estado corporal no pueden vivir solo en numeros.

Regla practica: todo sistema core necesita una lectura en pantalla en menos de 3 segundos.

## 2.3 Biologia como verbo

La biologia no entra como texto educativo. Entra como accion:

- fagocitar;
- adherir;
- fotosintetizar;
- dividir;
- secretar;
- oxidar;
- diferenciar;
- encapsular;
- liberar germinales.

## 2.4 La madre es personaje

La madre no es una unidad principal cualquiera. Es el linaje. Las hijas, el cuerpo, las rutas y el Champion Mode existen para contar su historia.

Regla practica: si una mecanica no cambia la vida de la madre, no es prioritaria.

---

# 3. MVP AAA compacto: 4 rutas si, profundidad compacta

Las 4 rutas se mantienen, pero el primer corte no intenta implementar todas las variantes. Cada ruta necesita:

- 1 fantasia jugable;
- 5-7 adaptaciones clave;
- 1 lectura visual clara;
- 1 counter o amenaza natural;
- 1 condicion de poder/apex;
- 1 momento memorable.

## 3.1 Bacteria - swarm rapido

Fantasia: "sobrevivo por cantidad, velocidad y division".

Adaptaciones minimas:

- Pared bacteriana
- Flagelo bacteriano
- Pili/Fimbrias
- Division rapida
- Tilacoide procariota o Membrana extremofila como variante situacional
- Adesina basica

Momento memorable: burst reproductivo o supercolonia visible.

Lectura visual: muchas celulas pequenas, movimiento rapido, halo de biofilm/adhesion cuando consolida territorio.

## 3.2 Ameba - depredador que crece

Fantasia: "cazo, fagocito y me vuelvo enorme".

Adaptaciones minimas:

- Nucleo
- Mitocondria
- Lisosoma
- Pseudopodos
- Vacuola contractil
- Cilios o Citostoma si se agrega como subrasgo

Momento memorable: devorar una presa y crecer de forma visible.

Lectura visual: celula grande, borde deformable, pseudopodos, vesiculas internas oscuras.

## 3.3 Productor fotosintetico - terraformador de oxigeno

Fantasia: "no persigo; transformo el mapa hasta que el mapa pelea por mi".

Nota de canon: evitar la confusion de "Cianobacteria eucariota". La ruta jugable se llama Productor Fotosintetico. Puede empezar procariota como Cianobacteria y cruzar a microalga/diatomea/Volvox si instala Nucleo y Cloroplasto.

Adaptaciones minimas:

- Tilacoide procariota
- Catalasa/ROS
- Nucleo
- Cloroplasto
- Pared de celulosa
- Frustula de silice como defensa opcional

Momento memorable: bloom de O2 que cambia color, limpia zona o dana enemigos vulnerables.

Lectura visual: verdes/dorados, pulso de luz, neblina de oxigeno, mapa mas brillante alrededor.

## 3.4 Volvox - cuerpo multicelular defensivo

Fantasia: "protejo germinales internas con un cuerpo especializado".

Adaptaciones minimas:

- Nucleo
- Cloroplasto
- Flagelo eucariota
- Adhesina colonial
- Adhesion persistente
- Diferenciacion celular
- Comunicacion por senales

Momento memorable: cuerpo se rompe y libera germinales o mini-Volvox.

Lectura visual: esfera/cuerpo ordenado, somaticas externas, productoras internas, germinales protegidas.

---

# 4. Sistemas V5 que se canonizan

No se crea V7. Se trabaja sobre estos sistemas:

| Sistema | Archivo | Decision |
| --- | --- | --- |
| Adaptaciones | `Assets/_ProjectV5/Scripts/Evolution/V5AdaptationSystem.cs` | Fuente de verdad del progreso biologico. Ajustar cap, requisitos y efectos canonicos. |
| Catalogo | `Assets/_ProjectV5/Scripts/Evolution/V5AdaptationLibrary.cs` | Recortar/etiquetar adaptaciones MVP. No ampliar antes del slice. |
| Identidad emergente | `Assets/_ProjectV5/Scripts/Evolution/V5IdentityRecognizer.cs` | Debe reconocer las 4 rutas compactas y explicar por que. |
| Castas | `Assets/_ProjectV5/Scripts/Colony/V5CasteLibrary.cs` | Consolidar a 4+1 para el MVP visible. Sensor/Estructural pueden fusionarse o quedar debug. |
| Cuerpo | `Assets/_ProjectV5/Scripts/Colony/V5MulticellularBodySystem.cs` | Expandir de 6 slots actuales a lectura canonica simple solo si mejora Volvox. |
| Champion/Husk | `Assets/_ProjectV5/Scripts/Colony/V5MulticellularBodySystem.cs` | Hacerlo mas dramatico, mas visible y mas largo. Es pilar, no bonus. |
| Endgame | `Assets/_ProjectV5/Scripts/Systems/V5EndgameDirector.cs` | Reescribir condiciones a Dominancia ecologica, Colapso rival, Apex biologico. |
| Escenarios | `Assets/_ProjectV5/Scripts/World/V5ScenarioDefinition.cs` | Mantener Freeplay/FirstDrop para slice. Campana de 8 escenarios queda congelada. |
| Bootstrap | `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | Punto de composicion runtime. No duplicar servicios. |
| Telemetria | `Assets/_ProjectV5/Scripts/Systems/V5TelemetrySystem.cs` | Usar para playtest de rutas, bloqueos y friccion de genoma. |

---

# 5. Lo que se congela hasta que el loop sea divertido

Congelado, no eliminado:

- campana de 8 escenarios;
- Modo Observatorio;
- Desafios Diarios;
- DLCs;
- multiplayer;
- editor avanzado de cuerpo;
- 19 slots;
- mas de 4 rutas core;
- mas adaptaciones fuera del corte MVP;
- arte final contratado;
- narracion final;
- trailer cinematografico.

Regla: si no mejora la partida libre de 15-25 minutos, no entra en el siguiente hito.

---

# 6. Primer hito: Four Routes Prototype

Duracion objetivo: 2 semanas.

Objetivo: una escena jugable donde las 4 rutas se puedan empujar desde el sistema de adaptaciones y se sientan distintas en menos de 3 minutos.

## 6.1 Entregables

- Selector rapido de intencion de ruta o botones debug limpios en Genoma.
- AdaptationLibrary etiquetada con corte MVP por ruta.
- Identidad emergente reconoce Bacteria, Ameba, Productor Fotosintetico y Volvox.
- HUD muestra ruta actual, sugerida, condicion de poder y siguiente adaptacion.
- Cada ruta tiene feedback visual minimo.
- Champion Mode se entiende cuando aparece.
- Una condicion de victoria comun permite terminar partida.

## 6.2 Criterios de exito

Una persona que no haya leido el GDD debe poder responder despues de 10 minutos:

1. "Que ruta estabas jugando?"
2. "Que cambio cuando instalaste adaptaciones?"
3. "Por que ganaste o perdiste?"
4. "Que momento fue mas memorable?"
5. "Que querias hacer despues?"

El hito pasa si 3 de 5 testers distinguen las 4 rutas sin explicacion larga.

---

# 7. Primer backlog tecnico

## P0 - Canonizar direccion sin romper build

- Crear tags/metadata de ruta MVP en `V5AdaptationDefinition` o resolverlo desde `V5BiologyCanon`.
- Definir helper `IsMvpCoreAdaptation(id)` para ocultar ruido post-MVP en UI principal.
- Renombrar en UI la ruta "Cianobacteria" a "Productor Fotosintetico" cuando se hable de fantasia jugable.
- Ajustar `V5IdentityRecognizer` para que Productor Fotosintetico sea identidad puente y no contradiccion biologica.
- Revisar `V5EndgameDirector` contra las 3 victorias canonicas.

## P1 - Ruta feel

- Bacteria: acelerar sensacion de division y swarm.
- Ameba: hacer que fagocitosis/crecimiento sea visible y gratificante.
- Productor: hacer que oxigeno/luz cambien mapa de forma clara.
- Volvox: hacer que cuerpo/proteccion/germinales sean legibles.

## P2 - Playtest

- Crear preset de escenario "Four Routes Prototype" o reutilizar Freeplay con parametros compactos.
- Exportar reporte de ruta elegida, adaptaciones instaladas, tiempo a identidad, tiempo a muerte/victoria.
- Registrar "momento memorable" manual post-test en notas, no en codigo.

---

# 8. Definicion de exito del pivote

El pivote funciona si:

- el juego deja de sentirse como acumulacion de sistemas y empieza a sentirse como metamorfosis;
- las 4 rutas se leen en pantalla;
- el jugador entiende por que el organismo cambio;
- el cuerpo multicelular no parece accesorio, sino destino biologico;
- Champion Mode se vuelve el momento que alguien contaria a otra persona;
- el equipo puede iterar sin crear otra rama conceptual gigante.

---

# 9. Frase guia

Construir menos cosas, pero hacer que cada cambio biologico se vea, se sienta y tenga consecuencia.

