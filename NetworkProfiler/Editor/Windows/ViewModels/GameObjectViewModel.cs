using System;
using Unity.Multiplayer.Tools.MetricTypes;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal class GameObjectViewModel : ViewModelBase
    {
        public GameObjectViewModel(NetworkObjectIdentifier objectIdentifier, IRowData parent, Action onSelectedCallback = null)
            : base(
                parent,
                GetName(objectIdentifier),
                string.Empty,
                "gameobject",
                onSelectedCallback,
                objectIdentifier.TreeViewId) { }

        static string GetName(NetworkObjectIdentifier objectIdentifier)
        {
            return !string.IsNullOrWhiteSpace(objectIdentifier.Name.ToString())
                ? objectIdentifier.Name.ToString()
                : objectIdentifier.NetworkId.ToString();
        }
    }
}
