# Changelog v5.15.8 -> v5.15.9

## Objetivo

Corregir la sensacion visual de adhesion: al adherir hijas al cuerpo, las celulas quedaban orbitando con una separacion visible en vez de verse pegadas a la madre.

## Cambios principales

| Area | Cambio |
| --- | --- |
| Posicion corporal | `V5MulticellularBodySystem` deja de usar un radio fijo de slot `1.35f` y calcula la distancia desde los radios reales de madre + hija. |
| Contacto visual | Los slots usan `AttachedContactOverlap = 0.88f`, generando un pequeno solapamiento biologico para que se vean adheridas. |
| Snap inmediato | Al ejecutar `TryAttach`, la hija se posiciona de inmediato en su slot corporal en vez de esperar varios frames de interpolacion. |
| Seguimiento corporal | Las celulas adheridas siguen a la madre con `AttachedFollowSpeed = 18f` para mantener el cuerpo mas compacto mientras se mueve. |
| API de celula | `V5CellEntity` agrega `SnapAttachedToBodySlot()` para posicionar celulas ya adheridas sin velocidad residual. |
| Smoke test | `V5PrototypeSmokeRunner` valida que la hija adherida quede dentro de distancia de contacto visual con la madre. |

## Archivos tocados

- `Assets/_ProjectV5/Scripts/Colony/V5MulticellularBodySystem.cs`
- `Assets/_ProjectV5/Scripts/Cells/V5CellEntity.cs`
- `Assets/_ProjectV5/Scripts/Editor/V5PrototypeSmokeRunner.cs`

## Validacion

- Unity batch compile OK: `Logs/codex_unity_compile_v5159.log`.
- Smoke test completo OK: `Logs/codex_unity_smoke_v5159.log` termina con `Failures: 0`.

