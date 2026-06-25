#if GRAVE_7DTD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraveAlive.Simulation;
using UnityEngine;

namespace GraveAlive.GameIntegration
{
    internal sealed class VisibleSurvivorSpawner
    {
        private readonly SimulationSettings _settings;

        public VisibleSurvivorSpawner(SimulationSettings settings)
        {
            _settings = settings;
        }

        public void Process(GraveAliveRuntime runtime)
        {
            IReadOnlyList<WorldPosition> players = GetPlayerPositions();
            IReadOnlyList<SurvivorSpawnRequest> requests = runtime.PlanVisibleSurvivorSpawns(players);

            foreach (SurvivorSpawnRequest request in requests)
            {
                if (request.RequestType == SpawnRequestType.Spawn)
                {
                    TrySpawn(runtime, request);
                }
                else
                {
                    TryDespawn(runtime, request);
                }
            }
        }

        private void TrySpawn(GraveAliveRuntime runtime, SurvivorSpawnRequest request)
        {
            object world = GetWorld();
            if (world == null)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "game world is not ready");
                return;
            }

            int entityClassId = ResolveEntityClassId(request.EntityClassName);
            if (entityClassId < 0)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "could not resolve entity class " + request.EntityClassName);
                return;
            }

            Vector3 position = ResolveSpawnPosition(request.Position, world);
            object entityCreationData = CreateEntityCreationData(entityClassId, position);
            if (entityCreationData == null)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "EntityCreationData was not found");
                return;
            }

            MethodInfo createEntity = ResolveCreateEntityMethod(entityCreationData.GetType());
            if (createEntity == null)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "EntityFactory.CreateEntity(EntityCreationData) was not found");
                return;
            }

            object entity = createEntity.Invoke(null, new[] { entityCreationData });
            if (entity == null)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "EntityFactory returned no entity");
                return;
            }

            MethodInfo spawnMethod = world.GetType().GetMethod("SpawnEntityInWorld", BindingFlags.Public | BindingFlags.Instance);
            if (spawnMethod == null)
            {
                runtime.MarkSpawnFailed(request.SurvivorId, "World.SpawnEntityInWorld was not found");
                return;
            }

            spawnMethod.Invoke(world, new[] { entity });

            SurvivorEntitySetup.Configure(entity, request.SurvivorName, world);

            int entityId = ReadInt(entity, "entityId", "EntityId");
            runtime.MarkSpawnSucceeded(request.SurvivorId, entityId);
            GameLog.Out("[GraveAlive] Spawned survivor " + request.SurvivorName + " near player as " + request.EntityClassName + ".");
        }

        private Vector3 ResolveSpawnPosition(WorldPosition requestPosition, object world)
        {
            Vector3 position = new Vector3(requestPosition.X, requestPosition.Y, requestPosition.Z);
            IReadOnlyList<WorldPosition> players = GetPlayerPositions();

            // Seed positions start near world origin with Y=0; convert to offsets from the player.
            if (players.Count > 0 && requestPosition.Y <= 1f)
            {
                WorldPosition anchor = players[0];
                position = new Vector3(
                    anchor.X + requestPosition.X,
                    anchor.Y,
                    anchor.Z + requestPosition.Z);
            }

            return SurvivorEntitySetup.SnapToGround(world, position);
        }

        private static MethodInfo ResolveCreateEntityMethod(Type entityCreationDataType)
        {
            Type entityFactoryType = FindType("EntityFactory");
            return entityFactoryType == null
                ? null
                : entityFactoryType.GetMethod("CreateEntity", BindingFlags.Public | BindingFlags.Static, null, new[] { entityCreationDataType }, null);
        }

        private static object CreateEntityCreationData(int entityClassId, Vector3 position)
        {
            Type entityCreationDataType = FindType("EntityCreationData");
            if (entityCreationDataType == null)
            {
                return null;
            }

            object data = Activator.CreateInstance(entityCreationDataType);
            SetMember(data, "entityClass", entityClassId);
            SetMember(data, "id", -1);
            SetMember(data, "pos", position);
            SetMember(data, "rot", Vector3.zero);
            SetMember(data, "lifetime", -1f);
            SetMember(data, "belongsPlayerId", -1);
            SetMember(data, "spawnById", -1);
            SetMember(data, "spawnByName", "GraveAlive");
            return data;
        }

        private void TryDespawn(GraveAliveRuntime runtime, SurvivorSpawnRequest request)
        {
            object world = GetWorld();
            if (world == null || !request.EntityId.HasValue)
            {
                runtime.MarkDespawnSucceeded(request.SurvivorId);
                return;
            }

            int entityId = request.EntityId.Value;
            SurvivorEntityRegistry.Unregister(entityId);

            object entity = TryGetEntity(world, entityId);
            if (entity != null)
            {
                TryRemoveEntity(world, entity, entityId);
            }

            runtime.MarkDespawnSucceeded(request.SurvivorId);
            GameLog.Out("[GraveAlive] Returned survivor " + request.SurvivorName + " to background simulation.");
        }

        private IReadOnlyList<WorldPosition> GetPlayerPositions()
        {
            object world = GetWorld();
            if (world == null)
            {
                return new List<WorldPosition>();
            }

            IEnumerable players = TryGetPlayers(world);
            if (players == null)
            {
                return new List<WorldPosition>();
            }

            List<WorldPosition> positions = new List<WorldPosition>();
            foreach (object player in players)
            {
                WorldPosition? position = TryGetPosition(player);
                if (position.HasValue)
                {
                    positions.Add(position.Value);
                }
            }

            return positions;
        }

        private int ResolveEntityClassId(string entityClassName)
        {
            int directClassId = ResolveEntityClassIdFromName(entityClassName);
            if (directClassId >= 0)
            {
                return directClassId;
            }

            Type entityGroupsType = FindType("EntityGroups");
            MethodInfo getRandomFromGroup = entityGroupsType == null
                ? null
                : entityGroupsType.GetMethod("GetRandomFromGroup", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (getRandomFromGroup != null)
            {
                object value = getRandomFromGroup.Invoke(null, new object[] { _settings.SurvivorEntityGroupName });
                int id = Convert.ToInt32(value);
                if (id >= 0)
                {
                    return id;
                }
            }

            return -1;
        }

        private static int ResolveEntityClassIdFromName(string entityClassName)
        {
            Type entityClassType = FindType("EntityClass");
            if (entityClassType == null)
            {
                return -1;
            }

            foreach (MethodInfo method in entityClassType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if ((method.Name == "FromString" || method.Name == "GetEntityClass") &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(string))
                {
                    object value = method.Invoke(null, new object[] { entityClassName });
                    return Convert.ToInt32(value);
                }
            }

            return -1;
        }

        private static object GetWorld()
        {
            object gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                return null;
            }

            PropertyInfo worldProperty = gameManager.GetType().GetProperty("World", BindingFlags.Public | BindingFlags.Instance);
            if (worldProperty != null)
            {
                return worldProperty.GetValue(gameManager, null);
            }

            FieldInfo worldField = gameManager.GetType().GetField("World", BindingFlags.Public | BindingFlags.Instance);
            return worldField == null ? null : worldField.GetValue(gameManager);
        }

        private static IEnumerable TryGetPlayers(object world)
        {
            foreach (string methodName in new[] { "GetPlayers", "GetPlayerEntities" })
            {
                MethodInfo method = world.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null && method.GetParameters().Length == 0)
                {
                    return method.Invoke(world, null) as IEnumerable;
                }
            }

            foreach (string memberName in new[] { "Players", "players" })
            {
                PropertyInfo property = world.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    return property.GetValue(world, null) as IEnumerable;
                }

                FieldInfo field = world.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(world) as IEnumerable;
                }
            }

            return null;
        }

        private static WorldPosition? TryGetPosition(object entity)
        {
            foreach (string methodName in new[] { "GetPosition", "GetPositionServer" })
            {
                MethodInfo method = entity.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null && method.GetParameters().Length == 0)
                {
                    object value = method.Invoke(entity, null);
                    if (value is Vector3)
                    {
                        Vector3 vector = (Vector3)value;
                        return new WorldPosition(vector.x, vector.y, vector.z);
                    }
                }
            }

            foreach (string memberName in new[] { "position", "Position" })
            {
                PropertyInfo property = entity.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    object value = property.GetValue(entity, null);
                    if (value is Vector3)
                    {
                        Vector3 vector = (Vector3)value;
                        return new WorldPosition(vector.x, vector.y, vector.z);
                    }
                }

                FieldInfo field = entity.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    object value = field.GetValue(entity);
                    if (value is Vector3)
                    {
                        Vector3 vector = (Vector3)value;
                        return new WorldPosition(vector.x, vector.y, vector.z);
                    }
                }
            }

            return null;
        }

        private static object TryGetEntity(object world, int entityId)
        {
            foreach (string methodName in new[] { "GetEntity", "GetEntityByID", "GetEntityById" })
            {
                MethodInfo method = world.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method != null &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(int))
                {
                    return method.Invoke(world, new object[] { entityId });
                }
            }

            return null;
        }

        private static void TryRemoveEntity(object world, object entity, int entityId)
        {
            foreach (string methodName in new[] { "RemoveEntity", "RemoveEntityFromWorld" })
            {
                MethodInfo method = world.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                {
                    method.Invoke(world, new object[] { entityId });
                    return;
                }

                if (parameters.Length == 1 && parameters[0].ParameterType.IsInstanceOfType(entity))
                {
                    method.Invoke(world, new[] { entity });
                    return;
                }
            }
        }

        private static int ReadInt(object target, string fieldName, string propertyName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return Convert.ToInt32(field.GetValue(target));
            }

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                return Convert.ToInt32(property.GetValue(target, null));
            }

            return -1;
        }

        private static void SetMember(object target, string memberName, object value)
        {
            FieldInfo field = target.GetType().GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            PropertyInfo property = target.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value, null);
            }
        }

        private static Type FindType(string typeName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(type => type != null);
        }
    }
}
#endif
