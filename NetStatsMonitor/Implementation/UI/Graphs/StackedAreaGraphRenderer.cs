// RNSM Implementation compilation boilerplate
// All references to UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED should be defined in the same way,
// as any discrepancies are likely to result in build failures
// ---------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR || ((DEVELOPMENT_BUILD && !UNITY_MP_TOOLS_NET_STATS_MONITOR_DISABLED_IN_DEVELOP) || (!DEVELOPMENT_BUILD && UNITY_MP_TOOLS_NET_STATS_MONITOR_ENABLED_IN_RELEASE))
    #define UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED
#endif
// ---------------------------------------------------------------------------------------------------------------------

#if UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class StackedAreaGraphRenderer : IGraphRenderer
    {
        /// Sums used to "stack" the graphs
        /// Stored as a member variable rather than a local to continually reuse
        /// the buffer and reduce the number of allocations
        float[] m_SampleSums;

        public void UpdateConfiguration(DisplayElementConfiguration config)
        {
            var sampleCount = config.GraphConfiguration.SampleCount;
            if ((m_SampleSums?.Length ?? 0) != sampleCount)
            {
                m_SampleSums = new float[sampleCount];
            }
        }

        public MinAndMax UpdateVertices(
            MultiStatHistory history,
            List<MetricId> stats,
            float yAxisMin,
            float yAxisMax,
            GraphParameters graphParams,
            float renderBoundsXMin,
            float renderBoundsXMax,
            float renderBoundsYMin,
            float renderBoundsYMax,
            Vertex[] vertices)
        {
            var xScale = (renderBoundsXMax - renderBoundsXMin) / (graphParams.SamplesPerStat - 1);
            var yScale = (renderBoundsYMax - renderBoundsYMin) / yAxisMax;

            var verticesPerStat = GraphBuffers.k_VerticesPerSample * graphParams.SamplesPerStat;

            // Clear the SampleSums buffer which is not reallocated between frames
            Array.Clear(m_SampleSums, 0, m_SampleSums.Length);

            var yValueMax = 0f;

            for (var statIndex = 0; statIndex < graphParams.StatCount; ++statIndex)
            {
                var statId = stats[statIndex];
                var statData = history.Data[statId].RecentValues;
                var statVerticesBegin = statIndex * verticesPerStat;

                for (var sampleIndex = 0; sampleIndex < graphParams.SamplesPerStat; ++sampleIndex)
                {
                    var sampleVerticesBegin = statVerticesBegin + GraphBuffers.k_VerticesPerSample * sampleIndex;

                    var sampleValue = statData.GetValueOrDefault(sampleIndex);

                    var prevSum = m_SampleSums[sampleIndex];
                    var nextSumUnclamped = prevSum + sampleValue;

                    yValueMax = MathF.Max(nextSumUnclamped, yValueMax);

                    // Clamp the sum to avoid drawing off of the plot
                    var nextSum = MathF.Min(nextSumUnclamped, yAxisMax);

                    m_SampleSums[sampleIndex] = nextSum;

                    var xValue      = sampleIndex * xScale + renderBoundsXMin;
                    var yValueBelow = prevSum     * yScale + renderBoundsYMin;
                    var yValueAbove = nextSum     * yScale + renderBoundsYMin;

                    vertices[sampleVerticesBegin + 1].position = new Vector3(xValue, yValueAbove);
                    vertices[sampleVerticesBegin + 0].position = new Vector3(xValue, yValueBelow);
                }
            }
            return new MinAndMax { Min = 0f, Max = yValueMax };
        }
    }
}
#endif
