using System;
using System.Collections.Generic;

namespace WanderingSouls
{
    public enum Trait
    {
        Brave,
        Cautious,
        Aggressive,
        Curious,
        Loyal
    }

    public class PersonalityTraits
    {
        public List<Trait> Traits = new List<Trait>();

        public static PersonalityTraits GenerateRandom()
        {
            var result = new PersonalityTraits();
            Trait[] allTraits = (Trait[])Enum.GetValues(typeof(Trait));

            foreach (var t in allTraits)
            {
                if (UnityEngine.Random.value < 0.5f)
                    result.Traits.Add(t);
            }

            if (result.Traits.Count == 0)
            {
                result.Traits.Add(allTraits[UnityEngine.Random.Range(0, allTraits.Length)]);
            }

            return result;
        }

        public bool HasTrait(Trait trait)
        {
            return Traits.Contains(trait);
        }

        public float ModifyTrustGain(float amount)
        {
            if (HasTrait(Trait.Cautious))
                amount *= 0.8f;

            if (HasTrait(Trait.Loyal))
                amount *= 1.2f;

            return amount;
        }

        public float ModifyAffectionGain(float amount)
        {
            if (HasTrait(Trait.Aggressive))
                amount *= 0.9f;

            if (HasTrait(Trait.Curious))
                amount *= 1.1f;

            if (HasTrait(Trait.Loyal))
                amount *= 1.15f;

            return amount;
        }

        public float ModifyLoyaltyGain(float amount)
        {
            if (HasTrait(Trait.Loyal))
                amount *= 1.3f;

            if (HasTrait(Trait.Cautious))
                amount *= 0.9f;

            return amount;
        }

        public float ModifyFearGain(float amount)
        {
            if (HasTrait(Trait.Brave))
                amount *= 0.6f;

            if (HasTrait(Trait.Cautious))
                amount *= 1.2f;

            return amount;
        }
    }
}
