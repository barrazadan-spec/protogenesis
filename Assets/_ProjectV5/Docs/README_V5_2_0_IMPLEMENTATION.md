# Protogenesis Primordia — V5 Prototype 2.0

## Focus

This milestone adds a stronger world-builder layer on top of the RTS/cellular loop.
The colony now claims macro districts as it colonizes the droplet, assigns ecological
functions to those districts, and receives supply/network pressure feedback.

## New systems

### Colony District System

File:

`Assets/_ProjectV5/Scripts/Districts/V5ColonyDistrictSystem.cs`

Hotkeys:

- `=` toggles the district panel.
- `Keypad +` auto-designs claimed districts.
- `Keypad -` exports district profile JSON.
- `Keypad *` imports district profile JSON.

District types:

- Nursery: improves biomass/amino acid growth and local colonization.
- Metabolic Reactor: improves ATP/minerals and oxygen output.
- Detox Matrix: reduces toxins and pulls acidity toward neutral.
- Defensive Bastion: repairs allied cells and stresses enemies inside the district.
- Photic Shelf: improves light/oxygen economy.
- Hunting Ground: converts detritus/enemy pressure into biomass/lipids.

The system also adds **Supply Pressure**:

- Higher claimed district count and network efficiency raise supply capacity.
- Excess cell population over supply adds stress to the colony.
- Storage vacuoles, fimbriae, farmers, colonizers and recyclers improve logistics.

### Demo Milestone 2.0 System

File:

`Assets/_ProjectV5/Scripts/Milestone/V5DemoMilestone20System.cs`

Hotkeys:

- `End` toggles the 2.0 demo milestone panel.
- `Ctrl + PageDown` stabilizes the run for demo testing.
- `Ctrl + Insert` spawns demo pressure.
- `Ctrl + PageUp` exports a 2.0 playtest report JSON.

The panel tracks whether the run proves the full loop:

1. LUCA awake.
2. Interior online.
3. Domain chosen.
4. Microcolony formed.
5. RTS control demonstrated.
6. World altered.
7. District claimed.
8. Crisis pressure present.
9. Endgame route visible.

### Auto-installer

File:

`Assets/_ProjectV5/Scripts/Core/V5Prototype20AutoInstaller.cs`

This automatically adds the 2.0 systems to any V5 scene at runtime.

## How to test

1. Open Unity.
2. Use `Protogenesis > V5 > Create Full Prototype Scene`.
3. Press Play.
4. Use `E` to install structures and choose metabolism.
5. Divide with `D` and assign colonize with `5`.
6. Press `=` after colonization starts to inspect claimed districts.
7. Press `End` to inspect 2.0 demo readiness.

## Notes

This version keeps all additions isolated in `_ProjectV5` and does not modify legacy systems.
