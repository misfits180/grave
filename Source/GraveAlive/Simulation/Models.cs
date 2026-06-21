using System;
using System.Collections.Generic;
using System.Linq;

namespace GraveAlive.Simulation
{
    public enum RelationshipKind
    {
        Stranger,
        Acquaintance,
        Friend,
        Rival,
        Enemy,
        LoveInterest,
        FactionMate
    }

    public enum SurvivorIntent
    {
        Gather,
        Craft,
        Build,
        Trade,
        Patrol,
        Rest,
        SeekAlly,
        Socialize
    }

    public enum ResourceKind
    {
        Food,
        Water,
        Wood,
        Stone,
        Iron,
        Cloth,
        Medicine,
        Weapons,
        Tools,
        Armor,
        Ammo
    }

    public enum SettlementKind
    {
        Camp,
        FortifiedCamp,
        Outpost,
        Base
    }

    public sealed class TraitProfile
    {
        public int Bravery { get; private set; }
        public int Sociability { get; private set; }
        public int Compassion { get; private set; }
        public int Aggression { get; private set; }
        public int Crafting { get; private set; }
        public int Building { get; private set; }
        public int Trading { get; private set; }

        public TraitProfile(
            int bravery,
            int sociability,
            int compassion,
            int aggression,
            int crafting,
            int building,
            int trading)
        {
            Bravery = Clamp(bravery);
            Sociability = Clamp(sociability);
            Compassion = Clamp(compassion);
            Aggression = Clamp(aggression);
            Crafting = Clamp(crafting);
            Building = Clamp(building);
            Trading = Clamp(trading);
        }

        public static TraitProfile Randomized(Random random)
        {
            return new TraitProfile(
                random.Next(20, 91),
                random.Next(20, 91),
                random.Next(10, 91),
                random.Next(5, 86),
                random.Next(10, 91),
                random.Next(10, 91),
                random.Next(10, 91));
        }

        private static int Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 100)
            {
                return 100;
            }

            return value;
        }
    }

    public sealed class Needs
    {
        public int Hunger { get; private set; }
        public int Safety { get; private set; }
        public int Belonging { get; private set; }
        public int Ambition { get; private set; }

        public Needs(int hunger, int safety, int belonging, int ambition)
        {
            Hunger = Clamp(hunger);
            Safety = Clamp(safety);
            Belonging = Clamp(belonging);
            Ambition = Clamp(ambition);
        }

        public void Change(int hunger, int safety, int belonging, int ambition)
        {
            Hunger = Clamp(Hunger + hunger);
            Safety = Clamp(Safety + safety);
            Belonging = Clamp(Belonging + belonging);
            Ambition = Clamp(Ambition + ambition);
        }

        private static int Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 100)
            {
                return 100;
            }

            return value;
        }
    }

    public sealed class Inventory
    {
        private readonly Dictionary<ResourceKind, int> _items = new Dictionary<ResourceKind, int>();

        public IReadOnlyDictionary<ResourceKind, int> Items
        {
            get { return _items; }
        }

        public int Get(ResourceKind kind)
        {
            int value;
            return _items.TryGetValue(kind, out value) ? value : 0;
        }

        public bool Has(ResourceKind kind, int amount)
        {
            return Get(kind) >= amount;
        }

        public void Add(ResourceKind kind, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _items[kind] = Get(kind) + amount;
        }

        public bool Remove(ResourceKind kind, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            int current = Get(kind);
            if (current < amount)
            {
                return false;
            }

            int remaining = current - amount;
            if (remaining == 0)
            {
                _items.Remove(kind);
            }
            else
            {
                _items[kind] = remaining;
            }

            return true;
        }

        public ResourceKind? MostAbundant()
        {
            if (_items.Count == 0)
            {
                return null;
            }

            return _items.OrderByDescending(pair => pair.Value).First().Key;
        }

        public ResourceKind? FirstSurplus(int threshold)
        {
            foreach (KeyValuePair<ResourceKind, int> pair in _items.OrderByDescending(pair => pair.Value))
            {
                if (pair.Value > threshold)
                {
                    return pair.Key;
                }
            }

            return null;
        }
    }

    public sealed class SurvivorNpc
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public TraitProfile Traits { get; private set; }
        public Needs Needs { get; private set; }
        public Inventory Inventory { get; private set; }
        public Guid? FactionId { get; set; }
        public Guid? HomeSettlementId { get; set; }
        public WorldPosition Position { get; set; }
        public int CraftingSkill { get; private set; }
        public int BuildingSkill { get; private set; }
        public int CombatSkill { get; private set; }
        public int TradeSkill { get; private set; }

        public SurvivorNpc(Guid id, string name, TraitProfile traits)
        {
            Id = id;
            Name = name;
            Traits = traits;
            Needs = new Needs(70, 60, 55, 55);
            Inventory = new Inventory();
            Position = new WorldPosition(0, 0, 0);
            CraftingSkill = traits.Crafting;
            BuildingSkill = traits.Building;
            CombatSkill = (traits.Bravery + traits.Aggression) / 2;
            TradeSkill = traits.Trading;
        }

        public ResourceKind MostNeededResource()
        {
            if (Needs.Hunger < 50)
            {
                return ResourceKind.Food;
            }

            if (Inventory.Get(ResourceKind.Tools) == 0)
            {
                return ResourceKind.Tools;
            }

            if (Inventory.Get(ResourceKind.Weapons) == 0 && CombatSkill > 45)
            {
                return ResourceKind.Weapons;
            }

            if (Inventory.Get(ResourceKind.Armor) == 0 && Needs.Safety < 60)
            {
                return ResourceKind.Armor;
            }

            if (Inventory.Get(ResourceKind.Medicine) == 0)
            {
                return ResourceKind.Medicine;
            }

            return ResourceKind.Food;
        }
    }

    public sealed class Relationship
    {
        public Guid FirstId { get; private set; }
        public Guid SecondId { get; private set; }
        public int Trust { get; private set; }
        public int Fear { get; private set; }
        public int Attraction { get; private set; }
        public int Loyalty { get; private set; }
        public int Grievance { get; private set; }

        public Relationship(Guid firstId, Guid secondId, int trust, int fear, int attraction, int loyalty, int grievance)
        {
            FirstId = firstId;
            SecondId = secondId;
            Trust = Clamp(trust);
            Fear = Clamp(fear);
            Attraction = Clamp(attraction);
            Loyalty = Clamp(loyalty);
            Grievance = Clamp(grievance);
        }

        public RelationshipKind Kind
        {
            get
            {
                if (Grievance >= 75 || Fear >= 85)
                {
                    return RelationshipKind.Enemy;
                }

                if (Grievance >= 55 || Fear >= 65)
                {
                    return RelationshipKind.Rival;
                }

                if (Trust >= 70 && Attraction >= 70 && Grievance < 35)
                {
                    return RelationshipKind.LoveInterest;
                }

                if (Trust >= 62 && Loyalty >= 50)
                {
                    return RelationshipKind.Friend;
                }

                if (Trust >= 35)
                {
                    return RelationshipKind.Acquaintance;
                }

                return RelationshipKind.Stranger;
            }
        }

        public void Change(int trust, int fear, int attraction, int loyalty, int grievance)
        {
            Trust = Clamp(Trust + trust);
            Fear = Clamp(Fear + fear);
            Attraction = Clamp(Attraction + attraction);
            Loyalty = Clamp(Loyalty + loyalty);
            Grievance = Clamp(Grievance + grievance);
        }

        private static int Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 100)
            {
                return 100;
            }

            return value;
        }
    }

    public sealed class Faction
    {
        private readonly HashSet<Guid> _members = new HashSet<Guid>();

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid LeaderId { get; private set; }
        public Guid? HomeSettlementId { get; set; }

        public IEnumerable<Guid> Members
        {
            get { return _members; }
        }

        public Faction(Guid id, string name, Guid leaderId)
        {
            Id = id;
            Name = name;
            LeaderId = leaderId;
            _members.Add(leaderId);
        }

        public void AddMember(Guid survivorId)
        {
            _members.Add(survivorId);
        }

        public bool Contains(Guid survivorId)
        {
            return _members.Contains(survivorId);
        }
    }

    public sealed class Settlement
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public SettlementKind Kind { get; private set; }
        public Inventory Stockpile { get; private set; }
        public int Defenses { get; private set; }
        public int Comfort { get; private set; }
        public int Workshops { get; private set; }

        public Settlement(Guid id, string name, SettlementKind kind)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Stockpile = new Inventory();
            Defenses = 10;
            Comfort = 10;
            Workshops = 0;
        }

        public void Improve(int defenses, int comfort, int workshops)
        {
            Defenses = Clamp(Defenses + defenses);
            Comfort = Clamp(Comfort + comfort);
            Workshops = Clamp(Workshops + workshops);

            if (Defenses >= 70 && Workshops >= 2)
            {
                Kind = SettlementKind.Base;
            }
            else if (Defenses >= 45)
            {
                Kind = SettlementKind.Outpost;
            }
            else if (Defenses >= 25)
            {
                Kind = SettlementKind.FortifiedCamp;
            }
        }

        private static int Clamp(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 100)
            {
                return 100;
            }

            return value;
        }
    }
}
