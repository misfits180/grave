#if GRAVE_7DTD
using System.Reflection;
using UnityEngine;

namespace GraveAlive.GameIntegration
{
    internal static class SurvivorEntitySetup
    {
        internal const string EntityClassName = "graveAliveSurvivorRanged";
        internal const string SurvivorTag = "graveAliveSurvivor";

        internal static bool IsGraveAliveSurvivor(object entity)
        {
            if (entity == null)
            {
                return false;
            }

            string className = ReadString(entity, "entityClass", "EntityClass");
            if (EntityClassName.Equals(className))
            {
                return true;
            }

            return HasTag(entity, SurvivorTag);
        }

        internal static void Configure(object entity, string survivorName, object world)
        {
            if (entity == null)
            {
                return;
            }

            Vector3 position = ReadVector3(entity);
            position = SnapToGround(world, position);
            SetPosition(entity, position);

            object physicsTransform = ReadMember(entity, "PhysicsTransform", "physicsTransform");
            if (physicsTransform is Transform transform)
            {
                transform.gameObject.SetActive(true);
            }

            SetBoolValue(entity, "IsNoCollisionMode", false);

            object modelTransform = ReadMember(entity, "modelTransform", "ModelTransform");
            if (modelTransform is Transform model && model.gameObject != null)
            {
                Collider[] colliders = model.gameObject.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].enabled = true;
                }
            }

            int entityId = ReadInt(entity, "entityId", "EntityId");
            if (entityId >= 0)
            {
                SurvivorEntityRegistry.Register(entityId, survivorName);
            }

            SetEntityName(entity, survivorName);
        }

        internal static Vector3 SnapToGround(object world, Vector3 position)
        {
            if (world == null)
            {
                return position;
            }

            float height;
            if (TryReadTerrainHeight(world, position.x, position.z, out height))
            {
                position.y = height + 1.6f;
            }

            return position;
        }

        private static bool TryReadTerrainHeight(object world, float x, float z, out float height)
        {
            height = 0f;
            foreach (string methodName in new[] { "GetHeight", "GetTerrainHeight" })
            {
                MethodInfo method = world.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 2 &&
                    parameters[0].ParameterType == typeof(float) &&
                    parameters[1].ParameterType == typeof(float))
                {
                    height = System.Convert.ToSingle(method.Invoke(world, new object[] { x, z }));
                    return true;
                }
            }

            return false;
        }

        private static bool HasTag(object entity, string tag)
        {
            object entityClass = ReadMember(entity, "EntityClass", "entityClass");
            if (entityClass == null)
            {
                return false;
            }

            object tags = ReadMember(entityClass, "Tags", "tags");
            return tags != null && tags.ToString().IndexOf(tag, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void SetEntityName(object entity, string survivorName)
        {
            MethodInfo setName = entity.GetType().GetMethod("SetEntityName", BindingFlags.Public | BindingFlags.Instance);
            if (setName != null && setName.GetParameters().Length == 1)
            {
                setName.Invoke(entity, new object[] { survivorName });
                return;
            }

            SetMember(entity, "entityName", survivorName);
            SetMember(entity, "EntityName", survivorName);
        }

        private static void SetBoolValue(object entity, string memberName, bool value)
        {
            object current = ReadMember(entity, memberName, memberName);
            if (current == null)
            {
                return;
            }

            PropertyInfo valueProperty = current.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (valueProperty != null && valueProperty.CanWrite)
            {
                valueProperty.SetValue(current, value, null);
            }
        }

        private static Vector3 ReadVector3(object entity)
        {
            object value = ReadMember(entity, "position", "Position");
            return value is Vector3 vector ? vector : Vector3.zero;
        }

        private static void SetPosition(object entity, Vector3 position)
        {
            SetMember(entity, "position", position);
            SetMember(entity, "Position", position);

            MethodInfo setPosition = entity.GetType().GetMethod("SetPosition", BindingFlags.Public | BindingFlags.Instance);
            if (setPosition != null && setPosition.GetParameters().Length == 1)
            {
                setPosition.Invoke(entity, new object[] { position });
            }
        }

        private static object ReadMember(object target, string fieldName, string propertyName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(target);
            }

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(target, null);
        }

        private static string ReadString(object target, string fieldName, string propertyName)
        {
            object value = ReadMember(target, fieldName, propertyName);
            return value as string;
        }

        private static int ReadInt(object target, string fieldName, string propertyName)
        {
            object value = ReadMember(target, fieldName, propertyName);
            return value == null ? -1 : System.Convert.ToInt32(value);
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
    }
}
#endif
