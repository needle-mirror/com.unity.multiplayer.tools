using System;

namespace Unity.Multiplayer.Tools.NetStats
{
    /// TODO: MTT-1852 - Document this API
    [AttributeUsage(AttributeTargets.Enum)]
    internal class MetricTypeEnumAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }

    /// Attribute internal fields cannot be referenced when applying attributes.
    /// As an alternative, this second, internal attribute can be used to hide metrics
    /// from the inspector that are only intended for internal MP Tools testing.
    [AttributeUsage(AttributeTargets.Enum)]
    internal class MetricTypeEnumHideInInspectorAttribute : Attribute {}
}