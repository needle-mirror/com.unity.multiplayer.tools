﻿using System;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    [Serializable]
    struct ObjectSpawnedEvent : INetworkMetricEvent, INetworkObjectEvent
    {
        public ObjectSpawnedEvent(ConnectionInfo connection, NetworkObjectIdentifier networkId, long bytesCount)
        {
            Connection = connection;
            NetworkId = networkId;
            BytesCount = bytesCount;
        }

        public ConnectionInfo Connection { get; }

        public NetworkObjectIdentifier NetworkId { get; }

        public long BytesCount { get; }
    }
}
