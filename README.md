# Grave Alive

Grave Alive is a **7 Days to Die** mod project aimed at making the world feel
populated by autonomous survivor NPCs.

The current repository contains:

- A C# simulation core for survivor NPC traits, needs, inventories, autonomous
  decisions, relationship changes, faction formation, settlement building,
  crafting, trading, patrols, and behavior logging.
- A conditional 7D2D `IModApi` / Harmony entry point in
  `Source/GraveAlive/GameIntegration/ModApi.cs`.
- A visible-survivor spawn planner and cautious game-side spawn adapter that can
  request nearby survivor entities without flooding the world.
- XML entity definitions for a named `Grave Alive Survivor` that reuses vanilla
  survivor behavior and art.
- XML save/load support for Grave Alive survivors, relationships, factions,
  settlements, and visible-spawn state.
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
- Nearby simulated survivors can now be staged near active players and converted
  into visible survivor entity spawn requests.
- The living-world simulation can save and load its state instead of resetting
  every time the game restarts.

## Repository layout

```text
grave/
├── Config/
│   ├── entityclasses.xml
│   ├── entitygroups.xml
│   └── items.xml
├── Source/
│   ├── GraveAlive/
│   │   ├── GameIntegration/
│   │   └── Simulation/
│   └── GraveAlive.Tests/
├── docs/
│   ├── living-world-roadmap.md
│   └── testing-for-non-coders.md
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
          │   ├── entityclasses.xml
          │   ├── entitygroups.xml
          │   └── items.xml
           └── Source/
   ```

The XML patch will load without a DLL. The living-world NPC behavior requires
building and installing `GraveAlive.dll`.

`Config/entityclasses.xml` and `Config/entitygroups.xml` define the named
`Grave Alive Survivor` used by the C# spawner. V 2.6 comments out vanilla
`npcSurvivorRanged`, so the mod restores that survivor definition under
`graveAliveSurvivorRanged` without adding custom models.

## Building the code mod

7 Days to Die code mods require the game's managed assemblies and EAC disabled.
If you are using Mono on Linux/macOS, or this cloud environment, build a
ready-to-copy mod folder with:

```bash
./scripts/package-mod.sh "/path/to/7 Days To Die/7DaysToDie_Data/Managed"
```

That writes:

```text
dist/grave/
```

Copy that whole `grave` folder into your `7 Days To Die/Mods` folder.

If you prefer to build only the DLL, use:

```bash
./scripts/build-game-mod.sh "/path/to/7 Days To Die/7DaysToDie_Data/Managed"
```

That writes:

```text
build/GraveAlive.dll
```

Then copy the built `GraveAlive.dll` into the mod root next to `ModInfo.xml`.
Launch the game or dedicated server with Easy Anti-Cheat disabled and check the
log for:

```text
[GraveAlive] Living-world simulation initialized.
[GraveAlive] Save file: ...
[GraveAlive] Spawned survivor ...
```

For a plain-English checklist, see `docs/testing-for-non-coders.md`.

You can also build from a machine that has a .NET SDK and access to the game
install:

```bash
dotnet build Source/GraveAlive/GraveAlive.csproj \
  -c Release \
  -p:GameManagedDir="/path/to/7 Days To Die/7DaysToDie_Data/Managed"
```

## Testing the simulation core

The test project does not require 7D2D assemblies:

```bash
dotnet run --project Source/GraveAlive.Tests/GraveAlive.Tests.csproj
```

Or with Mono:

```bash
./scripts/test-simulation.sh
```

## Important limitation

The current code advances an autonomous survivor society simulation, plans which
survivors should become visible near players, and includes a first game-side
adapter for spawning those survivor entities. The adapter still needs to be
compiled and tested against an installed 7D2D build because entity/player/world
method names can change between game versions.

POI claiming, actual block placement, advanced NPC pathing, and trader-dialog
integration are still future steps.
