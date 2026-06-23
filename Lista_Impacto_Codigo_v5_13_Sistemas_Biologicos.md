# Lista de Impacto Codigo v5.13 - Sistemas Biologicos Canonicos

## Objetivo

Pasar del MVP al proyecto original ordenando rutas, genes, estructuras, fenotipos, linaje y cuerpo bajo una fuente de verdad comun.

---

# 1. Cambios De Alta Prioridad

## 1.1 Crear `V5BiologyCanon.cs`

**Nuevo archivo recomendado:** `Assets/_ProjectV5/Scripts/Biology/V5BiologyCanon.cs`

Responsabilidad:

- Definir genes principales por ruta.
- Definir estructuras principales por ruta.
- Definir estructuras habilitadas por gen.
- Definir fenotipos naturales por ruta.
- Definir sesgo corporal por ruta.
- Exponer etiquetas biologicas de estructuras.

API sugerida:

```csharp
public static class V5BiologyCanon
{
    public static V5GeneId[] GenesForRoute(V5EvolutionPath path);
    public static V5StructureId[] StructuresForRoute(V5EvolutionPath path);
    public static V5StructureId[] StructuresUnlockedByGene(V5GeneId gene, V5CellDomain domain, V5EvolutionPath currentPath);
    public static V5GerminalCasteId[] NaturalPhenotypesForRoute(V5EvolutionPath path);
    public static V5BodySlotRole[] BodyBiasForRoute(V5EvolutionPath path);
    public static bool IsStructureRecommendedForRoute(V5StructureId structure, V5EvolutionPath path);
    public static bool IsStructureLicensedByGenes(V5StructureId structure, V5GeneSystem genes, V5CellEntity cell);
}
```

Primer uso: UI y diagnostico. No cambiar balance todavia.

---

## 1.2 Fusionar `L Roles` y `V Mejoras`

Archivos actuales:

- `Assets/_ProjectV5/Scripts/RTS/V5LineageSystem.cs`
- `Assets/_ProjectV5/Scripts/Lineages/V5LineageUpgradeSystem.cs`

Problema:

- Ambos representan "como trabaja mi linaje".
- Para el jugador son un solo sistema.

Implementacion recomendada:

- Crear `V5LineagePanelIMGUI.cs`.
- Mantener `V5LineageSystem` como logica de roles.
- Mantener `V5LineageUpgradeSystem` como logica de upgrades.
- Unificar UI en tabs:
  - `Roles`
  - `Mejoras`
  - `Doctrina` (placeholder)

Hotkeys:

- `L`: abre Linaje.
- `V`: alias temporal de Linaje o queda libre para futuro Evolution Planner.

---

## 1.3 Reordenar Panel Interior

Archivo:

- `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs`

Cambiar `DrawInteriorPanel` para agrupar estructuras:

1. Recomendadas por ruta actual.
2. Permitidas por genes.
3. Exploratorias.
4. Bloqueadas.

Nueva logica:

- `CanInstall` sigue protegiendo recursos/dominio/carga.
- UI agrega explicacion biologica:
  - "Recomendada para Hongo."
  - "Permitida por Adhesion."
  - "Exploratoria: instalable, pero sube stress/afinidad rara."
  - "Bloqueada: falta gen, dominio o recursos."

---

## 1.4 Ajustar Genes Para Que Sean Licencias

Archivo:

- `Assets/_ProjectV5/Scripts/Evolution/V5GeneSystem.cs`

Problema actual:

- `ApplyImmediateEffect` instala estructuras directas.
- Esto pisa el rol del Panel Interior.

Direccion:

- Mantener efectos inmediatos pequenos.
- Evitar instalar organelos finales siempre.
- Ejemplo:
  - Motility puede dar sensor + desbloquear flagelos/cilios.
  - Adhesion desbloquea Fimbriae/Hypha/Mucilage segun ruta.
  - Photosynthesis desbloquea Thylakoid o MicroalgalChloroplast, pero no decide sola.

Transicion segura:

- Sprint 1: no cambiar gameplay, solo agregar canon y labels.
- Sprint 2: cambiar UI para mostrar licencias.
- Sprint 3: mover installs automaticos a "starter structure" o quitar algunos.

---

# 2. Redundancias Con Accion Recomendada

| Redundancia | Archivos | Accion |
| --- | --- | --- |
| Estructuras instaladas por Genes y por Interior | `V5GeneSystem`, `V5HudIMGUI`, `V5CellEntity` | Genes = licencias; Interior = instalacion |
| Roles y Mejoras separados | `V5LineageSystem`, `V5LineageUpgradeSystem` | Panel unificado `Linaje` |
| Ciliado y Rotifero con cilios | `V5EvolutionLibrary`, `V5PhenotypeRecipeLibrary` | Ciliado = corrientes; Rotifero = filtrador multicelular |
| Hongo y Moho como red/detritus | `V5EvolutionAffinitySystem`, `V5EvolutionLibrary` | Hongo fijo; Moho memoria quimica movil |
| Arquea usando peptidoglicano | `V5EvolutionLibrary` | Crear futura `SLayerEnvelope` o `PseudomureinWall` |
| Fotosintesis empuja procariota y microalga a la vez | `V5GeneSystem`, `V5CellEntity.ApplyMetabolism` | Separar Tilacoide vs Cloroplasto |
| Rutas inmunes retiradas siguen en enum | `V5Types`, `V5EvolutionLibrary`, `V5RosterBalance` | Mantener enum por compatibilidad, ocultar de UI y usar rasgos |

---

# 3. Cambios De Balance/Diseno Recomendados

## 3.1 Rutas con mecanica unica antes de ampliar roster

Antes de agregar organismos nuevos, cada ruta principal debe tener una mecanica distintiva:

| Ruta | Mecanica minima pendiente |
| --- | --- |
| Bacteria | Quorum/microcolonia o biofilm que escala por cercania |
| Arquea | Control de acidez/minerales y tolerancia extrema |
| Cianobacteria | Frente de oxigeno/luz que cambia el mapa |
| Ameba | Fagocitosis por contacto con riesgo de digestion |
| Flagelado | Dash/raideo o evasion por gradiente |
| Ciliado | Corriente local que arrastra presas/particulas |
| Microalga | Produccion luminica y vulnerabilidad a sombra |
| Hongo | Red fija que digiere y fortifica territorio |
| Moho | Memoria quimica y rutas optimizadas a detritus |
| Rotifero | Filtracion anti-swarm |
| Nematodo | Perforacion lineal contra cuerpo/biofilm |

## 3.2 Estructuras con tags

Agregar enum futuro:

```csharp
public enum V5StructureTag
{
    Core,
    Metabolic,
    Locomotion,
    Defense,
    Predation,
    Network,
    Sensor,
    Phototrophy,
    Microfauna,
    LatentApex
}
```

Uso:

- UI de Interior.
- Afinidad.
- Germinal maturity.
- Tooltips.

---

# 4. Orden Programable

## Sprint 1 - Canon sin gameplay risk

- Crear `V5BiologyCanon.cs`.
- Agregar smoke/editor check basico:
  - cada ruta primaria tiene genes;
  - cada ruta primaria tiene estructuras;
  - cada ruta primaria tiene al menos 1 rol corporal;
  - Tardigrado no aparece como primaria.

Riesgo: bajo.

## Sprint 2 - UI consultando canon

- Interior muestra categorias biologicas.
- Germinal muestra "natural para ruta" y "auxiliar".
- Gene panel muestra "habilita".

Riesgo: bajo-medio.

## Sprint 3 - Fusion Linaje

- Crear panel unificado.
- Quitar duplicacion visible L/V.
- Mantener hotkeys por compatibilidad.

Riesgo: bajo.

## Sprint 4 - Genes como licencias reales

- Reducir auto-install de estructuras en genes.
- Ajustar tutorial/misiones.
- Ajustar smoke tests.

Riesgo: medio.

## Sprint 5 - Diferenciacion de rutas

- Implementar una mecanica unica por ruta por orden:
  1. Bacteria quorum/biofilm.
  2. Hongo fijo vs Moho memoria.
  3. Ciliado corriente.
  4. Arquea acidez.
  5. Nematodo perforacion.

Riesgo: medio-alto.

---

# 5. Nota De Implementacion

No borrar enums de rutas retiradas todavia. Pueden existir saves, sistemas o referencias que dependan de esos valores.

En vez de borrar:

- marcar como `Retired` en `V5RosterBalance`;
- ocultar de UI;
- migrar fantasia a estructuras/fenotipos;
- eliminar solo cuando haya migracion de save clara.
