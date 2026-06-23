# Protogenesis: Primordia — V5 Prototype 2.2

## Focus
V5 2.2 adds a clearer RTS/world-builder infrastructure layer. The colony can now build a soft **Supply Network** and issue **Tactical Pings** so expansion does not depend only on direct cell micro.

## Added Systems

### `V5SupplyNetworkSystem`
Creates data-only supply relays on the world map. Relays are not final art/prefabs yet; they are an infrastructure prototype for testing logistics, expansion pressure and colony support.

Relay types:

- **MetabolicHub** — basic backbone, improves ATP/oxygen flow.
- **DetoxFilter** — reduces toxins and acidity around the relay.
- **DefensiveMatrix** — reinforces nearby cells and stabilizes border zones.
- **NurseryNode** — helps colonization, stress recovery and safe expansion.
- **PhoticCollector** — boosts light/oxygen for photosynthetic colonies.

Panel data:

- Coverage
- Throughput
- Stability
- Bottleneck
- Relay list
- Export snapshot JSON

Hotkeys:

```txt
Keypad+             Toggle Supply Network panel
Ctrl + Keypad+      Build selected relay at mouse
Alt + Keypad+       Remove nearest relay at mouse
```

### `V5TacticalPingSystem`
Adds broad macro command pings. Pings affect nearby cells first, then fall back to non-mother cells if there are no local workers.

Ping types:

- Rally
- Harvest
- Expand
- FallBack
- AttackThreat
- DefendMother

Hotkeys:

```txt
Keypad/             Toggle Tactical Pings panel
Shift + Keypad7     Rally ping
Shift + Keypad8     Harvest ping
Shift + Keypad9     Expand ping
Shift + Keypad4     Fall Back ping
Shift + Keypad5     Attack Threat ping
Shift + Keypad6     Defend Mother ping
```

## Auto Installer
`V5Prototype22AutoInstaller` automatically adds:

- `V5SupplyNetworkSystem`
- `V5TacticalPingSystem`

This works in any existing V5 scene after `V5GameManager` is present.

## Updated

- `V5Balance.SaveVersion = 2.2`
- HUD label updated to `V5 BUILD 2.2`
- HUD controls updated with Supply Network and Tactical Pings hints.

## Intended Test Loop

1. Generate full prototype scene.
2. Play until first colony.
3. Build a MetabolicHub near the mother.
4. Extend with Nursery/Detox/Photic relays into colonized territory.
5. Use tactical pings to push harvest/expand/defense orders.
6. Watch whether stress and overexpansion become more manageable.

## Notes
This is still prototype infrastructure. Relays are intentionally data-only and drawn with Gizmos so the underlying design can be tuned before final sprites, shaders and placement UI.
