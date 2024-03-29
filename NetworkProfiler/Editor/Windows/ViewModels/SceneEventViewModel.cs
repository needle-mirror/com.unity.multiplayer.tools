using System;
using Unity.Multiplayer.Tools.MetricTypes;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal class SceneEventViewModel : ViewModelBase
    {
        public SceneEventViewModel(ulong treeViewid, string sceneName, string eventType, IRowData parent, Action onSelectedCallback = null)
            : base(
                parent,
                GenerateName(sceneName, eventType),
                MetricType.SceneEvent,
                onSelectedCallback,
                treeViewid) { }

        static string GenerateName(string sceneName, string eventType)
        {
            var name = eventType;

            if (!string.IsNullOrEmpty(sceneName))
            {
                name = $"{name} ({sceneName})";
            }

            return name;
        }
    }
}
