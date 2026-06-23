# Protogenesis V5 Prototype 1.0 — Internal Demo Milestone

Esta iteración convierte el prototipo V5 en una demo interna más testeable. No es una build comercial final; es una base jugable con loop completo, tutorial, resumen y herramientas de QA.

## Nuevos sistemas

- V5Prototype10AutoInstaller: instala automáticamente los sistemas 1.0 en cualquier escena V5.
- V5TutorialFlowSystem: onboarding guiado no bloqueante con recompensas.
- V5RunSummarySystem: panel de fin de run y scoring.
- V5PlaytestReportSystem: exporta reporte JSON a Application.persistentDataPath/ProtogenesisV5Reports.
- V5ReleaseReadinessSystem: checklist runtime de build/playtest.
- V5OptimizationGuardSystem: monitor de FPS aproximado y soft-culling opcional.
- V5HotkeyOverlayIMGUI: pantalla de controles.

## Controles nuevos

- F11: tutorial guiado.
- F7: resumen de run.
- Ctrl + F7: exportar reporte JSON.
- F12: release readiness / QA checklist.
- \\: ayuda de controles.
- Ctrl + O: activar/desactivar soft-culling automático si la run se pone pesada.

## Loop recomendado para playtest

1. Crear escena con Protogenesis > V5 > Create Full Prototype Scene.
2. Play.
3. Seguir el tutorial F11 hasta colonización y genes.
4. Probar una ruta: bacteria, arquea, cianobacteria o ameba.
5. Usar F7 para ver resumen.
6. Exportar reporte con Ctrl+F7.
7. Revisar F12 para readiness.

## Nota técnica

Todo sigue aislado en Assets/_ProjectV5/ y usa namespace Protogenesis.V5. Unity debería regenerar los .meta de los archivos nuevos.
