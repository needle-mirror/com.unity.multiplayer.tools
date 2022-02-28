using Unity.Multiplayer.Tools.Util;

namespace Unity.Multiplayer.Tools.NetStatsReporting
{
    internal class TestDataTrends
    {
        const float k_LargeMax = 20f;
        const float k_MediumMax = 10f;
        const float k_SmallMax = 5f;
        const float k_MinRtt = 30f;
        const float k_MaxRtt = 200f;

        public LogNormalRandomWalk NamedMessagesSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk NamedMessagesReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk UnnamedMessagesSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk UnnamedMessagesReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk NetworkVariableDeltasSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk NetworkVariableDeltasReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk OwnershipChangeEventsReceived { get; } = new LogNormalRandomWalk { Max = k_MediumMax };
        public LogNormalRandomWalk OwnershipChangeEventsSent { get; } = new LogNormalRandomWalk { Max = k_MediumMax };

        public LogNormalRandomWalk ObjectSpawnEventsSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk ObjectSpawnEventsReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk ObjectDestroyedEventsSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk ObjectDestroyedEventsReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk RpcEventsSent { get; } = new LogNormalRandomWalk { Max = k_LargeMax };
        public LogNormalRandomWalk RpcEventsReceived { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk ServerLogEventsSent { get; } = new LogNormalRandomWalk { Max = k_SmallMax };
        public LogNormalRandomWalk ServerLogEventsReceived { get; } = new LogNormalRandomWalk { Max = k_SmallMax };

        public LogNormalRandomWalk SceneEventsSent { get; } = new LogNormalRandomWalk { Max = k_SmallMax };
        public LogNormalRandomWalk SceneEventsReceived { get; } = new LogNormalRandomWalk { Max = k_SmallMax };

        public LogNormalRandomWalk PacketSentCount { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk PacketReceivedCount { get; } = new LogNormalRandomWalk { Max = k_LargeMax };

        public LogNormalRandomWalk RttToServer { get; } = new LogNormalRandomWalk() { Min = k_MinRtt, Max = k_MaxRtt };
    }
}