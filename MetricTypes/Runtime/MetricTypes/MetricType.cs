using Unity.Multiplayer.Tools.NetStats;
using MT = Unity.Multiplayer.Tools.MetricTypes.MetricType;
using ND = Unity.Multiplayer.Tools.MetricTypes.NetworkDirection;
using NDC = Unity.Multiplayer.Tools.MetricTypes.NetworkDirectionConstants;

namespace Unity.Multiplayer.Tools.MetricTypes
{
    enum MetricType
    {
        None,
        TotalBytes,
        Rpc,
        NamedMessage,
        UnnamedMessage,
        NetworkVariableDelta,
        ObjectSpawned,
        ObjectDestroyed,
        OwnershipChange,
        ServerLog,
        SceneEvent,
        NetworkMessage,
        Packets,
        RttToServer
    }

    // NOTE: Public because it needs to be exposed for RNSM configuration
    /// TODO: MTT-1852 - Document this API
    [MetricTypeEnum]
    internal enum DirectedMetricType
    {
        [MetricMetadata(Units        = Units.Bytes)]
        TotalBytesSent               = (MT.TotalBytes           << NDC.k_BitWidth) | ND.Sent,
        [MetricMetadata(Units        = Units.Bytes)]
        TotalBytesReceived           = (MT.TotalBytes           << NDC.k_BitWidth) | ND.Received,

        RpcSent                      = (MT.Rpc                  << NDC.k_BitWidth) | ND.Sent,
        RpcReceived                  = (MT.Rpc                  << NDC.k_BitWidth) | ND.Received,

        [MetricMetadata(DisplayName  = "Named Messages Sent")]
        NamedMessageSent             = (MT.NamedMessage         << NDC.k_BitWidth) | ND.Sent,

        [MetricMetadata(DisplayName  = "Named Messages Received")]
        NamedMessageReceived         = (MT.NamedMessage         << NDC.k_BitWidth) | ND.Received,

        [MetricMetadata(DisplayName  = "Unnamed Messages Sent")]
        UnnamedMessageSent           = (MT.UnnamedMessage       << NDC.k_BitWidth) | ND.Sent,

        [MetricMetadata(DisplayName  = "Unnamed Messages Received")]
        UnnamedMessageReceived       = (MT.UnnamedMessage       << NDC.k_BitWidth) | ND.Received,

        NetworkVariableDeltaSent     = (MT.NetworkVariableDelta << NDC.k_BitWidth) | ND.Sent,
        NetworkVariableDeltaReceived = (MT.NetworkVariableDelta << NDC.k_BitWidth) | ND.Received,

        ObjectSpawnedSent            = (MT.ObjectSpawned        << NDC.k_BitWidth) | ND.Sent,
        ObjectSpawnedReceived        = (MT.ObjectSpawned        << NDC.k_BitWidth) | ND.Received,

        ObjectDestroyedSent          = (MT.ObjectDestroyed      << NDC.k_BitWidth) | ND.Sent,
        ObjectDestroyedReceived      = (MT.ObjectDestroyed      << NDC.k_BitWidth) | ND.Received,

        OwnershipChangeSent          = (MT.OwnershipChange      << NDC.k_BitWidth) | ND.Sent,
        OwnershipChangeReceived      = (MT.OwnershipChange      << NDC.k_BitWidth) | ND.Received,

        ServerLogSent                = (MT.ServerLog            << NDC.k_BitWidth) | ND.Sent,
        ServerLogReceived            = (MT.ServerLog            << NDC.k_BitWidth) | ND.Received,

        SceneEventSent               = (MT.SceneEvent           << NDC.k_BitWidth) | ND.Sent,
        SceneEventReceived           = (MT.SceneEvent           << NDC.k_BitWidth) | ND.Received,

        NetworkMessageSent           = (MT.NetworkMessage       << NDC.k_BitWidth) | ND.Sent,
        NetworkMessageReceived       = (MT.NetworkMessage       << NDC.k_BitWidth) | ND.Received,

        PacketsSent                  = (MT.Packets              << NDC.k_BitWidth) | ND.Sent,
        PacketsReceived              = (MT.Packets              << NDC.k_BitWidth) | ND.Received,

        [MetricMetadata(
            DisplayName = "RTT To Server",
            MetricKind  = MetricKind.Gauge,
            Units       = Units.MilliSeconds)]
        RttToServer                  = (MT.RttToServer          << NDC.k_BitWidth) | ND.SentAndReceived
    }
}