#if GRAVE_7DTD
using System.Collections.Generic;

namespace GraveAlive.GameIntegration
{
    internal static class SurvivorEntityRegistry
    {
        private static readonly Dictionary<int, string> NamesByEntityId = new Dictionary<int, string>();

        internal static void Register(int entityId, string survivorName)
        {
            if (entityId < 0 || string.IsNullOrEmpty(survivorName))
            {
                return;
            }

            NamesByEntityId[entityId] = survivorName;
        }

        internal static void Unregister(int entityId)
        {
            if (entityId < 0)
            {
                return;
            }

            NamesByEntityId.Remove(entityId);
        }

        internal static bool TryGetName(int entityId, out string survivorName)
        {
            return NamesByEntityId.TryGetValue(entityId, out survivorName);
        }

        internal static void Clear()
        {
            NamesByEntityId.Clear();
        }
    }
}
#endif
