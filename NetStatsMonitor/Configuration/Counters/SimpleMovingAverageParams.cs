using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Parameters for the simple moving average smoothing method in <see cref="CounterConfiguration"/>.
    /// </summary>
    [Serializable]
    public sealed class SimpleMovingAverageParams
    {
        /// <summary>
        /// The number of samples that are maintained for the purpose of smoothing
        /// </summary>
        [field: SerializeField]
        [field: Min(1)]
        public int SampleCount { get; set; } = 64;
    }
}