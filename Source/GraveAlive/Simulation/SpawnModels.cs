using System;

namespace GraveAlive.Simulation
{
    public enum SurvivorVisibilityState
    {
        Simulated,
        SpawnRequested,
        Visible,
        DespawnRequested
    }

    public enum SpawnRequestType
    {
        Spawn,
        Despawn
    }

    public sealed class SurvivorSpawnState
    {
        public Guid SurvivorId { get; private set; }
        public string EntityClassName { get; private set; }
        public int? EntityId { get; private set; }
        public SurvivorVisibilityState VisibilityState { get; private set; }
        public long LastRequestTick { get; private set; }
        public long LastVisibleTick { get; private set; }
        public long LastFailureTick { get; private set; }
        public int FailedAttempts { get; private set; }

        public SurvivorSpawnState(Guid survivorId, string entityClassName)
        {
            SurvivorId = survivorId;
            EntityClassName = entityClassName;
            VisibilityState = SurvivorVisibilityState.Simulated;
            LastRequestTick = -1;
            LastVisibleTick = -1;
            LastFailureTick = -1;
        }

        public SurvivorSpawnState(
            Guid survivorId,
            string entityClassName,
            int? entityId,
            SurvivorVisibilityState visibilityState,
            long lastRequestTick,
            long lastVisibleTick,
            long lastFailureTick,
            int failedAttempts)
        {
            SurvivorId = survivorId;
            EntityClassName = entityClassName;
            EntityId = entityId;
            VisibilityState = visibilityState;
            LastRequestTick = lastRequestTick;
            LastVisibleTick = lastVisibleTick;
            LastFailureTick = lastFailureTick;
            FailedAttempts = failedAttempts;
        }

        public bool IsVisibleOrPending
        {
            get
            {
                return VisibilityState == SurvivorVisibilityState.Visible ||
                    VisibilityState == SurvivorVisibilityState.SpawnRequested;
            }
        }

        public void MarkSpawnRequested(long tick)
        {
            VisibilityState = SurvivorVisibilityState.SpawnRequested;
            LastRequestTick = tick;
        }

        public void MarkVisible(int entityId, long tick)
        {
            EntityId = entityId;
            VisibilityState = SurvivorVisibilityState.Visible;
            LastVisibleTick = tick;
            FailedAttempts = 0;
            LastFailureTick = -1;
        }

        public void MarkDespawnRequested(long tick)
        {
            VisibilityState = SurvivorVisibilityState.DespawnRequested;
            LastRequestTick = tick;
        }

        public void MarkSimulated()
        {
            EntityId = null;
            VisibilityState = SurvivorVisibilityState.Simulated;
        }

        public void MarkFailed(long tick)
        {
            MarkSimulated();
            FailedAttempts++;
            LastFailureTick = tick;
        }

        public bool IsCoolingDown(long currentTick, int retryDelayTicks)
        {
            return LastFailureTick >= 0 && currentTick - LastFailureTick < retryDelayTicks;
        }
    }

    public sealed class SurvivorSpawnRequest
    {
        public SpawnRequestType RequestType { get; private set; }
        public Guid SurvivorId { get; private set; }
        public string SurvivorName { get; private set; }
        public string EntityClassName { get; private set; }
        public int? EntityId { get; private set; }
        public WorldPosition Position { get; private set; }
        public string Reason { get; private set; }

        public SurvivorSpawnRequest(
            SpawnRequestType requestType,
            Guid survivorId,
            string survivorName,
            string entityClassName,
            int? entityId,
            WorldPosition position,
            string reason)
        {
            RequestType = requestType;
            SurvivorId = survivorId;
            SurvivorName = survivorName;
            EntityClassName = entityClassName;
            EntityId = entityId;
            Position = position;
            Reason = reason;
        }
    }
}
