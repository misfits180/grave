# Testing Grave Alive without coding experience

This guide explains what to check after somebody builds the `GraveAlive.dll`
file for you.

If somebody ran the package script for you, the installable folder is:

```text
dist/grave
```

Copy that whole `grave` folder into your `7 Days To Die/Mods` folder.

If you have a built file at:

```text
build/GraveAlive.dll
```

copy that file into your mod folder before starting the game.

## Before starting the game

Your mod folder should look like this:

```text
7 Days To Die/
└── Mods/
    └── grave/
        ├── GraveAlive.dll
        ├── ModInfo.xml
        ├── Config/
        │   ├── entityclasses.xml
        │   ├── entitygroups.xml
        │   └── items.xml
        └── Source/
```

Start 7 Days to Die with **Easy Anti-Cheat turned off**. Code mods usually will
not load with Easy Anti-Cheat enabled.

## What success looks like

Open the game log after loading into a world. Look for these messages:

```text
[GraveAlive] Living-world simulation initialized.
[GraveAlive] Save file: ...
[GraveAlive] Spawned survivor ...
```

If you see `Spawned survivor`, the mod has reached the important first visible
NPC milestone: it asked the game to create a survivor near a player and the game
accepted the request.

If you see `Save file`, the mod has picked a place to remember the living-world
state. That save file is separate from your normal 7 Days to Die save. It stores
the Grave Alive survivors, relationships, factions, settlements, and spawn
status.

The regular status line now also includes spawn counts:

```text
spawn visible=1, spawnPending=0, despawnPending=0, coolingDown=0, simulated=11
```

Plain English meaning:

- `visible`: survivors currently spawned into the game world.
- `spawnPending`: survivors the mod is trying to make visible.
- `despawnPending`: survivors the mod is trying to remove from the visible world
  because they are far away.
- `coolingDown`: survivors that failed to spawn recently and are waiting before
  trying again.
- `simulated`: survivors still living in the background simulation.

## If nothing appears

Check the log for messages like:

```text
could not resolve entity class graveAliveSurvivorRanged
EntityFactory.CreateEntity was not found
World.SpawnEntityInWorld was not found
```

Those messages are useful. They mean the mod loaded, but the exact 7 Days to Die
game method or entity name is different for your installed game version. That is
the next thing a developer would adjust.

## If the log repeats errors forever

The mod now has a retry cooldown. That means it should pause after failed spawn
attempts instead of spamming the same error every frame.

If you still see constant repeated errors, copy the repeated lines from the log.
Those lines will tell us what to fix next.

## If survivors reset after restarting

Look for:

```text
[GraveAlive] Saved living-world state
```

If you never see that line, the mod did not save successfully. Copy any line
that says:

```text
[GraveAlive] Failed to save living-world state
```

That message usually means the game could not write to the selected folder.

If the save file exists but cannot be read, the mod should start a fresh
simulation instead of crashing the game. The log will say it could not load the
Grave Alive save.
