using System;

namespace GraveAlive.Simulation
{
    public static class NameGenerator
    {
        private static readonly string[] FirstNames =
        {
            "Avery",
            "Blake",
            "Casey",
            "Dakota",
            "Emery",
            "Finley",
            "Harper",
            "Jordan",
            "Kai",
            "Logan",
            "Morgan",
            "Quinn",
            "Riley",
            "Sage",
            "Taylor",
            "Vale"
        };

        private static readonly string[] FactionNouns =
        {
            "Lanterns",
            "Rangers",
            "Scavengers",
            "Wardens",
            "Nomads",
            "Foundry",
            "Hearth",
            "Pioneers"
        };

        private static readonly string[] SettlementNouns =
        {
            "Crossing",
            "Watch",
            "Hollow",
            "Rest",
            "Yard",
            "Haven",
            "Post",
            "Gate"
        };

        public static string SurvivorName(Random random, int index)
        {
            return FirstNames[index % FirstNames.Length] + "-" + random.Next(10, 99);
        }

        public static string FactionName(Random random)
        {
            return "The " + FactionNouns[random.Next(FactionNouns.Length)];
        }

        public static string SettlementName(Random random)
        {
            return SettlementNouns[random.Next(SettlementNouns.Length)] + " Camp";
        }
    }
}
