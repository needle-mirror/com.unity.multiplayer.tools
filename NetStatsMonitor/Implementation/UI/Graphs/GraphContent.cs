// RNSM Implementation compilation boilerplate
// All references to UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED should be defined in the same way,
// as any discrepancies are likely to result in build failures
// ---------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR || ((DEVELOPMENT_BUILD && !UNITY_MP_TOOLS_NET_STATS_MONITOR_DISABLED_IN_DEVELOP) || (!DEVELOPMENT_BUILD && UNITY_MP_TOOLS_NET_STATS_MONITOR_ENABLED_IN_RELEASE))
    #define UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED
#endif
// ---------------------------------------------------------------------------------------------------------------------

#if UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class GraphContent : VisualElement
    {
        GraphBuffers m_Buffers = new();
        GraphParameters m_Parameters;
        IGraphRenderer m_Renderer;
        Rect m_GraphContentRect;

        public GraphContent()
        {
            m_GraphContentRect = contentRect;
            generateVisualContent += OnGenerateVisualContent;
        }

        public void UpdateConfiguration(DisplayElementConfiguration config)
        {
            var newParameters = new GraphParameters
            {
                StatCount = config.Stats.Count,
                SamplesPerStat = config.GraphConfiguration.SampleCount,
            };
            m_Buffers.UpdateConfiguration(m_Parameters, newParameters, config.GraphConfiguration.VariableColors);
            m_Parameters = newParameters;
            switch (config.Type)
            {
                case DisplayElementType.LineGraph:
                    if (m_Renderer is not LineGraphRenderer)
                    {
                        m_Renderer = new LineGraphRenderer();
                    }
                    break;
                case DisplayElementType.StackedAreaGraph:
                    if (m_Renderer is not StackedAreaGraphRenderer)
                    {
                        m_Renderer = new StackedAreaGraphRenderer();
                    }
                    break;
            }
            m_Renderer.UpdateConfiguration(config);
        }

        public MinAndMax UpdateDisplayData(
            MultiStatHistory history,
            List<MetricId> stats,
            float minPlotValue,
            float maxPlotValue)
        {
            if (m_Renderer == null)
            {
                return new();
            }
            m_GraphContentRect = contentRect;
            var minAndMaxPlotValue = m_Renderer.UpdateVertices(
                history,
                stats,
                minPlotValue,
                maxPlotValue,
                m_Parameters,
                renderBoundsXMin: m_GraphContentRect.xMin,
                renderBoundsXMax: m_GraphContentRect.xMax,

                // Inverting renderBoundsYMin and renderBoundsYMax
                // since we want our graph from bottom to top
                renderBoundsYMin: m_GraphContentRect.yMax,
                renderBoundsYMax: m_GraphContentRect.yMin,

                m_Buffers.Vertices);
            MarkDirtyRepaint();
            return minAndMaxPlotValue;
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (mgc == null)
            {
                return;
            }
            var currenctRect = contentRect;
            if (m_GraphContentRect != currenctRect)
            {
                RescalePlot();
                m_GraphContentRect = currenctRect;
            }
            m_Buffers.WriteToMeshGenerationContext(mgc);
        }

        void RescalePlot()
        {
            //TODO: MTT-2375 - Handle Graph Resizing Due to Configuration Changes Immediately
        }
    }
}
#endif
