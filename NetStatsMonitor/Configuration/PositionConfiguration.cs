using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Configuration for the position of the <see cref="RuntimeNetStatsMonitor"/> on screen
    /// </summary>
    [Serializable]
    public class PositionConfiguration
    {
        /// <summary>
        /// If enabled, the position here will override the position set by the USS styling.
        /// Disable this options if you would like to use the position from the USS styling
        /// instead.
        /// </summary>
        [field: Tooltip(
        "If enabled, the position here will override the position set by the USS styling. " +
        "Disable this options if you would like to use the position from the USS styling " +
        "instead.")]
        [field: SerializeField]
        public bool OverridePosition { get; set; } = true;

        /// <summary>
        /// The position of the Net Stats Monitor from left to right in the range from 0 to 1.
        /// 0 is flush left, 0.5 is centered, and 1 is flush right.
        /// </summary>
        [field: Tooltip(
            "The position of the Net Stats Monitor from left to right in the range from 0 to 1. " +
            "0 is flush left, 0.5 is centered, and 1 is flush right.")]
        [field: Range(0, 1)]
        [field: SerializeField]
        public float PositionLeftToRight { get; set; } = 0;

        /// <summary>
        /// The position of the Net Stats Monitor from top to bottom in the range from 0 to 1.
        /// 0 is flush to the top, 0.5 is centered, and 1 is flush to the bottom.
        /// </summary>
        [field: Tooltip(
            "The position of the Net Stats Monitor from top to bottom in the range from 0 to 1." +
            "0 is flush to the top, 0.5 is centered, and 1 is flush to the bottom.")]
        [field: Range(0, 1)]
        [field: SerializeField]
        public float PositionTopToBottom { get; set; } = 0;
    }
}