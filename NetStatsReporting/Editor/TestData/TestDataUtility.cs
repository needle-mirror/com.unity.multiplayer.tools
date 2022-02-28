using UnityEditor;
using UnityEngine.Profiling;

namespace Unity.Multiplayer.Tools.NetStatsReporting
{
    class TestDataUtility
    {
        [MenuItem("Window/Test/Dispatch Client Frame and Pause Profiler", false, 22)]
        public static void GenerateClientFrame()
        {
            var dispatcher = new TestDataDispatcher();

            dispatcher.DispatchClientFrame();

            Profiler.enabled = false;
        }

        [MenuItem("Window/Test/Dispatch Server Frame and Pause Profiler", false, 22)]
        public static void GenerateServerFrame()
        {
            var dispatcher = new TestDataDispatcher();

            dispatcher.DispatchServerFrame();

            Profiler.enabled = false;
        }
    }
}