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
using System.Linq;
using JetBrains.Annotations;

using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    /// A class for storing multiple RNSM stat histories over multiple frames,
    /// to facilitate moving averages and graphs over time
    internal class MultiStatHistory
    {
        [NotNull]
        readonly Dictionary<MetricId, StatHistory> m_Data = new();

        [NotNull]
        internal IReadOnlyDictionary<MetricId, StatHistory> Data => m_Data;

        /// Record sample time-stamps for graphs and counters, so that
        /// we can gracefully handle irregularly-timed metric dispatches.
        [NotNull]
        readonly RingBuffer<double> m_TimeStamps = new(0);

        internal MultiStatHistory(){}

        internal MultiStatHistory(MetricId metricId, StatHistory statHistory)
        {
            m_Data[metricId] = statHistory;
        }

        public void Clear()
        {
            m_Data.Clear();
        }

        public void Collect(StatsAccumulator statsAccumulator, double time)
        {
            m_TimeStamps.PushBack(time);
            foreach (var (metricId, history) in m_Data)
            {
                var collectedValue = statsAccumulator.Collect(metricId);
                history.Update(metricId, collectedValue, time);
            }
            statsAccumulator.LastCollectionTime = time;
        }

        /// Updates the requirements, while preserving all existing data that is still required.
        internal void UpdateRequirements(MultiStatHistoryRequirements requirements)
        {
            var allStatRequirements = requirements.Data;

            // Remove existing data that is no longer required
            var statsToRemove = m_Data.Keys
                .Where(metricId => !allStatRequirements.ContainsKey(metricId))
                .ToList();
            foreach (var statName in statsToRemove)
            {
                m_Data.Remove(statName);
            }

            // Add and update stats according to the requirements
            var maxSampleCount = 0;
            foreach (var (statName, statRequirements) in allStatRequirements)
            {
                maxSampleCount = Math.Max(maxSampleCount, statRequirements.SampleCount);
                if (m_Data.ContainsKey(statName))
                {
                    m_Data[statName].UpdateRequirements(statRequirements);
                }
                else
                {
                    m_Data[statName] = new StatHistory(statRequirements);
                }
            }
            m_TimeStamps.Capacity = maxSampleCount;
        }

        internal double? GetSimpleMovingAverageRate(MetricId metricId, int maxSampleCount, double time)
        {
            if (!Data.TryGetValue(metricId, out StatHistory statHistory))
            {
                return null;
            }

            var sampleCount = Math.Min(maxSampleCount, statHistory.RecentValues.Length);
            if (sampleCount <= 0)
            {
                return null;
            }
            var sampleSum = statHistory.RecentValues.SumLastN(sampleCount);

            var startTime = m_TimeStamps[^(sampleCount - 1)];
            var timeSpan = time - startTime;

            var rate = sampleSum / timeSpan;
            return rate;
        }

        /// The length of the history in seconds
        internal double TimeSpanOfLastNSamples(int sampleCount)
        {
            var validSampleCount = Math.Min(sampleCount, m_TimeStamps.Length);
            if (validSampleCount <= 1)
            {
                return 0;
            }
            var firstTimeStamp = m_TimeStamps[^(validSampleCount - 1)];
            var lastTimeStamp = m_TimeStamps[^1];
            return lastTimeStamp - firstTimeStamp;
        }
    }
}
#endif