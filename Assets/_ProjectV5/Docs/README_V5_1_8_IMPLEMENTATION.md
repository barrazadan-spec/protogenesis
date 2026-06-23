# Protogenesis: Primordia — V5 Prototype 1.8

## Objetivo de la iteración

La 1.8 agrega una capa más fuerte de **world builder jugable** encima del RTS: ahora el jugador puede gastar recursos celulares para modificar zonas concretas del mapa y usar un forecast ecológico para decidir cuándo expandir, limpiar, fortificar o buscar apex.

## Nuevos sistemas

### 1. V5HabitatEngineeringSystem

Archivo:

`Assets/_ProjectV5/Scripts/Habitat/V5HabitatEngineeringSystem.cs`

Controles:

- `0`: abrir/cerrar panel de ingeniería de hábitat.
- `Shift + 0`: aplicar el proyecto seleccionado en la posición del mouse.

Proyectos disponibles:

- **Bloom nutritivo**: sube nutrientes y detritus útil.
- **Bolsa de oxígeno**: crea un pocket de oxígeno para rutas respiradoras/eucariotas.
- **Biofilm detox**: limpia toxinas y aumenta colonización.
- **Buffer ácido**: estabiliza pH local.
- **Lente de luz**: aumenta luz local para fotosíntesis/cianobacteria.
- **Matriz defensiva**: fortifica territorio, baja stress y repara a la madre.

### 2. V5WorldHealthForecastSystem

Archivo:

`Assets/_ProjectV5/Scripts/Forecast/V5WorldHealthForecastSystem.cs`

Control:

- `Backspace`: abrir/cerrar forecast ecológico.

Mide:

- estabilidad del ecosistema;
- riesgo de colapso;
- readiness de expansión;
- tendencia de colonización;
- tendencia de toxinas;
- tendencia de oxígeno.

Genera una recomendación macro según estado real de la run.

### 3. Auto-installer 1.8

Archivo:

`Assets/_ProjectV5/Scripts/Core/V5Prototype18AutoInstaller.cs`

Instala automáticamente:

- `V5HabitatEngineeringSystem`
- `V5WorldHealthForecastSystem`

No requiere editar escena manualmente.

## Controles nuevos

```txt
0: abrir/cerrar ingeniería de hábitat
Shift + 0: aplicar proyecto seleccionado en el mouse
Backspace: abrir/cerrar forecast ecológico
```

## Notas de diseño

Esta iteración refuerza la identidad del juego:

> No solo produces células. Diseñas las condiciones de vida de la gota.

La ingeniería de hábitat convierte ATP, biomasa y precursores en territorio jugable. El forecast evita que el jugador se pierda entre muchas variables ambientales.

## Versión

`V5Balance.SaveVersion = 1.8`
