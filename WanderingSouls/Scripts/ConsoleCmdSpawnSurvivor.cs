using System.Collections.Generic;
using UnityEngine;

namespace WanderingSouls
{
    public class ConsoleCmdSpawnSurvivor : ConsoleCmdAbstract
    {
        public override string[] getCommands()
        {
            return new string[] { "spawn-survivor", "ssurvivor" };
        }

        public override string getDescription()
        {
            return "Spawns a custom Wandering Soul NPC at the player's position.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Debug.LogWarning("Wandering Souls: Player not found.");
                return;
            }

            string entityName = "npcWanderingSoul";
            int entityID = EntityClass.FromString(entityName);

            if (entityID == -1)
            {
                Debug.LogError($"Wandering Souls: Entity '{entityName}' not found. Check entityclasses.xml.");
                return;
            }

            Entity entity = EntityFactory.CreateEntity(entityID, player.position);
            GameManager.Instance.World.SpawnEntityInWorld(entity);

            Debug.Log($"Wandering Souls: Successfully spawned {entityName} at player position.");
        }
    }
}
