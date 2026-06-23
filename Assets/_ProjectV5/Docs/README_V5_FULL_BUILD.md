# Protogenesis: Primordia — V5 Full Prototype Build 0.2

Esta carpeta agrega una capa nueva e independiente del juego usando `namespace Protogenesis.V5`.
No reemplaza tus scripts antiguos: convive con ellos para que puedas probar sin romper el proyecto.

## Cómo probar

1. Abre el proyecto en Unity.
2. Espera que compile.
3. Menú superior: `Protogenesis > V5 > Create Full Prototype Scene`.
4. Presiona Play.

## Controles

- Click izquierdo: seleccionar célula.
- Shift + click: selección múltiple.
- Click derecho: mover.
- D: dividir la célula seleccionada o la madre.
- E: abrir/cerrar panel interior.
- Tab: cambiar overlay ambiental.
- F: cámara sigue a la madre.
- WASD: mover cámara.
- Scroll: zoom.
- 1: seguir madre.
- 2: farmear.
- 3: defender.
- 4: explorar.
- 5: colonizar.
- 6: atacar.

## Qué trae esta build

- Célula madre.
- División celular con herencia.
- Hijas y nietas.
- Recursos: ATP, biomasa, aminoácidos, lípidos, nucleótidos, minerales.
- Directivas RTS.
- Panel interior por IMGUI.
- Instalación de estructuras.
- Bifurcación metabólica: respiración, fotosíntesis, fermentación, quimiolitotrofía.
- Dominio procariota/eucariota.
- 12 rutas evolutivas representadas en la lógica.
- Grilla ambiental optimizada por ticks.
- Overlay visual de nutrientes/luz/oxígeno/toxinas/acidez/colonización/temperatura.
- Recursos regenerativos.
- Enemigos neutrales con IA simple.
- Combate por contacto, daño químico y fagocitosis simplificada.
- Objetivo de escenario: colonizar 40% o estabilizar el ecosistema.
- Cámara, HUD y generador de escena.

## Nota importante

Esto no es todavía un juego comercial terminado. Es una build base grande para desarrollo: sistemas conectados, extensibles y jugables.
El siguiente paso es probar dentro de Unity, corregir errores de compilación si aparece alguno por versión de Unity, y luego empezar a reemplazar IMGUI por UI final y sprites/shaders definitivos.
