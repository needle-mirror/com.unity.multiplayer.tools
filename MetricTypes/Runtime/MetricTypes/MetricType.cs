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
        RttToServer,
        NetworkObjects,
        Connections,
        PacketLoss
    }

    // NOTE: Public because it needs to be exposed for RNSM configuration

    /// <summary>
    /// The built in set of metrics that can be displayed in the multiplayer tools,
    /// such as the Network Profiler and the Runtime Net Stats Monitor.
    /// </summary>
    [MetricTypeEnum(DisplayName = "Built-In Metrics")]
    [MetricTypeSortPriority(SortPriority = SortPriority.VeryHigh)]
    public enum DirectedMetricType
    {
        /// <summary>
        /// Number of total bytes sent.
        /// </summary>
        [MetricMetadata(Units        = Units.Bytes)]
        TotalBytesSent               = (MT.TotalBytes           << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of total bytes received.
        /// </summary>
        [MetricMetadata(Units        = Units.Bytes)]
        TotalBytesReceived           = (MT.TotalBytes           << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of RPC sent.
        /// </summary>
        RpcSent                      = (MT.Rpc                  << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of RPC received.
        /// </summary>
        RpcReceived                  = (MT.Rpc                  << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of custom named message sent.
        /// </summary>
        [MetricMetadata(DisplayName  = "Named Messages Sent")]
        NamedMessageSent             = (MT.NamedMessage         << NDC.k_BitWidth) | ND.Sent,

        /// <summary>
        /// Number of custom named message received.
        /// </summary>
        [MetricMetadata(DisplayName  = "Named Messages Received")]
        NamedMessageReceived         = (MT.NamedMessage         << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of custom unnamed message sent.
        /// </summary>
        [MetricMetadata(DisplayName  = "Unnamed Messages Sent")]
        UnnamedMessageSent           = (MT.UnnamedMessage       << NDC.k_BitWidth) | ND.Sent,

        /// <summary>
        /// Number of custom unnamed message received.
        /// </summary>
        [MetricMetadata(DisplayName  = "Unnamed Messages Received")]
        UnnamedMessageReceived       = (MT.UnnamedMessage       << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of network variable delta message sent.
        /// </summary>
        NetworkVariableDeltaSent     = (MT.NetworkVariableDelta << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of network variable delta message received.
        /// </summary>
        NetworkVariableDeltaReceived = (MT.NetworkVariableDelta << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of network object spawned message sent.
        /// </summary>
        ObjectSpawnedSent            = (MT.ObjectSpawned        << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of network object spawned message received.
        /// </summary>
        ObjectSpawnedReceived        = (MT.ObjectSpawned        << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of network object destroyed message sent.
        /// </summary>
        ObjectDestroyedSent          = (MT.ObjectDestroyed      << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of network object destroyed message received.
        /// </summary>
        ObjectDestroyedReceived      = (MT.ObjectDestroyed      << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of ownership change message sent.
        /// </summary>
        OwnershipChangeSent          = (MT.OwnershipChange      << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of ownership change message received.
        /// </summary>
        OwnershipChangeReceived      = (MT.OwnershipChange      << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of server log message sent.
        /// </summary>
        ServerLogSent                = (MT.ServerLog            << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of server log message received.
        /// </summary>
        ServerLogReceived            = (MT.ServerLog            << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of scene event message sent.
        /// </summary>
        SceneEventSent               = (MT.SceneEvent           << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of scene event message received.
        /// </summary>
        SceneEventReceived           = (MT.SceneEvent           << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of network message sent.
        /// </summary>
        NetworkMessageSent           = (MT.NetworkMessage       << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of network message received.
        /// </summary>
        NetworkMessageReceived       = (MT.NetworkMessage       << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// Number of packets sent.
        /// </summary>
        PacketsSent                  = (MT.Packets              << NDC.k_BitWidth) | ND.Sent,
        /// <summary>
        /// Number of packets received.
        /// </summary>
        PacketsReceived              = (MT.Packets              << NDC.k_BitWidth) | ND.Received,

        /// <summary>
        /// The RTT from a client to a server.
        /// This include the processing time at the transport level
        /// but does not contain the processing time spent inside the Netcode for GameObjects.
        /// </summary>
        [MetricMetadata(
            DisplayName = "RTT To Server",
            MetricKind  = MetricKind.Gauge,
            Units       = Units.Seconds)]
        RttToServer                  = (MT.RttToServer          << NDC.k_BitWidth) | ND.SentAndReceived,

        /// <summary>
        /// Number of active Network Objects.
        /// </summary>
        [MetricMetadata(MetricKind   = MetricKind.Gauge)]
        NetworkObjects               = (MT.NetworkObjects       << NDC.k_BitWidth) | ND.SentAndReceived,

        /// <summary>
        /// Number of active network connections.
        /// A client will always show one active connection (client to server).
        /// </summary>
        [MetricMetadata(MetricKind   = MetricKind.Gauge)]
        Connections                  = (MT.Connections          << NDC.k_BitWidth) | ND.SentAndReceived,

        /// <summary>
        /// Percentage of packet loss over the lifetime of the connection.
        /// This is only valid for clients.
        /// This value will always be zero on a server.
        /// </summary>
        [MetricMetadata(
            MetricKind = MetricKind.Gauge,
            DisplayAsPercentage = true)]
        PacketLoss                   = (MT.PacketLoss           << NDC.k_BitWidth) | ND.Received,
    }
}