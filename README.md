# Grave Alive

Grave Alive is a **7 Days to Die** mod project aimed at making the world feel
populated by autonomous survivor NPCs.

The current repository contains:

- A C# simulation core for survivor NPC traits, needs, inventories, autonomous
  decisions, relationship changes, faction formation, settlement building,
  crafting, trading, patrols, and behavior logging.
- A conditional 7D2D `IModApi` / Harmony entry point in
  `Source/GraveAlive/GameIntegration/ModApi.cs`.
- A small XML quality-of-life patch that raises `resourceWood` stack size to
  `10000`.

The simulation code is intentionally separated from the game-facing hook so it
can be tested outside the game and then wired into real 7D2D entities once the
game assemblies are available.

## Current living-world systems

- Survivors spawn into the simulation with randomized traits and starting gear.
- Relationship state tracks trust, fear, attraction, loyalty, and grievances.
- Relationships can become acquaintances, friends, enemies, rivals, or love
  interests.
- Survivors can autonomously:
  - gather food and materials,
  - craft tools, weapons, and armor,
  - trade resources with compatible survivors,
  - form factions with trusted allies,
  - found and improve camps/outposts/bases,
  - patrol territory,
  - socialize and strengthen or damage relationships.
- Recent behavior events are retained for debugging and future in-game UI/log
  output.

## Repository layout

```text
grave/
├── Config/
│   └── items.xml
├── Source/
│   ├── GraveAlive/
│   │   ├── GameIntegration/
│   │   └── Simulation/
│   └── GraveAlive.Tests/
├── docs/
│   └── living-world-roadmap.md
├── ModInfo.xml
└── README.md
```

## Installing the XML modlet

1. Open your 7 Days to Die install directory.
2. Create a `Mods` directory if it does not already exist.
3. Copy this repository folder into `Mods` as `grave`.
4. Confirm the final layout starts like this:

   ```text
   7 Days To Die/
   └── Mods/
       └── grave/
           ├── ModInfo.xml
           ├── Config/
           │   └── items.xml
           └── Source/
   ```

The XML patch will load without a DLL. The living-world NPC behavior requires
building and installing `GraveAlive.dll`.

## Building the code mod

7 Days to Die code mods require the game's managed assemblies and EAC disabled.
Build from a machine that has a .NET SDK and access to the game install:

```bash
dotnet build Source/GraveAlive/GraveAlive.csproj \
  -c Release \
  -p:GameManagedDir="/path/to/7 Days To Die/7DaysToDie_Data/Managed"
```

Then copy the built `GraveAlive.dll` into the mod root next to `ModInfo.xml`.
Launch the game or dedicated server with Easy Anti-Cheat disabled and check the
log for:

```text
[GraveAlive] Living-world simulation initialized.
```

## Testing the simulation core

The test project does not require 7D2D assemblies:

```bash
dotnet run --project Source/GraveAlive.Tests/GraveAlive.Tests.csproj
```

## Important limitation

The current code advances an autonomous survivor society simulation and exposes
a Harmony update hook. Actual in-world entity spawning, POI claiming, block
placement, NPC pathing, and trader-dialog integration still need to be wired
against verified 7D2D 1.x `Assembly-CSharp.dll` method signatures.
