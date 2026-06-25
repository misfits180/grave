using System;

namespace WanderingSouls
{
    public static class RomanceDialogue
    {
        private static readonly Random rng = new Random();

        public static readonly string[] IntroductionLines =
        {
            "Easy… I’m not infected. Name’s {0}.",
            "Hey—don’t shoot. I’m {0}.",
            "Relax. I’m friendly. Call me {0}.",
            "Whoa, easy there. I’m {0}. Just trying to survive."
        };

        public static readonly string[] TrustLines =
        {
            "I’m glad you’re sticking around.",
            "You’ve got my back… I appreciate that.",
            "Feels safer with you nearby.",
            "You’re one of the good ones."
        };

        public static readonly string[] ConfessionLines =
        {
            "I… I care about you. More than I should.",
            "This world is hell, but you… you make it bearable.",
            "I didn’t think I’d feel this way again.",
            "I trust you. Completely. Maybe too much."
        };

        public static readonly string[] RomanticIdleLines =
        {
            "Stay close, okay?",
            "You’re the only one I trust completely.",
            "I’m not going anywhere. Not without you.",
            "You make this world feel less empty."
        };

        public static readonly string[] ProtectiveLines =
        {
            "Hey! You okay?",
            "Don’t scare me like that.",
            "Stay behind me—I’ve got this.",
            "I’m not losing you. Not today."
        };

        public static string GetIntroductionLine(string name)
        {
            return string.Format(GetRandom(IntroductionLines), name);
        }

        public static string GetTrustLine(string name)
        {
            return GetRandom(TrustLines);
        }

        public static string GetRomanceConfession(string name)
        {
            return GetRandom(ConfessionLines);
        }

        public static string GetRomanticIdleLine(string name)
        {
            return GetRandom(RomanticIdleLines);
        }

        public static string GetProtectiveLine(string name)
        {
            return GetRandom(ProtectiveLines);
        }

        public static string GetRandom(string[] list)
        {
            return list[rng.Next(list.Length)];
        }
    }
}
