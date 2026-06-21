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

        public SimulationSettings()
        {
            StartingSurvivorCount = 12;
            MaxFactions = 5;
            FactionTrustThreshold = 62;
            TickSeconds = 10;
            TradeSurplusThreshold = 12;
            SettlementWoodCost = 20;
            SettlementStoneCost = 10;
        }
    }
}
