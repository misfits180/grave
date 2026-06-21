using System;
using System.Collections.Generic;
using System.Linq;

namespace GraveAlive.Simulation
{
    public sealed class BehaviorEvent
    {
        public long Tick { get; private set; }
        public Guid? ActorId { get; private set; }
        public string Category { get; private set; }
        public string Message { get; private set; }

        public BehaviorEvent(long tick, Guid? actorId, string category, string message)
        {
            Tick = tick;
            ActorId = actorId;
            Category = category;
            Message = message;
        }
    }

    public sealed class WorldState
    {
        private readonly Dictionary<Guid, SurvivorNpc> _survivors = new Dictionary<Guid, SurvivorNpc>();
        private readonly Dictionary<string, Relationship> _relationships = new Dictionary<string, Relationship>();
        private readonly Dictionary<Guid, Faction> _factions = new Dictionary<Guid, Faction>();
        private readonly Dictionary<Guid, Settlement> _settlements = new Dictionary<Guid, Settlement>();
        private readonly List<BehaviorEvent> _events = new List<BehaviorEvent>();

        public Random Random { get; private set; }
        public long Tick { get; private set; }

        public IEnumerable<SurvivorNpc> Survivors
        {
            get { return _survivors.Values; }
        }

        public IEnumerable<Relationship> Relationships
        {
            get { return _relationships.Values; }
        }

        public IEnumerable<Faction> Factions
        {
            get { return _factions.Values; }
        }

        public IEnumerable<Settlement> Settlements
        {
            get { return _settlements.Values; }
        }

        public IReadOnlyList<BehaviorEvent> Events
        {
            get { return _events; }
        }

        public WorldState(Random random)
        {
            Random = random;
        }

        public void AdvanceTick()
        {
            Tick++;
        }

        public void SetTick(long tick)
        {
            Tick = tick < 0 ? 0 : tick;
        }

        public void AddSurvivor(SurvivorNpc survivor)
        {
            _survivors[survivor.Id] = survivor;
        }

        public SurvivorNpc GetSurvivor(Guid id)
        {
            SurvivorNpc survivor;
            return _survivors.TryGetValue(id, out survivor) ? survivor : null;
        }

        public void AddFaction(Faction faction)
        {
            _factions[faction.Id] = faction;
        }

        public Faction GetFaction(Guid id)
        {
            Faction faction;
            return _factions.TryGetValue(id, out faction) ? faction : null;
        }

        public void AddSettlement(Settlement settlement)
        {
            _settlements[settlement.Id] = settlement;
        }

        public Settlement GetSettlement(Guid id)
        {
            Settlement settlement;
            return _settlements.TryGetValue(id, out settlement) ? settlement : null;
        }

        public Relationship GetOrCreateRelationship(Guid firstId, Guid secondId)
        {
            string key = RelationshipKey(firstId, secondId);
            Relationship relationship;
            if (_relationships.TryGetValue(key, out relationship))
            {
                return relationship;
            }

            relationship = new Relationship(
                firstId,
                secondId,
                Random.Next(20, 61),
                Random.Next(0, 35),
                Random.Next(0, 65),
                Random.Next(10, 55),
                Random.Next(0, 35));
            _relationships[key] = relationship;
            return relationship;
        }

        public void AddRelationship(Relationship relationship)
        {
            _relationships[RelationshipKey(relationship.FirstId, relationship.SecondId)] = relationship;
        }

        public IEnumerable<Relationship> RelationshipsFor(Guid survivorId)
        {
            return _relationships.Values.Where(relationship =>
                relationship.FirstId == survivorId || relationship.SecondId == survivorId);
        }

        public SurvivorNpc OtherSurvivor(Relationship relationship, Guid survivorId)
        {
            Guid otherId = relationship.FirstId == survivorId ? relationship.SecondId : relationship.FirstId;
            return GetSurvivor(otherId);
        }

        public void Record(Guid? actorId, string category, string message)
        {
            _events.Add(new BehaviorEvent(Tick, actorId, category, message));
            if (_events.Count > 500)
            {
                _events.RemoveAt(0);
            }
        }

        public string Snapshot()
        {
            return string.Format(
                "tick={0}, survivors={1}, relationships={2}, factions={3}, settlements={4}",
                Tick,
                _survivors.Count,
                _relationships.Count,
                _factions.Count,
                _settlements.Count);
        }

        private static string RelationshipKey(Guid firstId, Guid secondId)
        {
            string first = firstId.ToString("N");
            string second = secondId.ToString("N");
            return string.CompareOrdinal(first, second) <= 0
                ? first + ":" + second
                : second + ":" + first;
        }
    }
}
