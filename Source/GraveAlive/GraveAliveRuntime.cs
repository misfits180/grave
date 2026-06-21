using System;
using System.Collections.Generic;
using System.Linq;
using GraveAlive.Simulation;

namespace GraveAlive
{
    public sealed class GraveAliveRuntime
    {
        private readonly AiDirector _director;
        private readonly SimulationSettings _settings;
        private readonly SurvivorSpawnCoordinator _spawnCoordinator;
        private float _accumulatedSeconds;

        public WorldState World { get; private set; }
        public SurvivorSpawnCoordinator SpawnCoordinator
        {
            get { return _spawnCoordinator; }
        }

        public GraveAliveRuntime(SimulationSettings settings, int seed)
        {
            _settings = settings ?? new SimulationSettings();
            World = new WorldState(new Random(seed));
            _director = new AiDirector(_settings);
            _spawnCoordinator = new SurvivorSpawnCoordinator(_settings);
            SeedSurvivors();
            PrimeRelationships();
            World.Record(null, "system", "GraveAlive initialized: " + World.Snapshot());
        }

        public void Update(float elapsedSeconds)
        {
            if (elapsedSeconds <= 0)
            {
                return;
            }

            _accumulatedSeconds += elapsedSeconds;
            while (_accumulatedSeconds >= _settings.TickSeconds)
            {
                _director.Advance(World, 1);
                _accumulatedSeconds -= _settings.TickSeconds;
            }
        }

        public void AdvanceTicks(int ticks)
        {
            _director.Advance(World, ticks);
        }

        public IReadOnlyList<SurvivorSpawnRequest> PlanVisibleSurvivorSpawns(IEnumerable<WorldPosition> playerPositions)
        {
            return _spawnCoordinator.Plan(World, playerPositions);
        }

        public void MarkSpawnSucceeded(Guid survivorId, int entityId)
        {
            SurvivorNpc survivor = World.GetSurvivor(survivorId);
            _spawnCoordinator.MarkSpawnSucceeded(survivorId, entityId, World.Tick);
            World.Record(
                survivorId,
                "spawn",
                (survivor == null ? "A survivor" : survivor.Name) + " became visible in the world as entity " + entityId + ".");
        }

        public void MarkSpawnFailed(Guid survivorId, string reason)
        {
            SurvivorNpc survivor = World.GetSurvivor(survivorId);
            _spawnCoordinator.MarkSpawnFailed(survivorId, World.Tick);
            World.Record(
                survivorId,
                "spawn",
                (survivor == null ? "A survivor" : survivor.Name) + " could not spawn: " + reason + ".");
        }

        public void MarkDespawnSucceeded(Guid survivorId)
        {
            SurvivorNpc survivor = World.GetSurvivor(survivorId);
            _spawnCoordinator.MarkDespawnSucceeded(survivorId);
            World.Record(
                survivorId,
                "despawn",
                (survivor == null ? "A survivor" : survivor.Name) + " returned to background simulation.");
        }

        public string Summary()
        {
            BehaviorEvent lastEvent = World.Events.LastOrDefault();
            string lastMessage = lastEvent == null ? "none" : lastEvent.Category + ": " + lastEvent.Message;
            return World.Snapshot() + ", " + _spawnCoordinator.Summary(World.Tick) + ", lastEvent=" + lastMessage;
        }

        private void SeedSurvivors()
        {
            for (int i = 0; i < _settings.StartingSurvivorCount; i++)
            {
                SurvivorNpc survivor = new SurvivorNpc(
                    Guid.NewGuid(),
                    NameGenerator.SurvivorName(World.Random, i),
                    TraitProfile.Randomized(World.Random));
                survivor.Position = StartingPositionFor(i);

                survivor.Inventory.Add(ResourceKind.Food, 2 + World.Random.Next(1, 5));
                survivor.Inventory.Add(ResourceKind.Wood, 18 + World.Random.Next(0, 18));
                survivor.Inventory.Add(ResourceKind.Stone, 8 + World.Random.Next(0, 12));
                survivor.Inventory.Add(ResourceKind.Iron, World.Random.Next(0, 8));
                survivor.Inventory.Add(ResourceKind.Cloth, World.Random.Next(0, 8));

                if (World.Random.Next(100) < 25)
                {
                    survivor.Inventory.Add(ResourceKind.Medicine, 1);
                }

                World.AddSurvivor(survivor);
                _spawnCoordinator.GetOrCreateState(survivor);
            }
        }

        private void PrimeRelationships()
        {
            List<SurvivorNpc> survivors = World.Survivors.ToList();
            for (int i = 0; i < survivors.Count; i++)
            {
                for (int j = i + 1; j < survivors.Count; j++)
                {
                    Relationship relationship = World.GetOrCreateRelationship(survivors[i].Id, survivors[j].Id);
                    if (i < 3 && j < 3)
                    {
                        relationship.Change(35, -10, 10, 35, -15);
                    }
                }
            }
        }

        private WorldPosition StartingPositionFor(int index)
        {
            float ring = index < 4 ? 35f : 90f + (index % 4) * 35f;
            double angle = (Math.PI * 2.0 * index) / Math.Max(1, _settings.StartingSurvivorCount);
            float x = (float)(Math.Cos(angle) * ring);
            float z = (float)(Math.Sin(angle) * ring);
            return new WorldPosition(x, 0, z);
        }
    }
}
