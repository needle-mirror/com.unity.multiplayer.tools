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
    internal class LineGraphRenderer : IGraphRenderer
    {
        internal float LineThickness { get; set; }

        public void UpdateConfiguration(DisplayElementConfiguration config)
        {
            LineThickness = config?.GraphConfiguration?.LineGraphConfiguration?.LineThickness ?? 1;
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
            // Example Inputs and Outputs 1
            // - Input points: P0, P1, P2, P3
            // - Output vertices: V00, V01, V10, V11, V20, V21, V30, V31
            //
            //        P0                                                            P3
            //  V00 *  *  * V01                                              V30 *  *  * V31
            //       \     \                                                    /     /
            //        \     \                                                  /     /
            //         \     \                                                /     /
            //          \     \                                              /     /
            //           \     \ V10                                    V20 /     /
            //            \     *------------------------------------------*     /
            //             \  * P1                                        P2 *  /
            //              *--------------------------------------------------*
            //            V11                                                  V21


            // Example Inputs and Outputs 2
            // - Input points: P0, P1, P2, P3
            // - Output vertices: V00, V01, V10, V11, V20, V21, V30, V31, V40, V41
            //
            //                     V10                       V30
            //  V00 *--------------*                           *------------------* V40
            //      * P0      P1 *  \                         /  * P3          P4 *
            //  V01 *----------*     \                       /     *--------------* V41
            //              V11 \     \                     /     / V30
            //                   \     \                   /     /
            //                    \     \                 /     /
            //                     \     \               /     /
            //                      \     \             /     /
            //                       \     \           /     /
            //                        \     \         /     /
            //                         \     \       /     /
            //                          \     \ V20 /     /
            //                           \     \   /     /
            //                            \     \ /     /
            //                             \     *     /
            //                              \         /
            //                               \   P2  /
            //                                \  *  /
            //                                 \   /
            //                                  \ /
            //                                   *
            //                                  V21

            if (graphParams.SamplesPerStat < 2)
            {
                return new MinAndMax();
            }
            if (LineThickness <= 0f)
            {
                return new MinAndMax();
            }

            // This terminology is a bit confusing, but:
            // 1. renderBoundsYMin is the render bound corresponding to the minimum value
            // 2. renderBoundsYMinActual is the actual minimum render bound
            var renderBoundsYMinActual = Math.Min(renderBoundsYMin, renderBoundsYMax);
            var renderBoundsYMaxActual = Math.Max(renderBoundsYMin, renderBoundsYMax);

            var halfLineThickness = 0.5f * LineThickness;

            var xScale = (renderBoundsXMax - renderBoundsXMin) / (graphParams.SamplesPerStat - 1);
            var yScale = (renderBoundsYMax - renderBoundsYMin) / yAxisMax;

            var sampleValueMin = 0f;
            var sampleValueMax = 0f;
            float GetSample(RingBuffer<float> values, int i)
            {
                var sampleUnclamped = values.GetValueOrDefault(i);
                sampleValueMin = MathF.Min(sampleValueMin, sampleUnclamped);
                sampleValueMax = MathF.Max(sampleValueMax, sampleUnclamped);
                var sampleClamped = MathF.Min(sampleUnclamped, yAxisMax);
                return sampleClamped;
            }

            var xScaleInverse = 1f / xScale;

            var verticesPerStat = GraphBuffers.k_VerticesPerSample * graphParams.SamplesPerStat;
            for (var statIndex = 0; statIndex < graphParams.StatCount; ++statIndex)
            {
                var statId = stats[statIndex];
                var statData = history.Data[statId].RecentValues;
                var statVerticesBegin = statIndex * verticesPerStat;

                // Values shared between iterations of the loop
                // At the end of each iteration, x1 becomes x2, y1 becomes y2, a01 becomes a12, and so on
                int i1;
                float x1;
                float y1;
                float a01;
                float b01a;
                float b01_delta;

                {
                    // Generating vertices for the zeroth point is a special case
                    // that must be handled outside of the main loop because:
                    //   1. There is no preceding point to connect to
                    //   2. There is no information about the zeroth point from the previous iteration

                    var i0 = 0; // The index of the left point in the current iteration
                    var sample0 = GetSample(statData, i0);
                    var x0 = 0f;
                    var y0 = sample0 * yScale + renderBoundsYMin;

                    i1 = 1; // The index of the center point in the current iteration
                    var sample1 = GetSample(statData, i1);
                    x1 = xScale;
                    y1 = sample1 * yScale + renderBoundsYMin;

                    // Definitions of names used only in comments:
                    // Let P0 = (x0, y0)
                    // Let P1 = (x1, y1)
                    // Let L01 be the line between P0 and P1
                    // Let L01a be the line offset from L01 by half the thickness to the left
                    // Let L01b be the line offset from L01 by half the thickness to the right

                    a01 = (y1 - y0) * xScaleInverse; // slope  of L01, according to y = a * x + b
                    var b01 = y0;                    // offset of L01, according to y = a * x + b

                    // Vertical offset between L01 and L01a
                    b01_delta = halfLineThickness * MathF.Sqrt(1 + a01 * a01);

                    b01a = b01 + b01_delta; // b-value of L01a

                    var p0VerticesBegin = statVerticesBegin;

                    var y0a = Math.Clamp(y0 + b01_delta, renderBoundsYMinActual, renderBoundsYMaxActual);
                    var y0b = Math.Clamp(y0 - b01_delta, renderBoundsYMinActual, renderBoundsYMaxActual);

                    vertices[p0VerticesBegin + 0].position = new Vector3(x0, y0a);
                    vertices[p0VerticesBegin + 1].position = new Vector3(x0, y0b);
                }

                for (; i1 < graphParams.SamplesPerStat - 1; ++i1)
                {
                    var i2 = i1 + 1; // The index of the right point in the current iteration
                    var sample2 = GetSample(statData, i2);
                    var x2 = i2 * xScale;
                    var y2 = sample2 * yScale + renderBoundsYMin;

                    // Definitions of names used only in comments:
                    // Let P2 = (x2, y2)
                    // Let L12 be the line between P1 and P2
                    // Let L12a be the line offset from L12 by half the thickness to the left
                    // Let L12b be the line offset from L12 by half the thickness to the right

                    var a12 = (y2 - y1) * xScaleInverse; //  slope of L12, according to y = a * x + b
                    var b12 = y2 - a12 * x2;             // offset of L12, according to y = a * x + b

                    // Vertical offset between L01 and L01a
                    var b12_delta = halfLineThickness * MathF.Sqrt(1 + a12 * a12);

                    var b12a = b12 + b12_delta; // b-value of L12a

                    // x-value of the intersection between L01a and L12a
                    var x1a = MathF.Abs(a01 - a12) < 1e-4f
                        ? (x1 - a12 / b12_delta) // Avoid division by zero if a01 ~= a12
                        : (b12a - b01a) / (a01 - a12);

                    // y-value of the intersection between L01a and L12a
                    var y1a = x1a * a12 + b12a;

                    // We can reflect the vertex (x1a, y1a) over the (x1, y1)
                    // to find the corresponding vertex (x1b, y1b) without needing
                    // to compute the intersection between L01b and L12b directly
                    var x1b = 2 * x1 - x1a; // x-value of the intersection between L01b and L12b
                    var y1b = 2 * y1 - y1a; // y-value of the intersection between L01b and L12b

                    // Clamp the vertex yValues
                    y1a = Math.Clamp(y1a, renderBoundsYMinActual, renderBoundsYMaxActual);
                    y1b = Math.Clamp(y1b, renderBoundsYMinActual, renderBoundsYMaxActual);

                    var p1VerticesBegin = statVerticesBegin + i1 * GraphBuffers.k_VerticesPerSample;
                    vertices[p1VerticesBegin + 0].position = new Vector3(x1a, y1a);
                    vertices[p1VerticesBegin + 1].position = new Vector3(x1b, y1b);

                    x1 = x2;
                    y1 = y2;
                    a01 = a12;
                    b01a = b12a;
                    b01_delta = b12_delta;
                }

                {
                    // Generating vertices for the last point is a special case
                    // that must be handled outside of the main loop because
                    // there is no next point to connect to.

                    var y1a = Math.Clamp(y1 + b01_delta, renderBoundsYMinActual, renderBoundsYMaxActual);
                    var y1b = Math.Clamp(y1 - b01_delta, renderBoundsYMinActual, renderBoundsYMaxActual);

                    var p1VerticesBegin = statVerticesBegin + i1 * GraphBuffers.k_VerticesPerSample;
                    vertices[p1VerticesBegin + 0].position = new Vector3(x1, y1a);
                    vertices[p1VerticesBegin + 1].position = new Vector3(x1, y1b);
                }
            }
            return new MinAndMax(sampleValueMin, sampleValueMax);
        }
    }
}
#endif
