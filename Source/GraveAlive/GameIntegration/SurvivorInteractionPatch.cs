#if GRAVE_7DTD
using HarmonyLib;
using UnityEngine;

namespace GraveAlive.GameIntegration
{
    [HarmonyPatch(typeof(EntityAlive), "GetActivationCommands")]
    internal static class SurvivorActivationCommandsPatch
    {
        private static void Postfix(EntityAlive __instance, ref EntityActivationCommand[] __result)
        {
            if (ModApi.IsShuttingDown || __instance == null || __instance.IsDead())
            {
                return;
            }

            if (!SurvivorEntitySetup.IsGraveAliveSurvivor(__instance))
            {
                return;
            }

            string survivorName;
            if (!SurvivorEntityRegistry.TryGetName(__instance.entityId, out survivorName))
            {
                survivorName = __instance.EntityName;
            }

            if (string.IsNullOrEmpty(survivorName))
            {
                survivorName = "Survivor";
            }

            __result = new[]
            {
                new EntityActivationCommand("Talk to " + survivorName, "talk", true)
            };
        }
    }

    [HarmonyPatch(typeof(EntityAlive), "OnEntityActivated")]
    internal static class SurvivorEntityActivatedPatch
    {
        private static bool Prefix(EntityAlive __instance, int _indexInBlockActivationCommands, EntityAlive _entityFocusing)
        {
            if (ModApi.IsShuttingDown || __instance == null || !SurvivorEntitySetup.IsGraveAliveSurvivor(__instance))
            {
                return true;
            }

            EntityPlayerLocal localPlayer = _entityFocusing as EntityPlayerLocal;
            if (localPlayer == null)
            {
                return true;
            }

            string survivorName;
            if (!SurvivorEntityRegistry.TryGetName(__instance.entityId, out survivorName))
            {
                survivorName = __instance.EntityName;
            }

            if (string.IsNullOrEmpty(survivorName))
            {
                survivorName = "Survivor";
            }

            string line = SurvivorChatHandler.BuildGreeting(survivorName);
            GameManager.ShowTooltip(localPlayer, line);
            GameLog.Out("[GraveAlive] " + survivorName + ": " + line);
            return false;
        }
    }
}
#endif
