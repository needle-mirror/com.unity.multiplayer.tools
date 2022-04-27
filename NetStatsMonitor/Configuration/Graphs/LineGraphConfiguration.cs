using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Configuration for Line Graph specific options.
    /// </summary>
    [Serializable]
    public sealed class LineGraphConfiguration
    {
        /// <summary>
        /// The line thickness for a line graph.
        /// By default this is set to one.
        /// </summary>
        [field: SerializeField]
        public float LineThickness { get; set; } = 1;

        internal int ComputeHashCode()
        {
            return LineThickness.GetHashCode();
        }
    }
}