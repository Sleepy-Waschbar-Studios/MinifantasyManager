#nullable enable

using System.Diagnostics;

namespace SleepyWaschbarStudios.MinifantasyManager
{
    public static class DebugExtensions
    {
        [Conditional("VERBOSE_LOGGING")]
        public static void VerboseLog(string msg)
        {
            UnityEngine.Debug.Log("Verbose: " + msg);    
        }

        [Conditional("VERBOSE_LOGGING")]
        public static void VerboseLogFormat(string msg, params object[] args)
        {
            UnityEngine.Debug.LogFormat("Verbose: " + msg, args);
        }
    }
}
