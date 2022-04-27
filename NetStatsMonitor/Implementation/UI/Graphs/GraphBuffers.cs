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
using Unity.Multiplayer.Tools.NetStatsMonitor.Implementation.Graphing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class GraphBuffers
    {
        public const int k_VerticesPerSample = 2;
        const int k_TrisPerLine = 2;
        const int k_IndicesPerTri = 3;
        const int k_IndicesPerLineSegment = k_TrisPerLine * k_IndicesPerTri;

        public Vertex[] Vertices { get; private set; }
        ushort[] Indices { get; set; }

        public void UpdateConfiguration(
            GraphParameters oldParams,
            GraphParameters newParams,
            List<Color> variableColors)
        {
            var sampleCount = newParams.StatCount * newParams.SamplesPerStat;
            var vertexCount = k_VerticesPerSample * sampleCount;

            var linesSegmentsPerStat = Math.Max(0, newParams.SamplesPerStat - 1);
            var lineSegmentCount = linesSegmentsPerStat * newParams.StatCount;
            var indexCount = k_IndicesPerLineSegment * lineSegmentCount;

            if ((Vertices?.Length ?? 0) != vertexCount)
            {
                Vertices = new Vertex[vertexCount];
            }
            if ((Indices?.Length ?? 0) != indexCount)
            {
                Indices = new ushort[indexCount];
            }
            if (newParams.StatCount != oldParams.StatCount || newParams.SamplesPerStat != oldParams.SamplesPerStat)
            {
                ComputeIndices(newParams.StatCount, newParams.SamplesPerStat);
            }

            // Since the configuration has changed, it's possible the colors have changed
            SetVertexColors(newParams.StatCount, newParams.SamplesPerStat, variableColors);
        }

        void ComputeIndices(int statCount, int samplesPerStat)
        {
            var lineSegmentsPerStat = Math.Max(0, samplesPerStat - 1);
            var indicesPerStat = k_IndicesPerLineSegment * lineSegmentsPerStat;

            var verticesPerStat = k_VerticesPerSample * samplesPerStat;

            for (var statIndex = 0; statIndex < statCount; ++statIndex)
            {
                var statIndicesBegin = statIndex * indicesPerStat;
                var statVerticesBegin = statIndex * verticesPerStat;
                for (var lineSegmentIndex = 0; lineSegmentIndex < lineSegmentsPerStat; ++lineSegmentIndex)
                {
                    var lineSegmentIndicesBegin = statIndicesBegin + k_IndicesPerLineSegment * lineSegmentIndex;
                    var lineSegmentVerticesBegin = statVerticesBegin + k_VerticesPerSample * lineSegmentIndex;

                    // First tri
                    // V0 - V2
                    //  | /
                    // V1   V3
                    Indices[lineSegmentIndicesBegin + 0] = (ushort)(lineSegmentVerticesBegin + 0);
                    Indices[lineSegmentIndicesBegin + 1] = (ushort)(lineSegmentVerticesBegin + 1);
                    Indices[lineSegmentIndicesBegin + 2] = (ushort)(lineSegmentVerticesBegin + 2);

                    // Second tri
                    // V0   V2
                    //    / |
                    // V1 - V3
                    Indices[lineSegmentIndicesBegin + 3] = (ushort)(lineSegmentVerticesBegin + 3);
                    Indices[lineSegmentIndicesBegin + 4] = (ushort)(lineSegmentVerticesBegin + 2);
                    Indices[lineSegmentIndicesBegin + 5] = (ushort)(lineSegmentVerticesBegin + 1);
                }
            }
        }

        void SetVertexColors(int statCount, int samplesPerStat, List<Color> variableColors)
        {
            var verticesPerStat = k_VerticesPerSample * samplesPerStat;
            for (var statIndex = 0; statIndex < statCount; ++statIndex)
            {
                var statVerticesBegin = statIndex * verticesPerStat;
                Color32 statColor = (variableColors != null && statIndex < variableColors.Count)
                    ? variableColors[statIndex]
                    : GraphColorUtils.GetColorForIndex(statIndex, statCount);
                for (var vertexIndex = 0; vertexIndex < verticesPerStat; ++vertexIndex)
                {
                    var vertexIndexAbsolute = statVerticesBegin + vertexIndex;
                    Vertices[vertexIndexAbsolute].tint = statColor;
                }
            }
        }

        public void WriteToMeshGenerationContext(MeshGenerationContext mgc)
        {
            if (Vertices == null ||
                Indices == null ||
                Vertices.Length == 0 ||
                Indices.Length == 0)
            {
                // This can occur if a graph is configured without any stats or with zero samples
                // in which case the buffers are not allocated
                return;
            }
            MeshWriteData mwd = mgc.Allocate(Vertices.Length, Indices.Length);
            mwd.SetAllVertices(Vertices);
            mwd.SetAllIndices(Indices);
        }
    }
}
#endif
