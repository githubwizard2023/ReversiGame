using UnityEngine;

namespace Game
{
    // This static class centralizes optional debug output behind explicit call-site flags.
    // It acts as a small utility instead of a service because it has no state or dependencies.
    // This keeps runtime classes free of repeated Debug.Log guards and avoids inspector config noise.
    public static class GameDebugLogger
    {
        // Writes a normal debug message only when the caller enables it.
        public static void Log(string message, bool is_enabled)
        {
            if (is_enabled == false)
            {
                return;
            }

            Debug.Log(message);
        }

        // Writes a warning message only when the caller enables it.
        public static void LogWarning(string message, bool is_enabled)
        {
            if (is_enabled == false)
            {
                return;
            }

            Debug.LogWarning(message);
        }

        // Writes an error message only when the caller enables it.
        public static void LogError(string message, bool is_enabled)
        {
            if (is_enabled == false)
            {
                return;
            }

            Debug.LogError(message);
        }
    }
}
