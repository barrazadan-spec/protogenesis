# Protogenesis: Primordia — V5 Prototype 0.8

Esta iteración agrega una capa de sensación de producto/playtest encima de la 0.7. No reemplaza los sistemas anteriores: se instala automáticamente al cargar cualquier escena V5 mediante `V5Prototype08AutoInstaller`.

## Nuevos sistemas

- Main Menu runtime (`Esc`): continuar, reiniciar run y cambiar escenario rápido.
- Evolution Planner (`V`): rutas evolutivas, progreso por estructuras y recomendación actual.
- Milestone Rewards: hitos de run que entregan recursos y desbloquean codex.
- Endgame Director: condiciones adicionales de victoria/derrota y advertencias de colapso.
- Feedback System: floating labels, toasts y eventos más visibles.
- Audio Feedback placeholder: beeps generativos para división, genes, estructuras, alerta, victoria y derrota.
- Performance Monitor (`K`): FPS, células, nodos y memoria aproximada.
- Build Validator (`Shift + R`): chequea referencias esenciales del runtime V5.
- Profile Progression: guarda runs iniciadas, victorias, apex invocados y mejor colonización en PlayerPrefs.

## Controles nuevos

```txt
Esc: menú runtime
V: plan evolutivo
K: monitor de rendimiento
Shift + R: validador de build
```

## Nota de integración

La 0.8 fue diseñada como patch/overlay sobre la 0.7 para evitar tocar archivos centrales. Si quieres integrarlo formalmente, puedes mover los sistemas que instala `V5Prototype08AutoInstaller` a `V5GameBootstrap`.
