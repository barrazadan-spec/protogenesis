# Protogenesis V5 Prototype 1.9 — Run Control Milestone

## Objetivo de la iteración

La 1.9 agrega una capa de control de run para playtesting: alertas inteligentes, claridad de rutas de victoria y respuestas rápidas para no perder tiempo reiniciando partidas por problemas obvios.

## Sistemas nuevos

### `V5RunControlSystem`

Panel runtime IMGUI instalado automáticamente en cualquier escena V5.

Mide y muestra:

- riesgo global de la run;
- alertas críticas de madre, stress, ATP, biomasa, toxicidad, pH, oxígeno, enemigos y límite de población;
- progreso de rutas de victoria:
  - Dominancia ecológica;
  - Biosfera estable;
  - Limpieza predatoria;
  - Ascensión apex;
- consejo de cierre recomendado;
- export JSON de snapshot de playtest.

### Acciones rápidas desde el panel

- **Defender madre**: asigna hijas a directiva `Defend`.
- **Mandar farmers**: asigna hijas a directiva `Farm`.
- **Estabilizar**: consume recursos, baja stress, cura madre y detoxifica área cercana.
- **Priorizar amenaza**: manda defenders/predators contra el enemigo más cercano a la madre.
- **Exportar snapshot**: escribe `protogenesis_v5_runcontrol_1_9.json` en `Application.persistentDataPath`.

## Hotkey nueva

```txt
CapsLock: mostrar/ocultar Run Control 1.9
```

## Archivos agregados

```txt
Assets/_ProjectV5/Scripts/RunControl/V5RunControlSystem.cs
Assets/_ProjectV5/Scripts/Core/V5Prototype19AutoInstaller.cs
Assets/_ProjectV5/Docs/README_V5_1_9_IMPLEMENTATION.md
```

## Archivos modificados

```txt
Assets/_ProjectV5/Scripts/Core/V5Balance.cs
Assets/_ProjectV5/Scripts/UI/V5HudIMGUI.cs
Assets/_ProjectV5/Scripts/Release/V5HotkeyOverlayIMGUI.cs
```

## Cómo probar

1. Abrir Unity.
2. Crear escena desde:

```txt
Protogenesis > V5 > Create Full Prototype Scene
```

3. Play.
4. Presionar `CapsLock` para abrir/cerrar Run Control.
5. Provocar estrés, toxinas o enemigos y confirmar que aparecen alertas.
6. Usar las acciones rápidas para responder sin reiniciar la run.

## Nota

Todo sigue aislado dentro de `_ProjectV5`. El auto-installer permite probar 1.9 incluso en escenas generadas por versiones anteriores.
