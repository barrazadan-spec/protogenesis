# Protogenesis: Primordia — V5 Prototype 2.1

## Focus
V5 2.1 adds a macro-RTS production layer: **Colony Operations**. The colony now has readable logistics health, bottlenecks and work orders.

## New system

`Assets/_ProjectV5/Scripts/Operations/V5ColonyOperationsSystem.cs`

It calculates:

- Logistics Health
- Nutrient Flow
- Network Load
- Worker Coverage
- Current Bottleneck

It also adds work orders:

- Harvest
- Colonize
- Defend
- Detox
- Hunt
- Expand

Orders auto-assign cells by distance, health, role affinity and current task.

## Controls

```txt
KeypadEnter: open/close Colony Operations panel
Keypad0: toggle auto-assign workers
Keypad .: toggle auto-stabilize
Ctrl + Keypad1: create Harvest order at mouse
Ctrl + Keypad2: create Colonize order at mouse
Ctrl + Keypad3: create Defend order at mouse
Ctrl + Keypad4: create Detox order at mouse
Ctrl + Keypad5: create Hunt order at mouse
Ctrl + Keypad6: create Expand order at mouse
```

## Gameplay effects

- Low logistics increases colony stress.
- Healthy logistics slowly reduces stress.
- Strong logistics can spend mother resources to support damaged cells.
- Auto-stabilize detoxes and strengthens the mother area when logistics drops too low.
- Work orders turn macro intent into cell directives without microing every unit.

## Auto-installer

`V5Prototype21AutoInstaller` installs the operations system automatically in any V5 scene.

## Version

`V5Balance.SaveVersion = 2.1`.

## Test path

1. Use `Protogenesis > V5 > Create Full Prototype Scene`.
2. Press Play.
3. Build a small colony.
4. Press `KeypadEnter`.
5. Create Harvest / Colonize / Defend orders.
6. Watch Logistics Health, Nutrient Flow and Bottleneck.
