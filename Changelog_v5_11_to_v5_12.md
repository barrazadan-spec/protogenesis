# Changelog: GDD v5.11 -> v5.12

## Resumen Ejecutivo

v5.12 no agrega otro gran sistema encima de v5.11. Hace lo contrario: **cierra la direccion final del juego terminado** y recorta el alcance para que el cuerpo multicelular sea legible, balanceable y producible.

La decision principal es:

> El juego terminado se organiza alrededor de una triada: madre/interior, cuerpo adherido y squads libres.

Todo lo que no fortalece esa triada pasa a expansion, escenario, codex o backlog.

---

## Cambios Mayores

### 1. Cuerpo multicelular reducido a 19 slots activos

v5.11 proponia una grilla de 4 anillos con hasta 60 posiciones potenciales.

v5.12 define:

- 1 nucleo madre.
- 6 slots en ring interno.
- 12 slots en ring externo.
- Cuerpos de 20+ organismos se representan por microcolonias visuales o extensiones abstractas, no por 60 slots administrables.

**Razon:** legibilidad, performance y control tactico.

---

### 2. Catalogo base de estructuras reducido

v5.11 catalogaba 66 elementos.

v5.12 mantiene ese catalogo como universo expandido, pero recomienda que el juego base use **28-32 piezas mecanicas**:

- 8-10 organelos madre.
- 5-6 paredes/membranas.
- 4-5 locomociones.
- 6-8 enzimas/proteinas.
- 4-5 toxinas/armas.
- 4-5 estructuras latentes/apex.

**Razon:** evitar que el GDD terminado parezca enciclopedia antes de ser juego.

---

### 3. Champion Mode recibe anti-cheese

v5.11 planteaba que el campeon despierta cuando cae el cuerpo.

v5.12 agrega una regla:

- Si el cuerpo cae por dano o presion real: Champion Mode pleno.
- Si el jugador usa Husk Drop desde cuerpo sano: Exposed Surge reducido.
- Las estructuras latentes se activan segun inversion previa, dano recibido y stress.

**Razon:** evitar que la estrategia optima sea autodestruir el cuerpo para llegar al heroe.

---

### 4. Joints fisicos dejan de ser canon tecnico

v5.11 sugeria `HingeJoint2D` o `FixedJoint2D`.

v5.12 establece:

- Vinculos logicos como implementacion base.
- Visualizacion clara de links.
- Fisica real solo si un prototipo posterior demuestra que mejora el feel.

**Razon:** reducir riesgo de bugs, performance y caos visual.

---

### 5. Roster reorganizado

v5.11 mezclaba rutas, subrutas, neutrales y organismos de expansion.

v5.12 define 12 rutas jugables principales:

1. Bacteria
2. Arquea
3. Cianobacteria
4. Ameba
5. Flagelado
6. Ciliado
7. Microalga
8. Diatomea
9. Hongo
10. Moho mucilaginoso
11. Rotifero
12. Nematodo

Y mueve a subruta/neutral/evento:

- Espiroqueta/Espirilo
- Anabaena
- Myxococcus
- Euglena
- Volvox
- Suctoria
- Heliozoo/Radiolaria
- Bacteriofago
- Daphnia
- Dinoflagelado bloom
- Tardigrado
- Hydra

**Razon:** roster amplio sin que todo tenga que ser ruta principal.

---

### 6. Victoria de skirmish simplificada

v5.11 mantenia muchas condiciones.

v5.12 deja tres condiciones estandar:

- Dominancia ecologica.
- Colapso del linaje rival.
- Apex biologico.

Las demas pasan a escenarios.

---

### 7. Doctrinas reemplazan reglas automaticas por unidad

v5.11 proponia hasta 3 reglas automaticas por unidad.

v5.12 lo simplifica:

- doctrinas de produccion;
- plantillas de cuerpo;
- modos celulares;
- coach contextual.

**Razon:** mantener profundidad sin convertir el juego en programacion granular.

---

## Cosas Que Se Mantienen

- Cuerpo multicelular como core.
- Madre germinal.
- Squads libres.
- Genome Lab con recetas.
- Madurez de recetas por afinidad.
- Evolution Affinity Tracker con memoria causal.
- Living Battlefield.
- Director Ecologico.
- Sucesion colonial.
- Husk Drop.
- Nucleo Expuesto.
- Daño por capas.
- Tardigrado no jugable principal.

---

## Cosas Que Se Agregan Como Direccion

- Plantillas de cuerpo: esfera, torpedo, red, bloom, predador frontal.
- Coach contextual biologico.
- Reporte post-partida de linaje.
- Codex vivo con counters.
- Anti-cheese de campeon.
- Criterio de producto: cada celula producida debe elegir cuerpo, squad o evolucion.

---

## Cosas Que Se Cortan del Base Game

- 60 slots visibles.
- 66 estructuras obligatorias.
- Joints fisicos como base.
- Tardigrado jugable core.
- Campeon como estrategia principal.
- 5+ condiciones de victoria estandar.
- Reglas automaticas detalladas por unidad.
- Microgestion de submateriales en HUD principal.

---

## Proximo Paso Recomendado

Crear:

**Protogenesis_v5_12_Version_Implementable_Basica.md**

Debe contener solo el prototipo que valida la columna vertebral:

1. Hija adherida o libre.
2. Slots logicos alrededor de madre.
3. Estados del cuerpo.
4. Buffs simples.
5. Desadherir/reagrupar.
6. Combate fisico golpea adheridas antes que madre.

Si ese prototipo no se siente bien, el resto del GDD debe iterar.

