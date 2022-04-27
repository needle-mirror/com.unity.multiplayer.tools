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
using Unity.Multiplayer.Tools.NetStats;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class CounterVisualElement : VisualElement
    {
        readonly Label m_Label = new();
        readonly Label m_Value = new();

        List<MetricId> m_Stats;
        BaseUnits m_Units;
        bool m_DisplayAsPercentage;

        SmoothingMethod m_SmoothingMethod = SmoothingMethod.ExponentialMovingAverage;
        AggregationMethod m_AggregationMethod = AggregationMethod.Sum;

        double? m_DecayConstant;
        int m_SampleCount;

        int m_SignificantDigits;

        float m_HighlightThresholdMin = float.MinValue;
        float m_HighlightThresholdMax = float.MaxValue;

        double m_DisplayValue;
        internal double DisplayValue
        {
            get => m_DisplayValue;
            set
            {
                m_DisplayValue = value;
                m_Value.text = NumericUtils.ToDisplayNotation(
                    m_DisplayValue,
                    significantDigits: m_SignificantDigits,
                    units: m_Units,
                    displayAsPercentage: m_DisplayAsPercentage);
                UpdateHighlightUssClasses();
            }
        }

        internal CounterVisualElement()
        {
            AddToClassList(UssClassNames.k_DisplayElement);
            AddToClassList(UssClassNames.k_Counter);

            m_Label.AddToClassList(UssClassNames.k_DisplayElementLabel);
            Add(m_Label);

            m_Value.AddToClassList(UssClassNames.k_CounterValue);
            Add(m_Value);
        }

        internal void UpdateConfiguration(DisplayElementConfiguration config)
        {
            var details = config.CounterConfiguration;

            m_Label.text = StringUtils.LabelFormatting(config.Label, config.Stats);
            m_Stats = new List<MetricId>(config.Stats);
            m_Units = UnitUtils.GetUnits(m_Stats, config.Label);
            m_DisplayAsPercentage = UnitUtils.ShouldDisplayAsPercentage(m_Stats, config.Label);
            m_SmoothingMethod = details.SmoothingMethod;
            m_AggregationMethod = details.AggregationMethod;
            m_DecayConstant = config.DecayConstant;
            m_SampleCount = Math.Max(config.SampleCount, 0);
            m_SignificantDigits = Math.Max(details.SignificantDigits, 1);

            m_HighlightThresholdMin = details.HighlightLowerBound;
            m_HighlightThresholdMax = details.HighlightUpperBound;

            UpdateHighlightUssClasses();
        }

        internal void UpdateDisplayData(MultiStatHistory history, double time)
        {
            var isLinearCombination =
                m_AggregationMethod == AggregationMethod.Average ||
                m_AggregationMethod == AggregationMethod.Sum;

            // Potential future work: MTT-1722
            // The current implementation of UpdateDisplayData is only correct for aggregations
            // that are linear combinations, and would need to be revised to support
            // non-linear aggregations like min, max, and median, which require more
            // sophisticated storage and aggregation over time.
            // The reason for this is that you can't calculate a smoothed, combined min, max, or average
            // of multiple variables over time from these variables current Simple Moving Averages
            // or Exponential Moving Averages
            Assert.IsTrue(isLinearCombination);

            var isEma = m_SmoothingMethod == SmoothingMethod.ExponentialMovingAverage;
            var hasDecayConstant = m_DecayConstant != null;
            Assert.IsTrue(isEma == hasDecayConstant);

            var displayValue = 0d;
            var statsFoundCount = 0;
            switch (m_SmoothingMethod)
            {
                case SmoothingMethod.ExponentialMovingAverage:
                {
                    if (!hasDecayConstant)
                    {
                        break;
                    }
                    var decayConstant = m_DecayConstant.Value;
                    foreach (var stat in m_Stats)
                    {
                        if (!history.Data.TryGetValue(stat, out StatHistory statHistory))
                        {
                            continue;
                        }
                        foreach (var cema in statHistory.ContinuousExponentialMovingAverages)
                        {
                            if (cema.DecayConstant == decayConstant)
                            {
                                var metricKind = stat.MetricKind;
                                switch (metricKind)
                                {
                                    case MetricKind.Counter:
                                        displayValue += cema.GetCounterValue(time);
                                        break;
                                    case MetricKind.Gauge:
                                        displayValue += cema.GetGaugeValue();
                                        break;
                                    default:
                                        throw new NotSupportedException($"Unhandled {nameof(MetricKind)} {metricKind}");
                                }
                                statsFoundCount++;
                                break;
                            }
                        }
                    }
                    break;
                }
                case SmoothingMethod.SimpleMovingAverage:
                {
                    foreach (var statName in m_Stats)
                    {
                        var rate = history.GetSimpleMovingAverageRate(statName, m_SampleCount, time);
                        if (!rate.HasValue)
                        {
                            continue;
                        }
                        displayValue += rate.Value;
                        statsFoundCount++;
                    }
                    break;
                }
            }
            if (m_AggregationMethod == AggregationMethod.Average && statsFoundCount > 0)
            {
                displayValue /= statsFoundCount;
            }

            DisplayValue = displayValue;
        }

        void UpdateHighlightUssClasses()
        {
            var belowMin = m_DisplayValue < m_HighlightThresholdMin;
            var aboveMax = m_DisplayValue > m_HighlightThresholdMax;
            EnableInClassList(UssClassNames.k_CounterBelowThreshold, belowMin);
            EnableInClassList(UssClassNames.k_CounterAboveThreshold, aboveMax);
            EnableInClassList(UssClassNames.k_CounterOutOfBounds, belowMin || aboveMax);
        }
    }
}
#endif
