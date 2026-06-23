# Protogenesis V5 Prototype 1.4

Iteración enfocada en hacer el prototipo más testeable como demo interna y más claro como juego de evolución RTS/world builder.

## Sistemas nuevos

### V5ResearchSystem
- Panel con `End`.
- Investigación por run con progreso en tiempo real.
- Proyectos: División eficiente, Economía de membrana, Autonomía guiada, Ingeniería ambiental, Preparación apex y Control homeostático.
- Las investigaciones gastan recursos de la madre al iniciar y luego progresan con síntesis/estructuras.

### V5EvolutionRouteBoardSystem
- Panel con `Home`.
- Muestra las 12 rutas evolutivas, dominio, fantasía y estructuras clave.
- Permite fijar ruta objetivo, elegir metabolismo recomendado e instalar la siguiente estructura clave si alcanza el recurso.

### V5DynamicObjectiveSystem
- Panel con `Delete`.
- Crea objetivos secundarios dinámicos según estado de la run.
- Recompensa con recursos y reducción de stress.
- Puede forzar objetivos para playtest.

### V5DemoBuildPrepSystem
- Panel con `Backspace`.
- Exporta manifest de demo con `Ctrl + Backspace`.
- Genera `protogenesis_v5_demo_manifest_1_4.json` en `Application.persistentDataPath/ProtogenesisV5`.

## Nuevos controles

- `Home`: tablero de rutas evolutivas.
- `End`: investigación celular.
- `Delete`: objetivos secundarios dinámicos.
- `Backspace`: demo build prep.
- `Ctrl + Backspace`: exportar manifest de demo.

## Notas

Todo sigue aislado en `Assets/_ProjectV5/` y se instala automáticamente con `V5Prototype14AutoInstaller` al cargar cualquier escena V5.

`V5Balance.SaveVersion` actualizado a `1.4`.
