# Protogenesis V5 Prototype 1.5

Iteración enfocada en sensación inmediata de juego: siluetas biológicas visibles, control RTS macro y cohesión colonial.

## Sistemas nuevos

### Morfología procedural
- `V5CellMorphologyRenderer` agrega siluetas sin assets externos.
- Visualiza flagelos, cápsula/pared, fimbrias, cilios, hifas y tilacoide.
- `Ctrl + F10`: activar/desactivar morfología para rendimiento.

### Cohesión colonial
- `V5ColonyCohesionSystem` mide distancia a la madre, células cercanas y dispersión.
- Alta cohesión reduce stress y da una pequeña ayuda económica.
- Baja cohesión con demasiadas células aumenta stress.

### Centro de comando colonial
- `0`: abrir/cerrar panel.
- `F` sin Ctrl/Alt: recall global.
- Órdenes macro: farmear, defender, explorar, colonizar, atacar, recall, formación defensiva.
- Plan de build: elige ruta, aplica metabolismo recomendado e instala la próxima estructura clave.

## Cambios
- HUD actualizado a `V5 BUILD 1.5`.
- `V5Balance.SaveVersion = 1.5`.
- Auto-instalador `V5Prototype15AutoInstaller`.

Todo sigue aislado en `Assets/_ProjectV5/`.
