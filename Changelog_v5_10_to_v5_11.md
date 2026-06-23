# Changelog: GDD v5.10 → v5.11

> Documento que lista exactamente qué cambió entre las dos versiones del GDD. Sirve para revisar rápido las modificaciones antes de implementar en código.

## Resumen ejecutivo

v5.11 introduce **dos cambios estructurales grandes** sobre v5.10:

1. **Sistema de cuerpo multicelular** (Adhesión funcional sube de "fase 2" a mecánica core)
2. **Madre como campeón latente** (Las estructuras de combate se activan al perder el cuerpo)

Y **tres cambios secundarios**:

3. Balance dinámico de la madre con 3 estados (Cuerpo Completo / Parcial / Núcleo Expuesto)
4. Husk Drop como jugada táctica (tecla H, libera todo el cuerpo)
5. Sistema de capas en combate (daño físico vs penetrante vs químico)

El resto del v5.10 se mantiene: modos celulares, Genome Lab con recetas, Affinity Tracker con memoria causal, las 12 rutas, Director Ecológico, sucesión colonial.

---

## Cambios sección por sección

### Sección 1 — Visión y cambio de dirección
**Cambio:** Reescrita completamente.
- Se agrega narrativa de partida en 5 fases (Cría → Construcción → Fortaleza → Caída → Campeón)
- Tabla comparativa v5.10 → v5.11
- Posicionamiento competitivo expandido

### Sección 2 — Pilares de diseño
**Cambio:** Se agregan 2 pilares nuevos.
- Pilar 11: Cuerpo multicelular como identidad
- Pilar 12: Campeón latente

### Sección 3 — Experiencia objetivo
**Cambio:** Nueva fantasía intentada.
- Antes: "Soy una colonia viva, no un cursor encima de una base"
- Ahora: "Soy una colonia viva que construye un organismo multicelular único, despliega squads tácticos, y guarda como último recurso una célula germinal que se vuelve guerrera definitiva si todo lo demás cae"

### Sección 5 — Core loop
**Cambio:** Se agrega "decisión recurrente clave" al dividir.
- Cada división ahora pregunta: subtipo + adherir vs squad libre + modo inicial
- Las decisiones de cuerpo se incorporan a los 3 horizontes temporales

### Sección 7 — Las tres capas jugables
**Cambio:** Reescrita completamente.
- Antes había 3 capas vagas (interior celular, ecosistema, ecología social)
- Ahora son 3 capas claras y bien definidas:
  - Capa 1: Builder interior (anatomía de la madre)
  - Capa 2: Cuerpo multicelular (anatomía de la colonia)
  - Capa 3: Squads libres (RTS clásico)

### Sección 8 — Modelo de datos de célula
**Cambio:** Se agrega CellBodyState al modelo.
- Nuevos campos: BodyState, Links, BodyPosition, AttachedToMotherId
- Nueva clase MotherCellState para el estado del cuerpo de la madre

### Sección 9 — Recursos y economía
**Cambio mayor:** Se agregan **buffs metabólicos por estado del cuerpo**.
- Cuerpo Completo: +25% ATP, +30% absorción, -15% costo división, +20% regen
- Cuerpo Parcial: bonus reducidos (proporcionales)
- Núcleo Expuesto: bonus de adrenalina temporal (60s)
- Husk Drop: buffs de liberación (30-60s)

### Sección 10 — Sistema de ambiente
**Cambio:** Se agrega sub-sección "Cómo afecta el cuerpo multicelular al ambiente".
- Cuerpos grandes generan huella ambiental densa
- Cadenas de Anabaena fijan corredores de N
- Esferas Microalga+Cianobacteria estabilizan O₂

### Sección 11 — Builder interior celular
**Cambio:** Reescrita con 3 grupos de estructuras.
- Grupo 1: Metabólicas (importan en construcción)
- Grupo 2: Combate (latentes hasta caída del cuerpo)
- Grupo 3: Identidad/Llaves (puertas evolutivas)
- Antes era una lista plana sin propósito narrativo

### Sección 13 — División celular y linajes
**Cambio crítico:** Reescrita completamente.
- Agrega panel de división con elección "adherir vs squad"
- Agrega sistema de recetas vs división rápida
- Agrega reglas de posicionamiento en grilla hexagonal
- Agrega Husk Drop (tecla H)

### Sección 14 — Directivas RTS y modos celulares
**Cambio:** Se agrega "Modos en cuerpo vs squad".
- Modos en cuerpo: interpretados dentro del rol estructural
- Modos en squad: completamente flexibles
- El resto se mantiene del v5.10

### Sección 15 — Árbol evolutivo y genes
**Cambio menor:** Se agrega sub-sección "Genes de supervivencia".
- 5 genes que se activan automáticamente en Núcleo Expuesto
- Heat Shock Response, SOS Response, Quorum-sensing, Esporulación, Criptobiosis

### Sección 16 — Affinity Tracker
**Cambio:** Se agrega sub-sección "Desbloqueos por afinidad en v5.11".
- Afinidad ya no solo desbloquea identidad
- Desbloquea **opciones tácticas del cuerpo multicelular**
- 6 desbloqueos específicos por ruta

### Sección 19 — Formas apex y campeón latente
**Cambio crítico:** Reescrita completamente.
- Antes: forma apex como transformación de madre
- Ahora: forma apex como **organismo emergente** del cuerpo construido
- Nueva tabla: 13 apex con configuraciones requeridas
- Nueva sub-sección: "El campeón latente — canon v5.11"
- Activación retroactiva de estructuras de combate
- Tiempo límite (60-90s) por stress acelerado
- Reactivación posible si readhiere

### Sección 20 — Combate, capas y daño penetrante
**Cambio crítico:** Reescrita completamente.
- Nuevo sistema de capas: daño físico va a célula adherida más cercana
- Nuevos tipos de daño: contacto / químico / penetrante / stress
- Daño penetrante: Espirilo, Nematodo, Bacteriófago, toxinas
- Combate cuerpo vs squad y cuerpo vs cuerpo

### Sección 23 — UI/UX
**Cambio:** Se agrega panel del cuerpo multicelular.
- Vista hexagonal con células adheridas visibles
- Buffs activos según estado
- Estructuras de combate latentes (con indicador de activación pendiente)
- Botón Husk Drop accesible
- Genome Lab con placa biológica detallada (estilo Dune)

### Sección 26 — Balance numérico
**Cambio:** Tabla maestra de stats por estado del cuerpo.
- 3 estados con valores numéricos exactos
- Cap dinámico por costo (no cap duro)
- Tiempos del modo campeón

### Sección 29 — Roadmap
**Cambio:** Nuevos sprints.
- Sprint A (3 sem): Modos celulares
- Sprint B (6 sem): Diferenciación + Genome Lab
- **Sprint C (5 sem): Adhesión funcional + cuerpo multicelular** ← NUEVO crítico
- **Sprint D (3 sem): Estado del cuerpo + estructuras combate latentes** ← NUEVO crítico
- Sprint E (2 sem): Husk Drop + buffs metabólicos
- Sprint F (2 sem): Sistema de capas en combate
- Sprint G (2 sem): Drift hereditario (experimental)

Total: 17-23 semanas (~5 meses).

### Sección 31 — Sistema de cuerpo multicelular (NUEVA)
Sección completamente nueva.
- Grilla hexagonal (4 anillos, 60 posiciones potenciales)
- Reglas de posicionamiento por subtipo
- Direccionalidad emergente (esfera vs torpedo vs cadena vs red)
- Sistema de adhesión (6 tipos de vínculo)
- Estados del cuerpo y transiciones
- Husk Drop en detalle
- Reagrupación

### Sección 32 — Catálogo completo de estructuras (NUEVA)
Sección completamente nueva.
- 22 organelos (3 tiers: Básico, Intermedio, Apex)
- 9 paredes/membranas
- 7 tipos de locomoción
- 12 proteínas/enzimas
- 9 toxinas
- 7 estructuras especiales/apex
- **Total: 66 elementos catalogados**
- Cada uno con costo, función y prerequisito

### Sección 33 — Catálogo de proteínas, enzimas y toxinas (NUEVA)
Sección completamente nueva.
- Justificación biológica real de cada elemento
- Sinergias y conflictos entre elementos

### Sección 34 — Balance dinámico de la madre (NUEVA)
Sección completamente nueva.
- Tabla maestra de balance (6 estados con stats exactos)
- Pseudocódigo de cálculo de daño
- Pseudocódigo de cálculo de velocidad
- Tiempos críticos del modo campeón
- Cap dinámico por costo

### Sección 35 — Métricas, riesgos y mitigaciones (NUEVA)
Sección completamente nueva.
- 11 métricas de éxito con metas iniciales
- 6 riesgos identificados con mitigaciones
- Indicadores de cuándo iterar
- Plan de validación (alfa → beta → soft launch)

---

## Cambios en sistemas existentes (sin romper código v5.10)

### V5CellEntity
- **Agregar:** campos `BodyState`, `Links`, `BodyPosition`, `AttachedToMotherId`
- **Agregar:** lógica de modificadores de stats según `BodyState`
- **No romper:** la mayoría de getters/setters existentes funcionan igual

### V5GerminalProductionSystem
- **Modificar:** la división ahora abre panel con elección "adherir vs squad"
- **Mantener:** sistema de recetas funciona igual
- **Agregar:** lógica de posicionamiento hexagonal

### V5CombatSystem
- **Agregar:** sistema de capas (daño va a célula adherida más cercana)
- **Agregar:** tipos de daño (físico, químico, penetrante, stress)
- **Mantener:** mecánicas básicas de combate squad vs squad

### V5EvolutionAffinitySystem
- **Mantener:** todo el sistema v5.9-v5.10 igual
- **Agregar:** desbloqueos de opciones de cuerpo multicelular
- **Conectar:** afinidad alta abre nuevas configuraciones de adhesión

### V5SquadTacticsSystem
- **Mantener:** cohesión, blob penalty, formaciones igual
- **Agregar:** lógica especial cuando squad incluye células ex-adheridas (post Husk Drop)

### V5EnvironmentGrid
- **Mantener:** difusión química igual
- **Agregar:** "huella ambiental" generada por cuerpos grandes inmóviles

### V5GameManager
- **Agregar:** tracking de estado del cuerpo, tiempos en cada estado, transiciones
- **Mantener:** todo lo demás del v5.10

---

## Lo que NO cambia respecto a v5.10

Para que tengas claridad de qué se mantiene:

✓ Las 12 rutas evolutivas (Bacteria, Arquea, Cianobacteria, Espirilo, Anabaena, Ameba, Flagelado, Ciliado, Microalga, Diatomea, Euglena, Hongo, Moho)
✓ El cap de 24 entidades controlables
✓ El sistema de microcolonias visuales
✓ La grilla ambiental con 8 variables
✓ El Affinity Tracker con memoria causal v5.9
✓ La Sucesión Colonial (cuando muere madre, sucesor viable)
✓ El Director Ecológico con threats adaptativos
✓ Los 6 modos celulares canónicos
✓ Las 5 condiciones de victoria
✓ Tardígrado como NPC neutral, no ruta jugable
✓ El sistema de recetas en Genome Lab
✓ Los 6 estados de Living Battlefield
✓ La fantasía core de "soy una colonia viva"

---

## Archivos del v5.11 entregados

1. **Protogenesis_GDD_v5_11_Cuerpo_Multicelular.docx** — Documento completo en Word
2. **Protogenesis_GDD_v5_11_Cuerpo_Multicelular.md** — Documento completo en Markdown
3. **Changelog_v5_10_to_v5_11.md** — Este documento
4. **Lista_Impacto_Codigo_v5_11.md** — Qué scripts de Unity hay que modificar y en qué orden
