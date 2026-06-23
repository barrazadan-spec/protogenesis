# Lista de impacto en código Unity — v5.11

> Documento técnico que lista qué scripts del proyecto Unity tienen que modificarse para implementar v5.11, en qué orden, y qué scripts nuevos hay que crear.

## Filosofía de cambios

- **Los scripts nuevos van con prefijo V6** (siguiendo la convención del v5.6).
- **Los scripts V5* existentes se modifican mínimamente** — solo donde es necesario.
- **Cada Sprint deja el juego jugable** — no se acumula deuda técnica.
- **Tests de regresión después de cada Sprint** para asegurar que sistemas anteriores siguen funcionando.

---

## Sprint A — Modos celulares (3 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6CellMode.cs` | Definición de un modo celular (nombre, comportamiento, stats) | ~120 líneas |
| `V6CellModeLibrary.cs` | Catálogo de los 6 modos canónicos por categoría | ~250 líneas |
| `V6ModeTransitionSystem.cs` | Lógica de transiciones manuales/automáticas/autónomas | ~200 líneas |
| `V6ModeUIRenderer.cs` | UI de selección de modo en panel de célula | ~150 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Agregar campo `CurrentMode`. Conectar tick con sistema de modos. |
| `V5SelectionSystem.cs` | Mostrar modo actual en panel de selección. |
| `V5HudIMGUI.cs` | Mostrar modo activo en HUD inferior. |

### Criterio de éxito

Test manual: una colonia de 6 Bacterias puede tener 3 modos diferentes simultáneamente, cambiar modos manualmente con teclas rápidas (1-6), y configurar reglas automáticas (HP<30% → Reparar).

---

## Sprint B — Diferenciación al dividir + Genome Lab UI (6 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6DifferentiationSystem.cs` | Lógica de qué subtipos disponibles según madre y desbloqueos | ~300 líneas |
| `V6SubtypeUnlockTracker.cs` | Tracker de qué subtipos desbloqueó la colonia | ~200 líneas |
| `V6NurseryUIRenderer.cs` | Panel del Vivero/Genome Lab al dividir | ~400 líneas |
| `V6PortraitGalleryRenderer.cs` | Sistema de ilustraciones detalladas estilo Dune | ~200 líneas |
| `V6RecipeSystem.cs` | Sistema de recetas de fenotipo (auto-producción) | ~250 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Función `Divide()` ahora abre Vivero o sigue receta. |
| `V5CellFactory.cs` | Spawn de célula recibe parámetro de subtipo target. |
| `V5EvolutionRoster.cs` | Reorganizar como árbol jerárquico con categorías. |
| `V5EvolutionAffinitySystem.cs` | Agregar callbacks de desbloqueo de subtipos. |

### Assets nuevos requeridos

- 12 ilustraciones detalladas (placeholder iniciales, arte final post-MVP)
- 12 sprites simplificados para gameplay (algunos ya existen en `/Prefabs/Units`)
- 4 íconos de categoría (Procariota, Eucariota predador, Eucariota productor, Red)

### Criterio de éxito

Test manual: una madre Bacteria puede dividir, abrir Vivero, ver 5 subtipos disponibles (Bacteria/Cianobacteria/Espirilo/Arquea/Anabaena con gating apropiado), elegir uno, y producir esa hija. Las hijas pueden ser de subtipos distintos coexistiendo en la colonia.

---

## Sprint C — Adhesión funcional + cuerpo multicelular (5 semanas)

**Este es el sprint más crítico de v5.11. Es la mecánica core.**

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6AdhesionLink.cs` | Componente de vínculo entre dos células | ~150 líneas |
| `V6AdhesionSystem.cs` | Lógica global de adhesión, vínculos, ruptura | ~400 líneas |
| `V6HexBodyGrid.cs` | Grilla hexagonal de posiciones del cuerpo | ~250 líneas |
| `V6BodyPositionValidator.cs` | Reglas de qué subtipos pueden ir dónde | ~180 líneas |
| `V6EmergentStructureRecognizer.cs` | Detecta cadenas, redes, esferas, torpedos | ~300 líneas |
| `V6CompositeEntityController.cs` | Comportamiento de organismos compuestos | ~280 líneas |
| `V6BodyVisualizer.cs` | Renderiza grilla hexagonal y vínculos visualmente | ~200 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Agregar campos `Links`, `BodyPosition`, `AttachedToMotherId`. Lógica de transferencia de recursos. |
| `V5SelectionSystem.cs` | Selección de cuerpo completo al click en madre. |
| `V5MovementSystem.cs` | Movimiento coordinado de cuerpo (madre + adheridas) como unidad táctica. |

### Tecnología Unity requerida

- **Joints 2D:** `HingeJoint2D` o `FixedJoint2D` para vínculos físicos
- **Constraints:** sistema de transferencia de recursos via mensajes (no joints físicos)
- **Hexagonal grid:** matemática de coordenadas axiales

### Criterio de éxito

Test manual: una madre Bacteria puede dividir y elegir "adherir al cuerpo". La hija aparece visualmente conectada a la madre. La adhesión es físicamente visible. Las dos comparten ATP. Si una recibe daño masivo, el vínculo se rompe. El jugador puede crear una cadena de 5 Anabaenas que se mueven juntas.

---

## Sprint D — Estado del cuerpo + estructuras de combate latentes (3 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6MotherBodyState.cs` | Sistema de 3 estados (Completo/Parcial/Núcleo Expuesto) | ~250 líneas |
| `V6ChampionModeActivator.cs` | Activación/desactivación retroactiva de estructuras combate | ~200 líneas |
| `V6BodyStateUIRenderer.cs` | Visualización del estado en HUD y panel | ~180 líneas |
| `V6StressTimerSystem.cs` | Timer de stress acelerado en Núcleo Expuesto | ~120 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Agregar `MotherCellState`, modificadores de stats por estado. |
| `V5CombatSystem.cs` | Aplicar modificadores de daño según estado del cuerpo. |
| `V5HudIMGUI.cs` | Mostrar estado actual del cuerpo y notificaciones de transición. |

### Criterio de éxito

Test manual: una madre con 8 Cianobacterias adheridas tiene Cuerpo Completo (buffs visibles). Si un atacante mata 3 adheridas, baja a Cuerpo Parcial (notificación, buffs reducidos). Si quedan 0-2, entra Núcleo Expuesto y las estructuras de combate instaladas se activan visualmente. El timer de stress aparece. Si readhiere 3+ células, el modo campeón se desactiva.

---

## Sprint E — Husk Drop + buffs metabólicos por estado (2 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6HuskDropSystem.cs` | Lógica de liberación masiva con tecla H | ~150 líneas |
| `V6BodyStateBuffsSystem.cs` | Aplica buffs metabólicos según estado | ~180 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Recibir buffs según estado de madre asociada. |
| `V5ResourceWallet.cs` | Aplicar modificadores de absorción y costo según buffs. |
| `V5InputSystem.cs` | Capturar tecla H para Husk Drop. |

### Criterio de éxito

Test manual: el jugador presiona H, todas las adheridas se liberan en 1s, las ex-adheridas reciben +20% velocidad por 30s, y la madre entra a Núcleo Expuesto con buffs de adrenalina. El cooldown de 90s se respeta.

---

## Sprint F — Sistema de capas en combate + daño penetrante (2 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6LayeredDamageSystem.cs` | Daño físico va a célula adherida más cercana | ~250 líneas |
| `V6PenetratingDamageSystem.cs` | Daño que ignora capas (Espirilo, Bacteriófago, toxinas) | ~150 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CombatSystem.cs` | Integrar nuevos sistemas de capas y penetrante. |
| `V5DamageProfile.cs` | Agregar tipos de daño (físico, químico, penetrante, stress). |

### Criterio de éxito

Test manual: una Ameba ataca a un cuerpo multicelular. Su daño físico va primero a las células adheridas externas. Cuando un Espirilo ataca, su daño penetra y va directo al núcleo de la madre. Las toxinas en zona afectan a todas las células del cuerpo simultáneamente.

---

## Sprint G — Drift hereditario experimental (2 semanas)

### Scripts nuevos a crear

| Script | Responsabilidad | Tamaño estimado |
|---|---|---|
| `V6DriftHeredity.cs` | Mutación pequeña de stats al dividir | ~150 líneas |
| `V6ColonyEvolutionTracker.cs` | Tracker acumulado por colonia | ~120 líneas |
| `V6SubspeciesArchive.cs` | Guardado y carga de subespecies (post-MVP) | ~100 líneas |

### Scripts V5 a modificar

| Script | Modificación |
|---|---|
| `V5CellEntity.cs` | Inicialización con drift de ±5% por stat. |

### Criterio de éxito

Test manual: dos partidas con la misma ruta inicial muestran drift visible al final (panel de "tu colonia evolucionó: velocidad +12%, HP -5%"). El drift no afecta PvP competitivo (flag de balance).

---

## Resumen de impacto total

### Scripts nuevos a crear (V6*)

**Total: 23 scripts nuevos** distribuidos en 7 sprints.

| Categoría | Scripts |
|---|---|
| Modos celulares (Sprint A) | 4 |
| Diferenciación + Genome Lab (Sprint B) | 5 |
| Adhesión + Cuerpo multicelular (Sprint C) | 7 |
| Estado madre + Campeón (Sprint D) | 4 |
| Husk Drop + Buffs (Sprint E) | 2 |
| Capas combate (Sprint F) | 2 |
| Drift (Sprint G) | 3 (post-MVP) |

### Scripts V5* a modificar

**Total: 13 scripts existentes** que requieren modificaciones (sin romperse, solo agregando funcionalidad).

| Script | Sprints que lo modifican |
|---|---|
| `V5CellEntity.cs` | A, B, C, D, E, G |
| `V5CombatSystem.cs` | D, F |
| `V5SelectionSystem.cs` | A, C |
| `V5CellFactory.cs` | B |
| `V5EvolutionRoster.cs` | B |
| `V5EvolutionAffinitySystem.cs` | B |
| `V5MovementSystem.cs` | C |
| `V5HudIMGUI.cs` | A, D |
| `V5ResourceWallet.cs` | E |
| `V5InputSystem.cs` | E |
| `V5DamageProfile.cs` | F |

### Estimación de trabajo total

**5 meses (17-23 semanas)** para implementar v5.11 completo con Claude Code.

Si hay que cortar:
- **MVP minimal:** Sprint A + B + C (14 semanas) — hay cuerpo multicelular básico
- **MVP estándar:** Sprint A + B + C + D + E (17 semanas) — incluye campeón
- **MVP completo:** Sprint A-F (19 semanas) — incluye combate por capas
- **Post-MVP:** Sprint G (drift)

---

## Recomendaciones de orden

1. **Empezá por Sprint A** aunque parezca menos urgente. Los modos celulares son base para todo lo demás.
2. **No saltees Sprint B antes de Sprint C.** El cuerpo multicelular sin diferenciación al dividir no se puede componer bien.
3. **Sprint C es el más arriesgado.** Hacé un prototipo simple antes de comprometer la implementación final. Ver si los joints 2D de Unity aguantan o si conviene vínculos lógicos sin físicos.
4. **Después de Sprint D, hacé un playtest serio.** Vas a tener el juego "v5.11 viable" funcionando. Validá el feel del modo campeón antes de seguir con E-F-G.
5. **Sprint E y F pueden simplificarse** si el playtest del Sprint D muestra que el sistema básico ya es suficientemente divertido.

---

## Banderas de balance / cambio rápido

Para facilitar iteración, recomiendo poner estas constantes en un único archivo `V6BalanceConstants.cs`:

```csharp
public static class V6BalanceConstants
{
    // Balance dinámico de la madre
    public const float DAMAGE_FULLBODY = 0.6f;
    public const float DAMAGE_PARTIAL = 0.85f;
    public const float DAMAGE_EXPOSED = 1.5f;
    
    public const float DEFENSE_FULLBODY = 0.85f;
    public const float DEFENSE_PARTIAL = 1.0f;
    public const float DEFENSE_EXPOSED = 0.8f;
    
    public const float SPEED_EXPOSED = 1.3f;
    
    public const float STRESS_EXPOSED_MULTIPLIER = 2.0f;
    public const float MAX_EXPOSED_TIME_SECONDS = 90f;
    
    // Buffs metabólicos
    public const float ATP_BUFF_FULLBODY = 0.25f;
    public const float ATP_BUFF_PARTIAL = 0.12f;
    public const float ABSORPTION_BUFF_FULLBODY = 0.30f;
    public const float DIVISION_COST_REDUCTION_FULLBODY = 0.15f;
    
    // Husk Drop
    public const float HUSK_DROP_COOLDOWN = 90f;
    public const float HUSK_DROP_SQUAD_BUFF_DURATION = 30f;
    public const float HUSK_DROP_MOTHER_BUFF_DURATION = 60f;
    public const float HUSK_DROP_SQUAD_SPEED_BUFF = 0.20f;
    public const float HUSK_DROP_SQUAD_DAMAGE_BUFF = 0.15f;
    
    // Costo de mantenimiento por adheridas
    public static readonly float[] MAINTENANCE_COST = { 1.0f, 1.5f, 2.0f, 3.0f, 4.0f };
    
    // Compartir recursos en vínculos
    public const float ATP_SHARING_RATE = 0.30f;
    public const float ATP_SHARING_INTERVAL = 5f;
    public const float BIOMASS_SHARING_RATE = 0.10f;
    public const float BIOMASS_SHARING_INTERVAL = 10f;
}
```

Esto permite que ajustes balance sin tocar lógica.
