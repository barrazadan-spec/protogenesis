# Protogenesis Primordia — V5 Prototype 1.2

Iteración 1.2 enfocada en **sistemas de ecología a largo plazo**. La colonia ya no solo crece: ahora genera relaciones internas, sufre crisis externas y acumula deuda adaptativa si se sobreespecializa.

## Nuevos sistemas

### 1. Red ecológica (`/`)
Archivo: `Assets/_ProjectV5/Scripts/Ecology/V5EcologicalRelationsSystem.cs`

Calcula mutualismo, competencia, depredación, estabilidad simbiótica, diversidad evolutiva, cobertura de roles de linaje y penalización por monocultivo.

Políticas disponibles:

- Balanced.
- Mutualistic Matrix.
- Competitive Selection.
- Defensive Symbiosis.
- Predatory Web.

Efectos jugables:

- Colonias diversas y bien colonizadas reducen stress.
- Monocultivos bajo toxinas compiten más y pierden biomasa.
- Biofilm + estabilidad simbiótica mejora detox y colonización local.
- Redes predadoras empujan daño físico de linajes depredadores.

### 2. Crisis + estabilidad (`[`)
Archivo: `Assets/_ProjectV5/Scripts/Crisis/V5CrisisAndStabilitySystem.cs`

Crisis disponibles:

- Osmotic Shock.
- Toxic Runoff.
- UV Pulse.
- Resource Crash.
- Predator Migration.
- Acid Storm.

Mutaciones negativas / liabilities:

- Fragile Membrane.
- Metabolic Leak.
- Heredity Noise.
- Autoimmune Drift.
- Resource Hunger.

Reglas:

- La **deuda adaptativa** sube por carga de estructuras, stress, exceso de población y competencia ecológica.
- Catalasa y Stem Plasticity reducen deuda.
- Al cruzar umbrales aparecen liabilities.
- `Estabilizar colonia` consume recursos para bajar deuda, stress y quitar una liability.

## Nuevo auto-instalador

`V5Prototype12AutoInstaller.cs` agrega automáticamente:

- `V5EcologicalRelationsSystem`
- `V5CrisisAndStabilitySystem`

No necesitas añadir objetos manualmente en escena.

## Controles nuevos

```text
/     abrir/cerrar Red ecológica
[     abrir/cerrar Crisis + Estabilidad
Ctrl+[ forzar crisis de test
```

## Nota de integración

Todo sigue aislado en `Assets/_ProjectV5/`. La versión de save/balance cambió a `1.2`.
