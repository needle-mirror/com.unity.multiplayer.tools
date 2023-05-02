using System;
using JetBrains.Annotations;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    internal interface IRowData
    {
        string Name { get; }

        [CanBeNull]
        IRowData Parent { get; }

        /// The full path of this row in the tree view, used to identify it
        string TreeViewPath { get; }

        BytesSentAndReceived Bytes { get; }

        bool SentOverLocalConnection { get; }

        string TypeDisplayName { get; }

        /// A string identifying the type: should be lowercase, all-one-word for filtering
        string TypeName { get; }

        Action OnSelectedCallback { get; }
    }
}
