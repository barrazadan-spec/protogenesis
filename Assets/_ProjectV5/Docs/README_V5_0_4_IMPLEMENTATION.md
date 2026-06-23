# Protogenesis: Primordia — V5 Prototype 0.4

Esta iteración continúa la capa aislada `Assets/_ProjectV5/` y agrega sistemas de juego que conectan la vertical slice con una experiencia más cercana a un juego completo.

## Sistemas nuevos

### 1. Fog of War / descubrimiento de mapa
Archivo: `Scripts/World/V5FogOfWarSystem.cs`

- Crea una textura procedural de niebla sobre la gota.
- Las células del jugador revelan el mapa según su rango sensorial.
- Las células con reconocimiento revelan más radio.
- Expone `DiscoveredPercent` para objetivos y HUD.
- Tecla debug `I`: revela todo el mapa.

### 2. Director de dificultad
Archivo: `Scripts/Systems/V5DifficultyDirector.cs`

- Calcula `ThreatLevel` según tiempo, colonización, población y enemigos activos.
- Genera oleadas biológicas adaptativas.
- Si el jugador queda sin ATP/biomasa, puede crear recursos cercanos como asistencia suave.
- Aumenta presión sin depender de oleadas fijas.

### 3. Sistema de misiones
Archivo: `Scripts/Systems/V5MissionSystem.cs`

Cadena actual:

1. Instalar Motor Metabólico.
2. Elegir metabolismo/dominio.
3. Dividirse hasta tener 3 células.
4. Mandar 2 hijas a farmear.
5. Colonizar 10%.
6. Desbloquear 2 genes.
7. Objetivo final: 40% de colonización o estabilización.

### 4. Telemetría de playtest
Archivo: `Scripts/Systems/V5TelemetrySystem.cs`

- Células creadas.
- Máximo de células.
- Estructuras instaladas.
- Recursos ganados estimados.

Sirve para balancear si el jugador se queda sin recursos, divide demasiado rápido o llega tarde a la colonización.

### 5. Cheats de debug
Archivo: `Scripts/Debug/V5DebugCheats.cs`

Controles:

- `T`: agrega recursos a la madre.
- `Y`: cura madre y baja stress.
- `U`: genera enemigo cercano.
- `I`: revela mapa.
- `O`: coloniza zona cercana para probar condición de victoria.

## Controles relevantes

- Click izquierdo: seleccionar.
- Shift + click: selección múltiple.
- Click derecho: mover.
- `D`: dividir.
- `E`: panel interior.
- `G`: panel génico.
- `M`: panel de misión/playtest.
- `P`: invocar apex.
- `Tab`: cambiar overlay ambiental.
- `F`: seguir cámara a la madre.
- `1`: seguir madre.
- `2`: farmear.
- `3`: defender.
- `4`: explorar.
- `5`: colonizar.
- `6`: atacar.

## Menú de escena

Usar:

`Protogenesis > V5 > Create Full Prototype Scene`

Luego Play.

## Nota de seguridad

Se corrigió un error de compilación en `V5CellEntity.cs` dentro de la directiva Colonize. Todo sigue aislado dentro de `_ProjectV5`.
