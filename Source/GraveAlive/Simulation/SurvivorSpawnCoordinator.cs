using System;
using System.Collections.Generic;
using System.Linq;

namespace GraveAlive.Simulation
{
    public sealed class SurvivorSpawnCoordinator
    {
        private readonly SimulationSettings _settings;
        private readonly Dictionary<Guid, SurvivorSpawnState> _states = new Dictionary<Guid, SurvivorSpawnState>();

        public SurvivorSpawnCoordinator(SimulationSettings settings)
        {
            _settings = settings;
        }

        public IEnumerable<SurvivorSpawnState> States
        {
            get { return _states.Values; }
        }

        public SurvivorSpawnState GetOrCreateState(SurvivorNpc survivor)
        {
            SurvivorSpawnState state;
            if (_states.TryGetValue(survivor.Id, out state))
            {
                return state;
            }

            state = new SurvivorSpawnState(survivor.Id, _settings.SurvivorEntityClassName);
            _states[survivor.Id] = state;
            return state;
        }

        public void RestoreState(SurvivorSpawnState state)
        {
            _states[state.SurvivorId] = state;
        }

        public IReadOnlyList<SurvivorSpawnRequest> Plan(WorldState world, IEnumerable<WorldPosition> playerPositions)
        {
            List<WorldPosition> players = playerPositions == null
                ? new List<WorldPosition>()
                : playerPositions.ToList();

            if (players.Count == 0)
            {
                return new List<SurvivorSpawnRequest>();
            }

            float spawnRadiusSquared = _settings.SpawnRadius * _settings.SpawnRadius;
            float despawnRadiusSquared = _settings.DespawnRadius * _settings.DespawnRadius;
            List<SurvivorSpawnRequest> requests = new List<SurvivorSpawnRequest>();

            StageNearbyWanderers(world, players, spawnRadiusSquared);

            foreach (SurvivorNpc survivor in world.Survivors)
            {
                SurvivorSpawnState state = GetOrCreateState(survivor);
                float nearestDistanceSquared = NearestDistanceSquared(survivor.Position, players);

                if (state.VisibilityState == SurvivorVisibilityState.Visible &&
                    nearestDistanceSquared > despawnRadiusSquared)
                {
                    state.MarkDespawnRequested(world.Tick);
                    requests.Add(new SurvivorSpawnRequest(
                        SpawnRequestType.Despawn,
                        survivor.Id,
                        survivor.Name,
                        state.EntityClassName,
                        state.EntityId,
                        survivor.Position,
                        "too far from all players"));
                }
            }

            int visibleOrPendingCount = _states.Values.Count(state => state.IsVisibleOrPending);
            int remainingSlots = Math.Max(0, _settings.MaxVisibleSurvivors - visibleOrPendingCount);
            if (remainingSlots <= 0)
            {
                return LimitRequests(requests);
            }

            IEnumerable<SurvivorNpc> spawnCandidates = world.Survivors
                .Where(survivor =>
                {
                    SurvivorSpawnState state = GetOrCreateState(survivor);
                    return state.VisibilityState == SurvivorVisibilityState.Simulated &&
                        !state.IsCoolingDown(world.Tick, _settings.SpawnRetryDelayTicks) &&
                        NearestDistanceSquared(survivor.Position, players) <= spawnRadiusSquared;
                })
                .OrderBy(survivor => NearestDistanceSquared(survivor.Position, players))
                .ThenByDescending(survivor => survivor.FactionId.HasValue ? 1 : 0)
                .Take(remainingSlots);

            foreach (SurvivorNpc survivor in spawnCandidates)
            {
                SurvivorSpawnState state = GetOrCreateState(survivor);
                state.MarkSpawnRequested(world.Tick);
                requests.Add(new SurvivorSpawnRequest(
                    SpawnRequestType.Spawn,
                    survivor.Id,
                    survivor.Name,
                    state.EntityClassName,
                    state.EntityId,
                    survivor.Position,
                    "near player"));
            }

            return LimitRequests(requests);
        }

        private void StageNearbyWanderers(WorldState world, List<WorldPosition> players, float spawnRadiusSquared)
        {
            bool hasNearbyCandidate = world.Survivors.Any(survivor =>
            {
                SurvivorSpawnState state = GetOrCreateState(survivor);
                return state.VisibilityState == SurvivorVisibilityState.Simulated &&
                    NearestDistanceSquared(survivor.Position, players) <= spawnRadiusSquared;
            });

            if (hasNearbyCandidate)
            {
                return;
            }

            WorldPosition anchor = players[0];
            int staged = 0;
            foreach (SurvivorNpc survivor in world.Survivors
                .Where(survivor => !GetOrCreateState(survivor).IsVisibleOrPending)
                .Take(_settings.MaxVisibleSurvivors))
            {
                float angle = (float)((Math.PI * 2.0 * staged) / Math.Max(1, _settings.MaxVisibleSurvivors));
                float distance = 28f + staged * 6f;
                survivor.Position = anchor.Offset(
                    (float)Math.Cos(angle) * distance,
                    0,
                    (float)Math.Sin(angle) * distance);
                staged++;
            }
        }

        public void MarkSpawnSucceeded(Guid survivorId, int entityId, long tick)
        {
            SurvivorSpawnState state;
            if (_states.TryGetValue(survivorId, out state))
            {
                state.MarkVisible(entityId, tick);
            }
        }

        public void MarkSpawnFailed(Guid survivorId, long tick)
        {
            SurvivorSpawnState state;
            if (_states.TryGetValue(survivorId, out state))
            {
                state.MarkFailed(tick);
            }
        }

        public void MarkDespawnSucceeded(Guid survivorId)
        {
            SurvivorSpawnState state;
            if (_states.TryGetValue(survivorId, out state))
            {
                state.MarkSimulated();
            }
        }

        public string Summary(long currentTick)
        {
            int visible = _states.Values.Count(state => state.VisibilityState == SurvivorVisibilityState.Visible);
            int pending = _states.Values.Count(state => state.VisibilityState == SurvivorVisibilityState.SpawnRequested);
            int despawnPending = _states.Values.Count(state => state.VisibilityState == SurvivorVisibilityState.DespawnRequested);
            int coolingDown = _states.Values.Count(state => state.IsCoolingDown(currentTick, _settings.SpawnRetryDelayTicks));
            int simulated = _states.Values.Count(state => state.VisibilityState == SurvivorVisibilityState.Simulated);

            return string.Format(
                "spawn visible={0}, spawnPending={1}, despawnPending={2}, coolingDown={3}, simulated={4}",
                visible,
                pending,
                despawnPending,
                coolingDown,
                simulated);
        }

        private IReadOnlyList<SurvivorSpawnRequest> LimitRequests(List<SurvivorSpawnRequest> requests)
        {
            return requests
                .OrderBy(request => request.RequestType == SpawnRequestType.Despawn ? 0 : 1)
                .Take(_settings.MaxSpawnRequestsPerTick)
                .ToList();
        }

        private static float NearestDistanceSquared(WorldPosition position, IEnumerable<WorldPosition> players)
        {
            float? nearest = null;
            foreach (WorldPosition player in players)
            {
                float distance = position.DistanceSquaredTo(player);
                if (!nearest.HasValue || distance < nearest.Value)
                {
                    nearest = distance;
                }
            }

            return nearest.HasValue ? nearest.Value : float.MaxValue;
        }
    }
}
