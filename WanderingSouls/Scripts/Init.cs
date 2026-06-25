using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace WanderingSouls
{
    public class Init : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            var harmony = new Harmony("com.mikemod.wanderingsouls");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Debug.Log("[Wandering Souls] Mod initialized and fully patched.");
        }
    }
}
