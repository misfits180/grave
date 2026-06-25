using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace GraveAlive.Simulation
{
    public sealed class PersistedWorld
    {
        public WorldState World { get; private set; }
        public SurvivorSpawnCoordinator SpawnCoordinator { get; private set; }

        public PersistedWorld(WorldState world, SurvivorSpawnCoordinator spawnCoordinator)
        {
            World = world;
            SpawnCoordinator = spawnCoordinator;
        }
    }

    public static class WorldPersistence
    {
        private const string Version = "1";

        public static void Save(string path, WorldState world, SurvivorSpawnCoordinator spawnCoordinator)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("A save path is required.", "path");
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            XDocument document = new XDocument(
                new XElement("graveAlive",
                    new XAttribute("version", Version),
                    new XAttribute("tick", world.Tick.ToString(CultureInfo.InvariantCulture)),
                    SaveSurvivors(world),
                    SaveRelationships(world),
                    SaveFactions(world),
                    SaveSettlements(world),
                    SaveSpawnStates(spawnCoordinator)));

            document.Save(path);
        }

        public static PersistedWorld Load(string path, SimulationSettings settings, int seed)
        {
            XDocument document = XDocument.Load(path);
            XElement root = document.Root;
            if (root == null || root.Name != "graveAlive")
            {
                throw new InvalidDataException("The save file is not a Grave Alive save.");
            }

            WorldState world = new WorldState(new Random(seed));
            world.SetTick(Long(root, "tick", 0));
            SurvivorSpawnCoordinator spawnCoordinator = new SurvivorSpawnCoordinator(settings);

            LoadSurvivors(root.Element("survivors"), world, spawnCoordinator);
            LoadRelationships(root.Element("relationships"), world);
            LoadFactions(root.Element("factions"), world);
            LoadSettlements(root.Element("settlements"), world);
            LoadSpawnStates(root.Element("spawnStates"), spawnCoordinator);

            world.Record(null, "system", "GraveAlive loaded save: " + world.Snapshot());
            return new PersistedWorld(world, spawnCoordinator);
        }

        private static XElement SaveSurvivors(WorldState world)
        {
            return new XElement("survivors",
                world.Survivors.Select(survivor =>
                    new XElement("survivor",
                        new XAttribute("id", survivor.Id),
                        new XAttribute("name", survivor.Name),
                        new XAttribute("factionId", IdOrEmpty(survivor.FactionId)),
                        new XAttribute("homeSettlementId", IdOrEmpty(survivor.HomeSettlementId)),
                        new XAttribute("x", F(survivor.Position.X)),
                        new XAttribute("y", F(survivor.Position.Y)),
                        new XAttribute("z", F(survivor.Position.Z)),
                        new XElement("traits",
                            new XAttribute("bravery", survivor.Traits.Bravery),
                            new XAttribute("sociability", survivor.Traits.Sociability),
                            new XAttribute("compassion", survivor.Traits.Compassion),
                            new XAttribute("aggression", survivor.Traits.Aggression),
                            new XAttribute("crafting", survivor.Traits.Crafting),
                            new XAttribute("building", survivor.Traits.Building),
                            new XAttribute("trading", survivor.Traits.Trading)),
                        new XElement("needs",
                            new XAttribute("hunger", survivor.Needs.Hunger),
                            new XAttribute("safety", survivor.Needs.Safety),
                            new XAttribute("belonging", survivor.Needs.Belonging),
                            new XAttribute("ambition", survivor.Needs.Ambition)),
                        SaveInventory(survivor.Inventory))));
        }

        private static XElement SaveRelationships(WorldState world)
        {
            return new XElement("relationships",
                world.Relationships.Select(relationship =>
                    new XElement("relationship",
                        new XAttribute("firstId", relationship.FirstId),
                        new XAttribute("secondId", relationship.SecondId),
                        new XAttribute("trust", relationship.Trust),
                        new XAttribute("fear", relationship.Fear),
                        new XAttribute("attraction", relationship.Attraction),
                        new XAttribute("loyalty", relationship.Loyalty),
                        new XAttribute("grievance", relationship.Grievance))));
        }

        private static XElement SaveFactions(WorldState world)
        {
            return new XElement("factions",
                world.Factions.Select(faction =>
                    new XElement("faction",
                        new XAttribute("id", faction.Id),
                        new XAttribute("name", faction.Name),
                        new XAttribute("leaderId", faction.LeaderId),
                        new XAttribute("homeSettlementId", IdOrEmpty(faction.HomeSettlementId)),
                        faction.Members.Select(memberId => new XElement("member", new XAttribute("id", memberId))))));
        }

        private static XElement SaveSettlements(WorldState world)
        {
            return new XElement("settlements",
                world.Settlements.Select(settlement =>
                    new XElement("settlement",
                        new XAttribute("id", settlement.Id),
                        new XAttribute("name", settlement.Name),
                        new XAttribute("kind", settlement.Kind),
                        new XAttribute("defenses", settlement.Defenses),
                        new XAttribute("comfort", settlement.Comfort),
                        new XAttribute("workshops", settlement.Workshops),
                        SaveInventory(settlement.Stockpile))));
        }

        private static XElement SaveSpawnStates(SurvivorSpawnCoordinator spawnCoordinator)
        {
            return new XElement("spawnStates",
                spawnCoordinator.States.Select(state =>
                    new XElement("spawnState",
                        new XAttribute("survivorId", state.SurvivorId),
                        new XAttribute("entityClassName", state.EntityClassName),
                        new XAttribute("entityId", state.EntityId.HasValue ? state.EntityId.Value.ToString(CultureInfo.InvariantCulture) : ""),
                        new XAttribute("visibilityState", state.VisibilityState),
                        new XAttribute("lastRequestTick", state.LastRequestTick),
                        new XAttribute("lastVisibleTick", state.LastVisibleTick),
                        new XAttribute("lastFailureTick", state.LastFailureTick),
                        new XAttribute("failedAttempts", state.FailedAttempts))));
        }

        private static XElement SaveInventory(Inventory inventory)
        {
            return new XElement("inventory",
                inventory.Items.Select(pair =>
                    new XElement("item",
                        new XAttribute("kind", pair.Key),
                        new XAttribute("amount", pair.Value))));
        }

        private static void LoadSurvivors(XElement survivorsElement, WorldState world, SurvivorSpawnCoordinator spawnCoordinator)
        {
            if (survivorsElement == null)
            {
                return;
            }

            foreach (XElement element in survivorsElement.Elements("survivor"))
            {
                XElement traits = element.Element("traits");
                XElement needs = element.Element("needs");
                SurvivorNpc survivor = new SurvivorNpc(
                    GuidValue(element, "id"),
                    String(element, "name", "Survivor"),
                    new TraitProfile(
                        Int(traits, "bravery", 50),
                        Int(traits, "sociability", 50),
                        Int(traits, "compassion", 50),
                        Int(traits, "aggression", 25),
                        Int(traits, "crafting", 50),
                        Int(traits, "building", 50),
                        Int(traits, "trading", 50)));

                survivor.FactionId = NullableGuid(element, "factionId");
                survivor.HomeSettlementId = NullableGuid(element, "homeSettlementId");
                survivor.Position = new WorldPosition(
                    Float(element, "x", 0),
                    Float(element, "y", 0),
                    Float(element, "z", 0));
                survivor.RestoreNeeds(
                    Int(needs, "hunger", 70),
                    Int(needs, "safety", 60),
                    Int(needs, "belonging", 55),
                    Int(needs, "ambition", 55));
                LoadInventory(element.Element("inventory"), survivor.Inventory);

                world.AddSurvivor(survivor);
                spawnCoordinator.GetOrCreateState(survivor);
            }
        }

        private static void LoadRelationships(XElement relationshipsElement, WorldState world)
        {
            if (relationshipsElement == null)
            {
                return;
            }

            foreach (XElement element in relationshipsElement.Elements("relationship"))
            {
                world.AddRelationship(new Relationship(
                    GuidValue(element, "firstId"),
                    GuidValue(element, "secondId"),
                    Int(element, "trust", 30),
                    Int(element, "fear", 0),
                    Int(element, "attraction", 0),
                    Int(element, "loyalty", 0),
                    Int(element, "grievance", 0)));
            }
        }

        private static void LoadFactions(XElement factionsElement, WorldState world)
        {
            if (factionsElement == null)
            {
                return;
            }

            foreach (XElement element in factionsElement.Elements("faction"))
            {
                Faction faction = new Faction(
                    GuidValue(element, "id"),
                    String(element, "name", "Faction"),
                    GuidValue(element, "leaderId"));
                faction.HomeSettlementId = NullableGuid(element, "homeSettlementId");
                faction.ClearMembers();
                foreach (XElement member in element.Elements("member"))
                {
                    faction.AddMember(GuidValue(member, "id"));
                }

                world.AddFaction(faction);
            }
        }

        private static void LoadSettlements(XElement settlementsElement, WorldState world)
        {
            if (settlementsElement == null)
            {
                return;
            }

            foreach (XElement element in settlementsElement.Elements("settlement"))
            {
                Settlement settlement = new Settlement(
                    GuidValue(element, "id"),
                    String(element, "name", "Camp"),
                    EnumValue(element, "kind", SettlementKind.Camp));
                settlement.RestoreState(
                    EnumValue(element, "kind", SettlementKind.Camp),
                    Int(element, "defenses", 10),
                    Int(element, "comfort", 10),
                    Int(element, "workshops", 0));
                LoadInventory(element.Element("inventory"), settlement.Stockpile);
                world.AddSettlement(settlement);
            }
        }

        private static void LoadSpawnStates(XElement spawnStatesElement, SurvivorSpawnCoordinator spawnCoordinator)
        {
            if (spawnStatesElement == null)
            {
                return;
            }

            foreach (XElement element in spawnStatesElement.Elements("spawnState"))
            {
                // Old in-game entity ids are not safe after a reload; survivors re-enter as background simulated actors.
                spawnCoordinator.RestoreState(new SurvivorSpawnState(
                    GuidValue(element, "survivorId"),
                    String(element, "entityClassName", "graveAliveSurvivorRanged"),
                    null,
                    SurvivorVisibilityState.Simulated,
                    Long(element, "lastRequestTick", -1),
                    Long(element, "lastVisibleTick", -1),
                    Long(element, "lastFailureTick", -1),
                    Int(element, "failedAttempts", 0)));
            }
        }

        private static void LoadInventory(XElement inventoryElement, Inventory inventory)
        {
            inventory.Clear();
            if (inventoryElement == null)
            {
                return;
            }

            foreach (XElement element in inventoryElement.Elements("item"))
            {
                inventory.Add(EnumValue(element, "kind", ResourceKind.Food), Int(element, "amount", 0));
            }
        }

        private static string IdOrEmpty(Guid? id)
        {
            return id.HasValue ? id.Value.ToString() : "";
        }

        private static string F(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string String(XElement element, string attributeName, string defaultValue)
        {
            if (element == null)
            {
                return defaultValue;
            }

            XAttribute attribute = element.Attribute(attributeName);
            return attribute == null ? defaultValue : attribute.Value;
        }

        private static int Int(XElement element, string attributeName, int defaultValue)
        {
            int value;
            return int.TryParse(String(element, attributeName, ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : defaultValue;
        }

        private static int? NullableInt(XElement element, string attributeName)
        {
            int value;
            return int.TryParse(String(element, attributeName, ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? (int?)value
                : null;
        }

        private static long Long(XElement element, string attributeName, long defaultValue)
        {
            long value;
            return long.TryParse(String(element, attributeName, ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : defaultValue;
        }

        private static float Float(XElement element, string attributeName, float defaultValue)
        {
            float value;
            return float.TryParse(String(element, attributeName, ""), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                ? value
                : defaultValue;
        }

        private static System.Guid GuidValue(XElement element, string attributeName)
        {
            System.Guid value;
            return System.Guid.TryParse(String(element, attributeName, ""), out value) ? value : System.Guid.Empty;
        }

        private static Guid? NullableGuid(XElement element, string attributeName)
        {
            System.Guid value;
            return System.Guid.TryParse(String(element, attributeName, ""), out value) ? (System.Guid?)value : null;
        }

        private static T EnumValue<T>(XElement element, string attributeName, T defaultValue) where T : struct
        {
            T value;
            return Enum.TryParse(String(element, attributeName, ""), out value) ? value : defaultValue;
        }
    }
}
