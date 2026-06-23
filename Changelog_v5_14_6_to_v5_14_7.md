# Changelog v5.14.6 -> v5.14.7

Fecha: 2026-05-08

## Objetivo

Migrar crisis, estabilidad, sucesion ecologica y continuidad colonial al sistema canonico de adaptaciones e identidad.

## Cambios En Codigo

| Archivo | Cambio |
| --- | --- |
| `Assets/_ProjectV5/Scripts/Crisis/V5CrisisAndStabilitySystem.cs` | La deuda adaptativa usa complejidad de adaptaciones. Las crisis y mutaciones negativas se eligen por causa biologica contextual. |
| `Assets/_ProjectV5/Scripts/Systems/V5EcosystemSuccessionSystem.cs` | Reescrito en ASCII. Diversidad, productividad y etapas leen adaptaciones, identidad y presion ecologica. |
| `Assets/_ProjectV5/Scripts/Colony/V5ColonialContinuitySystem.cs` | La sucesion tras muerte de madre suma adhesion, redes, memoria quimica, cubiertas, senales, diferenciacion y Champion. |

## Resultado

- Las crisis ya no son azar puro: emergen desde lo que el jugador construye.
- La biosfera estable premia diversidad funcional, no solo cantidad de unidades.
- La muerte de la madre queda mejor conectada al cuerpo/adaptaciones del linaje.

## Validacion

- Compilacion Unity batch: OK.
- Log: `Logs/codex_unity_compile_v5147.log`.
- Smoke test completo: OK.
- Log: `Logs/codex_unity_smoke_v5147.log`, `Failures: 0`.
