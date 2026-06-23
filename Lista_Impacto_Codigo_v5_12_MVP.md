# Lista de Impacto Codigo v5.12 MVP

**Objetivo:** convertir el pivote Madre / Cuerpo / Squads en sistemas concretos dentro del proyecto Unity actual.

Este documento es una cola de implementacion. No todo debe hacerse en un solo paso.

---

# 1. Estado Actual Relevante

El proyecto ya tiene varias piezas utiles:

| Sistema | Archivo | Estado |
| --- | --- | --- |
| Celulas, stats, estructuras, modos | `Assets/_ProjectV5/Scripts/Cells/V5CellEntity.cs` | base fuerte |
| Produccion germinal | `Assets/_ProjectV5/Scripts/Colony/V5GerminalProductionSystem.cs` | ya produce recetas |
| Recetas fenotipicas | `Assets/_ProjectV5/Scripts/Colony/V5PhenotypeRecipeLibrary.cs` | ya aplica madurez |
| Afinidad evolutiva | `Assets/_ProjectV5/Scripts/Evolution/V5EvolutionAffinitySystem.cs` | usable |
| Historial de afinidad | `Assets/_ProjectV5/Scripts/Evolution/V5AffinityEventLog.cs` | usable |
| Caps biologicos | `Assets/_ProjectV5/Scripts/Evolution/V5RosterBalance.cs` | usable |
| Combate | `Assets/_ProjectV5/Scripts/Combat/V5CombatSystem.cs` | necesita dano por capas |
| Seleccion RTS | `Assets/_ProjectV5/Scripts/RTS/V5SelectionSystem.cs` | necesita attach/detach |
| Squad tactics | `Assets/_ProjectV5/Scripts/RTS/V5SquadTacticsSystem.cs` | debe ignorar adheridas |
| Continuidad colonial | `Assets/_ProjectV5/Scripts/Colony/V5ColonialContinuitySystem.cs` | integrar cuerpo |
| HUD | `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs` | agregar estado corporal |
| Bootstrap | `Assets/_ProjectV5/Scripts/Core/V5GameBootstrap.cs` | registrar nuevos sistemas |

Conclusion: el cambio es grande, pero no es reinicio. Falta la capa corporal.

---

# 2. Nuevos Archivos

## 2.1 Obligatorios Sprint 1

| Archivo | Proposito |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Colony/V5MulticellularBodySystem.cs` | sistema central de slots, attach/detach, buffs y estado |
| `Assets/_ProjectV5/Scripts/Colony/V5BodySlotDefinition.cs` | datos simples de slots |
| `Assets/_ProjectV5/Scripts/UI/V5BodyPanelIMGUI.cs` | panel de estado corporal |

## 2.2 Utiles Sprint 2+

| Archivo | Proposito |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Debug/V5BodyDebugOverlay.cs` | gizmos/textos de slots para debug |
| `Assets/_ProjectV5/Scripts/Combat/V5BodyDamageRouter.cs` | opcional si el routing ensucia V5CombatSystem |
| `Assets/_ProjectV5/Scripts/Colony/V5BodyCompositionLibrary.cs` | opcional para buffs por rol |

---

# 3. Cambios por Archivo Existente

## 3.1 `V5Types.cs`

Agregar enums:

```csharp
public enum V5BodyRing { Nucleus, Inner, Outer }
public enum V5BodySlotRole { None, Armor, Motor, Producer, Mouth, Connector, Sensor, Reserve }
public enum V5BodyState { Exposed, Partial, Complete, Overloaded }
public enum V5CellDeploymentMode { Auto, FreeSquad, AttachedBody }
public enum V5AttachmentState { Free, Attached, Detaching, Cooldown }
```

Riesgo: bajo.

## 3.2 `V5GameManager.cs`

Agregar referencias:

```csharp
public V5MulticellularBodySystem Body;
public V5BodyPanelIMGUI BodyPanel;
```

Riesgo: bajo.

## 3.3 `V5GameBootstrap.cs`

Crear componentes al runtime:

- BodySystem;
- BodyPanel;
- debug overlay si corresponde.

Riesgo: bajo.

## 3.4 `V5CellEntity.cs`

Agregar estado:

```csharp
public V5AttachmentState AttachmentState;
public int BodySlotIndex = -1;
public float AttachmentCooldownUntil;
public bool IsAttachedToBody { get { return AttachmentState == V5AttachmentState.Attached; } }
```

Cambios:

- si esta adherida, no ejecutar `ThinkDirective` normal;
- si esta adherida, no ejecutar `MoveStep` normal;
- permitir que BodySystem controle posicion;
- preservar stats, HP, estructuras y receta;
- al morir, avisar al BodySystem.

Riesgo: medio-alto porque este archivo concentra mucha conducta.

Mitigacion: cambios pequenos y encapsulados, con early return claro para adheridas.

## 3.5 `V5CellFactory.cs`

Agregar overload o parametro de produccion:

```csharp
SpawnGerminalCell(position, mother, def, deploymentMode)
```

El spawn sigue igual, pero despues intenta attach si `deploymentMode == AttachedBody` o `Auto`.

Riesgo: medio.

## 3.6 `V5GerminalProductionSystem.cs`

Cambios:

- selector `SelectedDeploymentMode`;
- UI de destino;
- pasar destino al factory/body;
- mostrar slot recomendado;
- bloquear destino cuerpo si no hay slots.

Riesgo: medio.

## 3.7 `V5PhenotypeRecipeLibrary.cs`

Agregar helper:

```csharp
public static V5BodySlotRole RecommendedBodyRole(V5GerminalCasteId id)
```

Opcional:

```csharp
public static bool PrefersBody(V5GerminalCasteId id)
```

Riesgo: bajo.

## 3.8 `V5CombatSystem.cs`

Cambios Sprint 3:

- si target es madre y BodySystem tiene slots, routear dano;
- si target es celula adherida, aplicar dano a esa celula y fuga;
- piercing aumenta fuga;
- quimico puede afectar multiples slots.

Riesgo: alto.

Mitigacion: primero implementar una funcion publica en BodySystem:

```csharp
public bool TryAbsorbDamage(V5CellEntity nominalTarget, float amount, V5DamageKind kind, Vector2 source, out float leakedToMother)
```

## 3.9 `V5SelectionSystem.cs`

Cambios:

- hotkey `A` o boton para adherir seleccionadas cercanas;
- hotkey `X` para desprender seleccionadas adheridas o slot seleccionado;
- drag select ignora adheridas por defecto;
- modo edicion cuerpo en panel si se necesita.

Riesgo: medio.

## 3.10 `V5SquadTacticsSystem.cs`

Cambios:

- ignorar celulas adheridas al calcular blob penalty;
- ignorarlas al calcular cohesion de squad libre.

Riesgo: bajo.

## 3.11 `V5HudIMGUI.cs`

Cambios:

- estado de cuerpo;
- slots ocupados;
- fuga de dano;
- boton para abrir BodyPanel;
- warning "Nucleo expuesto".

Riesgo: medio por espacio visual.

## 3.12 `V5ColonialContinuitySystem.cs`

Cambios Sprint 4:

- sumar puntos si el cuerpo estaba estable antes de muerte;
- restar si madre murio con cuerpo destruido y sin sucesor;
- sucesor protegido por cuerpo cercano gana puntos.

Riesgo: medio.

---

# 4. Sprint 1 Detallado

## 4.1 `V5BodySlotDefinition.cs`

Contenido esperado:

```csharp
public struct V5BodySlotDefinition
{
    public int index;
    public V5BodyRing ring;
    public float angleDegrees;
    public float radius;
    public V5BodySlotRole preferredRole;
}
```

## 4.2 `V5MulticellularBodySystem.cs`

Responsabilidades Sprint 1:

- crear 6 slots internos;
- registrar celulas adheridas;
- liberar slots;
- mover celulas adheridas al transform relativo;
- calcular BodyState;
- exponer resumen para HUD;
- notificar al morir una adherida.

API sugerida:

```csharp
public bool TryAttach(V5CellEntity cell, V5BodySlotRole preferredRole);
public bool Detach(V5CellEntity cell, bool voluntary);
public void NotifyAttachedCellDied(V5CellEntity cell);
public V5BodyState CurrentState { get; }
public int OccupiedSlots { get; }
public int MaxSlots { get; }
public string Summary { get; }
```

## 4.3 Criterios de aceptacion Sprint 1

- compila sin errores;
- la madre puede tener 6 hijas orbitando;
- las adheridas no se van por AI;
- se pueden desprender;
- al morir una adherida, no queda slot fantasma;
- HUD/panel muestra estado.

---

# 5. Sprint 2 Detallado

## 5.1 Destino de produccion

Agregar en Germinal:

```csharp
public V5CellDeploymentMode SelectedDeploymentMode = V5CellDeploymentMode.Auto;
```

UI:

```text
Destino: [Auto] [Cuerpo] [Squad]
```

## 5.2 Mapeo receta -> rol

| Receta | Rol |
| --- | --- |
| PlasticDaughter | Connector |
| LineageGatherer | Producer |
| LineageScout | Motor |
| LineageDefender | Armor |
| LineageRaider | Mouth |
| AmoeboidGuard | Armor |
| BacterialSymbiont | Connector |
| MicroalgaSupport | Producer |

## 5.3 Criterios de aceptacion Sprint 2

- producir LIN-DEF con destino cuerpo la adhiere;
- producir LIN-SCT con destino squad la deja libre;
- Auto no falla silenciosamente;
- si no hay slot, crea squad o muestra razon;
- receta madura/inestable sigue funcionando.

---

# 6. Sprint 3 Detallado

## 6.1 Damage routing

Implementar primero dentro de BodySystem, no directamente en Combat si se puede.

Regla:

```text
Si madre recibe dano y cuerpo tiene slots:
  elegir slot protector
  aplicar 88-94% al slot
  filtrar 6-12% a madre
Si piercing:
  aumentar fuga
Si nucleo expuesto:
  no absorber
```

## 6.2 Criterios de aceptacion Sprint 3

- enemigos dañan capas antes que madre;
- dano piercing amenaza nucleo;
- perder slots cambia estado;
- floating text no satura pantalla;
- combate 6v6 no baja de 20s ni sube de 75s.

---

# 7. Validacion Tecnica

## 7.1 Compilacion

Usar el flujo actual de Unity batchmode/smoke runner si ya esta configurado.

Debe pasar:

- compile C#;
- escena prototipo carga;
- no NullReference en los primeros 30s;
- spawn inicial funciona;
- paneles IMGUI no explotan.

## 7.2 Smoke test manual

Checklist:

- iniciar run;
- abrir Genome Lab;
- producir hija libre;
- producir hija adherida;
- mover madre;
- verificar orbitas;
- seleccionar squad libre;
- combatir threat pequeno;
- perder una adherida;
- reconstruir.

---

# 8. Regla de Corte

No se implementa nada de esta lista hasta terminar Sprint 1-3:

- Hydra.
- Daphnia.
- Volvox ruta.
- Tardigrade como jugador.
- cuerpo con fisica.
- 60 slots.
- UI final no-IMGUI.
- campaña.
- multiplayer.
- apex victory.
- diez threats nuevos.

El proyecto necesita menos promesas y mas loop jugable.

---

# 9. Primer Ticket Real

**Ticket:** Implementar cuerpo logico minimo.

Entregables:

1. Enums nuevos.
2. `V5BodySlotDefinition.cs`.
3. `V5MulticellularBodySystem.cs`.
4. Integracion en GameManager/Bootstrap.
5. Campos de attachment en V5CellEntity.
6. Attach/detach manual.
7. Panel simple.
8. Compilacion Unity.

Este es el siguiente paso de programacion.
