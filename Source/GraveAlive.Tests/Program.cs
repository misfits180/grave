using System;
using System.Linq;
using GraveAlive;
using GraveAlive.Simulation;

namespace GraveAlive.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                SeedsLivingWorld();
                EvolvesAutonomousSociety();
                PlansVisibleSurvivorSpawns();
                AppliesSpawnRetryCooldown();
                Console.WriteLine("All GraveAlive simulation checks passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static void SeedsLivingWorld()
        {
            GraveAliveRuntime runtime = new GraveAliveRuntime(new SimulationSettings
            {
                StartingSurvivorCount = 10,
                MaxFactions = 4
            }, 180);

            Assert(runtime.World.Survivors.Count() == 10, "Expected 10 seeded survivors.");
            Assert(runtime.World.Relationships.Any(), "Expected seeded relationship graph.");
            Assert(runtime.World.Relationships.Any(relationship =>
                relationship.Kind == RelationshipKind.Friend ||
                relationship.Kind == RelationshipKind.LoveInterest),
                "Expected at least one strong starting relationship.");
        }

        private static void EvolvesAutonomousSociety()
        {
            GraveAliveRuntime runtime = new GraveAliveRuntime(new SimulationSettings
            {
                StartingSurvivorCount = 12,
                MaxFactions = 5,
                FactionTrustThreshold = 60
            }, 7331);

            runtime.AdvanceTicks(80);

            Assert(runtime.World.Factions.Any(), "Expected autonomous faction formation.");
            Assert(runtime.World.Settlements.Any(), "Expected survivors to build at least one camp/base.");
            Assert(runtime.World.Events.Any(evt => evt.Category == "gather"), "Expected gathering behavior.");
            Assert(runtime.World.Events.Any(evt => evt.Category == "relationship"), "Expected relationship behavior.");
            Assert(runtime.World.Events.Any(evt => evt.Category == "build"), "Expected building behavior.");
        }

        private static void PlansVisibleSurvivorSpawns()
        {
            SimulationSettings settings = new SimulationSettings
            {
                StartingSurvivorCount = 12,
                MaxVisibleSurvivors = 4,
                MaxSpawnRequestsPerTick = 4
            };
            GraveAliveRuntime runtime = new GraveAliveRuntime(settings, 42);

            SurvivorSpawnRequest[] requests = runtime
                .PlanVisibleSurvivorSpawns(new[] { new WorldPosition(1000, 0, 1000) })
                .ToArray();

            Assert(requests.Length > 0, "Expected visible survivor spawn requests near the player.");
            Assert(requests.All(request => request.RequestType == SpawnRequestType.Spawn), "Expected initial requests to be spawns.");
            Assert(requests.Length <= settings.MaxVisibleSurvivors, "Expected spawn requests to respect visible survivor limit.");
        }

        private static void AppliesSpawnRetryCooldown()
        {
            SimulationSettings settings = new SimulationSettings
            {
                StartingSurvivorCount = 4,
                MaxVisibleSurvivors = 2,
                MaxSpawnRequestsPerTick = 2,
                SpawnRetryDelayTicks = 5
            };
            GraveAliveRuntime runtime = new GraveAliveRuntime(settings, 99);
            WorldPosition[] playerPositions = { new WorldPosition(500, 0, 500) };

            SurvivorSpawnRequest first = runtime.PlanVisibleSurvivorSpawns(playerPositions).First();
            runtime.MarkSpawnFailed(first.SurvivorId, "test failure");

            SurvivorSpawnRequest[] immediateRetryRequests = runtime.PlanVisibleSurvivorSpawns(playerPositions).ToArray();
            Assert(
                immediateRetryRequests.All(request => request.SurvivorId != first.SurvivorId),
                "Expected failed survivor spawn to wait before retrying.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
