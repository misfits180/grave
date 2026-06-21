#if GRAVE_7DTD
using System;
using System.Linq;
using System.Reflection;

namespace GraveAlive.GameIntegration
{
    internal static class GameLog
    {
        private static readonly Type LogType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType("Log"))
            .FirstOrDefault(type => type != null);

        public static void Out(string message)
        {
            Write("Out", message);
        }

        private static void Write(string methodName, string message)
        {
            if (LogType == null)
            {
                return;
            }

            MethodInfo method = LogType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (method != null)
            {
                method.Invoke(null, new object[] { message });
            }
        }
    }
}
#endif
