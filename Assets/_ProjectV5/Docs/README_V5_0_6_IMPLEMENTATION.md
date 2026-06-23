# Protogenesis: Primordia — V5 Prototype 0.6

Esta iteración agrega herramientas de RTS y producción sobre la 0.5 sin tocar los sistemas legacy.

## Nuevos sistemas

### Minimap IMGUI
Archivo: `Scripts/UI/V5MinimapIMGUI.cs`

- `N`: mostrar/ocultar minimapa.
- Dibuja colonización, oxígeno/toxinas, recursos, células aliadas y amenazas.
- Usa muestreo ligero de la grilla para no redibujar tile por tile.

### Control Groups RTS
Archivo: `Scripts/RTS/V5ControlGroupSystem.cs`

- `Ctrl + 1..0`: asigna la selección actual a grupo.
- `Alt + 1..0`: recupera el grupo.
- La selección antigua de directivas 1-6 se desactiva cuando Ctrl/Alt está presionado para evitar conflicto.

### Roles de linaje
Archivo: `Scripts/RTS/V5LineageSystem.cs`

- `L`: abre/cierra panel de linajes.
- Roles: Generalist, Farmer, Scout, Defender, Colonizer, Predator, Recycler.
- El rol se guarda en quicksave y al cargar.
- Los roles empujan directivas persistentes sin bloquear órdenes manuales.

### Selector de escenarios en runtime
Archivos:
- `Scripts/UI/V5ScenarioMenuIMGUI.cs`
- `Scripts/Systems/V5RunResetSystem.cs`

- `F10`: abre/cierra selector de escenario.
- Cambiar escenario reinicia la run actual: limpia células, recursos, amenazas, ambiente y objetivos.

### Serialización ambiental
Archivos:
- `Scripts/World/V5EnvironmentSnapshot.cs`
- `Scripts/World/V5EnvironmentGrid.cs`
- `Scripts/Save/V5SaveSystem.cs`

- `F5`: quicksave ahora guarda también el estado ambiental completo.
- `F9`: quickload restaura colonización, toxinas, oxígeno, luz, acidez, nutrientes, temperatura y detritus.

## Controles actualizados

- Click izquierdo: seleccionar célula.
- Shift + click: selección múltiple.
- Click derecho: mover.
- D: dividir.
- E: panel interior.
- G: árbol génico.
- L: panel de roles de linaje.
- N: minimapa.
- F10: selector de escenario.
- Ctrl + 1..0: guardar grupo RTS.
- Alt + 1..0: seleccionar grupo RTS.
- 1-6: directivas cuando Ctrl/Alt no está presionado.
- F5/F9: guardar/cargar.

## Nota de implementación

Todo queda aislado en `Assets/_ProjectV5/` y usa el namespace `Protogenesis.V5`.
