# PROTOGENESIS: PRIMORDIA

## GDD v5.14 - Sistema Canonico de Adaptaciones

**Objetivo:** reemplazar la mezcla actual de genes, estructuras, rutas y afinidad por una sola gramatica de progresion: Adaptaciones. El jugador no elige una clase. Construye un organismo, y el juego reconoce su identidad emergente.

| Campo | Valor |
| --- | --- |
| Fecha | 2026-05-07 |
| Base | GDD v5.13 + documento externo `Protogenesis_v5_13_Sistema_Adaptaciones.md` + discusion de diseno |
| Estado | Direccion canonica para pasar del MVP al juego completo |
| Decision central | Adaptaciones son la fuente de verdad del progreso biologico |
| Regla de corte | Si una mejora no cambia madre, produccion, cuerpo, identidad o apex, se fusiona o se elimina |

---

# 1. Diagnostico

El prototipo actual se siente raro porque tres sistemas intentan responder la misma pregunta:

| Pregunta del jugador | Sistemas que hoy compiten |
| --- | --- |
| Que soy? | ruta, genes, estructuras, metabolismo, affinity tracker |
| Que puedo producir? | camara germinal, ruta, estructuras, genes |
| Que mejora debo elegir? | panel interior, arbol de genes, afinidad, rutas |
| Por que desbloquee esto? | pesos de afinidad, estructuras, ambiente, historial |
| Donde se toma la decision importante? | E, G, 8, B, L, V |

La solucion canonica es eliminar la division conceptual entre "gen activo", "estructura instalada" y "ruta consolidada". Todo eso pasa a ser una **Adaptacion**.

Las estructuras siguen existiendo en codigo y visuales, pero dejan de ser el lenguaje principal del jugador. Los genes dejan de ser un arbol separado. Las rutas dejan de ser clases elegidas. La identidad aparece por combinacion de adaptaciones.

---

# 2. Principios Canonicos

## 2.1 Una adaptacion tiene hasta cuatro consecuencias

Cada Adaptacion puede afectar:

| Capa | Pregunta que responde |
| --- | --- |
| Madre | Que le pasa directamente a mi celula central? |
| Produccion | Que hijas/castas/recetas puedo producir ahora? |
| Cuerpo | Que slots, capas o formas corporales habilito? |
| Identidad | Hacia que organismo o familia me reconoce el juego? |

No todas las adaptaciones necesitan las cuatro capas. Pero toda adaptacion debe afectar al menos una de estas cosas:

1. Desbloquea una receta nueva.
2. Mejora una receta existente.
3. Habilita un slot o forma corporal.
4. Cambia la identidad emergente.
5. Abre camino a apex.

Si no hace ninguna, sobra.

## 2.2 Hitos evolutivos y adaptaciones activas

No todo debe ocupar el mismo cap. La version original de adaptaciones proponia 12 slots para todo, pero eso castiga demasiado a builds eucariotas y multicelulares porque gastan medio cap solo en ser viables.

| Tipo | Cuenta contra cap 14 | Ejemplos |
| --- | ---: | --- |
| Hito evolutivo | No | Nucleo, Mitocondria, Cloroplasto, Adhesion Persistente |
| Adaptacion activa | Si | Lisosoma, Pseudopodos, Cilios, Frustula, Hifa |
| Estado especial | Variable | Criptobiosis, Quiste, Espora |
| Forma apex | No | Volvox completo, Ameba voraz, Supercolonia |

Regla base: la madre puede sostener **14 adaptaciones activas**. Champion/apex puede elevarlo a **16**. Los hitos evolutivos no cuentan, pero si tienen costo, tiempo y consecuencias.

## 2.3 Identidades emergentes

El jugador nunca elige "soy Ameba" en un menu. El juego lo reconoce.

Ejemplo:

```text
Nucleo + Mitocondria + Lisosoma + Pseudopodos
= Identidad: Ameba

Ameba + Vacuola contractil + Citostoma + biomasa fagocitada
= Forma avanzada: Ameba voraz
```

Estados de reconocimiento:

| Estado | Requisito | Texto UI |
| --- | --- | --- |
| Tendencia | 2 piezas compatibles | Tendencia predadora |
| Identidad | hitos + 3-4 adaptaciones clave | Ameba |
| Forma avanzada | identidad + 2-4 adaptaciones extra | Ameba voraz |
| Apex | forma avanzada + condicion ecologica/combat | Apex disponible |

## 2.4 Irreversibilidad limitada

Las bifurcaciones irreversibles tienen mucho valor narrativo, pero no deben estar por todas partes. Si todo cierra opciones, el juego se vuelve ansioso.

Irreversibles canonicas:

| Bifurcacion | Que significa |
| --- | --- |
| Nucleo | cruzas de procariota a eucariota; cierras adaptaciones procariotas exclusivas nuevas |
| Adhesion Persistente | cruzas a multicelularidad real; el cuerpo pasa a ser tu estilo principal |

Regla: el resto de conflictos se maneja preferentemente con costo aumentado, efectos dormidos, penalizaciones o cambios de forma, no con prohibicion dura.

## 2.5 Adesina basica vs Adhesion Persistente

Estas dos cosas deben separarse.

| Adaptacion | Momento | Funcion | Irreversible |
| --- | --- | --- | --- |
| Adesina basica | tutorial/T1 | permite pegar y despegar hijas al cuerpo | No |
| Adhesion Persistente | T4 | las divisiones pueden nacer integradas; cuerpo multicelular real | Si |

Esto conserva la tutorializacion temprana de "puedo pegar celulas", sin casar al jugador con multicelularidad demasiado pronto.

## 2.6 Ambiente como descuento, no bloqueo duro

El ambiente debe empujar la progresion, no encerrarla.

Ejemplo:

| Adaptacion | Sin ambiente | Con ambiente |
| --- | --- | --- |
| Tilacoide | instalable caro/lento | -40% costo si pasaste 60s en luz |
| Bomba de protones | instalable caro/lento | -40% costo si sobreviviste acidez |
| Membrana extremofila | instalable caro/lento | -40% costo tras crisis termica |

Esto hace que el mapa sea input evolutivo sin impedir planes.

---

# 3. Transiciones Evolutivas

La progresion sigue el arbol de la vida como metafora jugable.

| Transicion | Equivalente biologico | Rol de juego |
| --- | --- | --- |
| T1 - Vida procariota | LUCA, bacterias, arqueas, cianobacterias | inicio rapido, economia, ambiente |
| T2 - Endosimbiosis | origen eucariota | gran bifurcacion, mas energia y complejidad |
| T3 - Especializacion eucariota | protistas predadores/productores/redes | rutas principales de midgame |
| T4 - Multicelularidad | adhesion, diferenciacion, capas | cuerpo real, castas, arquitectura |
| T5 - Apex | culminaciones biologicas | cierre de linea evolutiva |

El jugador puede quedarse en T1 toda la partida y ganar como procariota. Eucariota no debe ser "la unica ruta correcta"; debe ser otra apuesta.

---

# 4. Catalogo De Adaptaciones

## 4.1 T1 - Procariotas y tutorial

| Adaptacion | Tipo | Tags | Desbloquea |
| --- | --- | --- | --- |
| Pared bacteriana | Activa | Procariota, Bacteria, Defensa | Bacteria base, Biofilm |
| Flagelo bacteriano | Activa | Procariota, Motilidad, Raider | Bacteria movil, Espirilo, Bdellovibrio |
| Capsule polisacarida | Activa | Defensa, Evasion, Biofilm | Biofilm resistente |
| Pili/Fimbrias | Activa | Adhesion, Colonizacion, Social | Adesina basica, Biofilm |
| Adesina basica | Activa temprana | Adhesion, Cuerpo, Tutorial | pegar/despegar hijas |
| Plasmido conjugativo | Activa | Social, Herencia, Bacteria | conjugacion |
| Quorum sensing | Activa | Social, Coordinacion, Swarm | Myxococcus, Biofilm avanzado |
| Division rapida | Activa | Swarm, Reproduccion | Bacteria swarm |
| Tilacoide procariota | Activa | Fotosintesis, Luz, Cianobacteria | Cianobacteria |
| Heterocisto | Activa | Sacrificio, Filamento, Anabaena | Anabaena |
| Vesicula de gas | Activa | Flotacion, Luz, Cianobacteria | Ciano flotante |
| Membrana extremofila | Activa | Arquea, Resistencia, Extremo | Arquea |
| Bomba de protones | Activa | Acido, Terraformacion, Arquea | Arquea acidofila |

## 4.2 T2 - Hitos de endosimbiosis

| Adaptacion | Tipo | Cuenta cap | Efecto |
| --- | --- | ---: | --- |
| Nucleo | Hito irreversible | No | abre eucariotas, cierra procariota exclusiva nueva |
| Mitocondria | Hito energetico | No | sostiene predadores y microfauna, aumenta ATP |
| Cloroplasto | Hito productor | No | fotosintesis eucariota, microalga, Volvox |
| Endosimbiosis activa | Hito avanzado | No | convierte simbiontes/hijas en capacidades internas |

## 4.3 T3 - Eucariota predador

| Adaptacion | Tipo | Tags | Desbloquea |
| --- | --- | --- | --- |
| Lisosoma | Activa | Predador, Digestion, Ameba | fagocitosis, Ameba |
| Pseudopodos | Activa | Movimiento blando, Ameba | Ameba movil |
| Vacuola contractil | Activa | Osmosis, Sustain | Ameba resistente, Ciliado estable |
| Cilios | Activa | Movimiento fino, Control, Ciliado | Ciliado, Paramecium |
| Citostoma | Activa | Boca, Fagocitosis, Control | Ciliado predador, Stentor |
| Toxocistos | Activa | Toxina, Burst, Predador | Dinoflagelado toxico, Hidra |
| Tentaculos suctores | Activa | Anclaje, Drenaje | Suctoria |
| Sensor quimico | Activa | Reconocimiento, Caza | Lacrymaria, Nematodo |

## 4.4 T3 - Eucariota productor

| Adaptacion | Tipo | Tags | Desbloquea |
| --- | --- | --- | --- |
| Pared celulosica | Activa | Productor, Defensa, Microalga | Microalga estable |
| Eyespot | Activa | Luz, Sensor, Motilidad | Euglena |
| Flagelo eucariota | Activa | Motilidad, Eucariota | Euglena, Volvox movil |
| Frustula de silice | Activa | Defensa, Diatomea, Silice | Diatomea |
| Captacion de silice | Activa | Mineral, Diatomea | Diatomea fortaleza |
| Catalasa / defensa ROS | Activa | Oxigeno, Fotosintesis, Proteccion | fotosintesis eficiente |
| Bioluminiscencia | Activa | Senal, Stress, Dinoflagelado | Dinoflagelado |
| Adhesina colonial | Activa | Colonia, Productor, Volvox | Volvox temprano |

## 4.5 T3 - Red y descomponedores

| Adaptacion | Tipo | Tags | Desbloquea |
| --- | --- | --- | --- |
| Hifa fungica | Activa | Red, Hongo, Detritus | Hongo |
| Quitina sintasa | Activa | Defensa, Hongo | hifas resistentes |
| Enzimas extracelulares | Activa P0 | Digestion externa, Reciclaje | Hongo saprofito |
| Plasmodio mucilaginoso | Activa | Red movil, Moho | Physarum |
| Memoria quimica | Activa | Navegacion, Recursos, Moho | Moho memorioso |
| Senal de hambre | Activa | Crisis, Agregacion | Dictyostelium |
| Esporas | Estado/Activa | Continuidad, Supervivencia | Hongo/Dictyostelium |

## 4.6 T4 - Multicelularidad

| Adaptacion | Tipo | Tags | Desbloquea |
| --- | --- | --- | --- |
| Adhesion Persistente | Hito irreversible | Multicelular, Cuerpo | cuerpo multicelular real |
| Diferenciacion celular | Hito funcional | Castas, Produccion | hijas con roles distintos |
| Comunicacion por senales | Activa | Coordinacion, Cuerpo | sincronizacion de cuerpo |
| Matriz extracelular | Activa | Forma, Defensa | cuerpo estable |
| Especializacion por capa | Activa | Capas, Volvox, Rotifero | exterior/interior funcional |
| Epitelio polarizado | Activa | Eje corporal, Microfauna | Hidra, Rotifero, Nematodo |
| Tejido contractil | Activa | Movimiento corporal | Hidra, Nematodo |
| Sistema nervioso difuso | Activa avanzada | Reaccion, Apex | Hidra apex |
| Mastax | Activa microfauna | Trituracion, Rotifero | Rotifero |
| Cuticula | Activa microfauna | Defensa, Nematodo, Cripto | Nematodo, Tardigrado |
| Estilete | Activa ofensiva | Perforacion | Nematodo |
| Pie retractil | Activa control | Anclaje | Rotifero |

## 4.7 Estados especiales

| Estado | Requisitos | Funcion |
| --- | --- | --- |
| Criptobiosis | Cuticula + resistencia osmotica | sobrevivir crisis, no ganar por si sola |
| Quiste | stress alto + nucleo viable | pausa defensiva |
| Espora resistente | hongo/moho + crisis | continuidad colonial |
| Champion biologico | cuerpo colapsa + madre viva | comeback temporal |

---

# 5. Identidades Emergentes

## 5.1 Reglas de reconocimiento

```text
Identidad = hitos requeridos + adaptaciones minimas + tags dominantes
```

Prioridad cuando una build cumple varias identidades:

1. Apex activo gana sobre todo.
2. Multicelular completo gana sobre unicelular.
3. Red especializada gana sobre eucariota generico.
4. Subruta especifica gana sobre ruta madre.
5. Gana la identidad con mas adaptaciones clave.
6. Si sigue empatado, se muestra como hibrido.

Ejemplos:

```text
Nucleo + Cloroplasto + Frustula
= Diatomea, no Microalga generica

Nucleo + Cilios + Citostoma + Tamano grande
= Stentor, no Ciliado generico

Nucleo + Cloroplasto + Hifa + Simbiosis
= Liquen microscopico
```

## 5.2 Rutas principales

| Identidad | Requisitos minimos | Fantasia | Produce |
| --- | --- | --- | --- |
| Bacteria swarm | Pared + Flagelo + Division rapida, sin Nucleo | masa barata, presion constante | bacterias moviles |
| Biofilm | Pared + Capsule + Pili/Adesina | defensa territorial | bacterias adhesivas |
| Arquea extremofila | Membrana extremofila + Bomba protones + Quimio | convertir zonas hostiles en hogar | arqueas resistentes |
| Cianobacteria | Pared + Tilacoide + Catalasa | economia solar y oxigeno | cianobacterias |
| Anabaena | Tilacoide + Heterocisto + Adhesion filamentosa | economia con sacrificio interno | filamentos |
| Ameba | Nucleo + Mitocondria + Lisosoma + Pseudopodos | caza por contacto | amebas |
| Ciliado | Nucleo + Mitocondria + Cilios + Vacuola | control de flujo y filtracion | ciliados |
| Microalga | Nucleo + Cloroplasto + Pared celulosica | productor estable | microalgas |
| Diatomea | Nucleo + Cloroplasto + Frustula | productor acorazado | diatomeas |
| Hongo | Nucleo + Hifa + Enzimas; Quitina como defensa P1 | red fija territorial saprofita | nodos hifales |
| Moho mucilaginoso | Nucleo + Plasmodio + Memoria | red movil que recuerda recursos | nodos plasmodiales |
| Volvox | Cloroplasto + Adhesina colonial + Adhesion Persistente + Diferenciacion | esfera multicelular productora | somaticas/germinales |
| Rotifero | Cilios + Mastax + Epitelio + Comunicacion | microfauna filtradora-combatiente | partes rotiferas |

## 5.3 Subrutas tacticas

| Subruta | Base | Adaptaciones distintivas | Rol |
| --- | --- | --- | --- |
| Myxococcus | Bacteria/Biofilm | Quorum sensing + Pili + Caza grupal | swarm coordinado |
| Espirilo/Spirochete | Bacteria movil P2 | Cuerpo helicoidal + Flagelo axial | infiltrador anti-biofilm |
| Bdellovibrio | Bacteria P2 | Invasion bacteriana + Digestion interna | asesino anti-bacteria |
| Ciano flotante | Cianobacteria | Vesicula de gas + Fototaxis | economia movil de luz |
| Euglena | Microalga | Mitocondria + Cloroplasto + Flagelo + Eyespot | hibrido luz/caza |
| Dinoflagelado | Microalga/toxico | Toxocistos + Bioluminiscencia | bloom toxico |
| Stentor | Ciliado | Tamano grande + Citostoma | tanque/control |
| Lacrymaria | Ciliado predador | Citostoma extensible + Sensor | sniper biologico |
| Suctoria | Ciliado | Anclaje + Tentaculos suctores | torreta drenadora |
| Dictyostelium | Moho | Senal de hambre + Pseudoplasmodio + Esporas | resiliencia en crisis |
| Liquen microscopico | Hongo + Microalga | Hifa + Cloroplasto simbionte + Simbiosis | red fotosintetica |

## 5.4 Hibridos

| Hibrido | Condicion | Valor |
| --- | --- | --- |
| Euglena | Productor + Motilidad + Mitocondria | alterna luz y heterotrofia |
| Mixotrofo predador | Cloroplasto + Lisosoma + Mitocondria | flexible, caro |
| Liquen | Hifa + Cloroplasto + Simbiosis | defensa + produccion |
| Predador toxico | Lisosoma + Toxocistos | caza con dano quimico |
| Productor defensivo | Cloroplasto + Frustula/Pared | economia protegida |
| Red simbiotica | Hifa + Simbiosis + productor aliado | territorio autosuficiente |

---

# 6. Produccion y Cuerpo

## 6.1 Roles canonicos de hija

| Rol | Funcion |
| --- | --- |
| Recolectora | trae recursos, combate bajo |
| Defensora | protege madre/cuerpo |
| Exploradora | vision, velocidad, mapa |
| Cazadora | dano directo o fagocitosis |
| Colonizadora | control territorial |
| Productora | ATP/biomasa/luz |
| Conectora | cuerpo/adherencia |
| Especialista | efecto unico por identidad |

## 6.2 Produccion por identidad

| Identidad | Produccion base | Variantes por adaptacion |
| --- | --- | --- |
| Bacteria swarm | Recolectora bacteriana, Movil bacteriana | Conjugadora, Cazadora social |
| Biofilm | Colonizadora adhesiva, Defensora encapsulada | Toxica, Conectora |
| Arquea | Recolectora extrema, Defensora acidofila | Termofila, Osmotica |
| Cianobacteria | Productora solar, Colonizadora fotosintetica | Flotante, Filamentosa |
| Anabaena | Celula fotosintetica, Heterocisto | Conector filamentoso |
| Ameba | Cazadora ameboide, Guardia blanda | Resistente, Devoradora |
| Ciliado | Controlador ciliado, Filtrador | Paramecium, Stentor |
| Microalga | Productora, Soporte fotosintetico | Movil, Fototactica |
| Euglena | Productora movil, Exploradora mixotrofa | Cazadora ligera |
| Diatomea | Defensora de silice, Productora acorazada | Fortaleza |
| Dinoflagelado | Toxica, Productora de bloom | Senaladora |
| Hongo | Nodo hifal, Recicladora | Hifa defensiva, Digestora |
| Moho | Nodo plasmodial, Exploradora quimica | Memoriosa, Esporuladora |
| Volvox | Somatica externa, Germinal interna | Motora externa, Productora |
| Hidra | Tentacular, Cuerpo tubular | Regeneradora, Cnidocito toxico |
| Rotifero | Corona ciliada, Mastax, Pie retractil | Filtradora avanzada |
| Nematodo | Segmento contractil, Estilete | Sensorial, Cuticular |

## 6.3 Familias de cuerpo

No todos deben usar la misma grilla con otro skin.

| Familia | Forma corporal |
| --- | --- |
| Bacteria swarm | microcolonias temporales |
| Biofilm | manta territorial |
| Cianobacteria/Anabaena | cadenas o filamentos |
| Eucariotas blandos | cuerpo hexagonal flexible |
| Diatomea | coraza exterior |
| Hongo | red de nodos |
| Moho | pseudoplasmodio movil |
| Volvox | esfera con capas |
| Hidra | eje tubular con tentaculos |
| Rotifero | cuerpo polarizado: corona, mastax, pie |
| Nematodo | cadena lineal segmentada |

---

# 7. Formas Apex

Regla:

```text
Identidad reconocida + 6-9 adaptaciones compatibles + condicion ecologica/combat = Apex disponible
```

| Apex | Viene de | Cambio mecanico | Counter |
| --- | --- | --- | --- |
| Supercolonia bacteriana | Bacteria/Biofilm | microcolonias coordinadas masivas | fagos, AOE quimico |
| Biofilm gigante | Biofilm | manta viva defensiva y toxica | nematodos, espiroquetas |
| Cianobacteria hiperoxigenadora | Ciano/Anabaena | oxigeno y ATP extremos | grazers, sombra |
| Arquea santuario extremo | Arquea | zona inhabitable para otros | movilidad enemiga |
| Ameba voraz | Ameba | bocas activas y absorcion mejorada | kiteo, toxinas |
| Ciliado apex | Ciliado | corrientes locales y control | burst, toxinas |
| Diatomea fortaleza | Diatomea | coraza fotosintetica | perforacion, oscuridad |
| Bloom dinoflagelado | Dinoflagelado | marea toxica temporal | detox, retirada |
| Hongo micorrizal | Hongo | red territorial que recicla | cortes de red |
| Physarum apex | Moho | rutas optimas y redistribucion | toxinas, cortes multiples |
| Volvox completo | Volvox | somaticas protegen, germinales reproducen | penetracion, sombra |
| Hidra | Predador multicelular | tentaculos y regeneracion | dano simultaneo |
| Rotifero | Ciliado multicelular | filtracion + mastax | predadores grandes |
| Nematodo | Microfauna perforadora | cuerpo lineal anti-red/coraza | swarms dispersos |
| Criptobiosis/Tardigrado | Estado especial | casi invulnerable pero inactivo | no gana terreno |

Primer paquete recomendado: Supercolonia, Ciano hiperoxigenadora, Ameba voraz, Diatomea fortaleza, Hongo micorrizal, Physarum, Volvox, Rotifero.

---

# 8. Ecologia de Counters

Ninguna identidad debe tener un counter binario. El loop correcto:

```text
Soy fuerte en X
dejo senal ecologica Y
el Director responde con amenaza Z
puedo responder con adaptacion W
```

## 8.1 Counters por identidad

| Identidad | Fuerte contra | Debil contra | Respuesta posible |
| --- | --- | --- | --- |
| Bacteria swarm | economia rapida, unidades aisladas | fagos, AOE, filtradores | diversidad, dispersion, capsule |
| Biofilm | defensa local | nematodos, espiroquetas | toxinas, capas, capsule |
| Arquea | crisis ambientales | enemigos moviles | santuario extremo |
| Cianobacteria | economia solar | Daphnia, sombra | flotacion, catalasa |
| Ameba | unidades medianas | kiteo, toxinas, espiculas | vacuola, citostoma |
| Ciliado | swarms pequenos | burst, toxinas | vacuola, comunicacion |
| Microalga | economia | grazers, sombra | movilidad, simbiosis |
| Diatomea | asedios frontales | perforacion, oscuridad | flotacion, defensa |
| Hongo | detritus, territorio | cortes de red | quitina, redundancia |
| Moho | exploracion economica | toxinas, cortes multiples | reconfiguracion |
| Volvox | economia multicelular | penetracion, grazers | capa somatica |
| Rotifero | swarms, particulas | predadores grandes | pie retractil |
| Nematodo | biofilm, hongo, diatomea | swarms dispersos | sensor, cuticula |

## 8.2 Director Ecologico

| Senal del jugador | Respuesta del Director |
| --- | --- |
| mucha bacteria densa | bacteriofagos |
| mucha fotosintesis/oxigeno | Daphnia, rotiferos, blooms |
| biofilm extenso | nematodos, espiroquetas |
| hongo con mucho detritus | nematodos, competidores saprofitos |
| moho muy expandido | toxinas de zona, sequia local |
| ameba devorando mucho | presas espinosas, heliozoos |
| diatomea fortaleza | perforadores, sombra |
| multicelular grande | depredadores grandes, dano penetrante |
| mucha homogeneidad genetica | epidemias especificas |

La UI debe explicar la causa:

```text
Bacteriofagos detectados.
Causa: alta densidad bacteriana local.
Respuesta sugerida: dispersa microcolonias o instala Capsule.
```

---

# 9. Panel del Genoma

La tecla G deja de ser "arbol de genes". Pasa a ser **Panel del Genoma / Adaptaciones**.

Objetivo: el jugador responde en menos de 5 segundos:

1. Que soy ahora?
2. Que puedo instalar ahora?
3. Que abre o cierra esta adaptacion?

## 9.1 Estructura

```text
GENOMA - 5/14 adaptaciones activas
Identidad: Ameba emergente - T3 Predador
Apex cercano: Ameba voraz - faltan Citostoma + Vacuola

Linea evolutiva:
T1 Procariota OK | T2 Nucleo OK | T3 Predador ACTIVA | T4 Multi pendiente

Instaladas:
Pared, Flagelo, Nucleo, Mitocondria, Lisosoma

Disponibles:
Pseudopodos - abre Ameba
Vacuola - sustain/osmosis
Citostoma - abre Ameba voraz
Cloroplasto - hibrido caro, abre Euglena/Microalga

Bloqueadas/Cerradas:
Heterocisto - cerrado por Nucleo
Adhesion Persistente - requiere 4 adaptaciones T3
```

## 9.2 Tabs

| Tab | Funcion |
| --- | --- |
| Actual | identidad, hitos, instaladas, apex cercano |
| Adaptar | lista contextual de adaptaciones disponibles |
| Rutas | identidades posibles/cercanas, sin elegirlas directamente |
| Codex | biologia, counters, ejemplos |

No se recomienda un arbol gigante como vista principal. Puede existir en Codex, pero la vista de juego debe ser contextual.

## 9.3 Colores funcionales

| Color | Significado |
| --- | --- |
| Verde | disponible/recomendada |
| Amarillo | hibrida o costo aumentado |
| Rojo | irreversible o cierra opciones |
| Azul | hito evolutivo |
| Gris | bloqueada |
| Morado suave | apex/late game |

---

# 10. Ficha Implementable de Adaptacion

Campos canonicos:

```text
Adaptacion:
Tipo: Hito / Activa / Estado / Apex
Transicion:
Tags:
Costo base:
Tiempo instalacion:
Cuenta para cap:
Prerequisitos:
Bloquea:
Descuentos ambientales:
Efecto madre:
Efecto produccion:
Efecto cuerpo:
Efecto identidad:
Tier visual:
Latente Champion:
Efecto Champion:
Counter natural:
Telemetria:
Sinergias:
Conflictos:
Irreversible:
```

Estados:

| Estado | Significado |
| --- | --- |
| Disponible | se puede instalar ahora |
| Recomendada | encaja con identidad actual |
| Hibrida | disponible, pero cara por desviacion |
| Bloqueada | faltan requisitos |
| Cerrada | una bifurcacion la prohibio |
| Instalada | activa |
| Dormida | instalada, pero sin efecto completo por conflicto/ambiente |

Ejemplo:

```text
Adaptacion: Lisosoma
Tipo: Activa
Transicion: T3 Predador
Cuenta para cap: Si
Prerequisitos: Nucleo
Efecto madre: +dano contacto, fagocitosis
Efecto produccion: desbloquea amebas cazadoras
Efecto cuerpo: habilita boca/contacto exterior
Efecto identidad: +Predador, +Ameba, +Fagocitosis
Sinergias: Vacuola, Citostoma, Pseudopodos
```

---

# 11. Migracion Tecnica

No se debe reescribir todo como V7 de inmediato. La ruta segura:

```text
V5AdaptationSystem pasa a ser fuente de verdad.
Genes, Structures y Affinity quedan como fachada legacy hasta migrar.
```

## 11.1 Scripts nuevos

| Script | Responsabilidad |
| --- | --- |
| `V5AdaptationTypes.cs` | enums: id, tags, transition, category |
| `V5AdaptationDefinition.cs` | datos de una adaptacion |
| `V5AdaptationLibrary.cs` | catalogo canonico |
| `V5AdaptationSystem.cs` | instalacion, gating, conflictos |
| `V5IdentityRecognizer.cs` | identidad, subruta, apex cercano |
| `V5GenomePanelIMGUI.cs` | UI nueva de G |

## 11.2 Mapping legacy

| Legacy | Adaptacion nueva |
| --- | --- |
| `V5GeneId.Respiration` | Mitocondria / respiracion aerobica |
| `V5GeneId.Photosynthesis` | Tilacoide o Cloroplasto segun dominio |
| `V5GeneId.Adhesion` | Adesina basica |
| `V5StructureId.Lysosome` | Lisosoma |
| `V5StructureId.BacterialFlagellum` | Flagelo bacteriano |
| `V5StructureId.EukaryoticFlagellum` | Flagelo eucariota |
| `V5StructureId.Thylakoid` | Tilacoide procariota |
| `V5StructureId.MicroalgalChloroplast` | Cloroplasto |
| `V5StructureId.InvasiveHypha` | Hifa fungica |
| `V5StructureId.MucilageMatrix` | Plasmodio/Mucilago |
| `V5StructureId.CryptobiosisTun` | Criptobiosis |

## 11.3 Orden de implementacion

1. Crear tipos y catalogo de adaptaciones.
2. Crear sistema de instalacion con mapping legacy.
3. Crear reconocedor de identidad.
4. Reemplazar G por GenomePanel contextual.
5. Hacer que produccion germinal lea identidad.
6. Hacer que cuerpo lea Adesina basica y Adhesion Persistente.
7. Ocultar arbol genico viejo.
8. Migrar Director, HUD y Codex a identidad emergente.

## 11.4 Primer paquete jugable

No se implementan las 39+ adaptaciones de golpe. Primer paquete sugerido:

| Grupo | Adaptaciones |
| --- | --- |
| T1 | Pared, Flagelo bacteriano, Capsule, Pili/Adesina, Tilacoide, Membrana extremofila, Bomba protones |
| T2 | Nucleo, Mitocondria, Cloroplasto |
| T3 | Lisosoma, Pseudopodos, Cilios, Vacuola, Frustula, Hifa, Enzimas extracelulares, Plasmodio, Memoria quimica |
| T4 | Adhesion Persistente, Diferenciacion, Comunicacion |

Identidades iniciales suficientes:

```text
Bacteria swarm, Biofilm, Arquea, Cianobacteria, Ameba, Ciliado,
Microalga, Diatomea, Hongo, Moho, Volvox temprano.
```

---

# 12. NPC, Threats y Backlog

No todo organismo debe ser jugable completo.

| Organismo | Categoria | Rol |
| --- | --- | --- |
| Bacteriofago | Threat | anti-snowball bacteriano |
| Daphnia | NPC/threat | grazer contra blooms fotosinteticos |
| Heliozoo | NPC | zona defensiva radial |
| Actinomicetos | NPC ambiental | zona antibiotica |
| Tardigrado neutral | NPC/estado | recompensa de supervivencia |
| Plasmodium/Trypanosoma | Post-launch | parasito intracelular |
| Hidra salvaje | Threat/apex futuro | depredador multicelular |
| Nematodo salvaje | Threat/apex futuro | anti-hifas y anti-biofilm |

---

# 13. Decisiones Abiertas

| Tema | Recomendacion actual |
| --- | --- |
| Tardigrado | no ruta core; usar como criptobiosis/neutral/apex raro |
| Hidra | apex futuro, no primer paquete |
| Nematodo | apex/counter futuro, no primer paquete completo |
| Arbol visual gigante | no vista principal; solo Codex o vista avanzada |
| Cap 14/16 | 14 adaptaciones activas base, 16 con Champion/apex; no hitos |
| Ambiente | descuentos y aceleradores, no locks absolutos |
| Reescritura V7 | evitar por ahora; migracion incremental V5 |

---

# 14. Resumen Ejecutivo

La direccion canonica queda asi:

```text
El progreso principal ya no es Genes + Estructuras + Affinity.
El progreso principal es Adaptaciones.

Las estructuras son representacion fisica/visual de adaptaciones.
Los genes son lenguaje legacy/interno, no la interfaz principal.
Las rutas son identidades emergentes reconocidas por combinacion.
La produccion y el cuerpo leen identidad + adaptaciones.
El Director Ecologico responde a senales biologicas visibles.
```

La fantasia queda mas fuerte:

```text
No eliges una clase.
No compras upgrades abstractos.
Construyes un organismo vivo.
```
