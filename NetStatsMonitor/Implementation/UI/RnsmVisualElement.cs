using System;
using System.Collections.Generic;
using Unity.Multiplayer.Tools.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    class RnsmVisualElement : VisualElement
    {
        readonly Label m_Title = new Label();
        readonly VisualElement m_DisplayElementsContainer = new();
        readonly List<CounterVisualElement> m_Counters = new();
        readonly List<GraphVisualElement> m_Graphs = new();
        readonly NoDataReceivedVisualElement m_NoDataReceivedMessage = new();

        EventCallback<GeometryChangedEvent> m_OnGeoChange;
        int m_LastUpdateFrame;
        const float k_EpsilonPixels = 0.5f;

        internal RnsmVisualElement()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(UssClassNames.k_Monitor);

            m_Title.AddToClassList(UssClassNames.k_MonitorTitle);
            m_Title.text = "Runtime Network Stats";
            Add(m_Title);

            m_DisplayElementsContainer.AddToClassList(UssClassNames.k_DisplayElements);
            Add(m_DisplayElementsContainer);
        }

        public void UpdateConfiguration(NetStatsMonitorConfiguration configuration)
        {
            // Begin by removing all children, as they may have been removed, reordered, etc.
            // All existing children that can be reused are reused below, just not necessarily
            // in the same order as they were before
            m_DisplayElementsContainer.Clear();

            if (configuration == null)
            {
                return;
            }

            var countersUsed = 0;
            var graphsUsed = 0;
            foreach (var displayElementConfig in configuration.DisplayElements)
            {
                var type = displayElementConfig.Type;
                switch (type)
                {
                    case DisplayElementType.Counter:
                    {
                        while (countersUsed >= m_Counters.Count)
                        {
                            m_Counters.Add(new CounterVisualElement());
                        }
                        var counter = m_Counters[countersUsed];
                        counter.UpdateConfiguration(displayElementConfig);

                        m_DisplayElementsContainer.Add(counter);
                        countersUsed++;
                        break;
                    }
                    case DisplayElementType.LineGraph:
                    case DisplayElementType.StackedAreaGraph:
                    {
                        while (graphsUsed >= m_Graphs.Count)
                        {
                            m_Graphs.Add(new GraphVisualElement());
                        }
                        var graph = m_Graphs[graphsUsed];

                        graph.UpdateConfiguration(displayElementConfig);

                        m_DisplayElementsContainer.Add(graph);
                        graphsUsed++;
                        break;
                    }
                    default:
                        throw new NotSupportedException(
                            $"Unhandled {nameof(DisplayElementType)} {type}");
                }
            }
            // Remove unused counters and graphs
            if (m_Counters.Count > countersUsed)
            {
                m_Counters.RemoveRange(countersUsed, m_Counters.Count - countersUsed);
            }
            if (m_Graphs.Count > graphsUsed)
            {
                m_Graphs.RemoveRange(graphsUsed, m_Graphs.Count - graphsUsed);
            }
        }

        /// Update the display data with new network data.
        public void UpdateDisplayData(MultiStatHistory stats, EnumMap<SampleRate, bool> newDataAvailable, double time)
        {
            if (m_DisplayElementsContainer.Contains(m_NoDataReceivedMessage))
            {
                m_DisplayElementsContainer.Remove(m_NoDataReceivedMessage);
            }
            foreach (var counter in m_Counters)
            {
                if (newDataAvailable[counter.SampleRate])
                {
                    counter.UpdateDisplayData(stats, time);
                }
            }
            foreach (var graph in m_Graphs)
            {
                if (newDataAvailable[graph.SampleRate])
                {
                    graph.UpdateDisplayData(stats);
                }
            }
        }

        public void DisplayDataNotReceivedMessage(double secondsSinceDataReceived)
        {
            if (!Contains(m_NoDataReceivedMessage))
            {
                m_DisplayElementsContainer.Insert(0, m_NoDataReceivedMessage);
            }
            m_NoDataReceivedMessage.Update(secondsSinceDataReceived);
        }

        public void ApplyPosition(PositionConfiguration positionConfiguration)
        {
            if (m_OnGeoChange != null)
            {
                parent.UnregisterCallback(m_OnGeoChange);
                UnregisterCallback(m_OnGeoChange);
                m_OnGeoChange = null;
            }
            
            if (positionConfiguration.OverridePosition)
            {
                // Using the forced flag to ensure user input is applied.
                ChangePosition(positionConfiguration, true);

                m_OnGeoChange = evt =>
                {
                    if (MathF.Abs(evt.newRect.width - evt.oldRect.width) > k_EpsilonPixels ||
                        MathF.Abs(evt.newRect.height - evt.oldRect.height) > k_EpsilonPixels)
                    {
                        ChangePosition(positionConfiguration);
                    }
                };

                parent.RegisterCallback(m_OnGeoChange);
                RegisterCallback(m_OnGeoChange);
            }
            else
            {
                style.left = StyleKeyword.Null;
                style.top = StyleKeyword.Null;
            }
        }

        void ChangePosition(PositionConfiguration positionConfiguration, bool forced = false)
        {
            // For performance reasons avoid multiple updates per frame but always allow forced updates.
            if (Time.frameCount == m_LastUpdateFrame && !forced)
            {
                return;
            }

            m_LastUpdateFrame = Time.frameCount;

            var parentWidth = parent.contentRect.width;
            var parentHeight = parent.contentRect.height;

            var left = positionConfiguration.PositionLeftToRight * (parentWidth - contentRect.width);
            var top = positionConfiguration.PositionTopToBottom * (parentHeight - contentRect.height);

            style.left = new Length(left, LengthUnit.Pixel);
            style.top = new Length(top, LengthUnit.Pixel);
        }
    }
}
