using System;

namespace WanderingSouls
{
    public static class NameGenerator
    {
        private static readonly Random rng = new Random();

        private static readonly string[] MaleFirstNames =
        {
            "Rhett", "Cole", "Jaxon", "Elias", "Mason",
            "Wyatt", "Silas", "Caleb", "Logan", "Hunter",
            "Dawson", "Rowan", "Asher", "Griffin", "Tanner"
        };

        private static readonly string[] FemaleFirstNames =
        {
            "Maya", "Riley", "Harper", "Ava", "Lena",
            "Zara", "Quinn", "Nova", "Ember", "Sage",
            "Willow", "Aria", "Skye", "Talia", "Jade"
        };

        private static readonly string[] LastNames =
        {
            "Carter", "Alvarez", "Reyes", "Walker", "Brooks",
            "Hayes", "Morgan", "Bennett", "Sullivan", "Hale",
            "Grayson", "Mercer", "Dalton", "Fletcher", "Hunt"
        };

        private static readonly string[] GenericNicknames =
        {
            "Ghost", "Stitch", "Ox", "Whisper", "Rook",
            "Viper", "Ash", "Bolt", "Shade", "Hawk"
        };

        public static string GenerateName(string gender, PersonalityTraits traits)
        {
            string first = gender == "female"
                ? GetRandom(FemaleFirstNames)
                : GetRandom(MaleFirstNames);

            string last = GetRandom(LastNames);
            string nickname = GetRandom(GenericNicknames);

            return $"{first} \"{nickname}\" {last}";
        }

        private static string GetRandom(string[] list)
        {
            return list[rng.Next(list.Length)];
        }
    }
}
