#nullable enable

using System.Diagnostics;
using UnityEngine;

namespace SleepyWaschbarStudios.MinifantasyManager
{
    public static class DebugExtensions
    {
        [Conditional("VERBOSE_LOGGING")]
        public static void VerboseLog(string msg)
        {
            UnityEngine.Debug.Log("Verbose: " + msg);    
        }
    }
}
