# Protogenesis: Primordia — V5 Prototype 1.7

## Enfoque de la iteración
La 1.7 agrega una capa de **macro RTS** para que el juego deje de depender de microgestionar cada célula. La colonia ahora puede operar con doctrinas, build orders y diagnóstico de salud global.

## Sistemas nuevos

### 1. Automatización colonial
Archivo: `Scripts/Automation/V5ColonyAutomationSystem.cs`

Hotkey: `7`

Permite activar/desactivar:
- auto-asignación de roles de linaje;
- auto-asignación de directivas RTS;
- homeostasis automática en crisis;
- doctrinas macro.

Doctrinas:
- Balanced
- Expansion
- Turtle
- Bloom Economy
- Predator Web
- Recovery

### 2. Build Order evolutivo
Archivo: `Scripts/Research/V5BuildOrderPlannerIMGUI.cs`

Hotkey: `8`

Permite elegir una ruta evolutiva objetivo y ejecutar un plan recomendado:
- gen de dominio;
- estructuras base;
- estructuras clave de la ruta;
- anillos génicos 2, 3 y 4.

Puede funcionar manual o en auto-ejecución cuando haya recursos/tiempo.

### 3. Dashboard de salud colonial
Archivo: `Scripts/UI/V5ColonyHealthDashboardIMGUI.cs`

Hotkey: `9`

Muestra:
- economía;
- defensa;
- expansión;
- ecología;
- score global;
- diagnóstico de balance de la run.

### 4. Auto-installer 1.7
Archivo: `Scripts/Core/V5Prototype17AutoInstaller.cs`

Instala automáticamente los sistemas 1.7 en cualquier escena V5.

### 5. Hotkey overlay actualizado
Archivo: `Scripts/Release/V5HotkeyOverlayIMGUI.cs`

La ayuda (`\`) ahora incluye los nuevos accesos 7/8/9.

## Cómo probar
1. Abrir Unity.
2. Usar `Protogenesis > V5 > Create Full Prototype Scene`.
3. Presionar Play.
4. Probar:
   - `7` automatización colonial.
   - `8` build order evolutivo.
   - `9` dashboard de salud.
   - `\` ayuda completa.

## Nota de integración
Este paquete es incremental y se puede copiar encima de la versión V5 previa. Todo sigue dentro de `Assets/_ProjectV5/`.
