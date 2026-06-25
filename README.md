# Wandering Souls

A 7 Days to Die mod that adds autonomous survivor NPCs with personality traits, loyalty, needs, and romance dialogue.

## Features

- Custom `EntityWanderingSurvivor` NPC with save/load persistence
- Random personality traits (Brave, Cautious, Aggressive, Curious, Loyal)
- Procedural name generation
- Brain/goal system (hunger, energy, follow player)
- Loyalty-based faction switching (friendly vs hostile)
- Romance dialogue lines (ready for UI/trigger hookup)
- Console command: `spawn-survivor` or `ssurvivor`

## Installation

1. Copy the `WanderingSouls` folder into your game's `Mods` directory.
2. Build the C# DLL (see below) and place `WanderingSouls.dll` in the mod folder.
3. Launch with EAC disabled.

## Building the DLL

1. Copy these files from your game install into `7dtd-binaries/Managed/`:
   - `Assembly-CSharp.dll`
   - `0Harmony.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `LogLibrary.dll`

   Source path: `7 Days To Die/7DaysToDie_Data/Managed/`

2. Build from the repo root:

```bash
dotnet build WanderingSouls/Scripts/WanderingSouls.csproj -c Release
```

Or set an explicit game path:

```bash
dotnet build WanderingSouls/Scripts/WanderingSouls.csproj -c Release -p:7DTD_MANAGED="/path/to/7DaysToDie_Data/Managed"
```

The compiled `WanderingSouls.dll` is output to the `WanderingSouls/` mod folder.

## In-Game Testing

Open the console (F1) and run:

```
spawn-survivor
```

## Mod Structure

```
WanderingSouls/
  ModInfo.xml
  WanderingSouls.dll          (build output)
  Config/
    archetypes.xml
    buffs.xml
    entityclasses.xml
    entitygroups.xml
    loot.xml
  Scripts/
    *.cs
    WanderingSouls.csproj
```

## Work In Progress

- Dialogue triggers not yet wired to gameplay events
- Food/water seeking is stubbed in the brain component
- Archetype randomization defined but not linked on entity class
