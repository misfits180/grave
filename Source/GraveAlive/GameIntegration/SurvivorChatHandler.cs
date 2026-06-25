#if GRAVE_7DTD
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace GraveAlive.GameIntegration
{
    internal static class SurvivorChatHandler
    {
        private static readonly string[] GreetingTriggers =
        {
            "hi",
            "hello",
            "hey",
            "howdy",
            "sup"
        };

        internal static void Register()
        {
            ModEvents.ChatMessage.RegisterHandler(OnChatMessage);
        }

        internal static void Unregister()
        {
            ModEvents.ChatMessage.UnregisterHandler(OnChatMessage);
        }

        internal static string BuildGreeting(string survivorName)
        {
            return "Easy... I'm not infected. Name's " + survivorName + ".";
        }

        private static bool OnChatMessage(
            ClientInfo clientInfo,
            EChatType chatType,
            int senderEntityId,
            string message,
            string mainName,
            bool localizeMain,
            List<int> recipientEntityIds)
        {
            if (ModApi.IsShuttingDown || string.IsNullOrEmpty(message))
            {
                return true;
            }

            TryRespondToNearbySurvivors(clientInfo, senderEntityId, message);
            return true;
        }

        internal static void TryRespondToNearbySurvivors(ClientInfo clientInfo, int senderEntityId, string message)
        {
            if (!IsGreeting(message))
            {
                return;
            }

            World world = GameManager.Instance == null ? null : GameManager.Instance.World;
            if (world == null)
            {
                return;
            }

            EntityPlayer sender = world.GetEntity(senderEntityId) as EntityPlayer;
            if (sender == null && clientInfo != null)
            {
                sender = world.GetEntity(clientInfo.entityId) as EntityPlayer;
            }

            if (sender == null)
            {
                sender = world.GetPrimaryPlayer();
            }

            if (sender == null)
            {
                return;
            }

            List<Entity> nearby = new List<Entity>();
            world.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(sender.position, Vector3.one * 12f), nearby);

            for (int i = 0; i < nearby.Count; i++)
            {
                EntityAlive alive = nearby[i] as EntityAlive;
                if (alive == null || alive.IsDead() || !SurvivorEntitySetup.IsGraveAliveSurvivor(alive))
                {
                    continue;
                }

                string survivorName;
                if (!SurvivorEntityRegistry.TryGetName(alive.entityId, out survivorName))
                {
                    survivorName = alive.EntityName;
                }

                if (string.IsNullOrEmpty(survivorName))
                {
                    survivorName = "Survivor";
                }

                string reply = BuildGreeting(survivorName);
                GameManager.Instance.ChatMessageServer(
                    clientInfo,
                    EChatType.Global,
                    -1,
                    reply,
                    survivorName,
                    false,
                    null);

                EntityPlayerLocal localPlayer = sender as EntityPlayerLocal;
                if (localPlayer != null)
                {
                    GameManager.ShowTooltip(localPlayer, survivorName + ": " + reply);
                }

                GameLog.Out("[GraveAlive] " + survivorName + " replied to chat greeting.");
                return;
            }
        }

        private static bool IsGreeting(string message)
        {
            string trimmed = message.Trim().Trim('"', '\'').ToLowerInvariant();
            for (int i = 0; i < GreetingTriggers.Length; i++)
            {
                if (trimmed == GreetingTriggers[i] ||
                    trimmed.StartsWith(GreetingTriggers[i] + " ", StringComparison.Ordinal) ||
                    trimmed.EndsWith(" " + GreetingTriggers[i], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
