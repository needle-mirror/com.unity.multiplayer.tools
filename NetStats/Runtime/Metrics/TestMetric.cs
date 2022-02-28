namespace Unity.Multiplayer.Tools.NetStats
{
#if !UNITY_MP_TOOLS_DEV
    [MetricTypeEnumHideInInspector]
#endif
    [MetricTypeEnum]
    internal enum TestMetric
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        Count,
    }
}