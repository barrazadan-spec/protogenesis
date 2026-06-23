# Protogenesis Primordia — V5 Prototype 0.5

Esta iteración convierte el prototipo en una base más testeable y extensible.

## Nuevo en 0.5

### Escenarios parametrizados
Los escenarios ahora viven en `V5ScenarioLibrary` y controlan tamaño de mapa, recursos, enemigos, objetivo de colonización, condiciones especiales y sesgos ambientales.

Menús nuevos:

- `Protogenesis > V5 > Create Full Prototype Scene`
- `Protogenesis > V5 > Create Scenario > First Drop`
- `Protogenesis > V5 > Create Scenario > Oxygen War`
- `Protogenesis > V5 > Create Scenario > Acid Frontier`
- `Protogenesis > V5 > Create Scenario > Predator Bloom`
- `Protogenesis > V5 > Create Scenario > Freeplay Sandbox`

### Guardado / carga de run
Sistema `V5SaveSystem` con quicksave en `PlayerPrefs`.

- `F5`: guardar run.
- `F9`: cargar run.

Guarda:

- tiempo de run;
- escenario;
- genes activos;
- células jugador/enemigas;
- recursos, stats, dominio, metabolismo, estructuras y posición.

Nota: el estado completo de la grilla ambiental aún no se serializa para evitar saves enormes. Se mantiene la grilla runtime actual al cargar.

### Advisor contextual
Sistema `V5AdvisorSystem`. Lee el estado de la célula madre, recursos, genes, stress, amenazas y colonización para sugerir el siguiente paso.

- `H`: mostrar/ocultar consejo.

### Codex biológico
Sistema `V5CodexSystem`. Desbloquea entradas cuando observas rutas, dominios, estructuras, genes y victoria.

- `C`: abrir/cerrar codex.

### Runtime settings
Sistema `V5RuntimeSettings`.

- `Espacio`: pausa/reanuda.
- `- / +`: velocidad de simulación x0.25 a x3.
- `` ` ``: alterna modo minimal de overlay para futuras optimizaciones.

### HUD 0.5
El HUD ahora muestra:

- build 0.5;
- controles actualizados;
- consejos contextuales;
- codex;
- toasts de feedback;
- objetivos por escenario.

## Archivos principales añadidos

- `Scripts/World/V5ScenarioDefinition.cs`
- `Scripts/UX/V5AdvisorSystem.cs`
- `Scripts/UX/V5CodexSystem.cs`
- `Scripts/UX/V5RuntimeSettings.cs`

## Archivos modificados

- `V5GameManager.cs`
- `V5GameBootstrap.cs`
- `V5ScenarioSystem.cs`
- `V5EnvironmentGrid.cs`
- `V5SaveSystem.cs`
- `V5CellFactory.cs`
- `V5CellEntity.cs`
- `V5HudIMGUI.cs`
- `V5PrototypeSceneMenu.cs`

## Próxima iteración recomendada 0.6

- serialización opcional de grilla ambiental comprimida;
- minimapa real;
- control groups RTS;
- comportamiento de linajes por rol;
- pantalla de selección de escenario;
- balance externo JSON/ScriptableObject;
- más feedback visual de división, colonización y daño.
