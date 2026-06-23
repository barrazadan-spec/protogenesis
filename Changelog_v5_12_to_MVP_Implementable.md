# Changelog v5.12 Direccion Final -> v5.12 MVP Implementable

## Resumen

Se creo una version implementable basica del GDD final. La direccion final sigue siendo el producto ideal; el MVP define el primer slice tecnico para programar.

---

# Decisiones Principales

## 1. El MVP valida una sola fantasia

Se define que el MVP debe responder:

**La fantasia Madre / Cuerpo / Squads es divertida, legible y programable?**

Todo lo que no ayude a contestar eso queda fuera.

## 2. Se reduce el roster inicial

El MVP empieza con rutas ya cercanas al estado actual:

- Bacteria.
- Archaea.
- Cyanobacteria.
- Amoeba.

Flagellate, Ciliate, Microalga, Fungus, Slime Mold, Rotifer, Nematode y Tardigrade quedan para fases posteriores o como contenido experimental/neutral.

## 3. El cuerpo final de 19 slots se implementa en dos pasos

Direccion final:

- 1 nucleo.
- 6 slots internos.
- 12 slots externos.

MVP:

- Sprint 1: 1 nucleo + 6 internos.
- Sprint 2: agregar 12 externos.

Esto permite validar la arquitectura sin ahogar al primer prototipo.

## 4. La produccion gana destino

Cada division debe elegir:

- receta;
- modo;
- destino: cuerpo, squad libre o automatico;
- afinidad/madurez.

Este cambio convierte el Genome Lab en el punto central del loop.

## 5. El combate se redefine por capas

El MVP agrega damage routing:

- cuerpo completo absorbe;
- cuerpo parcial filtra mas dano;
- nucleo expuesto recibe dano directo;
- piercing aumenta fuga;
- quimico puede afectar cuerpo y madre.

## 6. Champion queda fuera del primer sprint

La madre campeona no entra hasta que:

- adhesion funcione;
- produccion con destino funcione;
- dano por capas funcione.

Esto evita que el comeback tape problemas estructurales del cuerpo.

## 7. Ecosistema MVP se corta a tres estados

Estados iniciales:

- corredor nutritivo;
- cicatriz toxica;
- frente oxigenado.

Los demas estados y eventos quedan para fases posteriores.

## 8. Se crea lista de impacto de codigo

Se define que los primeros archivos nuevos son:

- `V5MulticellularBodySystem.cs`.
- `V5BodySlotDefinition.cs`.
- `V5BodyPanelIMGUI.cs`.

Y los principales archivos a modificar:

- `V5Types.cs`.
- `V5GameManager.cs`.
- `V5GameBootstrap.cs`.
- `V5CellEntity.cs`.
- `V5CellFactory.cs`.
- `V5GerminalProductionSystem.cs`.
- `V5PhenotypeRecipeLibrary.cs`.
- `V5CombatSystem.cs`.
- `V5SelectionSystem.cs`.
- `V5SquadTacticsSystem.cs`.
- `V5HudIMGUI.cs`.
- `V5ColonialContinuitySystem.cs`.

---

# Nuevo Orden de Trabajo

## Sprint 1

Cuerpo logico minimo:

- enums;
- 6 slots internos;
- attach/detach;
- orbitas;
- panel simple;
- compilar.

## Sprint 2

Produccion con destino:

- Auto/Cuerpo/Squad;
- recetas a slots;
- buffs;
- UI Genome Lab.

## Sprint 3

Combate por capas:

- absorcion;
- fuga;
- destruccion de slots;
- feedback.

## Sprint 4

Crisis y reconstruccion:

- continuidad con cuerpo;
- Husk Drop;
- champion regulado.

## Sprint 5

Run cerrada:

- ecosistema MVP;
- victoria/derrota;
- resumen y telemetria.

---

# Regla Final

El siguiente paso de programacion no es agregar organismos. Es construir:

**Cuerpo logico minimo: 6 slots internos, attach/detach, orbitas y HUD simple.**
