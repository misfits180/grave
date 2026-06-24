using System;
using System.Collections.Generic;
using System.Linq;

namespace GraveAlive.Simulation
{
    public sealed class AiDirector
    {
        private readonly SimulationSettings _settings;

        public AiDirector(SimulationSettings settings)
        {
            _settings = settings;
        }

        public void Advance(WorldState world, int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                Step(world);
            }
        }

        private void Step(WorldState world)
        {
            world.AdvanceTick();

            foreach (SurvivorNpc survivor in world.Survivors.ToList())
            {
                survivor.Needs.Change(-2, -1, -1, -1);
                SurvivorIntent intent = ChooseIntent(world, survivor);
                ExecuteIntent(world, survivor, intent);
            }

            TryFormFactions(world);
        }

        private SurvivorIntent ChooseIntent(WorldState world, SurvivorNpc survivor)
        {
            if (survivor.Needs.Hunger < 45 || survivor.Inventory.Get(ResourceKind.Food) < 2)
            {
                return SurvivorIntent.Gather;
            }

            if (!survivor.FactionId.HasValue && HasFriendlyProspect(world, survivor))
            {
                return SurvivorIntent.SeekAlly;
            }

            if (!survivor.HomeSettlementId.HasValue &&
                survivor.Inventory.Get(ResourceKind.Wood) >= _settings.SettlementWoodCost &&
                survivor.Inventory.Get(ResourceKind.Stone) >= _settings.SettlementStoneCost)
            {
                return SurvivorIntent.Build;
            }

            if (CanTrade(world, survivor))
            {
                return SurvivorIntent.Trade;
            }

            if (CanCraft(survivor))
            {
                return SurvivorIntent.Craft;
            }

            if (NeedsSettlementWork(world, survivor))
            {
                return SurvivorIntent.Build;
            }

            if (survivor.Needs.Safety < 45 || survivor.Traits.Aggression > 70)
            {
                return SurvivorIntent.Patrol;
            }

            return world.Random.Next(0, 3) == 0 ? SurvivorIntent.Socialize : SurvivorIntent.Gather;
        }

        private void ExecuteIntent(WorldState world, SurvivorNpc survivor, SurvivorIntent intent)
        {
            switch (intent)
            {
                case SurvivorIntent.Gather:
                    Gather(world, survivor);
                    break;
                case SurvivorIntent.Craft:
                    Craft(world, survivor);
                    break;
                case SurvivorIntent.Build:
                    Build(world, survivor);
                    break;
                case SurvivorIntent.Trade:
                    Trade(world, survivor);
                    break;
                case SurvivorIntent.Patrol:
                    Patrol(world, survivor);
                    break;
                case SurvivorIntent.SeekAlly:
                    SeekAlly(world, survivor);
                    break;
                case SurvivorIntent.Socialize:
                    Socialize(world, survivor);
                    break;
                default:
                    survivor.Needs.Change(4, 2, 1, -2);
                    world.Record(survivor.Id, "rest", survivor.Name + " rests and takes stock.");
                    break;
            }
        }

        private void Gather(WorldState world, SurvivorNpc survivor)
        {
            ResourceKind resource;
            int roll = world.Random.Next(100);
            if (survivor.Needs.Hunger < 55 && roll < 50)
            {
                resource = ResourceKind.Food;
            }
            else if (roll < 35)
            {
                resource = ResourceKind.Wood;
            }
            else if (roll < 58)
            {
                resource = ResourceKind.Stone;
            }
            else if (roll < 78)
            {
                resource = ResourceKind.Iron;
            }
            else if (roll < 90)
            {
                resource = ResourceKind.Cloth;
            }
            else
            {
                resource = ResourceKind.Medicine;
            }

            int amount = 2 + world.Random.Next(1, 5);
            survivor.Inventory.Add(resource, amount);
            survivor.Needs.Change(resource == ResourceKind.Food ? 8 : -1, -1, -1, 1);
            world.Record(survivor.Id, "gather", survivor.Name + " scavenged " + amount + " " + resource + ".");
        }

        private void Craft(WorldState world, SurvivorNpc survivor)
        {
            if (survivor.Inventory.Get(ResourceKind.Tools) == 0 &&
                survivor.Inventory.Has(ResourceKind.Wood, 4) &&
                survivor.Inventory.Has(ResourceKind.Stone, 3))
            {
                survivor.Inventory.Remove(ResourceKind.Wood, 4);
                survivor.Inventory.Remove(ResourceKind.Stone, 3);
                survivor.Inventory.Add(ResourceKind.Tools, 1);
                survivor.Needs.Change(0, 2, 0, 5);
                world.Record(survivor.Id, "craft", survivor.Name + " made a survival tool.");
                return;
            }

            if (survivor.Inventory.Get(ResourceKind.Weapons) == 0 &&
                survivor.Inventory.Has(ResourceKind.Iron, 4) &&
                survivor.Inventory.Has(ResourceKind.Wood, 2))
            {
                survivor.Inventory.Remove(ResourceKind.Iron, 4);
                survivor.Inventory.Remove(ResourceKind.Wood, 2);
                survivor.Inventory.Add(ResourceKind.Weapons, 1);
                survivor.Needs.Change(0, 5, 0, 4);
                world.Record(survivor.Id, "craft", survivor.Name + " assembled a weapon.");
                return;
            }

            if (survivor.Inventory.Get(ResourceKind.Armor) == 0 &&
                survivor.Inventory.Has(ResourceKind.Cloth, 5) &&
                survivor.Inventory.Has(ResourceKind.Iron, 2))
            {
                survivor.Inventory.Remove(ResourceKind.Cloth, 5);
                survivor.Inventory.Remove(ResourceKind.Iron, 2);
                survivor.Inventory.Add(ResourceKind.Armor, 1);
                survivor.Needs.Change(0, 7, 0, 3);
                world.Record(survivor.Id, "craft", survivor.Name + " stitched together armor.");
            }
        }

        private void Build(WorldState world, SurvivorNpc survivor)
        {
            Settlement settlement = survivor.HomeSettlementId.HasValue
                ? world.GetSettlement(survivor.HomeSettlementId.Value)
                : null;

            if (settlement == null)
            {
                if (!survivor.Inventory.Has(ResourceKind.Wood, _settings.SettlementWoodCost) ||
                    !survivor.Inventory.Has(ResourceKind.Stone, _settings.SettlementStoneCost))
                {
                    Gather(world, survivor);
                    return;
                }

                survivor.Inventory.Remove(ResourceKind.Wood, _settings.SettlementWoodCost);
                survivor.Inventory.Remove(ResourceKind.Stone, _settings.SettlementStoneCost);
                settlement = new Settlement(Guid.NewGuid(), NameGenerator.SettlementName(world.Random), SettlementKind.Camp);
                settlement.Improve(5 + survivor.BuildingSkill / 12, 4, survivor.CraftingSkill > 55 ? 1 : 0);
                world.AddSettlement(settlement);
                survivor.HomeSettlementId = settlement.Id;
                if (survivor.FactionId.HasValue)
                {
                    Faction faction = world.GetFaction(survivor.FactionId.Value);
                    if (faction != null && !faction.HomeSettlementId.HasValue)
                    {
                        faction.HomeSettlementId = settlement.Id;
                    }
                }

                world.Record(survivor.Id, "build", survivor.Name + " founded " + settlement.Name + ".");
                return;
            }

            if (survivor.Inventory.Has(ResourceKind.Wood, 6) && survivor.Inventory.Has(ResourceKind.Stone, 4))
            {
                survivor.Inventory.Remove(ResourceKind.Wood, 6);
                survivor.Inventory.Remove(ResourceKind.Stone, 4);
                int defenseGain = 2 + survivor.BuildingSkill / 15;
                int comfortGain = survivor.Traits.Compassion > 55 ? 2 : 1;
                int workshopGain = survivor.CraftingSkill > 70 && world.Random.Next(100) < 25 ? 1 : 0;
                settlement.Improve(defenseGain, comfortGain, workshopGain);
                survivor.Needs.Change(0, 4, 2, 3);
                world.Record(survivor.Id, "build", survivor.Name + " improved " + settlement.Name + ".");
            }
            else
            {
                Gather(world, survivor);
            }
        }

        private void Trade(WorldState world, SurvivorNpc survivor)
        {
            SurvivorNpc partner = FindTradePartner(world, survivor);
            if (partner == null)
            {
                Socialize(world, survivor);
                return;
            }

            ResourceKind needed = survivor.MostNeededResource();
            ResourceKind? offered = survivor.Inventory.FirstSurplus(_settings.TradeSurplusThreshold);
            if (!offered.HasValue || partner.Inventory.Get(needed) <= 1)
            {
                Socialize(world, survivor);
                return;
            }

            if (!survivor.Inventory.Has(offered.Value, 2) || !partner.Inventory.Has(needed, 1))
            {
                return;
            }

            survivor.Inventory.Remove(offered.Value, 2);
            partner.Inventory.Remove(needed, 1);
            survivor.Inventory.Add(needed, 1);
            partner.Inventory.Add(offered.Value, 2);
            survivor.Needs.Change(2, 1, 3, 2);
            partner.Needs.Change(1, 1, 2, 1);

            Relationship relationship = world.GetOrCreateRelationship(survivor.Id, partner.Id);
            relationship.Change(5, -2, 1, 3, -3);
            world.Record(survivor.Id, "trade", survivor.Name + " traded " + offered.Value + " with " + partner.Name + " for " + needed + ".");
        }

        private void Patrol(WorldState world, SurvivorNpc survivor)
        {
            survivor.Needs.Change(-1, 7, 0, 2);
            foreach (Relationship relationship in world.RelationshipsFor(survivor.Id).Take(3))
            {
                SurvivorNpc other = world.OtherSurvivor(relationship, survivor.Id);
                if (other == null)
                {
                    continue;
                }

                if (relationship.Kind == RelationshipKind.Enemy || relationship.Kind == RelationshipKind.Rival)
                {
                    relationship.Change(-1, 2, 0, -1, 2);
                }
                else if (survivor.FactionId.HasValue && survivor.FactionId == other.FactionId)
                {
                    relationship.Change(2, -1, 0, 2, -1);
                }
            }

            world.Record(survivor.Id, "patrol", survivor.Name + " patrolled their territory.");
        }

        private void SeekAlly(WorldState world, SurvivorNpc survivor)
        {
            Relationship best = world.RelationshipsFor(survivor.Id)
                .Where(relationship => relationship.Kind == RelationshipKind.Friend || relationship.Kind == RelationshipKind.LoveInterest)
                .OrderByDescending(relationship => relationship.Trust + relationship.Loyalty)
                .FirstOrDefault();

            if (best == null)
            {
                Socialize(world, survivor);
                return;
            }

            SurvivorNpc other = world.OtherSurvivor(best, survivor.Id);
            if (other == null)
            {
                return;
            }

            best.Change(4, -1, survivor.Traits.Sociability > 60 ? 2 : 0, 4, -2);
            survivor.Needs.Change(0, 2, 8, 3);
            other.Needs.Change(0, 1, 4, 1);
            world.Record(survivor.Id, "relationship", survivor.Name + " sought an alliance with " + other.Name + ".");
        }

        private void Socialize(WorldState world, SurvivorNpc survivor)
        {
            Relationship relationship = world.RelationshipsFor(survivor.Id)
                .OrderBy(_ => world.Random.Next())
                .FirstOrDefault();

            if (relationship == null)
            {
                return;
            }

            SurvivorNpc other = world.OtherSurvivor(relationship, survivor.Id);
            if (other == null)
            {
                return;
            }

            int trust = 1 + survivor.Traits.Sociability / 30;
            int attraction = survivor.Traits.Compassion > 55 && other.Traits.Compassion > 55 ? 2 : 0;
            int grievance = survivor.Traits.Aggression > 75 && other.Traits.Aggression > 75 ? 2 : -1;
            relationship.Change(trust, -1, attraction, 1, grievance);
            survivor.Needs.Change(0, 0, 5, 1);
            world.Record(survivor.Id, "relationship", survivor.Name + " spent time with " + other.Name + " (" + relationship.Kind + ").");
        }

        private void TryFormFactions(WorldState world)
        {
            if (world.Factions.Count() >= _settings.MaxFactions)
            {
                return;
            }

            foreach (SurvivorNpc leader in world.Survivors.Where(survivor => !survivor.FactionId.HasValue).OrderByDescending(survivor => survivor.Traits.Sociability + survivor.Traits.Bravery))
            {
                List<SurvivorNpc> candidates = world.RelationshipsFor(leader.Id)
                    .Where(relationship => relationship.Trust >= _settings.FactionTrustThreshold && relationship.Grievance < 35)
                    .Select(relationship => world.OtherSurvivor(relationship, leader.Id))
                    .Where(other => other != null && !other.FactionId.HasValue)
                    .Take(3)
                    .ToList();

                if (candidates.Count < 2)
                {
                    continue;
                }

                Faction faction = new Faction(Guid.NewGuid(), NameGenerator.FactionName(world.Random), leader.Id);
                leader.FactionId = faction.Id;
                foreach (SurvivorNpc candidate in candidates)
                {
                    candidate.FactionId = faction.Id;
                    faction.AddMember(candidate.Id);
                    Relationship relationship = world.GetOrCreateRelationship(leader.Id, candidate.Id);
                    relationship.Change(6, -2, 0, 8, -3);
                }

                world.AddFaction(faction);
                world.Record(leader.Id, "faction", leader.Name + " formed " + faction.Name + " with " + candidates.Count + " allies.");
                if (world.Factions.Count() >= _settings.MaxFactions)
                {
                    return;
                }
            }
        }

        private bool HasFriendlyProspect(WorldState world, SurvivorNpc survivor)
        {
            return world.RelationshipsFor(survivor.Id).Any(relationship =>
                relationship.Kind == RelationshipKind.Friend ||
                relationship.Kind == RelationshipKind.LoveInterest);
        }

        private bool CanCraft(SurvivorNpc survivor)
        {
            bool canMakeTool = survivor.Inventory.Get(ResourceKind.Tools) == 0 &&
                survivor.Inventory.Get(ResourceKind.Wood) >= 4 &&
                survivor.Inventory.Get(ResourceKind.Stone) >= 3;
            bool canMakeWeapon = survivor.Inventory.Get(ResourceKind.Weapons) == 0 &&
                survivor.Inventory.Get(ResourceKind.Iron) >= 4 &&
                survivor.Inventory.Get(ResourceKind.Wood) >= 2;
            bool canMakeArmor = survivor.Inventory.Get(ResourceKind.Armor) == 0 &&
                survivor.Inventory.Get(ResourceKind.Cloth) >= 5 &&
                survivor.Inventory.Get(ResourceKind.Iron) >= 2;

            return canMakeTool || canMakeWeapon || canMakeArmor;
        }

        private bool CanTrade(WorldState world, SurvivorNpc survivor)
        {
            return survivor.Inventory.FirstSurplus(_settings.TradeSurplusThreshold).HasValue &&
                FindTradePartner(world, survivor) != null;
        }

        private SurvivorNpc FindTradePartner(WorldState world, SurvivorNpc survivor)
        {
            ResourceKind needed = survivor.MostNeededResource();
            return world.RelationshipsFor(survivor.Id)
                .Where(relationship => relationship.Kind != RelationshipKind.Enemy)
                .OrderByDescending(relationship => relationship.Trust - relationship.Grievance)
                .Select(relationship => world.OtherSurvivor(relationship, survivor.Id))
                .FirstOrDefault(other => other != null && other.Inventory.Get(needed) > 1);
        }

        private bool NeedsSettlementWork(WorldState world, SurvivorNpc survivor)
        {
            if (!survivor.HomeSettlementId.HasValue)
            {
                return false;
            }

            Settlement settlement = world.GetSettlement(survivor.HomeSettlementId.Value);
            return settlement != null &&
                (settlement.Defenses < 45 || settlement.Comfort < 35 || settlement.Workshops == 0) &&
                survivor.Inventory.Get(ResourceKind.Wood) >= 6 &&
                survivor.Inventory.Get(ResourceKind.Stone) >= 4;
        }
    }
}
