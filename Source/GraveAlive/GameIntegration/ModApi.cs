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

        public void InitMod(Mod modInstance)
        {
            Runtime = new GraveAliveRuntime(new SimulationSettings(), System.Environment.TickCount);

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

            if (Time.realtimeSinceStartup - _lastReportTime > 60f)
            {
                _lastReportTime = Time.realtimeSinceStartup;
                Log.Out("[GraveAlive] " + ModApi.Runtime.Summary());
            }

            // Game entity spawning, POI claiming, and block placement should be wired here
            // after verifying exact 7D2D 1.x method signatures against Assembly-CSharp.dll.
        }
    }
}
#endif
