# Changelog v5.14 -> v5.14.1

## Agregado

- Nuevo documento: `Protogenesis_Catalogo_Adaptaciones_v5_14_1.md`.
- Se cierra el catalogo implementable de Adaptaciones.
- Cada adaptacion ahora tiene:
  - tipo,
  - prioridad,
  - costo,
  - tiempo,
  - tags,
  - requisitos,
  - efectos madre/produccion/cuerpo/identidad,
  - legacy mapping,
  - sinergias,
  - conflictos.

## Decisiones

- P0 quedo cerrado inicialmente con 28 entradas, incluyendo Champion biologico como estado existente. Nota v5.14.3: Enzimas extracelulares sube a P0, por lo que el primer corte pasa a 29 entradas.
- P1/P2 quedan como backlog claro.
- Los numeros son balance inicial, no dogma.
- La estructura canonica queda cerrada para implementar `V5AdaptationSystem`.
