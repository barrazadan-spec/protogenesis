# Changelog v5.14.9 -> v5.15.0

## Tema

Panel Interior convertido en lectura de fenotipo y morfologia procedural conectada a adaptaciones.

## Cambios principales

| Sistema | Cambio |
| --- | --- |
| HUD | Build actualizado a `2.18`. |
| Interior | El panel `E` ahora muestra identidad, canon, adaptaciones, recursos, rasgos corporales y lectura del fenotipo. |
| Legacy | El catalogo manual de estructuras queda oculto por defecto y se abre solo con `Modo avanzado`. |
| Herencia | La barra inferior habla de `rasgos corporales` cuando el sistema de adaptaciones esta activo. |
| Genoma | Desde Interior se puede abrir Genoma o instalar la adaptacion sugerida por identidad. |
| Morfologia | `V5CellMorphologyRenderer` lee adaptaciones activas para dibujar paredes/capsulas, centros fotosinteticos, flagelos, pili/adhesinas, cilios, hifas, plasmodio y seudopodos. |
| Compatibilidad | Estructuras legacy siguen funcionando para NPCs, saves antiguos y debug. |

## Archivos modificados

- `Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs`
- `Assets/_ProjectV5/Scripts/Visual/V5CellMorphologyRenderer.cs`
- `Lista_Impacto_Codigo_v5_14_Adaptaciones.md`

## Validacion

- Compile Unity batch: OK sin errores C# en `Logs/codex_unity_compile_v5150b.log`.
- Smoke test completo: OK en `Logs/codex_unity_smoke_v5150_rerun.log`, termina con `Failures: 0`.
- Nota: Unity registro el aviso externo intermitente de Burst JIT cache al cerrar batchmode; no produjo errores C# ni fallos del smoke.

## Siguiente capa sugerida

1. Agregar telemetria de adaptaciones para playtest: pickrate, tiempo de instalacion, abandonos por cap y correlacion con victoria.
2. Revisar paneles de balance/debug para separar modo jugador vs modo desarrollador.
3. Dar feedback visual mas diferenciado a Diatomea, Hongo, Moho y Ameba para que cada identidad se lea al primer vistazo.
