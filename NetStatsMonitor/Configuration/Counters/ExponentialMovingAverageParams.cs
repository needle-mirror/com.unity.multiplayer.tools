using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Parameters for the exponential moving average smoothing method in <see cref="CounterConfiguration"/>.
    /// </summary>
    [Serializable]
    public sealed class ExponentialMovingAverageParams
    {
        /// <summary>
        /// The half-life (in seconds) by which samples should decay.
        /// By default, this is set to one second.
        /// </summary>
        [field: SerializeField]
        [field: Min(0)]
        public double HalfLife { get; set; } = 1;
    }
}