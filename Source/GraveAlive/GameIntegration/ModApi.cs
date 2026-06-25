#if GRAVE_7DTD
using System;
using System.IO;
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
        internal static string SavePath { get; private set; }
        internal static bool IsShuttingDown { get; private set; }

        private static Harmony _harmony;

        public void InitMod(Mod modInstance)
        {
            SimulationSettings settings = new SimulationSettings();
            SavePath = Path.Combine(ResolveWritableModDirectory(modInstance), "Saves", "grave-alive-world.xml");
            Runtime = GraveAliveRuntime.LoadOrCreate(settings, Environment.TickCount, SavePath);
            Spawner = new VisibleSurvivorSpawner(settings);
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            _harmony = new Harmony("com.cursor.gravealive");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            SurvivorChatHandler.Register();

            GameLog.Out("[GraveAlive] Living-world simulation initialized.");
            GameLog.Out("[GraveAlive] Save file: " + SavePath);
        }

        internal static void SaveNow(string reason)
        {
            if (Runtime == null || string.IsNullOrEmpty(SavePath))
            {
                return;
            }

            try
            {
                Runtime.Save(SavePath);
                GameLog.Out("[GraveAlive] Saved living-world state (" + reason + ").");
            }
            catch (Exception exception)
            {
                GameLog.Out("[GraveAlive] Failed to save living-world state: " + exception.Message);
            }
        }

        private static void OnProcessExit(object sender, EventArgs eventArgs)
        {
            Shutdown("process exit");
        }

        internal static void Shutdown(string reason)
        {
            if (IsShuttingDown)
            {
                return;
            }

            SaveNow(reason);
            IsShuttingDown = true;

            try
            {
                SurvivorChatHandler.Unregister();
            }
            catch (Exception exception)
            {
                GameLog.Out("[GraveAlive] Chat handler cleanup failed: " + exception.Message);
            }

            SurvivorEntityRegistry.Clear();
            Spawner = null;

            try
            {
                _harmony?.UnpatchAll("com.cursor.gravealive");
            }
            catch (Exception exception)
            {
                GameLog.Out("[GraveAlive] Harmony cleanup failed: " + exception.Message);
            }
        }

        private static string ResolveWritableModDirectory(Mod modInstance)
        {
            string directory = TryReadStringMember(modInstance, "Path") ??
                TryReadStringMember(modInstance, "FolderPath") ??
                TryReadStringMember(modInstance, "ModPath") ??
                Application.persistentDataPath;

            return string.IsNullOrEmpty(directory) ? Application.persistentDataPath : directory;
        }

        private static string TryReadStringMember(object target, string name)
        {
            if (target == null)
            {
                return null;
            }

            PropertyInfo property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                return property.GetValue(target, null) as string;
            }

            FieldInfo field = target.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            return field == null ? null : field.GetValue(target) as string;
        }
    }

    [HarmonyPatch(typeof(GameManager), "Update")]
    internal static class GameManagerUpdatePatch
    {
        private static float _lastReportTime;
        private static bool _disabledDueToError;

        private static void Postfix()
        {
            if (_disabledDueToError || ModApi.IsShuttingDown || ModApi.Runtime == null)
            {
                return;
            }

            try
            {
                GameManager gameManager = GameManager.Instance;
                if (gameManager == null)
                {
                    return;
                }

                ModApi.Runtime.Update(Time.deltaTime);
                if (ModApi.Spawner != null)
                {
                    ModApi.Spawner.Process(ModApi.Runtime);
                }

                if (ModApi.Runtime.ShouldAutosave())
                {
                    ModApi.SaveNow("autosave");
                }

                if (Time.realtimeSinceStartup - _lastReportTime > 60f)
                {
                    _lastReportTime = Time.realtimeSinceStartup;
                    GameLog.Out("[GraveAlive] " + ModApi.Runtime.Summary());
                }
            }
            catch (Exception exception)
            {
                _disabledDueToError = true;
                GameLog.Out("[GraveAlive] Runtime disabled after exception: " + exception);
            }
        }
    }
}
#endif
