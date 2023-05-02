using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetVis.Configuration;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class TextOverlay : IDisposable
    {
        [NotNull]
        NetVisConfiguration m_Configuration;

        [NotNull]
        readonly NetVisDataStore m_NetVisDataStore;

        static GUIStyle s_TextLabelStyle;

        public TextOverlay(
            [NotNull] NetVisConfiguration configuration,
            [NotNull] NetVisDataStore netVisDataStore)
        {
            DebugUtil.TraceMethodName();
            m_Configuration = configuration;
            m_NetVisDataStore = netVisDataStore;
            SceneView.duringSceneGui += DuringSceneGui;
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();
            SceneView.duringSceneGui -= DuringSceneGui;
        }

        public void OnConfigurationChanged(NetVisConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        void DuringSceneGui(SceneView sceneView)
        {
            InitializeTextLabelStyle();

            var sceneViewCamera = sceneView.camera;
            var sceneViewCameraPosition = sceneViewCamera.transform.position;
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);

            foreach (var id in m_NetVisDataStore.GetObjectIds())
            {
                var gameObject = m_NetVisDataStore.GetGameObject(id);
                if (gameObject == null)
                {
                    continue;
                }

                var objectPosition = GetObjectPosition(gameObject);
                if (!ObjectIsVisibleToCamera(sceneViewCamera, sceneViewCameraPosition, screenRect, gameObject, objectPosition))
                {
                    continue;
                }

                var content = m_Configuration.Metric switch
                {
                    NetVisMetric.Bandwidth => m_NetVisDataStore.GetBandwidth(id).ToString("N0"),
                    NetVisMetric.Ownership => m_NetVisDataStore.GetOwner(id).ToString(),
                    _ => string.Empty,
                };

                Handles.Label(objectPosition, content, s_TextLabelStyle);
            }
        }

        // We can't assign this in the constructor, or Unity will warn us that we can
        // only use GUI functions inside OnGui
        void InitializeTextLabelStyle()
        {
            if (s_TextLabelStyle == null)
            {
                s_TextLabelStyle = new()
                {
                    padding = new RectOffset(2, 0, 0, 0),
                    normal =
                    {
                        textColor = Color.black,
                    }
                };
            }

            // For some reason the background texture is being reset to null in BossRoom when the scene view is
            // initially open. We're unsure as to why this happens, as it is not occurring in projects other than
            // BossRoom, or when the scene view is not initially open.
            // To work around this we can continue to re-create the background texture field whenever it is null.
            if (s_TextLabelStyle.normal.background == null)
            {
                // Unexpectedly, this produces a white background
                var backgroundTexture = new Texture2D(1, 1);
                backgroundTexture.SetPixels(new[] { Color.black });
                s_TextLabelStyle.normal.background = backgroundTexture;
            }
        }

        bool ObjectIsVisibleToCamera(
            Camera camera,
            Vector3 cameraPosition,
            Rect screenRect,
            GameObject targetObject,
            Vector3 targetPosition)
        {
            return screenRect.Contains(camera.WorldToScreenPoint(targetPosition)) &&
                   Physics.Raycast(cameraPosition, targetPosition - cameraPosition, out var hit) &&
                   hit.transform == targetObject.transform;
        }

        Vector3 GetObjectPosition(GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.center;
            }

            var collider = gameObject.GetComponent<Collider>();
            if (collider!= null)
            {
                return collider.bounds.center;
            }

            return gameObject.transform.position;
        }
    }
}
