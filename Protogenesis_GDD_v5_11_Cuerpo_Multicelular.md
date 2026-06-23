**PROTOGENESIS: PRIMORDIA**

Game Design Document v5.11

Biological RTS with Multicellular Body, Champion Mother and Genome Lab

"No comandás unidades: programás un linaje vivo que crece como cuerpo multicelular, despliega tropas y revela su célula germinal como campeón final cuando el cuerpo cae."

| Versión | v5.11 — Cuerpo multicelular y campeón latente |
| --- | --- |
| Fecha | 2026-05-04 |
| Formato | Documento de diseño para producción |
| Base de trabajo | GDD v5.10 + visión de cuerpo multicelular hexagonal y madre como campeón |
| Cambio canónico | La madre construye un cuerpo multicelular vía adhesión funcional, balancea entre estados (Cuerpo Completo / Cuerpo Parcial / Núcleo Expuesto), y sus estructuras de combate latentes se activan cuando el traje cae |
| Estado | Documento integrado: la mecánica de cuerpo multicelular reemplaza al modelo de "madre-nexo solo" del v5.10 sin perder los sistemas programables ni el Genome Lab |

# 0. Índice funcional

Este índice es una guía estática. La versión v5.11 integra el sistema de cuerpo multicelular con balance dinámico y madre como campeón latente, manteniendo el Genome Lab, las recetas de fenotipo y el historial de afinidad de v5.10.

1. Visión y cambio de dirección
2. Pilares de diseño
3. Experiencia objetivo
4. Modos de juego
5. Core loop
6. Estructura de partida
7. Las tres capas jugables
8. Modelo de datos de célula
9. Recursos y economía
10. Sistema de ambiente/ecosistema
11. Builder interior celular
12. Metabolismo y bifurcación
13. División celular y linajes
14. Directivas RTS y modos celulares
15. Árbol evolutivo y genes
16. Evolution Affinity Tracker (con memoria causal v5.9)
17. Rutas evolutivas completas
18. Fichas de las 12 rutas principales
19. Formas apex y campeón latente
20. Combate, capas y daño penetrante
21. Colonización y world building
22. IA y organismos salvajes
23. UI/UX y Genome Lab
24. Cámara, zoom y lectura táctica
25. Arte, sonido y feedback
26. Balance numérico inicial
27. Campaña, escenarios y progresión
28. Multiplayer y PvP
29. Roadmap de implementación
30. Apéndices de datos
31. Sistema de cuerpo multicelular (canon v5.11)
32. Catálogo completo de estructuras
33. Catálogo de proteínas, enzimas y toxinas
34. Balance dinámico de la madre y transiciones
35. Métricas, riesgos y mitigaciones

---

# 1. Visión y cambio de dirección

Protogenesis: Primordia v5.11 mantiene los sistemas programables y el Genome Lab del v5.10, pero el centro emocional del juego cambia: **la madre deja de ser solo un nexo móvil vulnerable y pasa a ser una célula germinal que construye un cuerpo multicelular alrededor de sí misma**. Ese cuerpo es la mecánica core que define cómo se siente cada partida. Cuando el cuerpo cae, la madre revela sus estructuras de combate latentes y se vuelve un campeón individual.

El jugador empieza como LUCA, una protocélula neutral. Mediante estructuras internas, metabolismo, división celular, adhesión funcional y modos celulares, convierte esa unidad inicial en una colonia coordinada con dos manifestaciones simultáneas: **un cuerpo multicelular construido alrededor de la madre** y **squads libres que se desprenden para tareas tácticas**. La partida se gana por dominancia ecológica, eliminación, biomasa, territorio o apex evolutivo.

| **v5.10** | **v5.11** | **Consecuencia de diseño** |
| --- | --- | --- |
| Madre como nexo móvil vulnerable, daño x0.35 | Madre con balance dinámico según estado (Cuerpo Completo / Parcial / Núcleo Expuesto) | La madre puede pelear como parte del cuerpo sin sentirse inútil |
| Producción solo via Genome Lab y recetas | Producción via Genome Lab + decisión "adherir vs squad libre" en cada nacimiento | Squads mixtos coexisten con cuerpo multicelular |
| Adhesión planificada para fase 2 | Adhesión funcional como mecánica core de v5.11 | El cuerpo multicelular es identidad visual e icónica del juego |
| Estructuras como buffs genéricos | Estructuras divididas en 3 grupos: metabólicas, combate, identidad/llaves | Las estructuras tienen propósito narrativo según fase |
| Forma apex como transformación de madre | Apex emergente desde cuerpo multicelular construido | Las formas apex son organismos compuestos visibles, no transformaciones mágicas |

## 1.1 Visión narrativa por partida

Cada partida cuenta una historia en 5 fases:

| **Fase** | **Tiempo aproximado** | **Lo que vive el jugador** |
| --- | --- | --- |
| La cría vulnerable | Min 0-5 | LUCA simple, frágil, lenta. Esquiva amenazas. Recolecta. Toma primeras decisiones de estructura. |
| La construcción del traje | Min 5-15 | Las primeras hijas se adhieren al cuerpo o salen como squad. El cuerpo crece. Estructuras internas se instalan. |
| La fortaleza completa | Min 15-25 | Cuerpo construido, estructuras instaladas, squads libres operando. La madre es invulnerable mientras el cuerpo aguante. |
| El traje cae | Min 25+ (o cuando el rival presiona) | El cuerpo se rompe capa por capa. La madre queda expuesta. Decisión: ¿reagrupás células o aceptás el modo campeón? |
| El campeón despierta | Variable | Las estructuras de combate latentes se activan. La madre pelea sola con todo lo que construyó durante la partida. |

Esta progresión es **única en el género RTS**. La mayoría de los RTS tienen late games anticlimáticos (la base cae y se acabó). En Protogenesis, la base cae y **ahí empieza otra fase**.

## 1.2 Posicionamiento competitivo

| **Juego** | **Diferencia con Protogenesis v5.11** |
| --- | --- |
| Cell Lab | Cell Lab es simulador offline; Protogenesis es RTS en vivo con cuerpo emergente |
| Cell to Singularity | Cell to Singularity es idle/clicker; Protogenesis es squad-based RTS con micro real |
| Bacterial Takeover | Bacterial Takeover es estratégico abstracto; Protogenesis es táctico con identidad celular |
| Eufloria | Eufloria es minimalista; Protogenesis es biológicamente honesto y profundo |
| StarCraft II | StarCraft tiene barracas + producción; Protogenesis tiene división celular + adhesión |
| Company of Heroes | CoH tiene squads de soldados; Protogenesis tiene squads + cuerpo multicelular |

La combinación **squads libres + cuerpo multicelular + campeón latente + biología honesta** no existe en ningún competidor.

---

# 2. Pilares de diseño

Los 10 pilares de v5.11 son los del v5.10 con dos adiciones críticas que reflejan la nueva visión:

1. **La célula es la base.** Todo nace desde la madre. La madre es el centro emocional y mecánico del juego.

2. **División = producción.** No hay barracas. Las hijas son producto de división celular biológicamente real.

3. **Biología como sistema, no como decoración.** Cada mecánica corresponde a algo biológico real (heterocistos, fagocitosis, criptobiosis, heat shock proteins, etc.).

4. **Mundo transformable.** El mapa reacciona a las acciones del jugador via la grilla ambiental con difusión química.

5. **Especialización emergente.** La identidad del jugador emerge de sus acciones (estructuras + genes + comportamiento + ambiente), no de una elección rígida temprana.

6. **RTS por escuadras biológicas.** Squads libres con roles, cohesión, autonomía. Pero no son la única manifestación: el cuerpo multicelular también es el jugador.

7. **Riesgo sistémico.** Crecer crea inestabilidad (biomasa, stress, ATP). Cuerpos grandes consumen más recursos.

8. **Lectura visual honesta.** Lo visible informa. El jugador puede leer la composición del rival mirando la pantalla.

9. **Campo de batalla vivo.** El ecosistema decide combates tanto como el micro del jugador.

10. **Legibilidad antes que simulación total.** El juego es más simple que la biología real en aspectos no críticos para que sea jugable.

**Pilares nuevos en v5.11:**

11. **Cuerpo multicelular como identidad.** Cada partida construye un organismo único visible. La forma del cuerpo cuenta tu historia.

12. **Campeón latente.** La madre acumula poder durante la partida que solo se manifiesta cuando el cuerpo cae. No hay "perder la base = perder partida".

---

# 3. Experiencia objetivo

## 3.1 Fantasía intentada

> "Soy una colonia viva que construye un organismo multicelular único, despliega squads tácticos, y guarda como último recurso una célula germinal que se vuelve guerrera definitiva si todo lo demás cae."

Esta fantasía combina cuatro elementos:
- **Colonia viva** (no cursor sobre base)
- **Organismo multicelular único** (cada partida construye algo distinto)
- **Squads tácticos** (RTS real con micro)
- **Campeón final** (momento épico cuando el cuerpo cae)

## 3.2 Tipos de jugadores y qué reciben

| **Tipo de jugador** | **Qué le entrega v5.11** | **Riesgo a vigilar** |
| --- | --- | --- |
| RTS táctico | Composición de squads + cuerpo multicelular + decisiones de transición | Que la curva no abrume |
| Jugador biológico/sistémico | 55+ elementos catalogados con justificación biológica real | Que no se vuelva enciclopedia |
| Jugador narrativo | Arco de partida (cría → traje → fortaleza → caída → campeón) | Que el campeón no se sienta scripted |
| Jugador macro | Carga biológica + recetas + balance multicelular vs squad | Que los límites sean visibles antes de gastar |
| Jugador competitivo | Receta + estructura de cuerpo + timing de transiciones | Que ninguna build domine sin counter |

## 3.3 Momento de "wow" objetivo

Cada partida debe entregar al menos uno de estos momentos:
- **Wow constructivo:** "Mi cuerpo finalmente tiene flagelo, motora, productora y coraza. Soy un organismo real."
- **Wow táctico:** "Solté todas mis adheridas en medio del combate y mi squad arrasó."
- **Wow épico:** "El rival rompió mi traje pero mi madre tiene Pseudópodos múltiples y Pared celular. Vamos a ver qué pasa ahora."
- **Wow ecológico:** "Mi colonia transformó la zona en pH 4 y el rival no puede entrar."

---

# 4. Modos de juego

| **Modo** | **Descripción** | **Estado v5.11** |
| --- | --- | --- |
| Sandbox | Construcción libre, sin condiciones de victoria | MVP |
| Escenario | Misiones específicas con condiciones predefinidas | MVP |
| Skirmish vs IA | Partida estándar contra IA con perfil ajustable | MVP |
| Campaña | Historia progresiva: del caldo primordial a la complejidad celular | Post-MVP |
| Sucesión generacional | Tu colonia evolucionada se guarda como subespecie para próxima partida | Post-MVP |
| Multiplayer 1v1 | Dos jugadores compiten por el ecosistema | Vertical slice |
| Multiplayer 2v2 / FFA | Hasta 4 jugadores en mapas grandes | Post-MVP |

---

# 5. Core loop

El core loop de v5.11 integra los sistemas de v5.10 con el cuerpo multicelular nuevo. Tiene tres horizontes temporales que se afectan mutuamente.

## 5.1 Loop de 5 segundos (micro)

- Mover madre o squad
- Cambiar modo celular de unidad seleccionada
- Activar ability (Q/E/R/F)
- Asignar directiva rápida (F1-F10)
- Reaccionar a daño entrante

## 5.2 Loop de 30 segundos (táctico)

- Decidir si dividir
- Si dividir: ¿hija adherida al cuerpo o suelta como squad?
- Si adherida: ¿qué posición en la grilla hexagonal?
- Asignar a la nueva unidad un modo y directiva inicial
- Mover squad para presionar / defender / recolectar
- Activar abilities especiales del rol

## 5.3 Loop de 3 minutos (estratégico)

- Instalar nueva estructura interna en madre
- Modificar receta de fenotipo en Genome Lab
- Activar nuevo gen del árbol evolutivo
- Decidir transición de estado del cuerpo (¿reforzás cuerpo o liberás todo en squad?)
- Responder a evento del Director Ecológico
- Modificar política ecológica con vecinos (si hay aliados)

## 5.4 Decisión recurrente clave

Cada vez que la madre divide, el jugador toma esta decisión:

```
[DIVIDIR · costo X biomasa + Y ATP]

¿Qué subtipo será la hija?
  ● Hija plástica del linaje (default)
  ○ Casta auxiliar (si compatible)
  ○ Apex unit (si desbloqueada)

¿Adónde va?
  ● Adherir al cuerpo (posición visible en grilla)
  ○ Squad libre (sale como unidad independiente)
  ○ Receta automática (sigue el patrón configurado)
```

Esta decisión es **el corazón del juego**. Cada división es una decisión real con consecuencias visibles.

---

# 6. Estructura de partida

| **Fase** | **Tiempo** | **Estado típico de la madre** | **Foco del jugador** |
| --- | --- | --- | --- |
| Apertura | Min 0-5 | LUCA sola, sin cuerpo | Recolectar, evitar amenazas, primera estructura |
| Construcción inicial | Min 5-12 | Cuerpo Parcial (3-5 células) | Adherir primeras hijas, instalar organelos básicos |
| Consolidación | Min 12-22 | Cuerpo Completo (6+ células) | Producción de squads, expansión territorial |
| Pico de poder | Min 22-32 | Cuerpo Completo con apex disponible | Combates decisivos, presión final |
| Resolución | Min 32+ | Variable: Completo / Parcial / Núcleo Expuesto | Empuje final por victoria o defensa de eliminación |

## 6.1 Variabilidad por ruta y composición

Una partida con ruta Hongo se siente distinta a una con ruta Flagelado:

- **Ruta Hongo:** la fase de construcción es lenta. El cuerpo crece como red de hifas. Pico de poder en min 25+.
- **Ruta Flagelado:** la fase de construcción es rápida. Cuerpo pequeño pero móvil. Pico de poder en min 15-20.
- **Ruta Cianobacteria:** la fase económica es la más larga. El cuerpo es defensivo. Pico en min 30+.
- **Ruta Bacteria:** poco cuerpo, mucho squad. Picos múltiples por raids.

---

# 7. Las tres capas jugables

v5.11 tiene **tres capas simultáneas** donde el jugador toma decisiones. Las tres se afectan entre sí.

## 7.1 Capa 1: Builder interior (la anatomía de la madre)

**Qué decide el jugador:**
- Qué estructuras internas instalar (hasta 8 slots de organelos)
- Qué pared/membrana usar (1 slot exclusivo)
- Qué locomoción primaria (1-2 slots)
- Qué proteínas/enzimas activar (4-6 slots)
- Qué genes desbloquear del árbol evolutivo

**Ritmo:** decisiones cada 1-3 minutos.

**Consecuencias:**
- Las metabólicas afectan economía durante toda la partida
- Las de combate determinan cómo será el campeón final
- Las de identidad/llaves abren rutas de fenotipo

## 7.2 Capa 2: Cuerpo multicelular (la anatomía de la colonia)

**Qué decide el jugador:**
- Qué hijas adherir vs liberar como squad
- Dónde posicionar cada célula adherida en la grilla hexagonal
- Cuándo desadherir o liberar todo (Husk Drop)
- Qué forma del cuerpo construir (esfera defensiva, torpedo direccional, red expansiva, etc.)

**Ritmo:** decisiones cada 30-90 segundos.

**Consecuencias:**
- Forma del cuerpo determina velocidad y dirección
- Composición determina capacidades (motoras, defensa, productoras, sensoras)
- Estado del cuerpo (Completo / Parcial / Expuesto) modifica los stats de la madre

## 7.3 Capa 3: Squads libres (RTS clásico)

**Qué decide el jugador:**
- Qué directiva asignar a cada squad
- Qué modo celular activo
- Movimientos, ataques, retiradas
- Composición de squads (recetas via Genome Lab)

**Ritmo:** decisiones cada 5-30 segundos.

**Consecuencias:**
- Combate moment-to-moment
- Recolección y economía
- Presión territorial al rival

## 7.4 Cómo se afectan las capas

Las tres capas no son independientes. Se afectan así:

| Cambio en | Afecta a |
| --- | --- |
| Estructura nueva en madre (Capa 1) | Buffs metabólicos a Capa 2 y Capa 3 |
| Más células adheridas (Capa 2) | Capa 1 cambia estado de madre, modifica producción de Capa 3 |
| Squad pierde células (Capa 3) | Capa 2 puede recuperar adheridas si sobreviven; Capa 1 sin cambio directo |
| Estructura de combate instalada (Capa 1) | Solo se activa cuando Capa 2 colapsa (Núcleo Expuesto) |
| Husk Drop (Capa 2) | Liberación masiva afecta Capa 3 con buffs temporales |

---

# 8. Modelo de datos de célula

## 8.1 Estructura base de toda célula

Toda célula del juego (madre, hija adherida, hija squad, neutral, threat) hereda de la misma clase base:

```csharp
public class V6CellEntity
{
    // Identidad
    public CellId Id;
    public PlayerId Owner;
    public RouteId Route;          // Bacteria, Ameba, Cianobacteria, etc.
    public CellRole Role;           // Madre, Recolectora, Atacante, Soporte, etc.
    public bool IsMother;
    
    // Estado físico
    public Vector2 Position;
    public Vector2 Velocity;
    public float WorldSize;
    
    // Salud y stress
    public float HP;
    public float HPMax;
    public float Stress;
    public CellMembraneSegments Segments;  // 4 segmentos N/E/S/O con HP individual
    
    // Recursos
    public ResourceWallet Wallet;  // ATP, biomasa, submateriales
    public float ATPGenerationRate;
    public float BiomassConsumptionRate;
    
    // Cuerpo multicelular (NUEVO v5.11)
    public CellBodyState BodyState;        // Adherida, Squad, Solo
    public List<AdhesionLink> Links;       // Links con células vecinas
    public HexPosition? BodyPosition;      // Posición en grilla hexagonal de cuerpo
    public CellId? AttachedToMotherId;     // Si está adherida, a qué madre
    
    // Genoma y fenotipo
    public Genome InstalledGenome;          // Genes activos
    public List<StructureId> InstalledStructures;
    public List<EnzymeId> ActiveEnzymes;
    public List<ToxinId> ActiveToxins;
    
    // Comportamiento
    public CellMode CurrentMode;            // Recolectar, Defender, Cazar, etc.
    public Directive CurrentDirective;      // Sistema interno de v5
    public List<AutoTransitionRule> AutoRules;
    
    // Combate
    public DamageProfile Damage;
    public DefenseProfile Defense;
    public List<AbilityState> Abilities;    // Q/E/R/F con cooldowns
    
    // Tracking y memoria
    public AffinityEventLog History;        // Memoria causal de v5.9
    public LineageId Lineage;
    public int Generation;
}
```

## 8.2 Estado del cuerpo de la madre

```csharp
public enum BodyState
{
    Solo,              // 0 células adheridas (Núcleo Expuesto)
    Partial,           // 1-2 células adheridas (Núcleo Expuesto, transición)
    PartialBody,       // 3-5 células adheridas (Cuerpo Parcial)
    FullBody           // 6+ células adheridas (Cuerpo Completo)
}

public class MotherCellState
{
    public BodyState State;
    public int AttachedCellCount;
    public List<AdhesionLink> ActiveLinks;
    public HexGrid BodyGrid;              // Grilla hexagonal con posiciones ocupadas
    public bool ChampionModeActive;       // True si está en Núcleo Expuesto + estructuras combate
    public float ChampionStressTimer;     // Tiempo restante antes de colapso
    public StatModifiers CurrentModifiers; // Calculados según estado
}
```

## 8.3 Membrana segmentada

Cada célula (incluyendo madre y adheridas) tiene 4 segmentos de membrana con HP individual:

```csharp
public class CellMembraneSegments
{
    public float HP_North;   // 0-100% del HP total / 4
    public float HP_East;
    public float HP_South;
    public float HP_West;
    public float Rotation;   // Permite girar membrana hacia el ataque
}
```

Esto permite que el jugador rote la membrana hacia el atacante, defendiéndose mejor.

---

# 9. Recursos y economía

## 9.1 Sistema de 6 recursos (con visibilidad jerárquica)

v5.11 mantiene los 6 recursos de v5.10 pero con regla canónica clara sobre qué ve el jugador:

### Recursos visibles en HUD principal (siempre):
- **ATP** — energía operativa, se gasta en abilities, división, mantenimiento
- **Biomasa** — material de construcción, se gasta en estructuras y división
- **Stress** — penalización acumulada, no es recurso pero es métrica visible

### Submateriales (visibles solo en Panel Interior):
- **Aminoácidos** — para proteínas/enzimas
- **Lípidos** — para membranas y vacuolas
- **Nucleótidos** — para genes y división
- **Minerales** — para frústulas, conchas, magnetosomas

**Regla:** la madre acumula automáticamente submateriales al absorber detritus. El jugador no los maneja activamente — solo los ve si quiere optimizar.

## 9.2 Tasas base por estructura

| Acción | ATP | Biomasa | Submateriales |
|---|---|---|---|
| Dividir hija plástica | 20 | 30 | 5 mixtos |
| Dividir casta auxiliar | 30 | 40 | 8 mixtos |
| Instalar organelo básico | 40 | 50 | 10 mixtos |
| Instalar organelo apex | 80 | 100 | 25 mixtos |
| Activar enzima | 15 | 5 | 8 aminoácidos |
| Producir toxina | 25 | 10 | 15 mixtos |
| Adherir célula al cuerpo | +5 ATP/s mantenimiento | 0 | 0 |

## 9.3 Buffs metabólicos por estado del cuerpo

Esto es **canónico v5.11**. La madre tiene economía distinta según estado:

### Cuerpo Completo (6+ adheridas)
- +25% ATP máximo a toda la colonia
- +30% absorción de detritus en radio
- -15% costo de producir hijas
- +20% velocidad de regeneración HP de la madre
- -20% generación de stress en combate

**Justificación biológica:** las células adheridas comparten productos metabólicos, distribuyen carga, tienen más superficie absortiva.

### Cuerpo Parcial (3-5 adheridas)
- +12% ATP máximo (proporcional)
- +15% absorción de detritus
- -8% costo de producir hijas
- Buffs leves, transición

### Núcleo Expuesto / Solo (0-2 adheridas)
- Sin buffs metabólicos del cuerpo
- **Pero:** +25% regeneración ATP propio durante 60s (adrenalina)
- +20% velocidad de squad libre durante 30s (post-Husk Drop si fue voluntario)
- Genes de combate latentes activados (ver Sección 19)

### Husk Drop (liberación voluntaria masiva)
Cuando el jugador presiona la tecla H:
- Todas las adheridas se liberan en 1 segundo
- Las células liberadas tienen +20% velocidad durante 30s
- La madre gana +25% velocidad inmediatamente
- Sinergia táctica: +15% daño cuando 3+ ex-adheridas atacan al mismo target durante 20s
- Cooldown de Husk Drop: 90 segundos (no se puede spamear)

---

# 10. Sistema de ambiente / ecosistema

v5.11 mantiene la grilla ambiental de v5.10 con difusión química. Sin cambios estructurales.

## 10.1 Variables ambientales

| Variable | Rango | Difusión | Decay |
| --- | --- | --- | --- |
| O₂ | 0.0 - 1.0 | Alta | Bajo |
| pH | 0.0 - 14.0 | Media | Alto (vuelve a 7) |
| Toxinas | 0.0 - 1.0 | Alta | Medio |
| Luz | 0.0 - 1.0 | Sin difusión (es ambiental) | N/A |
| Detritus | 0.0+ | Media | Lento |
| Temperatura | -20 a +50 | Baja | Bajo |
| Salinidad | 0.0 - 1.0 | Media | Bajo |
| Presión osmótica | 0.0 - 1.0 | Sin difusión | Local |

## 10.2 Estados visibles del Living Battlefield (canon v5.6+)

Los 6 estados se mantienen del v5.6:

| Estado | Trigger | Efecto |
|---|---|---|
| Cicatriz tóxica | Muchas muertes / secreciones < 30s | Toxinas suben, vision sucia |
| Corredor nutritivo | Squads repiten ruta | Partículas orgánicas, riesgo emboscadas |
| Frente oxigenado | Fotosintéticos + luz sostenida | Buff aerobio, ROS contra anaerobios |
| Bolsa ácida | Arqueas / fermentadores estabilizan pH bajo | Daño a no tolerantes |
| Red viva | Biofilm / hifas / matriz | Vision, transferencia, resistencia local |
| Zona de pánico químico | Predador mata varios pequeños | Neutrales huyen, restos disponibles |

## 10.3 Cómo afecta el cuerpo multicelular al ambiente

**Nuevo en v5.11:**

- Un cuerpo grande inmóvil genera "huella ambiental" más densa: las adheridas absorben/secretan en radio mayor
- Las cadenas de Anabaena (cuerpo lineal) generan corredor de nitrógeno fijado
- Los cuerpos esféricos de Microalga + Cianobacteria adheridas crean "frente oxigenado" estable
- Los cuerpos con motoras pueden empujar zonas químicas leves (mover su frente oxigenado al rival)

---

# 11. Builder interior celular

Esta sección integra el catálogo completo de estructuras (que se detalla en sección 32) con la lógica de cuándo y cómo se instalan.

## 11.1 Reglas de instalación

- La madre tiene **8 slots de organelos** (expandibles a 10 con apex)
- **1 slot de pared/membrana** (mutuamente excluyente)
- **1-2 slots de locomoción** (depende de subtipo)
- **4-6 slots de proteínas/enzimas** activas
- Toxinas no usan slot — son habilidades activables

## 11.2 Costo y tiempo de instalación

| Tier | Costo aproximado | Tiempo |
|---|---|---|
| Básico | 30-50 biomasa, 20-30 ATP | 8-12s |
| Intermedio | 60-100 biomasa, 40-60 ATP, 10-15 submateriales | 15-25s |
| Apex | 100-200 biomasa, 80-120 ATP, 25-40 submateriales | 30-50s |

Durante la instalación, la madre **no puede dividir** y tiene -50% velocidad.

## 11.3 Las 3 categorías de estructuras

### Grupo 1 — Metabólicas (importan durante construcción)
Estructuras que mejoran economía y producción mientras la colonia está activa. Importantes en early-mid game.

### Grupo 2 — Combate / Anatomía del campeón (importan cuando cae el cuerpo)
Estructuras que dan capacidades de combate a la madre. **Latentes mientras está protegida, activas cuando el cuerpo cae**.

### Grupo 3 — Identidad / Llaves evolutivas (puertas)
Estructuras que desbloquean qué subtipos puede producir la madre. Son prerrequisitos.

El catálogo completo está en Sección 32.

## 11.4 Ejemplo de progresión típica

**Min 0-5 (LUCA):**
- Sin estructuras
- Decisión: ¿qué instalo primero?

**Min 5-10 (Cría):**
- 1-2 estructuras del Grupo 1 (Mitocondria, Vacuola contráctil)
- 1 estructura del Grupo 3 (ej. Núcleo, para abrir eucariotas)

**Min 10-20 (Construcción):**
- 3-5 estructuras totales
- Mix de Grupo 1 (eficiencia) y Grupo 3 (desbloqueos)
- 1-2 del Grupo 2 (preparación combate)

**Min 20-30 (Consolidación):**
- 6-8 estructuras instaladas
- Grupo 2 fortalecido (Pseudópodos múltiples, Pared celular)
- Grupo 1 maximizado

**Min 30+ (Apex):**
- Posibles upgrades a Apex (Núcleo polipoide, Mitocondria gigante)
- Madre lista para campeón si cae el cuerpo

---

# 12. Metabolismo y bifurcación

## 12.1 Bifurcaciones metabólicas principales

v5.11 mantiene los 6 caminos metabólicos del v5.10:

| Camino | Requiere | Beneficio | Penalización |
|---|---|---|---|
| Respiración aeróbica | Mitocondria, O₂ alto | +ATP base alto | Daño en zonas anóxicas |
| Fermentación | Sin requisitos | Funciona en anóxico | -ATP/s, +stress |
| Quimiolitotrofía | Estructuras especializadas | Funciona en zonas extremas | Lento |
| Fotosíntesis | Cloroplasto / Tilacoide, luz | Autonomía solar | Necesita luz |
| Fagotrofia | Lisosoma | Energía de presas | Requiere caza activa |
| Saprotrofia | Enzimas extracelulares | Detritus eficiente | Sin presas no rinde |

## 12.2 Switch metabólico dinámico

Algunos subtipos pueden cambiar de camino metabólico:

- **Euglena:** alterna fotosíntesis (con luz) y heterotrofia (en oscuro)
- **Facultativos:** respiración aeróbica → fermentación cuando baja O₂
- **Quimioautotrofos:** quimiolitotrofía → mixotrofia con detritus

El switch tiene cooldown de 10-30s y costo de ATP.

---

# 13. División celular y linajes

Esta es **una de las secciones más reescritas en v5.11**. Integra el sistema de cuerpo multicelular.

## 13.1 Decisión de división

Cuando el jugador presiona D (Dividir) sobre la madre:

```
[PANEL DE DIVISIÓN]

Costo base: 30 biomasa + 20 ATP

¿Qué subtipo?
  ● Hija plástica (de tu ruta dominante)
  ○ Casta auxiliar (si compatible — ver lista)
  ○ Apex unit (si desbloqueada — ver lista)

¿Adónde va?
  ● Adherir al cuerpo
    └ Posiciones disponibles: [grilla hexagonal con slots iluminados]
  ○ Squad libre (sale como unidad independiente)
  ○ Receta automática (sigue receta configurada)

Modo inicial:
  ● Default según rol
  ○ Personalizado: [Recolectar / Defender / Cazar / Colonizar / Reparar]
```

## 13.2 Divisiones rápidas vs Genome Lab

**División rápida (D):** abre panel mínimo, jugador elige rápido.

**Genome Lab (panel principal):** define **recetas** que la madre ejecuta automáticamente:
- "70% adheridas, 30% squad libre"
- "Producir 3 recolectoras, 2 atacantes, 1 productora, en este orden"
- "Si stress < 50, producir squad ofensivo. Si > 50, producir defensoras adheridas"

Las recetas reducen la microgestión sin sacrificar control. El jugador puede sobreescribir cualquier división abriendo el panel manual.

## 13.3 Adhesión al cuerpo

Cuando una hija nace adherida al cuerpo:

1. Aparece como fantasma en posiciones válidas de la grilla hexagonal
2. El jugador click izquierdo coloca; click derecho cancela y la libera
3. La célula queda físicamente adherida con un vínculo visible
4. Comparte recursos parcialmente con la madre y vecinas
5. Contribuye con su función según subtipo (motora mueve, productora genera ATP, etc.)

## 13.4 Reglas de posicionamiento en grilla hexagonal

| Tipo de célula | Posiciones válidas | Razón |
|---|---|---|
| Coraza / Defensa | Solo ring exterior | Protege a las internas |
| Productora (Cianobacteria, Microalga) | Ring intermedio | Necesita luz pero protección |
| Motora (Flagelado, Cilios) | Ring exterior | Empuja desde afuera |
| Boca (Ameba, fagocito) | Ring exterior, dirección de ataque | Proyectada hacia presa |
| Estructural interna (heterocisto, vacuola colonial) | Centro / ring 1 | Cerca del núcleo |
| Conectiva / Biomasa | Cualquier ring | Es relleno |

Esto se muestra visualmente al jugador con slots iluminados.

## 13.5 Desadherir (Detach)

Una hija adherida puede desadherirse:
- Costo: 5 ATP + 8 segundos de "rotura del vínculo"
- Resultado: la célula queda como squad libre normal

Una hija squad libre puede readherirse:
- Si hay slot disponible y se acerca a la madre
- Costo: 10 ATP + 5 segundos de "formación del vínculo"

## 13.6 Husk Drop (liberación masiva)

Tecla H:
- Libera **todas** las células adheridas en 1 segundo
- Cooldown 90 segundos
- Buffs descritos en sección 9.3

Es la jugada de "cambio drástico de estrategia" en medio de combate.

---

# 14. Directivas RTS y modos celulares

v5.11 mantiene los modos celulares de v5.10 como capa de alto nivel sobre directivas internas.

## 14.1 Los 6 modos celulares canónicos

| Modo | Comportamiento |
|---|---|
| **Recolectar** | Busca nutrientes/luz/detritus, retorna a entregar |
| **Defender** | Permanece cerca de madre/nodo, intercepta amenazas |
| **Cazar / Atacar** | Busca contacto, prioriza presas o targets marcados |
| **Colonizar / Anclar** | Convierte posición en territorio |
| **Reparar / Retirarse** | Vuelve, baja stress, regenera, criptobiosis temporal si grave |
| **Especial de ruta** | Conducta única desbloqueada por afinidad |

## 14.2 Modos por subtipo (variaciones)

Cada subtipo expresa los modos de manera distinta:

| Subtipo | Recolectar | Defender | Cazar |
|---|---|---|---|
| Bacteria | Absorbe detritus | Biofilm pasivo | Swarm contacto |
| Ameba | Absorbe detritus + presas pequeñas | Intercepta con pseudópodo | Fagocita |
| Flagelado | Velocidad alta | Patrulla rápida | Hostiga, hit-and-run |
| Cianobacteria | Busca luz | Bloom defensivo | N/A (no caza) |
| Hongo | Hifas a detritus | Engrosa hifas | N/A |

## 14.3 Modos en cuerpo vs squad

**Adherida en cuerpo:**
- Los modos se interpretan dentro del rol estructural (motora siempre "mueve cuerpo", productora siempre "genera ATP")
- Pueden cambiar con costo de stress

**Squad libre:**
- Los modos son completamente flexibles
- El jugador asigna modo via UI o tecla rápida

## 14.4 Reglas automáticas

El jugador puede configurar reglas:
- "Si HP < 30%, modo Reparar"
- "Si enemigo en radio 3u, modo Defender"
- Hasta 3 reglas activas por unidad

## 14.5 Directivas RTS (capa interna)

V5Directive sigue siendo la capa de comportamiento técnico que el código ejecuta. Los modos celulares se traducen a directivas. El jugador interactúa con modos; el código con directivas.

| Modo (jugador) | Directivas (código) |
|---|---|
| Recolectar | Farm + ReturnHome + AutoIntercept off |
| Defender | FollowMother + Defend + AutoIntercept on |
| Cazar | Attack + Move + Pursue |
| Colonizar | Colonize + Hold + AutoBuild |
| Reparar | ReturnHome + Idle + LowProfile |

---

# 15. Árbol evolutivo y genes

v5.11 mantiene el árbol de 4 anillos de v5.10:

| Anillo | Color | Función | Ejemplos de genes |
|---|---|---|---|
| Metabólico | Verde | ¿Cómo obtenés energía? | Respiración, Fermentación, Fotosíntesis, Quimiolitotrofía, Fagotrofia |
| Estructural | Azul | ¿Cómo te protegés y movés? | Membrana rígida, Cápsula, Pseudópodo, Flagelo, Cilios, Adhesina |
| Trófico | Coral | ¿Cómo te alimentás y atacás? | Lisosoma, Hifa invasiva, ROS oxidativa, Secreción tóxica |
| Cognitivo | Morado | ¿Cómo percibís y comunicás? | Reconocimiento, Quorum sensing, Memoria química, Predicción |

## 15.1 Genes vs estructuras

- **Genes:** capacidades activadas (3 slots activos simultáneos)
- **Estructuras:** anatomía instalada (slots fijos en madre, ver sección 11)

Algunos genes requieren estructuras (Fotosíntesis necesita Cloroplasto/Tilacoide). Otros son independientes.

## 15.2 Genes de supervivencia (importantes para campeón)

Estos genes se activan automáticamente en estado Núcleo Expuesto si están instalados:

| Gen | Efecto en campeón |
|---|---|
| Heat Shock Response | -30% daño en zonas extremas |
| SOS Response | +50% velocidad de regeneración |
| Quorum-sensing señal | Convoca squads cercanas a la madre |
| Esporulación | Genera 2 esporas defensivas pasivas en radio |
| Criptobiosis | Pausa tiempo durante 10s una vez por partida |

---

*[Continúa en Mensaje 2 con secciones 16-30: Affinity Tracker, las 12 rutas integradas, formas apex, combate con capas, IA, UI/UX, balance numérico]*
# 16. Evolution Affinity Tracker

v5.11 mantiene el sistema de afinidad con memoria causal de v5.9-v5.10. La diferencia: ahora **desbloquea más cosas** (no solo identidad sino también fenotipos de cuerpo, posiciones de adhesión, modos especiales).

## 16.1 Cómo funciona

Cada acción significativa del jugador suma puntos a una o más rutas:

| Acción | Bacteria | Arquea | Cianobacteria | Ameba | Hongo |
|---|---|---|---|---|---|
| Instalar Tilacoide | -3 | -2 | +18 | -2 | -3 |
| Instalar Lisosoma | -5 | -3 | -3 | +18 | -3 |
| Vivir en pH < 5 (60s) | +2 | +15 | -8 | -3 | +2 |
| Fagocitar célula chica | -2 | 0 | 0 | +12 | 0 |
| Activar fermentación | +8 | +5 | -3 | 0 | +5 |
| Construir biofilm | +12 | 0 | 0 | 0 | +5 |
| Construir hifa | 0 | 0 | 0 | 0 | +18 |

(Lista completa en Apéndice 30)

## 16.2 Niveles de afinidad

| Puntos acumulados | Estado | Desbloquea |
|---|---|---|
| 0-30 | Latente | Acceso a hijas plásticas estándar |
| 31-60 | Sesgo emergente | Casta auxiliar de esa ruta |
| 61-100 | Identidad consolidada | Apex unit, modo especial de ruta |
| 100+ | Apex evolutivo | Forma campeón potenciada |

## 16.3 Memoria causal v5.9 (mantenida)

El Affinity Event Log mantiene los últimos 96 eventos durante 720 segundos. Esto permite que el panel de afinidad muestre **por qué** se acerca o desbloquea cada cosa:

```
Cianobacteria — 67% (consolidada)
Causas recientes:
  ✓ Pasaste 90s en zona luminosa (+18)
  ✓ Instalaste Tilacoide x2 (+36)
  ✓ Tu madre produjo ATP por fotosíntesis (+12)
```

## 16.4 Desbloqueos por afinidad en v5.11

**Nuevo en v5.11:** la afinidad ya no solo desbloquea identidad, también desbloquea **opciones tácticas** del cuerpo multicelular:

| Afinidad consolidada | Desbloquea (además de hijas) |
|---|---|
| Cianobacteria 60+ | Cuerpo "Frente Oxigenado": las productoras adheridas al cuerpo aumentan el O₂ ambiental en radio |
| Ameba 60+ | Cuerpo "Predador Compuesto": permite adherir bocas en posiciones direccionales (frente del cuerpo) |
| Hongo 60+ | Cuerpo "Red Anclada": el cuerpo extiende hifas adheridas a otras estructuras del mapa |
| Anabaena 60+ | Cadenas multicelulares con heterocistos cada 5 células |
| Diatomea 60+ | Coraza mineral: las defensivas adheridas tienen +50% defensa |
| Flagelado 60+ | Cuerpo direccional: si 60% de adheridas son motoras, ganás dirección y velocidad |

---

# 17. Rutas evolutivas completas

v5.11 mantiene las 12 rutas de v5.10 organizadas en 4 categorías:

## 17.1 Las 4 categorías

**Procariotas:**
1. Bacteria — swarm flexible
2. Cianobacteria — fotosintética bloom
3. Arquea — extremófila terraformadora
4. Espirilo — penetrador helicoidal (subruta)
5. Anabaena — cadena con heterocistos (subruta)

**Eucariotas predadores:**
6. Ameba — fagocito grande
7. Flagelado — raid hit-and-run
8. Ciliado — control corrientes

**Eucariotas productores:**
9. Microalga — colonia fotosintética
10. Diatomea — coraza de sílice (subruta)
11. Euglena — switch metabólico (subruta)

**Redes / descomponedores:**
12. Hongo — red fija defensiva
13. Moho mucilaginoso — red móvil con memoria

(Tardígrado se mantiene como NPC neutral, no ruta jugable, según canon v5.5+)

## 17.2 Cómo elegir tu ruta dominante

La ruta dominante emerge de las acciones, no se elige. Pero el jugador puede **influenciarla activamente**:

- Movéte hacia recursos asociados a la ruta deseada
- Instalá las estructuras llave de esa ruta
- Activá los genes correspondientes
- Sobreviví en ambientes que la favorecen

A los 5-8 minutos, una ruta dominante suele consolidarse.

## 17.3 Coexistencia de rutas

v5.11 permite **squads mixtos coherentes**:
- Tu ruta dominante define 70-80% de tu carga biológica
- Castas auxiliares de rutas vecinas pueden ser hasta 30% (40% con gen Simbiosis)
- El cuerpo multicelular puede tener células adheridas de varias rutas si compatibles

---

# 18. Fichas de las 12 rutas principales

Cada ruta tiene una ficha estandarizada con 12 secciones. Por brevedad acá va el resumen; las fichas completas están en el documento extendido del v5.5 (ya tenés esos archivos).

## 18.1 Resumen de las 12 rutas con cuerpo multicelular

| Ruta | Forma típica del cuerpo | Squads típicos | Apex |
|---|---|---|---|
| Bacteria | Pequeño (3-5 células), poco cuerpo, mucho squad | 8-15 bacterias swarm | Supercolonia bacteriana |
| Arquea | Mediano defensivo, cuerpo en zona ácida | 4-8 arqueas patrulla | Arquea hipertermófila |
| Cianobacteria | Grande estático, esfera fotosintética | Pocos squads, mucha presencia | Cianobacteria hiperoxigenadora |
| Espirilo | Pequeño móvil, helicoidal | Squads de penetración | Espirilo perforador apex |
| Anabaena | Cadena lineal larga | Squads pequeños | Filamento heterocístico |
| Ameba | Mediano direccional con bocas | 3-6 amebas cazadoras | Ameba apex voraz |
| Flagelado | Pequeño torpedo, motoras concentradas | 4-8 raiders | Flagelado predador apex |
| Ciliado | Mediano, cilios alrededor | 4-6 controladores | Ciliado vorticista |
| Microalga | Grande esfera con colonia adherida | Pocos squads | Supercolonia microalgal |
| Diatomea | Mediano blindado, coraza mineral | 2-4 tanques móviles | Diatomea fortaleza |
| Euglena | Mediano flexible | 4-6 versátiles | Euglena adaptativa |
| Hongo | Grande estático, red de hifas | Pocos squads, mucha estructura | Hongo micorrizal apex |
| Moho mucilaginoso | Mediano móvil, reconfigurable | Squads de exploración | Moho memorioso |

## 18.2 Ejemplo expandido: Ameba en v5.11

**Identidad:** "Te como, crezco, te como otra vez. Y cuando me rompas el traje, vas a conocer mi verdadera boca."

**Forma típica del cuerpo (Cuerpo Completo):**
- Ring 1: vacuolas internas (recursos)
- Ring 2: 2-3 cianobacterias (productoras de ATP)
- Ring 3: 4-5 amebas auxiliares (bocas)
- Ring 4 (frente): 2-3 bocas direccionales (donde "mira")
- Ring 4 (atrás): 2-3 motoras flageladas

**Squads típicos (mientras cuerpo está activo):**
- 3-6 amebas cazadoras hostigadoras
- 1-2 célulares defensoras

**Estado del cuerpo y stats:**
- Cuerpo Completo: la madre genera ATP gracias a las cianobacterias adheridas
- Cuerpo Parcial: la madre tiene autonomía media
- Núcleo Expuesto: la madre activa Pseudópodos múltiples y se vuelve fagocito apex

**Estructuras de combate típicas:**
- Pseudópodos múltiples (apex obligado)
- Lisosoma masivo
- Citoesqueleto desarrollado
- Núcleo reforzado

**Forma apex / Campeón:**
"Ameba Apex Voraz" — WorldSize hasta 0.55, fagocita células del 90% de su tamaño, F: Devorar enjambre, G: Pseudópodo extendido.

**Counters:**
- Vulnerable a Bacteria swarm si la pillás antes de min 8
- Cianobacteria con bloom de O₂ + ROS la oxida si no tiene Catalasa
- Espirilo que perfora cuerpo y daña al núcleo directo

---

# 19. Formas apex y campeón latente

**Esta es una de las secciones más reescritas en v5.11.**

## 19.1 Forma apex como organismo emergente, no transformación mágica

En v5.10 las formas apex se desbloqueaban al 100% del tracker y eran transformaciones de la madre. **En v5.11, las formas apex son organismos compuestos visibles** que emergen del cuerpo multicelular construido.

### Ejemplo: Volvox Apex

Antes (v5.10): "tu madre se transforma en Volvox apex con bonus".

Ahora (v5.11): "Cuando tu cuerpo tenga 8+ Microalgas adheridas + 2 Flageladas + 1 Cianobacteria con la madre central, **el conjunto se reconoce automáticamente como Volvox apex** y desbloquea sus abilities únicas".

Esto es **biológicamente correcto** (Volvox real es una colonia diferenciada) y **mecánicamente claro** (el jugador construye el apex, no lo conjura).

## 19.2 Las 12 formas apex y sus configuraciones

| Apex | Configuración requerida | Ability F | Ability G |
|---|---|---|---|
| **Supercolonia bacteriana** | 12+ bacterias adheridas en cluster denso | Burst conjugativo | Resistencia compartida |
| **Arquea hipertermófila** | Madre Arquea + 6+ adheridas + 80% afinidad | Eructo metanogénico | Aura térmica |
| **Cianobacteria hiperoxigenadora** | 8+ cianobacterias adheridas + Tilacoide | Pulso fotosintético | Bloom dirigido |
| **Espirilo perforador** | Madre Espirilo + 4+ con motilidad helicoidal | Sacacorchos cooperativo | Penetración masiva |
| **Filamento heterocístico** | Cadena de 10+ Anabaenas con heterocistos | Sacrificio coordinado | Fijación masiva N₂ |
| **Ameba apex voraz** | Madre Ameba + Pseudópodos múltiples + 4+ amebas auxiliares | Devorar enjambre | Pseudópodo extendido |
| **Flagelado predador** | Madre Flagelado + 6+ motoras | Sprint coordinado | Vortex flagelar |
| **Ciliado vorticista** | Madre Ciliado + 4+ ciliados auxiliares | Tormenta vorticista | Estancamiento |
| **Supercolonia microalgal** | 12+ microalgas adheridas | Resurrección colonial | Bloom defensivo |
| **Diatomea fortaleza** | Madre Diatomea + 6+ con frústula | Espículas radiales | Coraza compartida |
| **Euglena adaptativa** | Madre Euglena + switch metabólico activo + 4+ adheridas | Switch global | Fotosíntesis acelerada |
| **Hongo micorrizal** | Madre Hongo + 18+ hifas en red | Bloom de esporas | Red hipersensitiva |
| **Volvox emergente** | 8+ microalgas + 2 flageladas + 1 cianobacteria + madre central | Liberación masiva | Esfera defensiva |

## 19.3 El campeón latente — canon v5.11

**Esta es la mecánica más distintiva de v5.11.**

Mientras la madre tiene Cuerpo Completo o Parcial (3+ adheridas), las **estructuras de combate del Grupo 2** que instalaste están **latentes**: existen pero no se activan ni dan stats relevantes.

Cuando la madre entra en Núcleo Expuesto (0-2 adheridas), las estructuras de combate **se activan retroactivamente**:

### Activación automática del campeón

En el momento que la madre tiene < 3 células adheridas:

```
🟡 NÚCLEO EXPUESTO
Tu madre revela sus estructuras de combate latentes:

✓ Núcleo reforzado activado: HP +50%
✓ Pseudópodos múltiples activados: ataca a 3 enemigos simultáneos
✓ Membrana rígida activada: defensa +30%
✓ Citoesqueleto desarrollado activado: velocidad +40%

Estado: Daño x1.5 / Defensa x2.0 / Velocidad x1.3
Costo: Stress acelerado (60-90s antes de colapso)
```

### Lo que ve el jugador

- La madre cambia visualmente: sus estructuras antes invisibles ahora son visibles desde fuera
- Aparecen pseudópodos extendidos, pared celular gruesa, etc.
- El cuerpo cambia de color sutilmente (más saturado, más vivo)
- Aparecen las abilities F y G de la forma apex correspondiente

### Por qué tiene tiempo límite (60-90s de stress)

El campeón **no es sostenible eternamente**. Genera stress acelerado por estar en estado de emergencia. Esto fuerza:
- O ganás la pelea en 60-90s
- O reagrupás células (recoger squads cercanos para readherir)
- O perdés (la madre colapsa por agotamiento)

Esto evita que "perder cuerpo = jugar como campeón forever". Es un momento épico finito.

## 19.4 Reactivación: volver a tener cuerpo

Si durante el modo campeón la madre logra readherir 3+ células (porque squads cercanos vuelven), el modo se desactiva:
- Las estructuras de combate vuelven a estar latentes
- Los stats vuelven a Cuerpo Parcial
- El stress acelerado se detiene
- La madre vuelve a tener buffs metabólicos

**Esto crea un ciclo táctico potente:** el jugador puede entrar y salir de modo campeón estratégicamente.

---

# 20. Combate, capas y daño penetrante

v5.11 introduce el sistema de **capas** que respeta la topología del cuerpo multicelular.

## 20.1 Sistema de capas

Cuando un cuerpo multicelular recibe daño:

1. **El daño va primero a la célula adherida más cercana al atacante** (ring exterior si viene de afuera)
2. Solo si esa célula muere, el daño pasa a la siguiente capa
3. La madre central solo recibe daño si todas las capas exteriores cayeron

**Esto es lo que hace al cuerpo multicelular una fortaleza biológica real.**

## 20.2 Tipos de daño

| Tipo de daño | Respeta capas | Ejemplos |
|---|---|---|
| **Contacto físico** | Sí | Fagocitosis, pseudópodos, mordidas |
| **Químico ambiental** | No (penetra) | Toxinas en zona, ácido, ROS |
| **Penetrante** | No (ignora capas) | Espirilo helicoidal, Nematodo perforador, Bacteriófago |
| **Stress emocional** | No (afecta a toda la colonia) | Pánico químico, atrofia |

**Implicación táctica:** un cuerpo grande es invulnerable a contacto pero vulnerable a químico y penetrante. El rival que enfrenta un cuerpo construido tiene **soluciones claras**: producir Espirilos, mover su zona ácida hacia ti, lanzar bacteriófagos.

## 20.3 Daño dirigido al núcleo

Algunos subtipos especializados pueden hacer daño que **ignora capas y va directo al núcleo de la madre**:

| Atacante | Cómo lo hace |
|---|---|
| **Espirilo apex** | Movimiento helicoidal atraviesa membranas |
| **Nematodo** | Estilete perforador busca el centro |
| **Bacteriófago** | Inyecta ADN al núcleo (ignora cuerpo) |
| **Toxina dinoflagelada** | Saxitoxina afecta neuronas del núcleo |

Esto da **contraplay** al cuerpo multicelular: no es invencible, hay armas anti-cuerpo y anti-núcleo.

## 20.4 Combate squad vs squad

Sin cambios mayores respecto al v5.10:
- Cohesión 0-100 mantiene formación
- Blob penalty: 8+ células en 2u recibe +15% daño AOE
- Formaciones (línea, cuña, dispersa) modifican daño y movimiento
- Abilities Q/E/R/F por modo y rol

## 20.5 Combate cuerpo vs squad rival

Cuando un cuerpo multicelular pelea contra squad rival:
- Las células adheridas externas absorben el daño primero
- El cuerpo no se mueve tan rápido como squad libre
- Pero su HP combinado es muy alto
- El rival debe decidir: ¿enfrenta cuerpo (sobrevivencia tipo asedio) o esquiva y ataca recursos?

## 20.6 Combate cuerpo vs cuerpo (PvP)

Esto es **el escenario más visualmente espectacular del juego**. Dos cuerpos multicelulares chocando:
- Capas externas se rompen primero por ambos lados
- El que tiene más bocas direccionales (frente) tiene ventaja ofensiva
- El que tiene mejor coraza tiene ventaja defensiva
- Si uno entra en Núcleo Expuesto antes que el otro, se vuelve campeón vs cuerpo: pelea de gigantes

---

# 21. Colonización y world building

Sin cambios mayores respecto a v5.10. La colonización funciona via:

- **Biofilm** (bacterias): zona de control con buff a aliados
- **Hifas** (Hongo): red territorial expandible
- **Bloom** (Cianobacteria): saturación de O₂
- **Zona ácida** (Arquea): pH bajo letal a no-tolerantes
- **Mucilago** (Moho): rastros de memoria química

Una zona se considera "controlada" cuando >60% de las celdas tienen tu firma química.

## 21.1 Cómo afecta el cuerpo multicelular a colonización

**Nuevo v5.11:**
- Cuerpos grandes anclados generan colonización en su radio mientras estén ahí
- Los cuerpos móviles (Flagelado, Ameba) generan colonización débil temporal
- Los cuerpos en red (Hongo) son colonización permanente

---

# 22. IA y organismos salvajes

Sin cambios respecto a v5.10. El Director Ecológico mantiene los 9 perfiles de IA y los 6 arquetipos de threats adaptativos.

## 22.1 IA enfrentada a cuerpo multicelular del jugador

**Nuevo en v5.11:** la IA reconoce el estado del cuerpo del jugador y adapta tácticas:

- Cuerpo Completo grande → IA prioriza daño penetrante (Espirilo, Bacteriófago)
- Cuerpo móvil con motoras → IA prioriza control (Hongo, Ciliado contra-corrientes)
- Cuerpo defensivo (Diatomea/coraza) → IA prioriza químico (Arquea, toxinas)
- Núcleo Expuesto (campeón) → IA enjambra para bajar HP rápido antes de que reagrupe

Esto da contraplay inteligente sin que la IA se sienta scripteada.

---

# 23. UI/UX y Genome Lab

v5.11 mantiene el Genome Lab de v5.10 con adiciones para cuerpo multicelular.

## 23.1 Layout principal

```
┌─────────────────────────────────────────────────────────────┐
│ HUD SUPERIOR (42px)                                          │
│ Madre [estado cuerpo] · ATP · Biomasa · Stress · Tracker     │
├──────────┬───────────────────────────────────┬──────────────┤
│          │                                   │              │
│  PANEL   │                                   │   MINIMAPA   │
│ INTERIOR │      EXTERIOR (gameplay)          │              │
│          │                                   │   Squads     │
│ Genome   │   - Cuerpo multicelular visible   │   activos    │
│ Lab      │   - Squads libres                 │              │
│          │   - Threats                       │   Modo:      │
│ (280px)  │   - Zonas ambientales             │   Táctico    │
│          │                                   │              │
├──────────┴───────────────────────────────────┴──────────────┤
│ HUD INFERIOR (82px)                                          │
│ Squad selecc · Modos · Abilities · Formación · Directiva     │
└─────────────────────────────────────────────────────────────┘
```

## 23.2 Panel del cuerpo multicelular (NUEVO v5.11)

Cuando el jugador clickea sobre la madre, aparece un panel especial:

```
┌────────────────────────────────────────────────┐
│ MADRE · Linaje Ameba                           │
│ Estado: ● Cuerpo Completo (8 adheridas)        │
│                                                │
│ [Vista hexagonal del cuerpo]                   │
│      [F] [F]                                   │
│   [B]  [P]  [C]                                │
│  [P]  [N]  [P]    N = núcleo                   │
│   [B]  [P]  [C]   F = motora                   │
│      [M] [M]      P = productora               │
│                   B = biomasa                  │
│ Buffs activos:    C = coraza                   │
│ +25% ATP máx      M = boca                     │
│ +30% absorción                                 │
│ -15% costo división                            │
│                                                │
│ Estructuras instaladas (8/8):                  │
│ [Mitocondria] [Núcleo reforzado]               │
│ [Lisosoma] [Pseudópodos múltiples]             │
│ [Pared celular] [Citoesqueleto]                │
│ [Vacuola contráctil] [Catalasa]                │
│                                                │
│ Estructuras de combate latentes:               │
│ ⚪ Pseudópodos múltiples (activación pendiente) │
│ ⚪ Pared celular (activación pendiente)         │
│ ⚪ Citoesqueleto (activación pendiente)         │
│                                                │
│ [Husk Drop · H]   [Genome Lab · G]            │
└────────────────────────────────────────────────┘
```

## 23.3 Panel del Genome Lab

```
┌────────────────────────────────────────────────────┐
│ GENOME LAB · Recetas activas                       │
├────────────────────────────────────────────────────┤
│ Receta principal: "Predador eucariota"             │
│ ● 40% Ameba caza                                   │
│ ● 25% Flagelado raider                             │
│ ● 20% Ciliado control                              │
│ ● 15% Reparación                                   │
│                                                    │
│ Distribución cuerpo/squad:                         │
│ Cuerpo: 50% | Squad: 50%                           │
│                                                    │
│ [Modificar receta]   [Crear receta nueva]          │
├────────────────────────────────────────────────────┤
│ Fenotipos disponibles:                             │
│ ● Hija plástica (default)                          │
│ ● Recolectora de linaje                            │
│ ● Defensora de linaje                              │
│ ● Raider de linaje                                 │
│ 🔒 Apex unit (necesita afinidad 80+)              │
│                                                    │
│ [Click para detalle de cualquier fenotipo]         │
└────────────────────────────────────────────────────┘
```

## 23.4 Placa biológica detallada (Dune-style)

Cuando el jugador selecciona un fenotipo:

```
┌──────────────────────────────────────────────────┐
│ AMEBA CAZA                                       │
├──────────────────────────────────────────────────┤
│                                                  │
│   [ILUSTRACIÓN CIENTÍFICA DETALLADA]             │
│                                                  │
│ "Eucariota predador móvil. Caza por contacto     │
│  activo. Crece al fagocitar."                    │
│                                                  │
│ Stats:                                           │
│ HP base: 110 | Velocidad: 1.1× | ATP máx: 180   │
│                                                  │
│ Mecánica única:                                  │
│ Cada fagocitosis +0.02 WorldSize (cap 0.42)      │
│                                                  │
│ Costo: 30 biomasa + 20 ATP + 5 lípidos          │
│ Tiempo división: 14s                             │
│                                                  │
│ Compatibilidad:                                  │
│ ✓ Núcleo instalado                               │
│ ✓ Lisosoma instalado                             │
│ ✓ Afinidad Ameba: 45% (suficiente)              │
│                                                  │
│ Modo default: Cazar                              │
│                                                  │
│ [DIVIDIR HIJA]      [Cancelar]                  │
└──────────────────────────────────────────────────┘
```

---

# 24. Cámara, zoom y lectura táctica

v5.11 mantiene los 4 niveles de zoom de v5.10. Sin cambios.

## 24.1 Visualización del cuerpo según zoom

| Zoom | Cómo se ve el cuerpo multicelular |
|---|---|
| 1 (Mapa) | Punto grande con anillo de color (indica composición) |
| 2 (Estratégico) | Forma reconocible (esfera/torpedo/red) con color de ruta |
| 3 (Táctico) | Cuerpo detallado con células adheridas visibles |
| 4 (Microscópico) | Cuerpo con orgánulos internos visibles, sectores HP |

---

# 25. Arte, sonido y feedback

## 25.1 Estilo visual

- **Exterior:** microscopía científica estilizada, paleta marina
- **Interior celular:** ilustración científica detallada (cell biology textbook)
- **Genome Lab portraits:** alta resolución, estilo libro de zoología antiguo

## 25.2 Feedback visual del estado del cuerpo

- **Cuerpo Completo:** células adheridas con contornos nítidos, animaciones sutiles (cilios moviéndose, pseudópodos vibrando)
- **Cuerpo Parcial:** algunas posiciones vacías visibles, células externas dañadas con grietas
- **Núcleo Expuesto:** la madre brilla, sus estructuras internas se vuelven visibles desde fuera, color saturado

## 25.3 Audio

- Adhesión: sonido orgánico de "fusión" suave
- Husk Drop: estampido suave de liberación masiva
- Núcleo Expuesto: cambio de música a tema épico
- Daño penetrante: sonido distintivo (no genérico) de perforación

---

# 26. Balance numérico inicial

## 26.1 Stats base de la madre

| Estadística | Cuerpo Completo | Cuerpo Parcial | Núcleo Expuesto |
|---|---|---|---|
| Daño hecho | x0.6 | x0.85 | x1.5 |
| Daño recibido | x0.85 | x1.0 | x0.8 |
| Velocidad | depende motoras | normal | x1.3 |
| Regeneración HP | +0.5/s | +0.3/s | +1.0/s (60s) |
| Stress generación | x0.8 | x1.0 | x2.0 |

## 26.2 Cap de cuerpo

**No hay cap duro.** El balance es económico:

| Adheridas | Costo de mantenimiento ATP/s |
|---|---|
| 1-4 | +1 por célula |
| 5-8 | +1.5 por célula |
| 9-12 | +2 por célula |
| 13-16 | +3 por célula |
| 17+ | +4 por célula |

Una madre con 20 adheridas necesita producir 60+ ATP/s solo para mantenerlas. Esto es **el cap natural**.

## 26.3 Recetas y mezcla

| Restricción | Valor inicial |
|---|---|
| Carga biológica máxima | 30 |
| Casta auxiliar máxima | 30% de carga (40% con Simbiosis) |
| Recetas guardadas simultáneas | 5 |

---

# 27. Campaña, escenarios y progresión

Sin cambios respecto a v5.10. La campaña se diseña post-MVP.

## 27.1 Estructura propuesta

| Capítulo | Contexto biológico | Nueva mecánica introducida |
|---|---|---|
| 1. LUCA | Caldo primordial | Movimiento, recolección |
| 2. Primera división | Origen procariotas | División, primera adhesión |
| 3. Endosimbiosis | Origen eucariotas | Núcleo, mitocondria |
| 4. Multicelularidad | Volvox y colonias | Cuerpo multicelular completo |
| 5. Diferenciación | Cnidarios primitivos | Castas auxiliares |
| 6. Edad de fagos | Crisis de epidemia | Daño penetrante |
| 7. Bloom anóxico | Gran oxidación | Combate ambiental masivo |
| 8. Apex evolutivo | Late game completo | Modo campeón completo |

---

# 28. Multiplayer y PvP

## 28.1 Modos planeados

| Modo | Estado |
|---|---|
| 1v1 | Vertical slice |
| 2v2 | Post-MVP |
| FFA | Post-MVP |
| Asynchronous (genoma vs genoma) | Post-MVP, conecta con potencial Lab futuro |

## 28.2 Balance PvP

El sistema de cuerpo multicelular agrega profundidad PvP:
- Build "cuerpo grande defensivo" tiene counter en daño penetrante
- Build "squad masivo" tiene counter en zonas químicas
- Build "campeón rápido" (sacrificar cuerpo temprano) tiene counter en presión sostenida

Ningún build domina universalmente. Se valida con telemetría.

---

# 29. Roadmap de implementación

## 29.1 Sprints prioritarios para v5.11

| Sprint | Duración | Objetivo |
|---|---|---|
| **Sprint A** | 2-3 semanas | Modos celulares completos sobre V5Directive |
| **Sprint B** | 4-6 semanas | Diferenciación al dividir + Genome Lab UI |
| **Sprint C** | 4-5 semanas | **Adhesión funcional + cuerpo multicelular** (el más crítico) |
| **Sprint D** | 2-3 semanas | Estado del cuerpo + estructuras de combate latentes |
| **Sprint E** | 2 semanas | Husk Drop + buffs metabólicos por estado |
| **Sprint F** | 2 semanas | Sistema de capas en combate + daño penetrante |
| **Sprint G** | 1-2 semanas | Drift hereditario (experimental) |

**Total estimado: 17-23 semanas (~5 meses) de trabajo con Claude Code.**

## 29.2 Priorización si hay que cortar

Si el tiempo aprieta, el orden de criticidad es:
1. Sprint A (modos) — base para todo lo demás
2. Sprint B (diferenciación) — resuelve problema de "casarse con ruta"
3. Sprint C (cuerpo multicelular) — identidad del juego
4. Sprint D (estado madre) — núcleo del campeón latente
5. Sprint E (Husk Drop) — táctica avanzada (puede simplificarse inicial)
6. Sprint F (capas combate) — refinamiento (puede simplificarse inicial)
7. Sprint G (drift) — opcional, post-MVP

---

# 30. Apéndices de datos

## 30.1 Tabla completa de pesos del Affinity Tracker

(Esta tabla se mantiene del v5.10 sin cambios. 30+ acciones × 12 rutas = 360+ entradas. Ver documento técnico separado.)

## 30.2 Tabla de costos de estructuras

(Ver Sección 32 con catálogo completo.)

## 30.3 Tabla de cooldowns y abilities

(Cada subtipo tiene Q/E/R/F propios. Ver fichas individuales del v5.5.)

## 30.4 Telemetría inicial

Métricas a trackear desde el día 1:
- Tiempo en cada estado del cuerpo (Completo/Parcial/Expuesto)
- Frecuencia de Husk Drop
- Tasa de victoria por receta dominante
- Tiempo total de modo campeón en partidas ganadas
- Composición típica del cuerpo (motoras vs defensivas vs productoras)

---

*[Continúa en Mensaje 3 con secciones 31-35: Sistema de cuerpo multicelular en detalle, catálogo completo de estructuras/proteínas/enzimas/toxinas, balance dinámico de la madre, métricas y riesgos]*
# 31. Sistema de cuerpo multicelular (canon v5.11)

Esta sección documenta en detalle la mecánica core de v5.11: la construcción y operación del cuerpo multicelular alrededor de la madre.

## 31.1 Concepto

La madre construye un organismo multicelular emergente adhiriendo sus hijas en una grilla hexagonal. Cada célula adherida cumple un rol funcional según su subtipo. El cuerpo resultante es **único en cada partida** y define el estilo de juego.

## 31.2 La grilla hexagonal

La madre ocupa el centro. Alrededor hay anillos hexagonales de posiciones disponibles:

| Anillo | Posiciones | Distancia al núcleo |
|---|---|---|
| Ring 1 (interno) | 6 | 1u |
| Ring 2 | 12 | 2u |
| Ring 3 | 18 | 3u |
| Ring 4 (externo) | 24 | 4u |

Total potencial: 60 posiciones. Pero el costo metabólico hace que cuerpos > 16-20 sean inviables económicamente.

## 31.3 Reglas de posicionamiento

Cada subtipo tiene posiciones válidas según su rol:

| Tipo de célula | Anillos válidos | Razón biológica |
|---|---|---|
| Coraza / Defensa | Ring 4 (exterior) | Protege el resto |
| Motora (Flagelado, Cilios) | Ring 4 (preferido), Ring 3 | Empuja desde afuera |
| Boca (Ameba, fagocito) | Ring 4 dirigida, Ring 3 | Proyecta hacia presa |
| Productora (Cianobacteria, Microalga) | Ring 2-3 (intermedio) | Acceso a luz pero protección |
| Sensora (Cilios receptores) | Ring 3-4 | Necesita exposición |
| Estructural interna (heterocisto, vacuola) | Ring 1 (cerca del núcleo) | Cerca del centro |
| Conectiva / Biomasa | Cualquier anillo | Relleno funcional |
| Apex / Especial | Centro o Ring 1 | Alta prioridad biológica |

## 31.4 Direccionalidad emergente

El cuerpo puede ser **simétrico** (esfera) o **direccional** (torpedo):

### Cuerpo simétrico (esfera)
- Distribución equilibrada en todos los anillos
- Sin frente claro
- Defensa pareja en todas direcciones
- Velocidad media
- **Ejemplo:** Volvox apex

### Cuerpo direccional (torpedo)
- Mayoría de motoras en un lado (atrás)
- Mayoría de bocas en el otro lado (frente)
- Velocidad alta direccional
- Vulnerable a ataques laterales
- **Ejemplo:** Ameba con flageladas atrás y bocas adelante

### Cuerpo lineal (cadena)
- Células en línea recta (no en grilla circular)
- Cada célula afecta a sus 2 vecinas
- Romper el medio divide en 2 cuerpos parciales
- **Ejemplo:** Anabaena con heterocistos

### Cuerpo en red (Hongo)
- Células conectadas por hifas largas, no necesariamente adyacentes
- Comparten ATP por la red
- Resistente a daño localizado
- **Ejemplo:** Hongo apex micorrizal

El sistema **detecta automáticamente** la forma del cuerpo y aplica los modificadores correspondientes.

## 31.5 Sistema de adhesión

### Tipos de vínculo

| Tipo | Subtipos que lo usan | Comportamiento | Resistencia |
|---|---|---|---|
| **Adhesión colonial** | Microalga, Cianobacteria, Diatomea | Permanente, defensa compartida | Alta |
| **Conjugación procariota** | Bacterias | Temporal (30s), comparten genes | Baja |
| **Filamento de hifa** | Hongo, Moho | Permanente, comparten ATP | Media |
| **Cadena lineal** | Anabaena | Permanente, propagación de recursos | Media |
| **Adhesina cuticular** | Eucariotas con adhesina | Define cuerpo de organismo | Muy alta |
| **Sinapsis fagocitaria** | Ameba, Macrófago | Temporal (5s) durante fagocitosis | N/A |

### Cómo se forman vínculos

Al dividir y elegir "Adherir":
1. La hija nace adyacente a la madre (o a célula del cuerpo)
2. Aparece automáticamente un vínculo del tipo correspondiente
3. La célula contribuye con su rol funcional desde ese momento

Manualmente entre células ya existentes:
1. Seleccionar 2+ células compatibles
2. Tecla **G** (Glue) o comando "Adherir"
3. Si son compatibles, vínculo se forma (costo: 5 ATP cada una + 3s)

### Cómo se rompen vínculos

| Forma | Trigger |
|---|---|
| Daño dirigido al vínculo | El atacante apunta al joint visible |
| Desadherir voluntario | Tecla **Shift+G** o comando, costo ATP+tiempo |
| Husk Drop | Tecla **H**, libera todos los vínculos en 1s |
| Stress excesivo | Si una célula tiene stress > 90, su vínculo se rompe automáticamente |

### Compartir recursos en vínculos

Las células adheridas comparten parcialmente:

| Recurso | Tasa de compartir |
|---|---|
| ATP | 30% redistribuido cada 5s |
| Biomasa | 10% redistribuido cada 10s |
| HP (regeneración) | Suma neta entre células conectadas |
| Stress | Se distribuye en cadena (-15% promedio) |

Esto significa que una célula adherida con bajo HP puede recuperarse parcialmente del flujo de la red.

## 31.6 Estados del cuerpo y transiciones

### Tabla maestra de estados

| Estado | Adheridas | Daño hecho | Daño recibido | Velocidad | Stress/s | Buffs |
|---|---|---|---|---|---|---|
| **Cuerpo Completo** | 6+ | x0.6 | x0.85 | depende motoras | x0.8 | +25% ATP, +30% absorción, -15% costo división, +20% regen |
| **Cuerpo Parcial** | 3-5 | x0.85 | x1.0 | normal | x1.0 | +12% ATP, +15% absorción |
| **Núcleo Expuesto** | 0-2 | x1.5 | x0.8 | x1.3 | x2.0 (acelerado) | +25% regen ATP (60s), Estructuras combate activadas |

### Cómo se calcula la velocidad del cuerpo

```
velocidad_base = velocidad_de_madre_subtipo
si Cuerpo Completo:
    velocidad = velocidad_base * (motoras_adheridas / 4)
    minimo = velocidad_base * 0.3
    maximo = velocidad_base * 1.5
si Cuerpo Parcial:
    velocidad = velocidad_base * (1 + motoras_adheridas * 0.05)
si Núcleo Expuesto:
    velocidad = velocidad_base * 1.3 (con estructuras de locomoción activas multiplicador)
```

### Transiciones automáticas

El estado se recalcula **cada vez** que cambia el número de adheridas:
- División con adhesión → puede subir estado
- Muerte de adherida → puede bajar estado
- Husk Drop → instantánea a Núcleo Expuesto
- Readhesión → puede subir estado

### Notificaciones al jugador

```
🟢 CUERPO COMPLETO formado
Tu colonia ahora es un organismo multicelular.
Buffs activados.
```

```
🟡 CUERPO PARCIAL
Algunas células de tu cuerpo cayeron.
Buffs reducidos. Considera readherir o reagrupar.
```

```
🔴 NÚCLEO EXPUESTO
Tu madre quedó sola con su núcleo.
Estructuras de combate latentes activadas.
Tiempo límite por stress: 90s.
```

## 31.7 Husk Drop — la jugada crítica

### Mecánica

Tecla **H** (Husk Drop):
- Todas las células adheridas se liberan en 1 segundo
- Se vuelven squad libre con buffs temporales
- La madre recupera movilidad y entra en Núcleo Expuesto
- Cooldown 90 segundos antes de poder usarlo de nuevo

### Buffs post-Husk Drop

Aplicados a las ex-adheridas durante 30 segundos:
- +20% velocidad
- +15% daño cuando 3+ atacan al mismo target
- Resetean su modo a "Cazar / Atacar" automáticamente

Aplicados a la madre durante 60 segundos:
- +25% velocidad
- +25% regeneración ATP
- Estructuras de combate latentes activadas

### Cuándo conviene usarlo

- **En medio de combate:** liberás todo de golpe para asalto masivo
- **Si vas a perder el cuerpo igual:** mejor liberar voluntariamente y aprovechar buffs
- **Para escapar:** Núcleo Expuesto con velocidad +25% es más rápido que cuerpo grande lento
- **Para sorprender:** el rival enfrentando tu cuerpo no espera que de golpe te conviertas en swarm

### Cuándo NO usarlo

- En early game (no tenés estructuras de combate instaladas)
- Si la madre tiene HP bajo (Núcleo Expuesto la deja vulnerable a salvo de ser tank)
- Si no hay enemigos cerca (no aprovechás buffs de combate)

## 31.8 Reagrupar después de Núcleo Expuesto

Si las ex-adheridas sobreviven al combate, pueden volver:
- Acercarse a la madre (radio 2u)
- Seleccionar célula + tecla **G** (Glue)
- Costo: 10 ATP cada una + 5 segundos
- La madre vuelve a ganar adheridas y el estado sube

**Esto crea ciclos tácticos:** Cuerpo → Husk Drop → asalto masivo → reagrupación → Cuerpo nuevo.

---

# 32. Catálogo completo de estructuras

Lista exhaustiva de las 55+ estructuras del juego, organizadas por categoría.

## 32.1 Categoría 1: Organelos (slots fijos en madre)

La madre tiene 8 slots de organelos. Cada uno ocupa 1 slot (a menos que se indique lo contrario).

### Tier Básico (disponibles desde el inicio)

| Organelo | Función mecánica | Costo |
|---|---|---|
| **Núcleo** | Slot 1, requisito eucariotas | 50b + 30 ATP |
| **Mitocondria** | +25% ATP máximo colonia | 60b + 40 ATP |
| **Vacuola contráctil** | Regen HP +0.5/s madre | 40b + 25 ATP |
| **Tilacoide** | Permite fotosíntesis básica | 50b + 30 ATP + 5 minerales |
| **Lisosoma básico** | +15% daño hijas predadoras | 50b + 30 ATP |
| **Pseudópodo simple** | Movimiento ameboide básico | 30b + 20 ATP |

### Tier Intermedio (requieren progresión)

| Organelo | Función mecánica | Prerrequisito | Costo |
|---|---|---|---|
| **Núcleo reforzado** | +50% HP madre | Núcleo | 80b + 60 ATP |
| **Mitocondria múltiple** | +50% ATP máx, +20% regen ATP | Mitocondria | 100b + 70 ATP |
| **Cloroplasto** | Fotosíntesis avanzada | Tilacoide | 90b + 60 ATP + 15 min |
| **Lisosoma azurófilo** | +30% daño + digestión rápida | Lisosoma básico | 80b + 50 ATP |
| **Peroxisoma** | Resistencia ROS, neutraliza zonas oxidadas | — | 70b + 50 ATP + 10 lip |
| **Aparato de Golgi** | -10% tiempo producción hijas | Núcleo | 80b + 60 ATP + 10 amino |
| **Retículo endoplásmico** | -10% costo de estructuras nuevas | Núcleo | 80b + 60 ATP + 10 amino |
| **Citoesqueleto** | +20% velocidad madre | — | 60b + 40 ATP |

### Tier Apex (late game)

| Organelo | Función mecánica | Prerrequisito | Costo |
|---|---|---|---|
| **Núcleo polipoide** | +100% HP madre, regen +1/s | Núcleo reforzado + afinidad 70+ | 200b + 120 ATP + 25 nuc |
| **Mitocondria gigante** | +100% ATP máx, autonomía energética | Mitocondria múltiple | 200b + 140 ATP |
| **Citoesqueleto desarrollado** | +40% velocidad y +30% defensa | Citoesqueleto | 150b + 100 ATP + 20 amino |
| **Pseudópodos múltiples** | Atacar a 3 enemigos simultáneamente | Pseudópodo simple + afinidad Ameba 70+ | 180b + 120 ATP |
| **Magnetosoma** | Inmune a corrientes/desplazamiento | Afinidad cualquier ruta 60+ | 100b + 80 ATP + 20 min |
| **Aparato fagocitario completo** | Madre fagocita células de su tamaño | Lisosoma azurófilo + Pseudópodos múltiples | 250b + 150 ATP |
| **Sistema de regeneración** | Restaura células perdidas del cuerpo lentamente | Núcleo polipoide | 200b + 130 ATP + 30 mixtos |
| **Endosimbiosis activa** | Madre absorbe otra célula como organelo | — (especial) | Variable |

## 32.2 Categoría 2: Pared y membrana (1 slot exclusivo)

Solo se puede tener una pared/membrana activa simultáneamente.

| Estructura | Defensa | Modificadores | Costo |
|---|---|---|---|
| **Membrana simple** | base | — (default) | 0 |
| **Membrana rígida** | +20% | -10% velocidad | 40b + 25 ATP |
| **Pared celular básica** | +30% | Inmune a presión osmótica leve | 60b + 30 ATP |
| **Pared celular reforzada** | +50% | -15% velocidad | 100b + 60 ATP |
| **Cápsula de polisacáridos** | +15% | Evade reconocimiento NK | 50b + 30 ATP + 8 lip |
| **Glicocáliz** | +10% | Inmune parcial a toxinas | 60b + 40 ATP + 10 amino |
| **Frústula de sílice** | +60% | Inmóvil cuando activa | 120b + 70 ATP + 25 min |
| **Concha de carbonato** | +60% | -20% velocidad permanente | 100b + 60 ATP + 30 min |
| **Espículas siliceas** | +40% | Aura defensiva 0.5/s contacto | 130b + 80 ATP + 20 min |

## 32.3 Categoría 3: Locomoción (1-2 slots)

| Estructura | Velocidad | Modificadores | Costo |
|---|---|---|---|
| **Sin locomoción** | x0.5 | LUCA inicial | 0 |
| **Flagelo bacteriano** | x1.3 | Solo procariotas | 40b + 25 ATP |
| **Flagelo eucariota** | x1.4 | Requiere ATP +0.5/s | 60b + 40 ATP |
| **Cilios** | x1.0 | Genera corrientes locales | 60b + 35 ATP |
| **Cilios coordinados** | x1.0 | Corrientes amplias | 100b + 60 ATP |
| **Pseudópodos** | x0.9 | Movimiento adaptable | 30b + 20 ATP |
| **Filopodia** | x1.2 | Direccional rápido | 50b + 35 ATP |
| **Movimiento helicoidal** | x1.1 | Atraviesa biofilms | 80b + 50 ATP (Espirilo) |

Algunas combinaciones son posibles (Pseudópodos + Filopodia para Ameba avanzada).

## 32.4 Categoría 4: Proteínas y enzimas (4-6 slots)

Las proteínas/enzimas son **buffs activos** que tienen costo de mantenimiento (ATP/s).

| Proteína/Enzima | Función | Mantenimiento ATP/s | Costo activación |
|---|---|---|---|
| **Catalasa** | Inmune a ROS | 0.3 | 30 ATP + 8 amino |
| **Peroxidasa** | -50% daño toxinas oxidativas | 0.3 | 30 ATP + 8 amino |
| **Heat Shock Proteins** | -30% daño zonas extremas | 0.5 | 50 ATP + 12 amino |
| **Proteasas** | +25% velocidad digestión | 0.2 | 25 ATP + 6 amino |
| **Lipasas** | +30% eficiencia zonas grasas | 0.2 | 25 ATP + 8 amino |
| **Nitrogenasa** | Fijación N₂ autónoma | 0.8 | 80 ATP + 20 amino |
| **Adhesina** | Permite adhesión funcional | 0.1 | 20 ATP + 5 amino |
| **Quitin sintasa** | Permite pared de quitina | 0.4 | 40 ATP + 10 amino |
| **Fosfolipasas** | Daño penetrante a membranas | 0.5 | 60 ATP + 12 amino |
| **Antioxidantes** | -50% daño estrés oxidativo | 0.3 | 30 ATP + 10 amino |
| **Crioproteínas** | Resistencia a glaciación | 0.4 | 40 ATP + 12 amino |
| **Antibióticos pasivos** | Daño 0.3/s a procariotas en radio 3u | 0.6 | 60 ATP + 15 amino |

## 32.5 Categoría 5: Toxinas y armas químicas

Las toxinas son **abilities activables** con cooldown. No usan slot fijo, pero requieren genes específicos.

| Toxina | Efecto | Cooldown | Costo activar |
|---|---|---|---|
| **Toxina ácida** | Daño 1/s en radio 2u, persiste 30s | 45s | 30 ATP + 5 amino |
| **Toxina neurotóxica** | Stunea células rivales 3s | 60s | 40 ATP + 10 amino |
| **Toxina hemolítica** | Rompe membranas a contacto | 30s | 25 ATP + 8 amino |
| **Toxina dinoflagelada** | Letal anaerobios+fotótrofos zona | 90s | 60 ATP + 15 amino |
| **Toxina alelopática** | Inhibe crecimiento rival en radio | 40s | 30 ATP + 8 amino |
| **Quorum-sensing señal** | Convoca aliados al radio | 60s | 25 ATP |
| **Antibiótico activo** | Daño 0.5/s a procariotas en radio 4u por 20s | 75s | 50 ATP + 12 amino |
| **Citolisina** | Daño penetrante directo a 1 célula | 30s | 35 ATP + 10 amino |
| **Mucilago tóxico** | Zona pegajosa, ralentiza + daña | 50s | 40 ATP + 10 lip |

## 32.6 Categoría 6: Estructuras especiales (apex / forma campeona)

Solo disponibles a alta afinidad. Definen el campeón final.

| Estructura | Función | Prerrequisito |
|---|---|---|
| **Sistema de regeneración avanzado** | Restaura cuerpo perdido lentamente | Núcleo polipoide + afinidad 90+ |
| **Aparato fagocitario apex** | Devorar enjambre AOE | Pseudópodos múltiples + Lisosoma azurófilo |
| **Bloom hipersensitivo** | Genera bloom de fotosíntesis instantáneo | Cloroplasto + afinidad Cianobacteria 90+ |
| **Red micorrizal apex** | 18+ hifas, daño compartido | Hifas + afinidad Hongo 90+ |
| **Memoria colonial** | Guarda 3 firmas químicas, las activa según contexto | Quorum sensing + afinidad 80+ |
| **Switch metabólico apex** | Cambia metabolismo instantáneo sin cooldown | Múltiples vías metabólicas + afinidad Euglena 90+ |
| **Esporulación masiva** | Genera 4 esporas defensivas | Esporulación + afinidad 70+ |

## 32.7 Total y distribución

**Resumen:**
- 22 organelos (8 slots disponibles, decisión real)
- 9 paredes/membranas (1 slot exclusivo)
- 7 tipos de locomoción (1-2 slots)
- 12 proteínas/enzimas (4-6 slots activos)
- 9 toxinas (no usan slot)
- 7 estructuras especiales/apex

**Total: 66 elementos catalogados.**

Esto puede parecer mucho pero:
- No todos están disponibles desde el principio (gating por progresión)
- No todos compiten por los mismos slots (categorías separadas)
- Una partida típica usa 15-25 elementos, no los 66

---

# 33. Catálogo de proteínas, enzimas y toxinas

Esta sección expande el catálogo con justificación biológica para cada elemento. (Sirve como referencia para arte y narrativa.)

## 33.1 Por qué cada elemento existe biológicamente

| Elemento | Realidad biológica |
|---|---|
| Catalasa | Enzima real que descompone H₂O₂. Existe en todos los aerobios. |
| Peroxidasa | Familia de enzimas que reducen peróxidos. Las hay en bacterias, hongos, plantas. |
| Heat Shock Proteins | Chaperonas proteicas que protegen otras proteínas del calor. Ubicuas. |
| Nitrogenasa | Único complejo enzimático que rompe el triple enlace N₂. Solo en algunas procariotas. |
| Adhesina | Proteínas de superficie celular que median adhesión. Bacterias y muchas células eucariotas las tienen. |
| Crioproteínas | Anti-freeze proteins reales. Tardígrados, peces árticos, algunos insectos. |
| Toxina dinoflagelada | Saxitoxina (PSP), brevetoxina, etc. Causan mareas rojas reales. |
| Quorum-sensing | Sistema de comunicación bacteriana real. Vibrio, Pseudomonas, etc. |
| Quitin sintasa | Enzima que sintetiza quitina. Hongos, artrópodos, nematodos. |

**Cada elemento es real.** Esto convierte al juego en una "mini enciclopedia biológica jugable" sin ser educativo explícitamente.

## 33.2 Compatibilidad entre elementos

Algunas combinaciones son sinérgicas (potencian) o conflictivas (anulan):

### Sinergias

- **Catalasa + Cloroplasto:** la fotosíntesis genera ROS, Catalasa los neutraliza → +20% eficiencia fotosíntesis
- **Peroxidasa + Lisosoma azurófilo:** digestión más limpia → +15% recuperación de biomasa post-fagocitosis
- **Adhesina + Quitin sintasa:** vínculos super-resistentes → +50% HP de vínculos en cuerpo
- **Nitrogenasa + Anabaena adheridas:** producción de N₂ doble → -30% costo de aminoácidos
- **Heat Shock Proteins + Crioproteínas:** inmune a glaciación + zonas extremas → -50% daño ambiental

### Conflictos

- **Pared celular reforzada + Pseudópodos:** la pared rígida impide pseudópodos → no se pueden coexistir
- **Frústula de sílice + Locomoción:** la frústula te inmoviliza → solo combinable con Cilios coordinados
- **Cápsula + Reconocimiento por NK:** la cápsula protege pero si tu reconocimiento depende de NK pierde efectividad

---

# 34. Balance dinámico de la madre y transiciones

Esta sección documenta el balance numérico exacto del sistema multicelular.

## 34.1 Tabla maestra de balance

| Estado | Adheridas | Daño (out) | Daño (in) | Velocidad | Stress/s | ATP/s mantenimiento | Buffs metabólicos |
|---|---|---|---|---|---|---|---|
| Solo (Núcleo Expuesto) | 0 | x1.5 | x0.8 | x1.3 | x2.0 | 0 | Estructuras combate activadas |
| Núcleo Expuesto débil | 1-2 | x1.2 | x0.9 | x1.2 | x1.5 | -1 | Algunas estructuras combate activas |
| Cuerpo Parcial | 3-5 | x0.85 | x1.0 | x1.0 | x1.0 | -1 a -2 | +12% ATP, +15% absorción |
| Cuerpo Completo (mediano) | 6-8 | x0.7 | x0.9 | x0.85-x1.1 | x0.85 | -3 a -5 | +25% ATP, +30% absorción, -15% costo, +20% regen |
| Cuerpo Completo (grande) | 9-15 | x0.6 | x0.85 | x0.7-x1.4 | x0.8 | -6 a -10 | Como mediano + bonus emergente |
| Cuerpo masivo | 16+ | x0.55 | x0.85 | depende | x0.8 | -12+ | Como grande pero con costo metabólico crítico |

## 34.2 Cómo se calcula el daño en combate

```python
# Pseudocódigo
daño_efectivo = daño_base * modificador_estado_madre
                * modificador_modo_celular
                * modificador_modificadores_ambientales
                * modificador_genes_activos

# Ejemplo: Madre Ameba en Cuerpo Completo, modo Cazar, en zona oxigenada con gen Fagotrofia
daño = 25 * 0.6 * 1.4 * 1.1 * 1.2
daño = 25 * 1.108
daño = ~27.7
```

## 34.3 Cómo se calcula la velocidad

```python
# Pseudocódigo
if estado_madre == FullBody:
    motoras = contar_celulas_adheridas_tipo("motora")
    velocidad = velocidad_base * (motoras / 4) * modificador_locomoción
    velocidad = clamp(velocidad, base * 0.3, base * 1.5)
    
elif estado_madre == PartialBody:
    motoras = contar_celulas_adheridas_tipo("motora")
    velocidad = velocidad_base * (1 + motoras * 0.05)
    
elif estado_madre == NúcleoExpuesto:
    velocidad = velocidad_base * 1.3
    if estructura_instalada("Citoesqueleto desarrollado"):
        velocidad *= 1.2
```

## 34.4 Tiempos críticos del modo campeón

| Tiempo en Núcleo Expuesto | Stress acumulado | Estado |
|---|---|---|
| 0-30s | 0-60 | Pleno poder, óptimo |
| 30-60s | 60-90 | Pleno poder, advertencia |
| 60-90s | 90-120 | Stress crítico, daño autoinflingido |
| 90-120s | 120+ | Madre colapsa por agotamiento |

El temporizador puede detenerse si:
- La madre vuelve a tener 3+ adheridas
- Se activa la habilidad Criptobiosis (gen, una vez por partida)
- Se entra a una "zona de pánico químico" estabilizada

## 34.5 Cap dinámico por costo

No hay cap duro, pero el costo metabólico hace que cuerpos > 16-20 sean inviables a menos que tengas estructuras avanzadas:

```python
# Costo de mantenimiento por adherida
def costo_adherida(n_adheridas):
    if n_adheridas <= 4: return 1.0
    elif n_adheridas <= 8: return 1.5
    elif n_adheridas <= 12: return 2.0
    elif n_adheridas <= 16: return 3.0
    else: return 4.0

# Para sostener un cuerpo de 20:
# 20 * 4.0 = 80 ATP/s solo en mantenimiento
# Necesitás Mitocondria gigante + Cloroplasto + 8 productoras adheridas
```

---

# 35. Métricas, riesgos y mitigaciones

## 35.1 Métricas de éxito v5.11

Validar con telemetría desde el día 1:

| Métrica | Meta inicial | Qué valida |
|---|---|---|
| % de partidas con cuerpo formado (3+ adheridas) | >85% | El cuerpo multicelular es accesible |
| % de partidas con cuerpo grande (8+ adheridas) | >50% | El cuerpo grande es viable |
| Promedio de transiciones de estado por partida | 3-7 | Las transiciones son tácticas, no caóticas |
| % de partidas que llegan a Núcleo Expuesto | 30-50% | El modo campeón se ve, pero no es default |
| % de Núcleos Expuestos que se recuperan | >25% | Reagrupar es posible |
| Husk Drops por partida | 0-3 | Es jugada táctica, no spam |
| Tiempo en cada estado del cuerpo | balanceado | Ningún estado domina la partida |
| % de partidas con 3+ subtipos en colonia | >70% | Diferenciación funciona |
| Composición típica del cuerpo | varía por ruta | Identidad por ruta visible |
| Tiempo total en modo campeón en partidas ganadas | 30-90s | El campeón es momento épico, no constante |

## 35.2 Riesgos de diseño

### Riesgo 1: Sobrecarga de UX

**Síntoma:** El jugador no sabe qué hacer con tantas decisiones (modos + recetas + adhesión + posicionamiento + estructuras).

**Mitigación:**
- Defaults sensatos (la madre divide hijas plásticas adheridas por default)
- Tutorial progresivo: 1 mecánica por escenario
- Modo "Auto" donde la IA decide adhesión y modo
- UI minimalista por default, panel avanzado opcional

### Riesgo 2: Cuerpo dominante

**Síntoma:** El jugador descubre que cuerpos grandes ganan siempre y todos juegan igual.

**Mitigación:**
- Costo metabólico creciente
- Daño penetrante como counter (Espirilo, Bacteriófago, toxinas)
- Telemetría que detecta builds dominantes
- Ajuste de tasas de costo según playtest

### Riesgo 3: Modo campeón frustrante

**Síntoma:** El jugador siente que cuando entra en Núcleo Expuesto pierde inevitablemente, o al revés que es muy fuerte y se vuelve estrategia central.

**Mitigación:**
- Balance dinámico (60-90s de límite de stress)
- Reactivación si readhiere
- Estructuras de combate son inversión (jugador eligió tenerlas)
- Telemetría de % de campeones que ganan vs pierden

### Riesgo 4: Performance con cuerpos grandes

**Síntoma:** Cuerpos de 20+ células adheridas + squads activos genera lag en hardware bajo.

**Mitigación:**
- Cap suave por performance: si FPS < 30, sistema sugiere Husk Drop
- Optimización: agrupar células adheridas como una sola entidad táctica para cálculos
- Microcolonias visuales (canon v5.5) ya hace esto parcialmente

### Riesgo 5: Combinatoria rota

**Síntoma:** Una combinación específica (ruta + estructura + modo + receta) domina el meta.

**Mitigación:**
- Telemetría detallada por combinación
- Iteración de balance basada en data, no opinión
- Ajustes cuantitativos (no quitar features, ajustar números)
- Counters claros documentados desde el diseño

### Riesgo 6: Curva de aprendizaje brutal

**Síntoma:** Jugadores nuevos abandonan en hora 1 sin entender.

**Mitigación:**
- Escenarios introductorios (1 mecánica por capítulo)
- Tooltips contextuales que aparecen cuando son relevantes
- Coach virtual (recomendaciones del juego en momentos clave)
- Modo "Práctica" sin presión

## 35.3 Indicadores de cuándo iterar

Si en playtest observás cualquiera de estos, hay que iterar:

- Más del 70% de partidas terminan en Cuerpo Completo masivo (cuerpo dominante → revisar costos)
- Menos del 20% de jugadores usan Husk Drop (mecánica subutilizada → mejorar UX)
- Más del 60% de jugadores describen el juego como "confuso" (UX → simplificar)
- Modo campeón gana o pierde más del 70% (balance → ajustar stats)
- Menos del 40% de partidas tienen subtipos diversos (diferenciación no funciona → revisar afinidad)

## 35.4 Plan de validación

| Fase | Duración | Foco |
|---|---|---|
| Alpha cerrada | 2-4 semanas | 5-10 jugadores cercanos, validar core loop |
| Alpha abierta | 6-8 semanas | 30-50 jugadores, validar progresión y curvas |
| Beta cerrada | 8-12 semanas | 100-200 jugadores, validar PvP y balance |
| Beta abierta | 4 meses | 500+ jugadores, telemetría masiva |
| Soft launch | 2 meses | Mercado pequeño (Chile/región) antes de release global |

---

*[Continúa en Mensaje 4: Changelog v5.10 → v5.11, lista de impacto en código, entrega de archivos]*
