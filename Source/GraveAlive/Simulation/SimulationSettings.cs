namespace GraveAlive.Simulation
{
    public sealed class SimulationSettings
    {
        public int StartingSurvivorCount { get; set; }
        public int MaxFactions { get; set; }
        public int FactionTrustThreshold { get; set; }
        public int TickSeconds { get; set; }
        public int TradeSurplusThreshold { get; set; }
        public int SettlementWoodCost { get; set; }
        public int SettlementStoneCost { get; set; }
        public int MaxVisibleSurvivors { get; set; }
        public int MaxSpawnRequestsPerTick { get; set; }
        public float SpawnRadius { get; set; }
        public float DespawnRadius { get; set; }
        public string SurvivorEntityClassName { get; set; }
        public string SurvivorEntityGroupName { get; set; }
        public int SpawnRetryDelayTicks { get; set; }
        public int AutosaveTickInterval { get; set; }

        public SimulationSettings()
        {
            StartingSurvivorCount = 12;
            MaxFactions = 5;
            FactionTrustThreshold = 62;
            TickSeconds = 10;
            TradeSurplusThreshold = 12;
            SettlementWoodCost = 20;
            SettlementStoneCost = 10;
            MaxVisibleSurvivors = 1;
            MaxSpawnRequestsPerTick = 1;
            SpawnRadius = 75f;
            DespawnRadius = 125f;
            SurvivorEntityClassName = "graveAliveVisibleSurvivor";
            SurvivorEntityGroupName = "GraveAliveSurvivors";
            SpawnRetryDelayTicks = 6;
            AutosaveTickInterval = 0;
        }
    }
}
