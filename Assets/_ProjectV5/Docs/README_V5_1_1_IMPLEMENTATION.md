# Protogenesis: Primordia — V5 Prototype 1.1

## Objetivo de la iteración

La 1.1 convierte la demo interna 1.0 en una run con más contenido jugable y herramientas de playtest:

- contratos ecológicos opcionales;
- encuentros miniboss microscópicos;
- campaña interna con capítulos sugeridos;
- presets de balance rápidos;
- versión de save/balance actualizada a 1.1.

Todo sigue aislado en `Assets/_ProjectV5/`.

## Sistemas nuevos

### V5ContractSystem
Archivo: `Assets/_ProjectV5/Scripts/Systems/V5ContractSystem.cs`

Tecla: `,`

Genera tres contratos opcionales por vez. El jugador acepta uno y recibe recompensa al completarlo.

Contratos incluidos:

- Colonizar matriz local.
- Detoxificar microambiente.
- Cazar amenazas.
- Microcolonia estable.
- Homeostasis de pH.
- Recolectar precursores.

La idea es meter objetivos cortos tipo RTS/world builder sin reemplazar el objetivo principal de escenario.

### V5BossEncounterSystem
Archivo: `Assets/_ProjectV5/Scripts/Threats/V5BossEncounterSystem.cs`

Tecla: `.`
Debug: `Ctrl + .`

Minibosses incluidos:

- Rotifer Hunter: presión física de cazador.
- Phage Cloud: nube tóxica.
- Acid Matriarch: frente ácido móvil.
- Bloom Leviathan: presión fotosintética/enjambre.

El sistema elige miniboss según el estado ecológico de la gota.

### V5CampaignFlowSystem
Archivo: `Assets/_ProjectV5/Scripts/Meta/V5CampaignFlowSystem.cs`

Tecla: `;`

Campaña interna ligera. No reemplaza los escenarios; sugiere la siguiente run según hitos:

1. Primera gota.
2. Matriz colonial.
3. Dominio metabólico.
4. Forma apex.
5. Depredadores microscópicos.
6. Demo completa.

Guarda progreso con `PlayerPrefs`.

### V5BalancePresetSystem
Archivo: `Assets/_ProjectV5/Scripts/Balance/V5BalancePresetSystem.cs`

Tecla: `'`

Presets incluidos:

- Tutorial.
- Easy.
- Standard.
- Hard.
- Sandbox.

Modifica el `V5BalanceProfileSystem` existente sin tocar los valores base hardcodeados.

### V5Prototype11AutoInstaller
Archivo: `Assets/_ProjectV5/Scripts/Core/V5Prototype11AutoInstaller.cs`

Instala automáticamente los sistemas 1.1 en cualquier escena V5 previa.

## Controles nuevos

```txt
,        abrir/cerrar contratos ecológicos
.        abrir/cerrar minibosses
Ctrl+.   invocar miniboss recomendado para test
]        abrir/cerrar campaña interna
'        abrir/cerrar presets de balance
```

## Prueba rápida recomendada

1. Abre Unity.
2. Usa `Protogenesis > V5 > Create Full Prototype Scene`.
3. Play.
4. Pulsa `'` y aplica `Tutorial` o `Sandbox` si quieres testear rápido.
5. Pulsa `,` y acepta un contrato.
6. Usa `D` para dividir, `2` para farmear, `5` para colonizar.
7. Pulsa `Ctrl + .` para invocar un miniboss.
8. Pulsa `]` para revisar progreso de campaña.

## Notas técnicas

- `V5Balance.SaveVersion` subió a `1.1`.
- `V5BalanceProfile.version` subió a `1.1`.
- El JSON de balance ahora se llama `protogenesis_v5_balance_1_1.json`.
- No se tocó código legacy fuera de `_ProjectV5`.

## Siguiente iteración sugerida: 1.2

- Pulido visual de feedback para contratos/miniboss.
- Sistema de drops biológicos desde miniboss.
- Objetivos de campaña con escenas/briefings más elaborados.
- Mejor serialización de contratos/campaña en save real.
- Primer pase de arte procedural específico por especie.
