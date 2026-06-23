# PROTOGENESIS: PRIMORDIA

## Catalogo Definitivo de Adaptaciones v5.14.1

**Objetivo:** cerrar la tabla maestra implementable del sistema de Adaptaciones. Este documento traduce el GDD v5.14 a datos concretos: costos, requisitos, efectos, identidades, sinergias, conflictos y prioridad de implementacion.

| Campo | Valor |
| --- | --- |
| Fecha | 2026-05-07 |
| Base | `Protogenesis_GDD_v5_14_Sistema_Adaptaciones_Canonico.md` |
| Estado | Catalogo canonico para implementacion V5 incremental |
| Fuente de verdad | Adaptaciones |
| UI principal | Panel del Genoma / G |
| Cap base | 14 adaptaciones activas |
| Cap apex | 16 adaptaciones activas con Champion/apex |
| Hitos | No cuentan contra cap |

---

# 1. Convenciones

## 1.1 Recursos

Los costos usan el `V5ResourceWallet` actual:

| Abrev | Recurso |
| --- | --- |
| ATP | ATP |
| Bio | Biomasa |
| AA | Aminoacidos |
| Lip | Lipidos |
| NT | Nucleotidos |
| Min | Minerales |

Formato:

```text
ATP/Bio/AA/Lip/NT/Min
```

Ejemplo:

```text
18/8/2/4/0/2
```

## 1.2 Tipos

| Tipo | Cuenta cap | Regla |
| --- | ---: | --- |
| Hito | No | transicion mayor, puede ser irreversible |
| Activa | Si | mejora instalada normal |
| Estado | Variable | respuesta a crisis o forma dormida |
| Apex | No | resultado final, no se compra como upgrade comun |

## 1.3 Prioridad de implementacion

| Prioridad | Significado |
| --- | --- |
| P0 | primer paquete jugable de Adaptaciones |
| P1 | segundo paquete, despues de validar P0 |
| P2 | backlog/expansion |

## 1.4 Estados UI

| Estado | Significado |
| --- | --- |
| Disponible | puede instalarse ahora |
| Recomendada | encaja con identidad actual |
| Hibrida | disponible, pero con costo aumentado |
| Bloqueada | faltan requisitos |
| Cerrada | una bifurcacion la prohibio |
| Instalada | activa |
| Dormida | instalada, pero parcial por conflicto o ambiente |

---

# 2. Reglas Globales

## 2.1 Instalacion

```text
1. Jugador elige adaptacion en G.
2. Sistema revisa requisitos, locks y costo dinamico.
3. Si es irreversible, abre confirmacion.
4. Madre paga recursos.
5. Madre no puede dividir durante instalacion.
6. Al terminar:
   - marca adaptacion instalada
   - aplica efecto legacy si existe
   - recalcula identidad emergente
   - actualiza recetas germinales
   - actualiza cuerpo
   - registra evento en HUD/Codex
```

## 2.2 Descuentos ambientales

El ambiente reduce costo o tiempo. No bloquea adaptaciones basicas.

| Condicion | Bonificacion |
| --- | --- |
| 60s acumulados en luz alta | -35% costo para fotosintesis/flotacion |
| 60s acumulados en oxigeno alto | -25% costo para catalasa/mitocondria |
| 60s acumulados en acidez | -35% costo para bomba protones/membrana extrema |
| 60s acumulados en toxinas | -25% costo para capsule/catalasa |
| 3 marcadores quimicos generados | -30% costo para memoria quimica |

## 2.3 Irreversibles

Solo dos locks son duros en el paquete canonico:

| Adaptacion | Cierra |
| --- | --- |
| Nucleo | Plasmido nuevo, Heterocisto nuevo, Vesicula de gas nueva, Procariota Apex nuevo |
| Adhesion Persistente | estilo de squads 100% libres como plan principal |

Lo demas usa conflictos, costos aumentados o estados dormidos.

---

# 3. T1 - Procariotas y Tutorial

## T1_BacterialWall - Pared bacteriana

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 14/8/2/2/0/2 |
| Tiempo | 8s |
| Tags | Procariota, Bacteria, Defensa, Biofilm |
| Requisitos | ninguno |
| Bloquea | Pseudopodos activos hasta Nucleo + Membrana flexible |
| Legacy | `V5StructureId.PeptidoglycanWall` |
| Madre | +18% HP max, +10% resistencia fisica |
| Produccion | desbloquea Bacteria base |
| Cuerpo | habilita defensoras exteriores simples |
| Identidad | +Bacteria, +Biofilm |
| Sinergias | Capsule, Pili/Fimbrias, Division rapida |
| Conflictos | Pseudopodos blandos sin eucariota |

## T1_BacterialFlagellum - Flagelo bacteriano

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 18/8/5/2/0/1 |
| Tiempo | 9s |
| Tags | Procariota, Motilidad, Raider, Bacteria |
| Requisitos | ninguno |
| Bloquea | ninguno |
| Legacy | `V5StructureId.BacterialFlagellum` |
| Madre | +22% velocidad, +1.5 sensor |
| Produccion | desbloquea bacteria movil |
| Cuerpo | motora procariota exterior |
| Identidad | +Bacteria swarm, +Espirilo, +Bdellovibrio |
| Sinergias | Division rapida, Quorum sensing |
| Conflictos | Frustula activa reduce beneficio |

## T1_PolysaccharideCapsule - Capsule polisacarida

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 18/12/2/6/0/1 |
| Tiempo | 10s |
| Tags | Defensa, Evasion, Biofilm, AntiRecon |
| Requisitos | Pared bacteriana o Membrana extremofila |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Capsule` |
| Madre | +12% HP, -20% deteccion enemiga, +10% resistencia toxina |
| Produccion | defensora encapsulada |
| Cuerpo | capa externa anti-reconocimiento |
| Identidad | +Biofilm, +Bacteria defensiva |
| Sinergias | Pared bacteriana, Quorum sensing |
| Conflictos | Reconocimiento aliado pierde 15% eficacia |

## T1_PiliFimbriae - Pili/Fimbrias

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 8/4/1/1/0/0 |
| Tiempo | 6s |
| Tags | Adhesion, Colonizacion, Social, Tutorial |
| Requisitos | ninguno |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Fimbriae` |
| Madre | +0.25 colonizacion |
| Produccion | conectora procariota basica |
| Cuerpo | permite anclaje a superficie |
| Identidad | +Biofilm, +Myxococcus |
| Sinergias | Adesina basica, Capsule, Quorum sensing |
| Conflictos | ninguno |

## T1_BasicAdhesin - Adesina basica

| Campo | Valor |
| --- | --- |
| Tipo | Activa temprana |
| Prioridad | P0 |
| Costo | 6/3/1/1/0/0 |
| Tiempo | 5s |
| Tags | Adhesion, Cuerpo, Tutorial, Colonial |
| Requisitos | ninguno |
| Bloquea | ninguno |
| Legacy | `V5GeneId.Adhesion` temporal |
| Madre | +0.15 colonizacion |
| Produccion | conectora basica |
| Cuerpo | permite pegar/despegar hijas manualmente |
| Identidad | +Colonial, +Biofilm leve |
| Sinergias | Pili/Fimbrias, Adhesion Persistente |
| Conflictos | ninguno |

**Nota canonica:** esta es la mejora tutorial para entender cuerpos pegados. No es multicelularidad irreversible.

## T1_ConjugativePlasmid - Plasmido conjugativo

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 24/12/4/2/8/0 |
| Tiempo | 12s |
| Tags | Social, Herencia, Bacteria, Conjugacion |
| Requisitos | Bacteria tag x2 |
| Bloquea | cerrado por Nucleo |
| Legacy | `V5StructureId.Plasmid` |
| Madre | hijas cercanas comparten 10% bonus de adaptacion bacteriana |
| Produccion | bacteria conjugadora |
| Cuerpo | vinculos temporales entre bacterias |
| Identidad | +Bacteria swarm, +Myxococcus |
| Sinergias | Quorum sensing, Division rapida |
| Conflictos | Nucleo lo cierra |

## T1_QuorumSensing - Quorum sensing

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 22/12/6/2/4/0 |
| Tiempo | 12s |
| Tags | Social, Coordinacion, Swarm, Myxococcus |
| Requisitos | 3+ celulas aliadas o Biofilm tag |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | bonus de cohesion: +12% dano/produccion si hay 4+ hijas cerca |
| Produccion | coordinadora de quorum |
| Cuerpo | microcolonias coordinadas |
| Identidad | +Myxococcus, +Supercolonia |
| Sinergias | Plasmido, Pili/Fimbrias, Division rapida |
| Conflictos | dispersion reduce efecto |

## T1_RapidDivision - Division rapida

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 26/16/8/4/8/2 |
| Tiempo | 14s |
| Tags | Swarm, Reproduccion, Bacteria |
| Requisitos | 2 adaptaciones T1 |
| Bloquea | ninguno |
| Legacy | `V5GeneId.RapidDivision` |
| Madre | costo de division -25%, stress por division +8% |
| Produccion | bacteria swarm eficiente |
| Cuerpo | microcolonias visuales mas densas |
| Identidad | +Bacteria swarm |
| Sinergias | Flagelo bacteriano, Quorum sensing |
| Conflictos | Homogeneidad atrae fagos |

## T1_ProkaryoticThylakoid - Tilacoide procariota

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 22/10/4/5/3/2 |
| Tiempo | 12s |
| Tags | Fotosintesis, Luz, Cianobacteria, Oxigeno |
| Requisitos | ninguno |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Thylakoid` |
| Madre | +ATP en luz, produce O2 local |
| Produccion | cianobacteria productora |
| Cuerpo | productora solar exterior |
| Identidad | +Cianobacteria |
| Sinergias | Catalasa, Vesicula de gas, Heterocisto |
| Conflictos | sombra reduce 50% efecto |
| Descuento | -35% costo con 60s en luz alta |

## T1_Heterocyst - Heterocisto

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 24/14/6/4/5/2 |
| Tiempo | 14s |
| Tags | Sacrificio, Filamento, Anabaena, Nitrogeno |
| Requisitos | Tilacoide procariota |
| Bloquea | cerrado por Nucleo |
| Legacy | nuevo |
| Madre | convierte parte de produccion en biomasa/AA estable |
| Produccion | heterocisto, filamento Anabaena |
| Cuerpo | slots sacrificados de cadena |
| Identidad | +Anabaena |
| Sinergias | Adhesion filamentosa/Pili, Vesicula de gas |
| Conflictos | cortes de filamento reducen economia |

## T1_GasVesicle - Vesicula de gas

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 18/10/2/6/2/1 |
| Tiempo | 10s |
| Tags | Flotacion, Luz, Cianobacteria |
| Requisitos | Tilacoide o Cianobacteria tendencia |
| Bloquea | cerrado por Nucleo |
| Legacy | nuevo |
| Madre | cambia preferencia hacia zonas luminosas, +movilidad en corriente |
| Produccion | ciano flotante |
| Cuerpo | slots flotantes/productores |
| Identidad | +Ciano flotante |
| Sinergias | Tilacoide, Eyespot si hibrido |
| Conflictos | grazers filtradores la priorizan |
| Descuento | -35% costo con 60s en luz alta |

## T1_ExtremophileMembrane - Membrana extremofila

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 20/10/2/8/2/4 |
| Tiempo | 11s |
| Tags | Arquea, Resistencia, Extremo |
| Requisitos | ninguno |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Capsule` temporal si falta visual |
| Madre | +30% resistencia termica/acida/osmotica |
| Produccion | arquea resistente |
| Cuerpo | defensa en zonas extremas |
| Identidad | +Arquea |
| Sinergias | Bomba de protones, Quimiolitotrofia legacy |
| Conflictos | division -8% velocidad |
| Descuento | -35% tras crisis ambiental |

## T1_ProtonPump - Bomba de protones

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 22/10/3/4/4/6 |
| Tiempo | 12s |
| Tags | Acido, Terraformacion, Arquea, Quimio |
| Requisitos | Membrana extremofila o 60s en acidez |
| Bloquea | ninguno |
| Legacy | metabolismo Chemolithotrophy |
| Madre | acidifica zona, +ATP/minerales en acidez |
| Produccion | arquea acidofila |
| Cuerpo | bolsa acida defensiva |
| Identidad | +Arquea acidofila |
| Sinergias | Membrana extremofila |
| Conflictos | dania aliados no adaptados si estan demasiado cerca |
| Descuento | -35% con 60s en pH bajo |

---

# 4. T2 - Hitos de Endosimbiosis

## T2_Nucleus - Nucleo

| Campo | Valor |
| --- | --- |
| Tipo | Hito irreversible |
| Prioridad | P0 |
| Costo | 45/32/8/10/18/4 |
| Tiempo | 22s |
| Tags | Eucariota, Complejidad, Hito |
| Requisitos | 3 adaptaciones T1 o evento endosimbiotico |
| Bloquea | nuevas exclusivas procariotas, Procariota Apex nuevo |
| Legacy | `V5StructureId.GeneticCompartment` |
| Madre | +25% HP, +1 cap interno de complejidad |
| Produccion | hija eucariota basica |
| Cuerpo | habilita slots internos |
| Identidad | +Protista base, +Eucariota |
| Sinergias | Mitocondria, Cloroplasto |
| Conflictos | cierra Plasmido/Heterocisto/Vesicula nuevas |
| Confirmacion | obligatoria |

## T2_Mitochondria - Mitocondria

| Campo | Valor |
| --- | --- |
| Tipo | Hito energetico |
| Prioridad | P0 |
| Costo | 36/24/8/10/12/3 |
| Tiempo | 18s |
| Tags | Energia, Eucariota, Respiracion |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.MetabolicEngine` + Respiration |
| Madre | +25% ATP max, +0.8 ATP/s si O2 suficiente |
| Produccion | eucariotas heterotrofas |
| Cuerpo | productoras internas |
| Identidad | +Protista base, +Predador potencial |
| Sinergias | Lisosoma, Cilios, Tejido contractil |
| Conflictos | acidez extrema reduce eficiencia sin membrana |
| Descuento | -25% costo con 60s en O2 alto |

## T2_Chloroplast - Cloroplasto

| Campo | Valor |
| --- | --- |
| Tipo | Hito productor |
| Prioridad | P0 |
| Costo | 42/28/8/12/12/6 |
| Tiempo | 20s |
| Tags | Fotosintesis, Eucariota, Productor, Luz |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.MicroalgalChloroplast` |
| Madre | +ATP/Bio en luz, O2 local |
| Produccion | microalga base |
| Cuerpo | productoras en ring medio/exterior |
| Identidad | +Microalga, +Euglena/Volvox potencial |
| Sinergias | Pared celulosica, Eyespot, Adhesina colonial |
| Conflictos | predador puro paga +50% si instala tarde |
| Descuento | -35% costo con 60s en luz alta |

## T2_ActiveEndosymbiosis - Endosimbiosis activa

| Campo | Valor |
| --- | --- |
| Tipo | Hito avanzado |
| Prioridad | P2 |
| Costo | 70/45/18/18/25/10 |
| Tiempo | 28s |
| Tags | Simbiosis, Complejidad, Hibrido |
| Requisitos | Nucleo + Mitocondria o Cloroplasto |
| Bloquea | volver a procariota conceptual |
| Legacy | nuevo |
| Madre | puede convertir una hija/simbionte en bono organelar |
| Produccion | endosimbiontes especializados |
| Cuerpo | slots simbiontes internos |
| Identidad | +Endosimbionte avanzado, +Liquen/Euglena |
| Sinergias | Cloroplasto, Hifa, Simbiosis |
| Conflictos | alto stress si hay demasiada hibridacion |

---

# 5. T3 - Eucariota Predador

## T3_Lysosome - Lisosoma

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 30/18/8/6/6/1 |
| Tiempo | 12s |
| Tags | Predador, Digestion, Fagocitosis, Ameba |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Lysosome` |
| Madre | +0.9 dano contacto, habilita fagocitosis |
| Produccion | cazadora ameboide |
| Cuerpo | boca/contacto exterior |
| Identidad | +Ameba, +Predador |
| Sinergias | Pseudopodos, Vacuola, Citostoma |
| Conflictos | productor puro paga +50% si instala tarde |

## T3_Pseudopods - Pseudopodos

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 28/18/7/8/4/0 |
| Tiempo | 12s |
| Tags | MovimientoBlando, Ameba, Contacto |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | nuevo o `StemPlasticity` temporal |
| Madre | +movilidad adaptable, +agarre de presas |
| Produccion | ameba movil, guardia blanda |
| Cuerpo | slots de contacto flexible |
| Identidad | +Ameba |
| Sinergias | Lisosoma, Vacuola |
| Conflictos | Pared rigida y Frustula reducen efecto 35% |

## T3_ContractileVacuole - Vacuola contractil

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 26/16/4/8/5/1 |
| Tiempo | 11s |
| Tags | Osmosis, Sustain, Ameba, Ciliado |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.StorageVacuole` temporal |
| Madre | +0.45 HP/s, -12 stress osmotic |
| Produccion | ameba resistente, ciliado estable |
| Cuerpo | slot interno de osmorregulacion |
| Identidad | +Ameba avanzada, +Ciliado |
| Sinergias | Lisosoma, Cilios |
| Conflictos | ninguno |

## T3_Cilia - Cilios

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 32/20/10/8/5/1 |
| Tiempo | 13s |
| Tags | Ciliado, Control, MovimientoFino, Filtracion |
| Requisitos | Nucleo + Mitocondria |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Cilia` |
| Madre | +control de movimiento, empuje leve de particulas |
| Produccion | controlador ciliado, filtrador |
| Cuerpo | corona/capa motora exterior |
| Identidad | +Ciliado, +Rotifero potencial |
| Sinergias | Vacuola, Citostoma, Comunicacion |
| Conflictos | toxinas persistentes reducen control |

## T3_Cytostome - Citostoma

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 38/24/10/8/8/2 |
| Tiempo | 15s |
| Tags | Boca, Fagocitosis, Ciliado, Predador |
| Requisitos | Nucleo + Lisosoma o Cilios |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | +35% eficiencia fagocitosis, ataque frontal |
| Produccion | devoradora, Paramecium, Stentor |
| Cuerpo | boca fija dirigida |
| Identidad | +Ameba voraz, +Ciliado predador |
| Sinergias | Vacuola, Cilios, Tamano grande futuro |
| Conflictos | Frustula limita orientacion |

## T3_Toxicysts - Toxocistos

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 36/22/10/8/8/4 |
| Tiempo | 14s |
| Tags | Toxina, Burst, Predador, Dinoflagelado |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.SecretoryVesicle` temporal |
| Madre | pulso toxico corto, +dano quimico |
| Produccion | toxica, predador quimico |
| Cuerpo | tentaculo/cnidocito futuro |
| Identidad | +Dinoflagelado, +Hidra potencial |
| Sinergias | Lisosoma, Bioluminiscencia |
| Conflictos | alta toxicidad atrae arqueas resistentes |

## T3_SuctorialTentacles - Tentaculos suctores

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P2 |
| Costo | 40/24/12/8/8/2 |
| Tiempo | 16s |
| Tags | Suctoria, Drenaje, Estacionario |
| Requisitos | Cilios + Citostoma |
| Bloquea | movilidad ofensiva plena mientras anclada |
| Legacy | nuevo |
| Madre | drenaje a distancia si anclada |
| Produccion | suctoria |
| Cuerpo | torreta biologica |
| Identidad | +Suctoria |
| Sinergias | Anclaje/Pie retractil |
| Conflictos | baja eficacia moviendose |

## T3_ChemicalSensor - Sensor quimico

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 26/16/8/4/8/2 |
| Tiempo | 11s |
| Tags | Reconocimiento, Caza, Sensor |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.RecognitionReceptors` |
| Madre | +3 sensor, detecta presas/amenazas |
| Produccion | exploradora sensorial |
| Cuerpo | slot sensor |
| Identidad | +Lacrymaria, +Nematodo potencial |
| Sinergias | Citostoma, Estilete |
| Conflictos | Capsule enemiga reduce eficacia |

---

# 6. T3 - Eucariota Productor

## T3_CelluloseWall - Pared celulosica

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 26/18/4/8/4/3 |
| Tiempo | 12s |
| Tags | Productor, Defensa, Microalga |
| Requisitos | Cloroplasto o Nucleo + tendencia productor |
| Bloquea | Pseudopodos a eficacia reducida |
| Legacy | `V5StructureId.CelluloseWall` |
| Madre | +20% HP, +10% defensa |
| Produccion | microalga estable |
| Cuerpo | productora/defensora exterior |
| Identidad | +Microalga |
| Sinergias | Cloroplasto, Adhesina colonial |
| Conflictos | Movimiento blando -25% |

## T3_Eyespot - Eyespot

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 24/14/6/4/6/1 |
| Tiempo | 10s |
| Tags | Luz, Sensor, Motilidad, Euglena |
| Requisitos | Cloroplasto o Tilacoide |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | busca luz optima, +vision de luz |
| Produccion | fototactica, euglena |
| Cuerpo | sensor luminoso |
| Identidad | +Euglena, +Ciano flotante |
| Sinergias | Flagelo eucariota, Cloroplasto |
| Conflictos | oscuridad reduce utilidad |

## T3_EukaryoticFlagellum - Flagelo eucariota

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 32/20/10/8/5/2 |
| Tiempo | 13s |
| Tags | Motilidad, Eucariota, Euglena, Volvox |
| Requisitos | Nucleo |
| Bloquea | ninguno |
| Legacy | `V5StructureId.EukaryoticFlagellum` |
| Madre | +24% velocidad, +1 sensor |
| Produccion | eucariota movil |
| Cuerpo | motora externa |
| Identidad | +Euglena, +Volvox temprano |
| Sinergias | Eyespot, Cloroplasto, Adhesina colonial |
| Conflictos | Frustula reduce velocidad |

## T3_SilicaFrustule - Frustula de silice

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 34/22/4/6/6/12 |
| Tiempo | 16s |
| Tags | Defensa, Diatomea, Silice, Productor |
| Requisitos | Cloroplasto |
| Bloquea | ninguno |
| Legacy | nuevo o `CelluloseWall` + stat custom |
| Madre | +45% defensa, -18% velocidad |
| Produccion | diatomea acorazada |
| Cuerpo | coraza exterior |
| Identidad | +Diatomea |
| Sinergias | Captacion de silice, Catalasa |
| Conflictos | movilidad libre reducida; perforacion la counterea |

## T3_SilicaUptake - Captacion de silice

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 28/16/4/4/6/14 |
| Tiempo | 14s |
| Tags | Mineral, Diatomea, Fortaleza |
| Requisitos | Frustula de silice |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | +20% eficiencia de frustula, consume Min para repararse |
| Produccion | diatomea fortaleza |
| Cuerpo | coraza reparable |
| Identidad | +Diatomea fortaleza |
| Sinergias | Frustula, Catalasa |
| Conflictos | dependencia mineral |

## T3_CatalaseROS - Catalasa / defensa ROS

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 24/14/6/4/4/3 |
| Tiempo | 10s |
| Tags | Oxigeno, Proteccion, Fotosintesis |
| Requisitos | O2 alto o fotosintesis o Mitocondria |
| Bloquea | ninguno |
| Legacy | `V5StructureId.Catalase` |
| Madre | +25% resistencia oxidativa/toxina |
| Produccion | soporte antioxidante |
| Cuerpo | defensa quimica |
| Identidad | +Cianobacteria eficiente, +Microalga |
| Sinergias | Cloroplasto, Tilacoide, Frustula |
| Conflictos | ninguno |
| Descuento | -25% con 60s en O2 alto |

## T3_Bioluminescence - Bioluminiscencia

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 30/18/8/6/8/3 |
| Tiempo | 12s |
| Tags | Senal, Stress, Dinoflagelado |
| Requisitos | Cloroplasto o Toxocistos |
| Bloquea | sigilo completo |
| Legacy | nuevo |
| Madre | revela amenazas cercanas al recibir dano |
| Produccion | dinoflagelado senalador |
| Cuerpo | alarma luminica |
| Identidad | +Dinoflagelado |
| Sinergias | Toxocistos |
| Conflictos | revela posicion bajo stress |

## T3_ColonialAdhesin - Adhesina colonial

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 30/20/8/8/8/2 |
| Tiempo | 14s |
| Tags | Colonia, Productor, Volvox |
| Requisitos | Cloroplasto + Adesina basica |
| Bloquea | ninguno |
| Legacy | nuevo o body flag |
| Madre | hijas productoras se agrupan con menos stress |
| Produccion | colonia Volvox temprana |
| Cuerpo | forma esferica simple |
| Identidad | +Volvox temprano |
| Sinergias | Flagelo eucariota, Diferenciacion |
| Conflictos | baja sin luz |

---

# 7. T3 - Red y Descomponedores

## T3_FungalHypha - Hifa fungica

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 32/22/8/8/6/4 |
| Tiempo | 15s |
| Tags | Red, Hongo, Detritus |
| Requisitos | Nucleo |
| Bloquea | cuerpo hexagonal estandar como forma dominante mientras red activa |
| Legacy | `V5StructureId.InvasiveHypha` |
| Madre | absorbe detritus en area, baja velocidad |
| Produccion | nodo hifal |
| Cuerpo | red de nodos |
| Identidad | +Red hifal inmadura; Hongo requiere Enzimas extracelulares |
| Sinergias | Quitina, Enzimas, Simbiosis |
| Conflictos | Nematodo/perforadores |

## T3_ChitinSynthase - Quitina sintasa

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 28/18/6/8/4/6 |
| Tiempo | 12s |
| Tags | Defensa, Hongo, Red |
| Requisitos | Hifa fungica |
| Bloquea | ninguno |
| Legacy | nuevo o `CelluloseWall` temporal |
| Madre | +25% defensa red/hifas |
| Produccion | hifa defensiva |
| Cuerpo | nodos resistentes |
| Identidad | +Hongo avanzado |
| Sinergias | Hifa, Enzimas |
| Conflictos | movilidad baja |

## T3_ExtracellularEnzymes - Enzimas extracelulares

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 20/12/7/3/4/2 |
| Tiempo | 13s |
| Tags | DigestionExterna, Reciclaje, Hongo |
| Requisitos | Hifa fungica |
| Bloquea | ninguno |
| Legacy | `V5StructureId.SecretoryVesicle` temporal |
| Madre | digiere detritus/cadaveres en area |
| Produccion | recicladora/digestora |
| Cuerpo | red digestiva |
| Identidad | +Hongo saprofito, +Moho |
| Sinergias | Hifa, Memoria quimica |
| Conflictos | atrae competidores saprofitos |
| Tier visual | 2 - aura digestiva/red activa |
| Latente Champion | No |
| Counter natural | Diatomea acorazada, corte de hifas, zonas pobres en detritus |
| Telemetria | pickrate, detritus convertido, supervivencia con/sin Enzimas |

## T3_SlimePlasmodium - Plasmodio mucilaginoso

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 34/24/8/10/8/3 |
| Tiempo | 16s |
| Tags | RedMovil, Moho, Reconfiguracion |
| Requisitos | Nucleo |
| Bloquea | cuerpo hexagonal estandar mientras plasmodio activo |
| Legacy | `V5StructureId.MucilageMatrix` |
| Madre | comparte recursos entre nodos cercanos |
| Produccion | nodo plasmodial |
| Cuerpo | pseudoplasmodio movil |
| Identidad | +Moho mucilaginoso |
| Sinergias | Memoria quimica, Enzimas |
| Conflictos | toxinas de zona, cortes multiples |

## T3_ChemicalMemory - Memoria quimica

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 30/18/6/4/10/2 |
| Tiempo | 14s |
| Tags | Navegacion, Recursos, Moho, Memoria |
| Requisitos | Plasmodio o 3 marcadores quimicos |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | recuerda zonas ricas 120s, vuelve a recursos |
| Produccion | exploradora quimica |
| Cuerpo | rutas plasmodiales optimizadas |
| Identidad | +Physarum |
| Sinergias | Plasmodio, Enzimas |
| Conflictos | sequia/toxinas borran rastros |
| Descuento | -30% con 3 marcadores quimicos |

## T3_StarvationSignal - Senal de hambre

| Campo | Valor |
| --- | --- |
| Tipo | Activa/Estado |
| Prioridad | P2 |
| Costo | 24/14/6/4/8/1 |
| Tiempo | 10s |
| Tags | Crisis, Agregacion, Dictyostelium |
| Requisitos | Nucleo + stress o baja biomasa |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | activa agregacion cuando recursos bajos |
| Produccion | celula agregativa |
| Cuerpo | pseudoplasmodio temporal |
| Identidad | +Dictyostelium |
| Sinergias | Esporas |
| Conflictos | combate directo bajo |

## T3_Spores - Esporas

| Campo | Valor |
| --- | --- |
| Tipo | Estado/Activa |
| Prioridad | P1 |
| Costo | 28/16/6/8/8/2 |
| Tiempo | 12s |
| Tags | Continuidad, Supervivencia, Hongo, Moho |
| Requisitos | Hifa o Senal de hambre |
| Bloquea | ninguno |
| Legacy | nuevo |
| Madre | al morir/red colapsar deja continuidad parcial |
| Produccion | espora resistente |
| Cuerpo | puntos de rebrote |
| Identidad | +Hongo/Dictyostelium resiliente |
| Sinergias | Sucesion colonial |
| Conflictos | baja produccion activa mientras esporula |

---

# 8. T4 - Multicelularidad

## T4_PersistentAdhesion - Adhesion Persistente

| Campo | Valor |
| --- | --- |
| Tipo | Hito irreversible |
| Prioridad | P0 |
| Costo | 50/34/12/14/12/4 |
| Tiempo | 22s |
| Tags | Multicelular, Cuerpo, Hito |
| Requisitos | Adesina basica + 4 adaptaciones T3 o identidad colonial |
| Bloquea | squads 100% libres como estilo principal |
| Legacy | body flag |
| Madre | divisiones pueden nacer integradas |
| Produccion | castas corporales iniciales |
| Cuerpo | cuerpo multicelular real |
| Identidad | +Multicelular |
| Sinergias | Diferenciacion, Comunicacion |
| Conflictos | Husk Drop pasa a ruptura parcial |
| Confirmacion | obligatoria |

## T4_CellDifferentiation - Diferenciacion celular

| Campo | Valor |
| --- | --- |
| Tipo | Hito funcional |
| Prioridad | P0 |
| Costo | 42/28/14/10/16/4 |
| Tiempo | 18s |
| Tags | Castas, Produccion, Multicelular |
| Requisitos | Adhesion Persistente o Volvox temprano |
| Bloquea | ninguno |
| Legacy | germinal caste unlock |
| Madre | hijas pueden nacer con roles distintos |
| Produccion | somatica, germinal, conectora |
| Cuerpo | slots especializados |
| Identidad | +Volvox completo, +Microfauna |
| Sinergias | Comunicacion, Especializacion por capa |
| Conflictos | stress si faltan recursos |

## T4_SignalingCommunication - Comunicacion por senales

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P0 |
| Costo | 38/24/12/8/14/3 |
| Tiempo | 16s |
| Tags | Coordinacion, Cuerpo, Multicelular |
| Requisitos | Adesina basica + 4 celulas o Adhesion Persistente |
| Bloquea | ninguno |
| Legacy | new/body coordination |
| Madre | modos corporales sincronizan mejor |
| Produccion | coordinadora/senalizadora |
| Cuerpo | +20% eficiencia slots adheridos |
| Identidad | +Volvox, +Rotifero, +Hidra |
| Sinergias | Diferenciacion, Cilios |
| Conflictos | Capsule reduce reconocimiento aliado |

## T4_ExtracellularMatrix - Matriz extracelular

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 40/28/10/14/10/6 |
| Tiempo | 17s |
| Tags | Forma, Defensa, Multicelular |
| Requisitos | Adhesion Persistente |
| Bloquea | ninguno |
| Legacy | body defense flag |
| Madre | -10% velocidad, +20% defensa cuerpo |
| Produccion | conectora matriz |
| Cuerpo | forma estable |
| Identidad | +Volvox, +Esponja futura |
| Sinergias | Especializacion por capa |
| Conflictos | movilidad reducida |

## T4_LayerSpecialization - Especializacion por capa

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 46/32/14/12/18/5 |
| Tiempo | 20s |
| Tags | Capas, Volvox, Rotifero |
| Requisitos | Diferenciacion + Comunicacion |
| Bloquea | ninguno |
| Legacy | body slot specialization |
| Madre | exterior/interior tienen reglas distintas |
| Produccion | somatica externa, germinal interna |
| Cuerpo | capas funcionales |
| Identidad | +Volvox completo, +Rotifero |
| Sinergias | Matriz, Cloroplasto |
| Conflictos | dano penetrante la counterea |

## T4_PolarizedEpithelium - Epitelio polarizado

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 44/30/14/12/16/4 |
| Tiempo | 19s |
| Tags | EjeCorporal, Microfauna, Hidra, Rotifero |
| Requisitos | Adhesion Persistente + Diferenciacion |
| Bloquea | cuerpo completamente radial |
| Legacy | body shape flag |
| Madre | define frente/dorso o boca/pie |
| Produccion | segmentos polarizados |
| Cuerpo | eje corporal |
| Identidad | +Hidra, +Rotifero, +Nematodo |
| Sinergias | Tejido contractil, Mastax, Pie retractil |
| Conflictos | baja compatibilidad con Volvox esferico |

## T4_ContractileTissue - Tejido contractil

| Campo | Valor |
| --- | --- |
| Tipo | Activa |
| Prioridad | P1 |
| Costo | 48/32/16/14/16/5 |
| Tiempo | 20s |
| Tags | MovimientoCorporal, Hidra, Nematodo |
| Requisitos | Mitocondria + Epitelio polarizado |
| Bloquea | ninguno |
| Legacy | body movement flag |
| Madre | cuerpo se mueve como unidad con mas fuerza |
| Produccion | segmento contractil |
| Cuerpo | locomocion multicelular |
| Identidad | +Hidra, +Nematodo |
| Sinergias | Mitocondria, Sistema nervioso difuso |
| Conflictos | energia alta requerida |

## T4_DiffuseNerveNet - Sistema nervioso difuso

| Campo | Valor |
| --- | --- |
| Tipo | Activa avanzada |
| Prioridad | P2 |
| Costo | 60/40/20/16/24/8 |
| Tiempo | 26s |
| Tags | Reaccion, Hidra, Apex |
| Requisitos | Comunicacion + Tejido contractil |
| Bloquea | ninguno |
| Legacy | new |
| Madre | reacciones defensivas automaticas |
| Produccion | celula sensorial difusa |
| Cuerpo | reflejos simples |
| Identidad | +Hidra apex |
| Sinergias | Toxocistos, Regeneracion futura |
| Conflictos | alto costo energetico |

## T4_Mastax - Mastax

| Campo | Valor |
| --- | --- |
| Tipo | Activa microfauna |
| Prioridad | P1 |
| Costo | 46/32/16/12/16/6 |
| Tiempo | 19s |
| Tags | Trituracion, Rotifero, Microfauna |
| Requisitos | Cilios + Epitelio polarizado |
| Bloquea | ninguno |
| Legacy | new |
| Madre | tritura presas pequenas |
| Produccion | mastax/rotifero |
| Cuerpo | boca interna rotifera |
| Identidad | +Rotifero |
| Sinergias | Corona ciliada/Cilios, Pie retractil |
| Conflictos | inutil contra presas grandes sin soporte |

## T4_Cuticle - Cuticula

| Campo | Valor |
| --- | --- |
| Tipo | Activa microfauna |
| Prioridad | P1 |
| Costo | 44/30/12/14/14/8 |
| Tiempo | 18s |
| Tags | Defensa, Nematodo, Cripto, Microfauna |
| Requisitos | Adhesion Persistente o Epitelio polarizado |
| Bloquea | Pseudopodos blandos |
| Legacy | `V5StructureId.Cuticle` |
| Madre | +30% defensa fisica/osmotica, -8% velocidad |
| Produccion | segmento cuticular |
| Cuerpo | armadura lineal |
| Identidad | +Nematodo, +Criptobiosis |
| Sinergias | Estilete, Criptobiosis |
| Conflictos | movimiento blando reducido |

## T4_Stylet - Estilete

| Campo | Valor |
| --- | --- |
| Tipo | Activa ofensiva |
| Prioridad | P1 |
| Costo | 48/30/16/10/16/10 |
| Tiempo | 20s |
| Tags | Perforacion, Nematodo, AntiRed |
| Requisitos | Cuticula + Sensor quimico |
| Bloquea | ninguno |
| Legacy | `V5StructureId.PiercingStylet` |
| Madre | dano penetrante contra coraza/red |
| Produccion | nematodo perforador |
| Cuerpo | punta lineal |
| Identidad | +Nematodo |
| Sinergias | Tejido contractil |
| Conflictos | baja eficacia contra swarms dispersos |

## T4_RetractileFoot - Pie retractil

| Campo | Valor |
| --- | --- |
| Tipo | Activa control |
| Prioridad | P1 |
| Costo | 38/26/12/10/12/4 |
| Tiempo | 16s |
| Tags | Anclaje, Rotifero, Control |
| Requisitos | Epitelio polarizado |
| Bloquea | ninguno |
| Legacy | new |
| Madre | anclarse/soltarse rapido |
| Produccion | pie rotifero |
| Cuerpo | anclaje defensivo |
| Identidad | +Rotifero |
| Sinergias | Mastax, Cilios |
| Conflictos | menos valor en mapas sin estructuras |

---

# 9. Estados Especiales

## S_Cryptobiosis - Criptobiosis

| Campo | Valor |
| --- | --- |
| Tipo | Estado |
| Prioridad | P1 |
| Costo | 36/22/8/10/10/6 |
| Tiempo | 14s |
| Tags | Supervivencia, Dormancia, Tardigrado |
| Requisitos | Cuticula o alta resistencia osmotica |
| Bloquea | produccion mientras esta en tun |
| Legacy | `V5StructureId.CryptobiosisTun` |
| Madre | puede entrar en tun: casi invulnerable, inactiva |
| Produccion | no produce ejercito propio |
| Cuerpo | continuidad ante crisis |
| Identidad | estado Cripto/Tardigrado |
| Sinergias | Sucesion colonial |
| Conflictos | no gana terreno |

## S_Cyst - Quiste

| Campo | Valor |
| --- | --- |
| Tipo | Estado |
| Prioridad | P2 |
| Costo | 20/12/4/6/6/2 |
| Tiempo | 8s |
| Tags | Dormancia, Continuidad |
| Requisitos | Nucleo + stress alto |
| Bloquea | movimiento/produccion mientras esta activo |
| Legacy | new |
| Madre | pausa defensiva temporal |
| Produccion | quiste viable |
| Cuerpo | protege sucesor |
| Identidad | ninguna, estado transversal |
| Sinergias | Sucesion colonial |
| Conflictos | perdida de tempo |

## S_BiologicalChampion - Champion biologico

| Campo | Valor |
| --- | --- |
| Tipo | Estado/Apex temporal |
| Prioridad | P0 existente |
| Costo | automatico |
| Tiempo | 25s activo |
| Tags | Comeback, Crisis |
| Requisitos | cuerpo colapsa + madre viva + anti-cheese ok |
| Bloquea | abuso por Husk Drop reciente |
| Legacy | sistema actual de Champion |
| Madre | buff temporal de supervivencia |
| Produccion | ninguna directa |
| Cuerpo | emergencia tras nucleo expuesto |
| Identidad | no cambia especie |
| Estructuras latentes | activa efectos Champion definidos en adaptaciones marcadas como latentes |
| Regla | si una adaptacion tiene `Latente Champion = Si`, su `Efecto Champion` se aplica solo durante la ventana de nucleo expuesto |
| Ejemplo | Pseudopodos: dano contacto normal; en Champion puede golpear 3 amenazas cercanas |
| Sinergias | cuerpo completo previo |
| Conflictos | cooldown y anti-cheese |

## Campos nuevos v5.14.3

Todas las fichas pueden declarar estos campos sin cambiar su logica base:

| Campo | Uso |
| --- | --- |
| Tier visual | Prioridad artistica: 1 sutil, 2 visible, 3 dramatico, 4 transformacion grande |
| Latente Champion | Si/No; activa una regla extra solo durante Champion/Nucleo Expuesto |
| Efecto Champion | Texto programable del efecto latente |
| Counter natural | Amenazas o adaptaciones que el Director puede usar como respuesta |
| Telemetria | metricas iniciales para balance: pickrate, tiempo, winrate, supervivencia, uso real |

---

# 10. Identidades Reconocidas

## 10.1 Reglas minimas

| Identidad | Requisitos |
| --- | --- |
| Bacteria swarm | Pared + Flagelo + Division rapida, sin Nucleo |
| Biofilm | Pared + Capsule + Pili/Adesina |
| Myxococcus | Quorum + Pili + Caza grupal/Swarm |
| Espirilo | P2: Flagelo + Cuerpo helicoidal/flagelo axial |
| Bdellovibrio | P2: Flagelo + Invasion bacteriana + digestion interna |
| Arquea | Membrana extremofila + Bomba protones |
| Cianobacteria | Tilacoide + Pared/Catalasa |
| Anabaena | Tilacoide + Heterocisto + adhesion filamentosa |
| Protista base | Nucleo + Mitocondria |
| Ameba | Nucleo + Mitocondria + Lisosoma + Pseudopodos |
| Ciliado | Nucleo + Mitocondria + Cilios + Vacuola |
| Microalga | Nucleo + Cloroplasto + Pared celulosica |
| Diatomea | Nucleo + Cloroplasto + Frustula |
| Euglena | Nucleo + Mitocondria + Cloroplasto + Flagelo + Eyespot |
| Dinoflagelado | Nucleo + Cloroplasto/Toxocistos + Bioluminiscencia |
| Hongo | Nucleo + Hifa + Enzimas; Quitina mejora defensa P1 |
| Moho | Nucleo + Plasmodio + Memoria |
| Volvox temprano | Cloroplasto + Adhesina colonial + Flagelo eucariota |
| Volvox completo | Volvox temprano + Adhesion Persistente + Diferenciacion + Capas |
| Rotifero | Adhesion Persistente + Cilios + Epitelio + Mastax |
| Nematodo | Adhesion Persistente + Cuticula + Estilete + Tejido contractil |

## 10.2 Prioridad

1. Apex activo.
2. Multicelular completo.
3. Red especializada.
4. Subruta especifica.
5. Ruta principal.
6. Tendencia.
7. LUCA/protista base.

---

# 11. Primer Corte Cerrado P0

Estas son las adaptaciones que deben entrar primero para validar el sistema:

| ID | Nombre |
| --- | --- |
| T1_BacterialWall | Pared bacteriana |
| T1_BacterialFlagellum | Flagelo bacteriano |
| T1_PolysaccharideCapsule | Capsule polisacarida |
| T1_PiliFimbriae | Pili/Fimbrias |
| T1_BasicAdhesin | Adesina basica |
| T1_RapidDivision | Division rapida |
| T1_ProkaryoticThylakoid | Tilacoide procariota |
| T1_ExtremophileMembrane | Membrana extremofila |
| T1_ProtonPump | Bomba de protones |
| T2_Nucleus | Nucleo |
| T2_Mitochondria | Mitocondria |
| T2_Chloroplast | Cloroplasto |
| T3_Lysosome | Lisosoma |
| T3_Pseudopods | Pseudopodos |
| T3_ContractileVacuole | Vacuola contractil |
| T3_Cilia | Cilios |
| T3_EukaryoticFlagellum | Flagelo eucariota |
| T3_CelluloseWall | Pared celulosica |
| T3_SilicaFrustule | Frustula de silice |
| T3_CatalaseROS | Catalasa / ROS |
| T3_ColonialAdhesin | Adhesina colonial |
| T3_FungalHypha | Hifa fungica |
| T3_ExtracellularEnzymes | Enzimas extracelulares |
| T3_SlimePlasmodium | Plasmodio mucilaginoso |
| T3_ChemicalMemory | Memoria quimica |
| T4_PersistentAdhesion | Adhesion Persistente |
| T4_CellDifferentiation | Diferenciacion celular |
| T4_SignalingCommunication | Comunicacion por senales |
| S_BiologicalChampion | Champion biologico |

Identidades que P0 debe reconocer:

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
Moho
Volvox temprano
Volvox completo parcial
```

---

# 12. Backlog P1/P2

P1:

```text
Plasmido, Quorum, Heterocisto, Vesicula de gas,
Citostoma, Toxocistos, Sensor quimico, Captacion de silice,
Bioluminiscencia, Quitina, Esporas,
Matriz, Capas, Epitelio, Mastax, Cuticula, Estilete, Pie.
```

P2:

```text
Endosimbiosis activa, Tentaculos suctores, Senal de hambre,
Sistema nervioso difuso, Quiste, Cuerpo helicoidal/flagelo axial,
Invasion bacteriana, Bdellovibrio avanzado, Espirilo completo,
Hidra completa, Nematodo completo.
```

---

# 13. Nota Final de Implementacion

Este catalogo es "definitivo" en estructura canonica, pero los numeros son balance inicial. Se pueden ajustar costos y porcentajes durante playtest sin cambiar el modelo.

Lo que no debe cambiar sin reabrir diseno:

```text
Adaptaciones son fuente de verdad.
Hitos no cuentan contra cap.
Cap base 14; Champion/apex eleva cap a 16.
Adesina basica no es Adhesion Persistente.
Rutas son identidades emergentes.
G es Panel del Genoma.
E no compite con G como progreso principal.
```
