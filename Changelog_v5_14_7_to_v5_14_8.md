# Changelog v5.14.7 -> v5.14.8

## Tema

Economia territorial y objetivos iniciales conectados al sistema canonico de adaptaciones.

## Cambios principales

| Sistema | Cambio |
| --- | --- |
| Relaciones ecologicas | Mutualismo, competencia y predacion ahora leen diversidad adaptativa, adhesion, redes, metabolismo compatible y depredacion por organelos. |
| Distritos coloniales | La recomendacion de distrito combina senales ambientales con adaptaciones activas. Una zona luminosa favorece fototrofia si hay tilacoide/cloroplasto; detritus favorece reactor si hay hifa/enzimas o caza si hay lisosoma/pseudopodos. |
| Efectos de distrito | Cada distrito recibe un multiplicador biologico por adaptaciones coherentes, con limite para evitar snowball excesivo. |
| Supply colonial | Adhesion, pili, comunicacion, diferenciacion, adhesion persistente y memoria quimica aumentan capacidad territorial. |
| Misiones | El fallback de misiones se actualizo a Genoma/Adaptaciones: primera adaptacion, Adesina basica, division, roles, colonizacion, identidad y consolidacion. |

## Archivos modificados

- `Assets/_ProjectV5/Scripts/Ecology/V5EcologicalRelationsSystem.cs`
- `Assets/_ProjectV5/Scripts/Districts/V5ColonyDistrictSystem.cs`
- `Assets/_ProjectV5/Scripts/Systems/V5MissionSystem.cs`
- `Lista_Impacto_Codigo_v5_14_Adaptaciones.md`

## Validacion

- Compile Unity batch: OK sin errores C# en `Logs/codex_unity_compile_v5148.log`.
- Smoke test completo: OK en `Logs/codex_unity_smoke_v5148.log`, termina con `Failures: 0`.

## Siguiente capa sugerida

1. Actualizar los paneles restantes que todavia muestran genes/anillos legacy como fuente primaria.
2. Llevar la telemetria de adaptaciones a un panel de balance para playtest.
3. Revisar si el `PlayableLoopSystem` debe reemplazar nombres de etapas legacy aunque ya use adaptaciones internamente.
