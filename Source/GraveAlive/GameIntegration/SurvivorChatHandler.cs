#if GRAVE_7DTD
using System;
using System.Collections.Generic;
using System.Reflection;
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
            // Intentionally empty: V2.6 reflection/type loading can fail if
            // we hard-reference ModEvents.ChatMessage types across builds.
        }

        internal static void Unregister()
        {
            // Intentionally empty, see Register().
        }

        internal static string BuildGreeting(string survivorName)
        {
            return "Easy... I'm not infected. Name's " + survivorName + ".";
        }

        internal static void TryRespondToNearbySurvivors(ClientInfo clientInfo, int senderEntityId, string message)
        {
            if (!IsGreeting(message))
            {
                return;
            }

            World world = ResolveWorld();
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

        private static World ResolveWorld()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return null;
            }

            Type gameManagerType = gameManager.GetType();
            PropertyInfo property = gameManagerType.GetProperty("World", BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                return property.GetValue(gameManager, null) as World;
            }

            FieldInfo field = gameManagerType.GetField("World", BindingFlags.Public | BindingFlags.Instance);
            return field == null ? null : field.GetValue(gameManager) as World;
        }
    }

    internal static class SurvivorChatMessagePatch
    {
        private static void Postfix(
            ClientInfo _cInfo,
            EChatType _chatType,
            int _senderEntityId,
            string _msg,
            string _mainName,
            bool _localizeMain,
            List<int> _recipientEntityIds)
        {
            if (ModApi.IsShuttingDown || string.IsNullOrEmpty(_msg))
            {
                return;
            }

            SurvivorChatHandler.TryRespondToNearbySurvivors(_cInfo, _senderEntityId, _msg);
        }
    }
}
#endif
