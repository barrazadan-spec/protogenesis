# PROTOGENESIS: PRIMORDIA

## GDD v5.13 - Sistemas Biologicos Canonicos Post-MVP

**Objetivo:** salir del MVP sin perder lo ganado. Esta version ordena rutas, genes, estructuras, fenotipos, roles y cuerpo para que el proyecto original pueda crecer sin duplicar sistemas.

| Campo | Valor |
| --- | --- |
| Fecha | 2026-05-05 |
| Base | GDD v5.12 + codigo V5 Build 2.14/2.95 |
| Estado | Auditoria de sistemas biologicos y direccion post-MVP |
| Decision central | Una sola cadena legible: Genes -> Estructuras -> Rutas -> Fenotipos -> Cuerpo/Squads |
| Regla de corte | Si dos sistemas responden la misma pregunta, uno se fusiona, se reubica o pasa a backlog |

---

# 1. Diagnostico Honesto

El MVP ya demuestra la fantasia base:

- La madre existe como nucleo productivo.
- El cuerpo multicelular existe como sistema logico.
- La Camara Germinal produce fenotipos y puede decidir destino Auto/Cuerpo/Squad.
- El jugador ya tiene paneles para Interior, Genes, Germinal, Cuerpo, Roles y Mejoras.

El problema post-MVP no es falta de contenido. El problema es que varias capas todavia compiten por autoridad.

| Pregunta | Hoy la responden demasiados sistemas |
| --- | --- |
| "Que soy?" | Ruta, estructuras, metabolismo, genes y fenotipo |
| "Que puedo construir?" | Panel Interior, genes inmediatos, recetas germinales |
| "Que hace esta celula?" | Modo celular, rol de linaje, fenotipo, slot corporal |
| "Por que desbloquee esta ruta?" | Afinidad, estructuras, genes, historial, entorno |
| "Que panel debo abrir?" | E, G, 8, B, L, V con fronteras aun borrosas |

La solucion no es borrar sistemas. La solucion es asignarles responsabilidades canonicas.

---

# 2. Modelo Canonico

## 2.1 Responsabilidades finales

| Capa | Responsabilidad unica | No debe hacer |
| --- | --- | --- |
| Ruta | Identidad ecologica emergente | Ser una clase fija que encierra toda la partida |
| Gen | Permiso heredable, tendencia y regla global | Reemplazar al organelo fisico |
| Estructura | Pieza anatomica instalada en una celula | Decidir sola toda la ruta sin contexto |
| Fenotipo | Receta de produccion de hijas | Ser otra ruta evolutiva |
| Modo | Conducta RTS inmediata | Ser una mejora permanente |
| Rol de linaje | Preferencia persistente de trabajo | Duplicar todo el fenotipo |
| Mejora de linaje | Bonus permanente a roles | Abrir una segunda progresion genetica paralela |
| Cuerpo | Arquitectura espacial de la colonia | Ser otra especie aparte |
| Squad libre | Alcance tactico externo | Reemplazar al cuerpo |

## 2.2 Cadena de decision del jugador

1. **G - Genes:** elijo capacidades heredables y direccion evolutiva.
2. **E - Interior:** instalo estructuras fisicas habilitadas o recomendadas por genes/ruta.
3. **8 - Germinal:** produzco fenotipos con preview y madurez biologica.
4. **B - Cuerpo:** decido si esas celulas forman tejido adherido o salen como squad.
5. **L/V - Linaje:** ajusto roles y mejoras de comportamiento.

Esta cadena debe verse en UI como una sola historia, no como seis minijuegos.

---

# 3. Inventario Actual En Codigo

## 3.1 Rutas

| Categoria | Rutas actuales |
| --- | --- |
| MVP | Bacteria, Arquea, Cianobacteria, Ameba |
| Core post-MVP | Flagelado, Ciliado, Microalga, Hongo, Moho mucilaginoso |
| Experimental | Rotifero, Nematodo |
| Neutral/Apex | Tardigrado |
| Retiradas/legado | Neutrofilo, Macrofago, NK, B Cell, StemCell como ruta primaria |

Decision: las 11 rutas primarias pueden seguir existiendo como meta de producto, pero no todas necesitan el mismo nivel de implementacion inmediata.

## 3.2 Genes

Hay 14 genes activos en 4 anillos:

| Anillo | Genes |
| --- | --- |
| R1 Metabolismo | Respiracion, Fotosintesis, Fermentacion, Quimiolitotrofia |
| R2 Funcion | Motilidad, Secrecion, Reconocimiento, Adhesion |
| R3 Division | Division Rapida, Herencia Fuerte, Autonomia, Reabsorcion Total |
| R4 Ecosistema | Simbiosis, Maduracion Apex |

Decision: el sistema de genes esta bien como esqueleto. Lo que falta es que actue como "licencia biologica" y no como instalador magico de organelos definitivos.

## 3.3 Estructuras

Hay 26 estructuras instalables.

| Familia | Estructuras |
| --- | --- |
| LUCA/base | Compartimento genetico, Motor metabolico, Sintesis, Vacuola, Catalasa |
| Procariota | Flagelo bacteriano, Pared peptidoglicano, Capsula, Fimbrias, Plasmido, Tilacoide |
| Eucariota | Flagelo eucariota, Lisosoma, Pared celulosa/quitina, Hifa, Cilios, Granulo toxico, Receptores, Vesicula, Plasticidad |
| Especial/post-MVP | Cloroplasto microalgal, Mucilago, Corona ciliada, Estilete, Cuticula, Criptobiosis |

Decision: el numero es correcto para post-MVP. La mejora es clasificar mejor permisos, rutas y redundancias biologicas.

## 3.4 Fenotipos germinales

Hay 9 recetas:

| Tipo | Fenotipos |
| --- | --- |
| Nucleo de linaje | Hija plastica, Recolectora, Exploradora, Defensora, Raider |
| Auxiliares | Ameboide defensiva, Ciliada de control, Simbionte bacteriano, Microalga soporte |

Decision: este sistema debe crecer horizontalmente con variantes por ruta, pero sin convertir cada variante en una ruta nueva.

---

# 4. Matriz Canonica Ruta/Gen/Estructura/Fenotipo

## 4.1 Rutas MVP y Core

| Ruta | Estado | Genes que empujan | Estructuras clave | Fenotipos naturales | Cuerpo | Squad libre | Mecanica unica |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Bacteria | MVP | Fermentacion, Division Rapida, Secrecion, Adhesion | Fimbrias, Plasmido, Flagelo bacteriano, pared | Simbionte bacteriano, Recolectora, Colonizadora | Conectores, biofilm vivo | Swarm barato | Quorum/microcolonia visual |
| Arquea | MVP | Quimiolitotrofia, Reabsorcion, Herencia Fuerte | Capsula, Catalasa, futura S-layer | Defensora, Reserva, Productora extrema | Coraza/reserva | Unidades lentas resistentes | Control de pH/zona extrema |
| Cianobacteria | MVP | Fotosintesis, Adhesion, Simbiosis | Tilacoide, pared, Catalasa | Productora, Conectora luminica | Productoras y tapete | Colonizadoras de luz | Frente de oxigeno |
| Ameba | MVP | Reconocimiento, Herencia Fuerte, Secrecion | Lisosoma, Vacuola, Catalasa | Ameboide defensiva, Raider | Bocas/armadura blanda | Tanques/cazadores | Fagocitosis por contacto |
| Flagelado | Core | Motilidad, Autonomia, Respiracion | Flagelo eucariota, Catalasa, Motor metabolico | Scout, Raider | Motor/sensor | Raideo y persecucion | Hit-and-run biologico |
| Ciliado | Core | Motilidad, Reconocimiento, Autonomia | Cilios, Vacuola, Lisosoma | Ciliada de control | Boca/sensor | Control de zona | Corrientes y arrastre pasivo |
| Microalga | Core | Fotosintesis, Simbiosis, Herencia | Cloroplasto, pared, Tilacoide | Microalga soporte, Productora | Productoras | Eco-squads de luz | Economia luminica |
| Hongo | Core | Adhesion, Secrecion, Herencia | Hifa, pared quitinosa, Vesicula | Defensora, Digestora | Coraza/conector fijo | Poca movilidad | Fortaleza digestiva fija |
| Moho mucilaginoso | Core | Adhesion, Reabsorcion, Autonomia | Mucilago, Vacuola, Vesicula | Conectora, Scout economica | Red movil/conectiva | Rutas de detritus | Memoria quimica |

## 4.2 Rutas experimentales y especiales

| Ruta | Estado | Mantener como | Razon |
| --- | --- | --- | --- |
| Rotifero | Experimental | Microfauna anti-swarm | Se diferencia de Ciliado si su rol es filtrador multicelular, no simple cilia extra |
| Nematodo | Experimental | Counter lineal de cuerpos/biofilms | Su nicho es perforacion y corredores, no movilidad generica |
| Tardigrado | Neutral/Apex | Criptobiosis, neutral raro o apex defensivo | Como ruta pura sigue siendo anti-juego si solo "no muere" |
| Neutrofilo/Macrofago/NK/B | Retiradas | Rasgos, eventos o estructuras | Son celulas inmunes de organismos grandes, no microfauna/ecologia de gota |
| StemCell | Sistema, no ruta | Plasticidad de madre/linaje | Debe ser estado o gen, no especie jugable |

---

# 5. Genes Como Licencias Biologicas

## 5.1 Regla nueva

Un gen no deberia forzar siempre una estructura final. Debe:

- habilitar una familia de estructuras;
- dar descuento o calidad a estructuras coherentes;
- sumar afinidad visible a rutas;
- mejorar madurez de recetas germinales;
- desbloquear conductas o efectos persistentes.

## 5.2 Mapa gen -> licencia

| Gen | Licencia canonica | Instalacion inicial permitida |
| --- | --- | --- |
| Respiracion | Metabolismo eucariota eficiente | Motor metabolico mejorado, Catalasa |
| Fotosintesis | Fototrofia | Tilacoide en procariota, Cloroplasto en eucariota |
| Fermentacion | Procariota anaerobio/swarm | Plasmido, toxina leve |
| Quimiolitotrofia | Extremofilia mineral | Catalasa, capsula, futura S-layer |
| Motilidad | Locomocion y sensores | Flagelo bacteriano/eucariota, cilios segun ruta |
| Secrecion | Enzimas/toxinas externas | Plasmido o vesicula secretora |
| Reconocimiento | Lectura de firmas/presas | Receptores, estilete selectivo, filtracion avanzada |
| Adhesion | Superficie, biofilm, red | Fimbrias, hifa, mucilago segun ruta |
| Division Rapida | Swarm y reposicion | No requiere estructura obligatoria |
| Herencia Fuerte | Estabilidad del linaje | Plasticidad madre o compartimento reforzado |
| Autonomia | Squads menos dependientes | Sensorial, motilidad, doctrina scout |
| Reabsorcion Total | Reciclaje y continuidad | Vacuola, mucilago, detritus |
| Simbiosis | Auxiliares y cuerpo mixto | Mayor cupo auxiliar, slots conectivos mejores |
| Maduracion Apex | Forma premium | Volvox/Hydra/Tardigrado/Paramecium segun ruta |

## 5.3 Cambios recomendados al codigo

1. Crear `V5BiologyCanon` como fuente de verdad para:
   - genes recomendados por ruta;
   - estructuras recomendadas por ruta;
   - estructuras habilitadas por gen;
   - fenotipos naturales por ruta;
   - rol corporal preferido por estructura/fenotipo.

2. Cambiar `V5GeneSystem.ApplyImmediateEffect`:
   - ahora instala estructuras directamente;
   - post-MVP deberia otorgar licencia y, como mucho, instalar una pieza starter barata.

3. Cambiar `V5CellEntity.CanInstall`:
   - hoy solo revisa dominio, recursos y carga;
   - debe consultar licencia biologica o marcar la instalacion como "exploratoria/inestable".

4. Cambiar Panel Interior:
   - separar estructuras en "Recomendadas", "Permitidas", "Exploratorias" y "Bloqueadas".

---

# 6. Redundancias Detectadas

## 6.1 E Panel Interior vs G Genes

Problema: el panel Interior permite instalar estructuras, mientras Genes tambien instala estructuras de inmediato. Eso debilita la cadena causal.

Decision:

- G no debe competir con E.
- G decide permiso/herencia/tendencia.
- E materializa organelos.

## 6.2 L Roles vs V Mejoras

Problema: Roles y Mejoras son el mismo sistema desde la fantasia del jugador: "como trabaja mi linaje".

Decision:

- Fusionarlos en un panel `Linaje`.
- Tabs:
  - Roles: asignacion a seleccion.
  - Mejoras: upgrades por rol.
  - Doctrina: reglas futuras por defecto para nuevas hijas.

## 6.3 Ciliado vs Rotifero

Problema: ambos usan cilios/corona y controlan particulas.

Decision:

- Ciliado = unicelular, corrientes locales y control de flujo.
- Rotifero = microfauna multicelular, filtrador anti-swarm y amenaza de area.
- Corona ciliada debe pertenecer principalmente a Rotifero; Cilios comunes a Ciliado.

## 6.4 Hongo vs Moho mucilaginoso

Problema: ambos son redes que consumen detritus.

Decision:

- Hongo = red fija, defensa, digestion territorial y anclaje.
- Moho = red movil, baja defensa, rutas de detritus y memoria quimica.
- Mucilago no debe ser "hifa movil"; debe recordar zonas buenas y optimizar caminos.

## 6.5 Arquea con pared de peptidoglicano

Problema biologico: las arqueas no usan peptidoglicano bacteriano como estructura canonica.

Decision:

- Mantener temporalmente por balance si hace falta.
- Reemplazo futuro: `SLayerEnvelope` o `PseudomureinWall`.
- Peptidoglicano queda para Bacteria/Cianobacteria.

## 6.6 Fotosintesis procariota vs microalga

Problema: hoy Fotosintesis desde LUCA tiende a procariota e instala Tilacoide. Eso favorece Cianobacteria y confunde Microalga.

Decision:

- Tilacoide = fototrofia procariota/cianobacteria.
- Cloroplasto = fototrofia eucariota/microalga.
- El gen Fotosintesis debe abrir ambas ramas; la estructura elegida decide la ruta.

## 6.7 Estructuras inmunes legadas

Problema: Granulo azurofilo, reconocimiento y rutas inmunes siguen arrastrando lenguaje de Neutrofilo/NK/B.

Decision:

- `AzurophilicGranule` debe renombrarse semanticamente como granulo toxico/vesicula litica.
- Rutas inmunes quedan retiradas.
- Sus fantasias se reciclan como conductas: toxina, reconocimiento, marca, digestion.

---

# 7. Paneles Objetivo Post-MVP

| Tecla | Panel final | Funcion |
| --- | --- | --- |
| E | Interior | Instalar estructuras fisicas, metabolismo activo, submateriales |
| G | Genes | Elegir licencias heredables por anillo |
| 8 | Germinal | Producir fenotipos y elegir destino |
| B | Cuerpo | Slots, adheridas, buffs, detach/readhere |
| L | Linaje | Roles + mejoras + doctrina |
| V | Libre o alias | Recomendado: alias de Linaje o retirar para evitar duplicacion |

Decision recomendada: fusionar L/V y dejar `V` como alias temporal del mismo panel o liberarla para evolucion visual/planner.

---

# 8. Orden De Implementacion Recomendado

## Sprint A - Canon biologico en codigo

Crear `V5BiologyCanon.cs` sin cambiar gameplay todavia.

Debe exponer:

- `GenesForRoute(path)`
- `StructuresForRoute(path)`
- `StructuresUnlockedByGene(gene, domain, path)`
- `NaturalPhenotypesForRoute(path)`
- `BodyBiasForRoute(path)`
- `RedundancyWarning(path/structure/gene)`

Objetivo: que UI, afinidad, Germinal e Interior consulten la misma tabla.

## Sprint B - Panel Linaje unificado

Fusionar:

- `V5LineageSystem`
- `V5LineageUpgradeSystem`

En un panel `Linaje` con tabs.

Mantener clases internas si conviene, pero unificar la experiencia.

## Sprint C - Interior guiado por genes

Actualizar E:

- Recomendadas por ruta actual.
- Permitidas por genes.
- Exploratorias: pueden instalarse pero suman stress o menor madurez.
- Bloqueadas: faltan dominio, gen o recursos.

## Sprint D - Germinal orientado por canon

Actualizar 8:

- mostrar fenotipos naturales de la ruta;
- mostrar auxiliares coherentes;
- mostrar por que la receta esta madura/inestable;
- sugerir destino cuerpo/squad por deficit corporal real.

## Sprint E - Rutas diferenciadas

Implementar una mecanica unica por ruta antes de agregar mas organismos.

---

# 9. Ideas De Optimizacion De Sistemas

## 9.1 Una sola fuente de verdad

Ahora hay informacion de rutas repartida en:

- `V5EvolutionLibrary`
- `V5EvolutionAffinitySystem`
- `V5RosterBalance`
- `V5PhenotypeRecipeLibrary`
- `V5GerminalProductionSystem`
- UI de HUD/Interior/Germinal

Eso funciona para MVP, pero escala mal. `V5BiologyCanon` debe convertirse en el diccionario central.

## 9.2 Afinidad explicable

La afinidad ya muestra razones, pero debe conectar con acciones futuras:

- "Tienes +45 Hongo porque instalaste Hifa + Adhesion + Secrecion."
- "Instalar Mucilago moveria +22 Moho."
- "Producir Ciliada de control esta inestable porque falta Reconocimiento o zona fluida."

## 9.3 Rutas como paquetes de tension, no clases

Cada ruta debe tener:

- una ventaja obvia;
- un costo obvio;
- un counter natural;
- una expresion corporal;
- una expresion de squad.

Si una ruta solo es "otra forma de hacer DPS", se corta o se vuelve fenotipo.

## 9.4 Fenotipos por deficit

La Camara Germinal deberia sugerir:

- "Faltan productoras en cuerpo."
- "Tienes demasiadas bocas, baja economia."
- "El squad libre necesita scout."
- "Ruta actual madura para Raider, inestable para Ciliada."

Esto convierte el Genome Lab en decision estrategica, no tienda de unidades.

## 9.5 Estructuras con tags

Agregar tags a estructuras:

- `Metabolica`
- `Locomocion`
- `Defensa`
- `Predacion`
- `Red`
- `Sensorial`
- `Fototrofia`
- `Microfauna`
- `Latente/Apex`

Sirve para UI, balance, afinidad y tooltips.

---

# 10. Decision Final v5.13

El juego original no debe volver a ser "muchas rutas y muchos paneles".

Debe ser:

> una colonia que toma decisiones biologicas legibles, donde genes abren posibilidades, estructuras las vuelven cuerpo, rutas emergen de lo que haces, fenotipos expresan tacticas, y el cuerpo/squad decide como esa biologia pelea en el mapa.

El siguiente cambio de codigo debe ser el canon biologico central. Despues de eso se puede fusionar Linaje y reordenar E/G/8/B sin improvisar.
