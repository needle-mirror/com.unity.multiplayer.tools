using System;
using JetBrains.Annotations;

namespace Unity.Multiplayer.Tools.NetStats
{
    /// TODO: MTT-1852 - Document this API
    [AttributeUsage(AttributeTargets.Field)]
    internal class MetricMetadataAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public MetricKind MetricKind { get; set; } = MetricKind.Counter;

        public Units Units { get; set; } = Units.None;
    }
}