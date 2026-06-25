using HarmonyLib;

namespace WanderingSouls
{
    [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.CopyPropertiesFromEntityClass))]
    public class SurvivorBrainPatcher
    {
        static void Postfix(EntityAlive __instance)
        {
            if (__instance is EntityPlayer) return;
            if (__instance.Buffs != null && __instance.Buffs.HasBuff("buffIsSurvivor"))
            {
                if (__instance.gameObject.GetComponent<SurvivorBrainComponent>() == null)
                    __instance.gameObject.AddComponent<SurvivorBrainComponent>();
            }
        }
    }
}
