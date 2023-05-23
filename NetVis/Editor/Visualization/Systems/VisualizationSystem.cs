﻿using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class VisualizationSystem : IDisposable
    {
        // External dependencies
        //---------------------------------------------------------------------
        readonly NetVisConfigurationWithEvents m_ConfigurationWithEvents;
        NetVisConfiguration Configuration => m_ConfigurationWithEvents.Configuration;
        readonly NetVisDataStore m_DataStore;
        readonly IRuntimeUpdater m_RuntimeUpdater;

        // Members
        //---------------------------------------------------------------------
        [MaybeNull]
        MeshShading m_MeshShading;

        [MaybeNull]
        TextOverlay m_TextOverlay;

        internal VisualizationSystem(
            NetVisConfigurationWithEvents configurationWithEvents,
            NetVisDataStore netVisDataStore,
            IRuntimeUpdater runtimeUpdater)
        {
            DebugUtil.TraceMethodName();

            m_ConfigurationWithEvents = configurationWithEvents;
            m_DataStore = netVisDataStore;
            m_RuntimeUpdater = runtimeUpdater;

            m_ConfigurationWithEvents.ConfigurationChanged += OnConfigurationChanged;
            m_RuntimeUpdater.OnLateUpdate += OnLateUpdate;
            EditorApplication.pauseStateChanged += OnPauseStateChanged;

            OnConfigurationChanged(configurationWithEvents.Configuration);
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();

            m_ConfigurationWithEvents.ConfigurationChanged -= OnConfigurationChanged;

            m_RuntimeUpdater.OnLateUpdate -= OnLateUpdate;
            EditorApplication.pauseStateChanged -= OnPauseStateChanged;

            m_MeshShading?.Dispose();
            m_MeshShading = null;

            m_TextOverlay?.Dispose();
            m_TextOverlay = null;
        }

        void OnPauseStateChanged(PauseState pauseState)
        {
            DebugUtil.TraceMethodName();
            m_DataStore.OnPauseStateChanged(pauseState);
            switch (pauseState)
            {
                case PauseState.Paused:
                    // Must update the colors in response to the datastore update
                    m_MeshShading?.UpdateObjectColors();
                    break;
                case PauseState.Unpaused:
                    // No further action required, the colors will be updated in the next OnLateUpdate
                    break;
            }
        }

        void OnLateUpdate()
        {
            m_MeshShading?.UpdateObjectColors();
        }

        void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            DebugUtil.TraceMethodName();

            m_DataStore.OnConfigurationChanged(configuration);

            Debug.Assert(Application.isPlaying);

            var metricSelected = configuration.Metric != NetVisMetric.None;
            var meshShadingRequired = metricSelected && configuration.MeshShadingEnabled;
            var textOverlayRequired = metricSelected && configuration.TextOverlayEnabled;

            if (meshShadingRequired)
            {
                if (m_MeshShading == null)
                {
                    m_MeshShading = new(Configuration, m_DataStore);
                }
                else
                {
                    m_MeshShading.OnConfigurationChanged(Configuration);
                }
            }
            else
            {
                m_MeshShading?.Dispose();
                m_MeshShading = null;
            }

            if (textOverlayRequired)
            {
                if (m_TextOverlay == null)
                {
                    m_TextOverlay = new(Configuration, m_DataStore);
                }
                else
                {
                    m_TextOverlay.OnConfigurationChanged(Configuration);
                }
            }
            else
            {
                m_TextOverlay?.Dispose();
                m_TextOverlay = null;
            }
        }
    }
}