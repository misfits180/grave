#if GRAVE_7DTD
using System.Reflection;
using GraveAlive.Simulation;
using HarmonyLib;
using UnityEngine;

namespace GraveAlive.GameIntegration
{
    public sealed class ModApi : IModApi
    {
        internal static GraveAliveRuntime Runtime { get; private set; }
        internal static VisibleSurvivorSpawner Spawner { get; private set; }

        public void InitMod(Mod modInstance)
        {
            SimulationSettings settings = new SimulationSettings();
            Runtime = new GraveAliveRuntime(settings, System.Environment.TickCount);
            Spawner = new VisibleSurvivorSpawner(settings);

            Harmony harmony = new Harmony("com.cursor.gravealive");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Out("[GraveAlive] Living-world simulation initialized.");
        }
    }

    [HarmonyPatch(typeof(GameManager), "Update")]
    internal static class GameManagerUpdatePatch
    {
        private static float _lastReportTime;

        private static void Postfix()
        {
            if (ModApi.Runtime == null)
            {
                return;
            }

            ModApi.Runtime.Update(Time.deltaTime);
            if (ModApi.Spawner != null)
            {
                ModApi.Spawner.Process(ModApi.Runtime);
            }

            if (Time.realtimeSinceStartup - _lastReportTime > 60f)
            {
                _lastReportTime = Time.realtimeSinceStartup;
                Log.Out("[GraveAlive] " + ModApi.Runtime.Summary());
            }
        }
    }
}
#endif
