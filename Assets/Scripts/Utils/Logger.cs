
using System.Diagnostics;

namespace Game.Utils.Logger
{
    /// <summary>
    /// A wrapper class for Unity's Debug class that only logs messages in the editor.
    /// </summary>
    public static class Debug
    {
        [Conditional("GAME_DEBUG")]
        public static void Log(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("GAME_DEBUG")]
        public static void LogWarning(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional("GAME_DEBUG")]
        public static void LogError(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("GAME_DEBUG")]
        public static void LogException(System.Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        [Conditional("GAME_DEBUG")]
        public static void Assert(bool condition)
        {
            UnityEngine.Debug.Assert(condition);
        }

        [Conditional("GAME_DEBUG")]
        public static void LogTag(string tag, object message)
        {
            UnityEngine.Debug.Log($"[{tag}] {message}");
        }

    }

}