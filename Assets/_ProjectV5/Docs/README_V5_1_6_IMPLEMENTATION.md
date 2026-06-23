# Protogenesis: Primordia — V5 Prototype 1.6

Iteración enfocada en **macroestrategia RTS**: posturas globales, formaciones, inteligencia de counters y plan explícito de victoria.

## Nuevos sistemas

### 1. Strategic Posture System
Archivo: `Assets/_ProjectV5/Scripts/Strategy/V5StrategicPostureSystem.cs`

Hotkey: `Home`

Posturas disponibles:

- **Balanced**: sin sesgo global.
- **Expansion**: acelera colonización en células colonizadoras, con leve aumento de stress.
- **Fortify**: mejora reparación, detox local y defensa alrededor de la madre.
- **Predation**: mejora presión de contacto en células atacantes/depredadoras, consume ATP.
- **Terraform**: refuerza el impacto del metabolismo sobre el ambiente.
- **Recovery**: baja stress y cura lentamente, a costa de ATP.

### 2. Formation Planner
Archivo: `Assets/_ProjectV5/Scripts/Formations/V5FormationPlannerIMGUI.cs`

Hotkey: `` ` ``

Formaciones disponibles:

- **Swarm**: agrupa en blob alrededor del cursor.
- **Anillo madre**: defensa circular alrededor de la madre.
- **Pantalla**: línea entre madre y cursor.
- **Línea**: despliegue lineal centrado en cursor.
- **Fan farmeo**: dispersa recolectoras y asigna farm.
- **Red colonial**: distribuye colonizadores alrededor del cursor.

Si hay células seleccionadas, opera sobre ellas. Si no hay selección, opera sobre toda la colonia excepto la madre.

### 3. Counterplay Intel
Archivo: `Assets/_ProjectV5/Scripts/Intel/V5CounterplayIntelSystem.cs`

Hotkey: `Delete`

Escanea amenazas enemigas y recomienda:

- estructura counter sugerida;
- objetivo prioritario;
- orden de ataque para depredadores/atacantes;
- consejo táctico según amenaza dominante.

### 4. Victory Plan System
Archivo: `Assets/_ProjectV5/Scripts/Meta/V5VictoryPlanSystem.cs`

Hotkey: `ScrollLock`

Permite elegir un plan explícito de victoria:

- **Ecological Dominance**: colonizar 40% de la gota.
- **Stable Biosphere**: estabilizar oxígeno, toxinas, pH y colonización.
- **Apex Ascension**: llegar a forma apex.
- **Predatory Elimination**: eliminar amenazas activas.
- **Research Supremacy**: completar research.

El sistema muestra progreso y entrega micro-bonuses suaves alineados con el plan activo.

### 5. Auto-installer 1.6
Archivo: `Assets/_ProjectV5/Scripts/Core/V5Prototype16AutoInstaller.cs`

Instala automáticamente los sistemas nuevos cuando existe `V5GameManager` en escena.

## Archivos modificados

- `V5Balance.cs`: `SaveVersion = 1.6`.
- `V5HudIMGUI.cs`: build label actualizado a `V5 BUILD 1.6` y controles resumidos.
- `V5HotkeyOverlayIMGUI.cs`: controles actualizados.

## Cómo probar

1. Abrir Unity.
2. Crear escena con:

```txt
Protogenesis > V5 > Create Full Prototype Scene
```

3. Play.
4. Probar hotkeys nuevas:

```txt
Home       Postura estratégica
`          Formaciones RTS
Delete     Inteligencia de counters
ScrollLock Plan de victoria
```

## Nota de integración

Todo está aislado bajo `Assets/_ProjectV5/`. No toca los sistemas antiguos fuera de V5.
