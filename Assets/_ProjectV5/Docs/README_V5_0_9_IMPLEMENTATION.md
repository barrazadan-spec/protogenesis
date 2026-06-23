# Protogenesis V5 Prototype 0.9 — Active Biology + Mutation Draft

Esta iteración añade una capa más jugable encima de la 0.8: habilidades activas, mutaciones de run y herramientas para destrabar playtests.

## Sistemas nuevos

### 1. V5AbilitySystem
Activa habilidades biológicas en las células seleccionadas. Si no hay selección, usa la célula madre.

Controles:

- `Q` — **Metabolic Surge**: gasta ATP, sube velocidad/síntesis/ATP/s temporalmente y añade algo de stress.
- `W` — **Ecological Pulse**: pulso dependiente de biología:
  - fotosintéticas/cianobacterias: bloom de oxígeno y colonización;
  - arqueas/quimiolitótrofas: bolsa extrema ácida;
  - amebas/macrófagos/lisosomas: digestión radial;
  - bacterias/hongos/neutrófilos: toxina radial;
  - genéricas: homeostasis/detox local.
- `R` — **Emergency Repair**: gasta ATP/lípidos para reparar y bajar stress.
- `K` — mostrar/ocultar panel de habilidades.

### 2. V5MutationDraftSystem
Agrega decisiones de mutación durante la run para que la partida tenga arco estratégico.

Controles:

- `V` — abrir/cerrar panel de mutaciones.
- `1 / 2 / 3` — elegir mutación cuando hay draft pendiente.

Los drafts se otorgan por tiempo y por crecimiento de colonia. Ejemplos: membrana reforzada, eficiencia metabólica, matriz colonial, fotosistemas apilados, proteoma acidófilo, protocolo swarm.

### 3. V5RunDoctorIMGUI
Herramienta de playtest para detectar estancamientos y arreglar la run sin reiniciar.

Controles:

- `;` — abrir/cerrar Run Doctor.
- `F1` — agregar recursos a la madre.
- `F2` — bajar stress y curar células aliadas.
- `F3` — sanear ecología local.
- `F4` — generar amenaza de prueba.

### 4. V5Prototype09AutoInstaller
Instala automáticamente los sistemas nuevos en escenas V5 previas sin tener que editar manualmente la escena ni el bootstrap.

## Cómo probar

1. Abrir el proyecto en Unity.
2. Usar `Protogenesis > V5 > Create Full Prototype Scene`.
3. Play.
4. Probar una run normal hasta que aparezcan mutation drafts.
5. Usar `Q/W/R` sobre grupos seleccionados para sentir la capa RTS activa.

## Nota de implementación

Todo sigue aislado dentro de `Assets/_ProjectV5/` y namespace `Protogenesis.V5`.
