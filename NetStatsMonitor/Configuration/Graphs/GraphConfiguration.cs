using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Graph configuration used by <see cref="DisplayElementConfiguration"/>.
    /// This configuration contain all information about a Graph.
    /// </summary>
    [Serializable]
    public sealed class GraphConfiguration
    {
        /// <summary>
        /// The number of samples that are maintained for the purpose of graphing.
        /// </summary>
        [field: SerializeField]
        public int SampleCount { get; set; } = 256;

        /// <summary>
        /// List of colors to override the default colors of the graph.
        /// </summary>
        [field: SerializeField]
        public List<Color> VariableColors { get; set; } = new();

        /// <summary>
        /// The units used for displaying the bounds of the graph's x-axis.
        /// By default the graph bounds are displayed in units of sample count.
        /// If set to time, the the x-axis graph bounds will display
        /// the time over which these samples were collected.
        /// </summary>
        [field: SerializeField]
        public GraphXAxisType XAxisType { get; set; } = GraphXAxisType.Samples;

        /// <summary>
        /// Line-graph specific options.
        /// </summary>
        [field: SerializeField]
        public LineGraphConfiguration LineGraphConfiguration { get; set; } = new();

#if UNITY_2021_2_OR_NEWER // HashCode isn't defined in Unity < 2021.2
        internal int ComputeHashCode()
        {
            var hash = HashCode.Combine(SampleCount, (int)XAxisType, LineGraphConfiguration.ComputeHashCode());
            if (VariableColors != null)
            {
                foreach (var color in VariableColors)
                {
                    hash = HashCode.Combine(hash, color);
                }
            }
            return hash;
        }
#endif
    }
}