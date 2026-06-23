# Changelog v5.15.9 -> v5.16.0

## Sistema base de castas funcionales

Objetivo: empezar la implementacion del documento `Protogenesis_Sistema_Castas_Diferenciacion_v6.md` sin introducir todavia el editor completo de 19 slots. Esta version deja una base jugable y testeada: cada hija puede tener una casta funcional persistente, visible y con efecto mecanico.

### Cambios principales

- Nuevo enum `V5FunctionalCasteId` con 7 castas: `Hybrid`, `Gatherer`, `Attacker`, `Defender`, `Producer`, `Sensor`, `Structural`.
- Nueva biblioteca `V5CasteLibrary` con color, nombre, resumen, rol corporal recomendado y multiplicadores funcionales.
- `V5CellEntity` ahora guarda `FunctionalCaste`, muestra halo/marker por casta y aplica multiplicadores sobre modo celular:
  - dano
  - velocidad
  - recoleccion/sintesis
  - dano recibido
  - colonizacion
  - reparacion
- `V5PhenotypeRecipeLibrary` mapea recetas germinales a castas funcionales:
  - Recolectora de linaje -> Gatherer
  - Raider de linaje -> Attacker
  - Defensora/Ameboide -> Defender
  - Exploradora/Ciliada -> Sensor
  - Microalga soporte -> Producer
  - Simbionte bacteriano -> Structural
  - Hija plastica -> Hybrid
- `V5CellFactory` conserva la casta cuando una hija diferenciada genera una nieta.
- `V5ResourceNode` usa la eficiencia de casta para cosecha pasiva por contacto.
- `V5SaveSystem` guarda/restaura `functionalCaste` con compatibilidad hacia atras (`Hybrid` por defecto).
- HUD build visible sube a `2.21` y muestra resumen de composicion de castas.
- Panel de cuerpo muestra la casta funcional del ocupante.
- Camara germinal muestra el short label de casta, el sesgo corporal y usa el color de casta en la placa de preview.
- Smoke test valida que la biblioteca exista, que `LineageRaider` mapee a `Attacker`, que la camara germinal produzca una unidad atacante y que su multiplicador de combate este activo.

### Alcance intencional

No se implementa todavia:

- editor completo de genoma/cuerpo por 19 slots;
- muerte automatica de casta estructural al salir del cuerpo;
- recetas de plantilla para organismos iconicos;
- UI dedicada de composicion de castas.

Esta es la primera capa estable para probar si la diferenciacion funcional se siente bien antes de agregar complejidad.

### Validacion

- Unity batch compile OK: `Logs/codex_unity_compile_v5160.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5160.log` termina con `Failures: 0`.
