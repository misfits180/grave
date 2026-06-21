# Grave Alive living-world roadmap

This document tracks the path from the current autonomous simulation core to a
fully in-world 7 Days to Die NPC mod.

## Design goals

- Survivor NPCs should feel self-directed rather than quest props.
- Relationships should evolve from repeated behavior, not fixed labels.
- Factions should emerge when trusted survivors have aligned needs.
- Settlements should be practical: camps first, then defended outposts/bases.
- Trade should connect survivors, factions, trader NPCs, and the player.
- The system should be tunable enough for single-player and dedicated servers.

## Current implementation

Implemented in `Source/GraveAlive/Simulation`:

- Survivor traits: bravery, sociability, compassion, aggression, crafting,
  building, and trading.
- Needs: hunger, safety, belonging, and ambition.
- Inventories and stockpiles.
- Relationship scores: trust, fear, attraction, loyalty, and grievance.
- Relationship kinds: stranger, acquaintance, friend, rival, enemy, and love
  interest.
- Autonomous intents: gather, craft, build, trade, patrol, rest, seek ally, and
  socialize.
- Faction creation from high-trust relationship clusters.
- Camp/base creation and improvement.
- Survivor world positions and visibility state.
- Spawn planning that stages nearby simulated survivors around active players,
  respects a maximum visible survivor count, and creates spawn/despawn requests.
- Behavior event log for debugging and future in-game display.

Implemented in `Source/GraveAlive/GameIntegration`:

- Conditional `IModApi` entry point.
- Harmony patch against the game update loop.
- Periodic log summary of the living-world simulation.
- First-pass visible survivor spawner that tries the standard
  `EntityFactory.CreateEntity` plus `World.SpawnEntityInWorld` flow.
- Reflection-based player/world/entity lookup so minor 7D2D method changes are
  easier to patch after a local compile/test.

## Integration phases

### Phase 1: Verified game lifecycle hooks

- Build against the local 7D2D `7DaysToDie_Data/Managed` assemblies.
- Verify the `GameManager.Update` patch for the target game version.
- Replace periodic log-only updates with save/load-aware lifecycle hooks.
- Persist simulation state alongside the world save.

### Phase 2: Survivor entity spawning

- Use `Config/entitygroups.xml` to define the `GraveAliveSurvivors` group from
  vanilla survivor-like entity names.
- Stage a limited first wave of simulated survivors around active players.
- Create visible entities through the game-world spawn adapter.
- Add spawn rules for wilderness, roads, POI edges, and faction camps.
- Map each `SurvivorNpc.Id` to an in-world entity id.
- Despawn distant survivors back into simulation state to protect performance.

Current status: the planning and first adapter code are implemented. The next
step is a local 7D2D compile/play-test to verify the exact entity group,
`EntityFactory`, player-list, spawn, and remove methods for the installed game
version.

### Phase 3: AI action adapters

Map simulation intents to game actions:

- `Gather`: navigate to resource nodes/loot containers and add stockpile items.
- `Craft`: create inventory items from recipes.
- `Build`: place or upgrade blocks at camp/base claims.
- `Trade`: open a survivor barter interface or exchange generated stock.
- `Patrol`: path around claimed territory and react to enemies.
- `Socialize`: idle near allies and update relationship scores.

### Phase 4: Settlements and factions in the world

- Claim camp anchors away from protected trader areas.
- Use block-placement budgets per tick to avoid server stalls.
- Add faction ownership metadata to camps/outposts/bases.
- Let factions abandon, merge, defend, or expand settlements based on needs and
  rival pressure.

### Phase 5: Player and trader interaction

- Add dialogue/interaction hooks for survivor barter.
- Let survivors trade with vanilla traders when close enough.
- Track player reputation per survivor and per faction.
- Add recruit, ally, threaten, hire, romance, and feud paths.

## Technical risks

- 7D2D code-mod APIs shift between versions; exact Harmony targets must be
  verified against the installed `Assembly-CSharp.dll`.
- Easy Anti-Cheat must be disabled for C# code mods.
- Large numbers of active NPC entities can hurt server performance; distant NPCs
  should remain simulated instead of fully spawned.
- Building bases requires careful block placement validation to avoid protected
  POIs, trader areas, land claims, and corrupted saves.
