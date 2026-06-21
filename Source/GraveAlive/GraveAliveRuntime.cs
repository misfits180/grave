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
        private float _accumulatedSeconds;

        public WorldState World { get; private set; }

        public GraveAliveRuntime(SimulationSettings settings, int seed)
        {
            _settings = settings ?? new SimulationSettings();
            World = new WorldState(new Random(seed));
            _director = new AiDirector(_settings);
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

        public string Summary()
        {
            BehaviorEvent lastEvent = World.Events.LastOrDefault();
            string lastMessage = lastEvent == null ? "none" : lastEvent.Category + ": " + lastEvent.Message;
            return World.Snapshot() + ", lastEvent=" + lastMessage;
        }

        private void SeedSurvivors()
        {
            for (int i = 0; i < _settings.StartingSurvivorCount; i++)
            {
                SurvivorNpc survivor = new SurvivorNpc(
                    Guid.NewGuid(),
                    NameGenerator.SurvivorName(World.Random, i),
                    TraitProfile.Randomized(World.Random));

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
    }
}
