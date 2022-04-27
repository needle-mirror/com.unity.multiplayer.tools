using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// The NetStatsMonitorConfiguration includes all fields required to
    /// configure the contents of the RuntimeNetStatsMonitor
    /// </summary>
    [CreateAssetMenu(
        fileName = "NetStatsMonitorConfiguration",
        menuName = "Multiplayer/NetStatsMonitorConfiguration",
        order = 900)]
    public class NetStatsMonitorConfiguration : ScriptableObject
    {
        /// <summary>
        /// List of elements to be rendered by the <see cref="RunTimeNetStatsMonitor"/>.
        /// </summary>
        [field: SerializeField]
        public List<DisplayElementConfiguration> DisplayElements { get; set; } = new();

        [field: SerializeField]
        internal int? ConfigurationHash { get; private set; } = null;

        /// <summary>
        /// Force a configuration reload.
        /// This needs to be called if the configuration has been modified at runtime
        /// by a script.
        /// </summary>
        public void OnConfigurationModified()
        {
            RecomputeConfigurationHash();
        }

        internal void OnValidate()
        {
            for (var i = 0; i < DisplayElements.Count; ++i)
            {
                if (!DisplayElements[i].FieldsInitialized)
                {
                    DisplayElements[i] = new DisplayElementConfiguration();
                }
                else
                {
                    // A new element in a Reordable list will either be copied from the previous element
                    // or zero initialized if it's the first
                    // In those scenarios, if all the elements have a black color (r=0, g=0, b=0)
                    // And the alpha is also 0, we assume these are new custom colors and we set the alpha to 1
                    var element = DisplayElements[i];

                    if (element.GraphConfiguration?.VariableColors == null)
                    {
                        continue;
                    }

                    var variableColors = element.GraphConfiguration.VariableColors;
                    var areAllColorsZeroInitialized = true;
                    for (int j = 0; j < variableColors.Count; ++j)
                    {
                        var graphConfigurationVariableColor = variableColors[j];
                        if (graphConfigurationVariableColor.a != 0f ||
                            graphConfigurationVariableColor.r != 0f ||
                            graphConfigurationVariableColor.g != 0f ||
                            graphConfigurationVariableColor.b != 0f)
                        {
                            areAllColorsZeroInitialized = false;
                            break;
                        }
                    }

                    if (areAllColorsZeroInitialized)
                    {
                        for (int j = 0; j < variableColors.Count; ++j)
                        {
                            var graphConfigurationVariableColor = variableColors[j];
                            graphConfigurationVariableColor.a = 1f;
                            variableColors[j] = graphConfigurationVariableColor;
                        }
                    }
                }
            }
            RecomputeConfigurationHash();
        }

#if UNITY_2021_2_OR_NEWER // HashCode isn't defined in Unity < 2021.2
        /// Re-computes the configuration hash and stores it in the ConfigurationHash field
        internal void RecomputeConfigurationHash()
        {
            int hashCode = 0;
            foreach (var displayElementConfiguration in DisplayElements)
            {
                hashCode = HashCode.Combine(hashCode, displayElementConfiguration.ComputeHashCode());
            }
            ConfigurationHash = hashCode;
        }
#endif
    }
}