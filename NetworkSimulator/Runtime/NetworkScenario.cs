using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
using Unity.Multiplayer.Tools.NetworkSimulator.Runtime.Analytics;
using UnityEditor;
#endif

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    /// <summary>
    /// Base class to implement network scenarios, used to start, pause, and resume their behavior at runtime.
    /// </summary>
    /// <remarks>
    /// This base class should be used for custom scenarios.
    /// If an frame update is desired, <see cref="NetworkScenarioBehaviour"/> instead.
    /// If an asynchronous task-based implementation is desired, <see cref="NetworkScenarioTask"/> instead.
    /// </remarks>
    [Serializable]
    public abstract class NetworkScenario
    {
        INetworkEventsApi m_NetworkEventsApi;

        bool m_Initialized;
        internal bool IsInitialized => m_Initialized;
        
        bool m_HasStarted;
        internal bool HasStarted => m_HasStarted;

        bool m_IsPaused = true;

        /// <summary>
        /// Pause state of the scenario.
        /// Returns true when the scenario is paused, false otherwise.
        /// Set to true to pause, set to false when paused to resume.
        /// </summary>
        public bool IsPaused
        {
            get => m_IsPaused;
            set
            {
                // When not initialized, we don't want to change the state
                // or call any OnPause or OnResume methods
                if (!m_Initialized)
                {
                    return;
                }

                // When setting the same state when the scenario has
                // already started, we don't to trigger OnPause or OnResume
                if (m_IsPaused == value && m_HasStarted)
                {
                    return;
                }

                // When setting paused when already paused, we don't want to trigger
                // OnPause even if the scenario has not started. However, we want to
                // make sure that the user can Resume the scenario even when not paused.
                // This happens when we want to Resume a scenario that has not started
                // due to AutoRun being false
                if (m_IsPaused && value)
                {
                    return;
                }

                m_IsPaused = value;

                if (m_IsPaused)
                {
                    OnPause();
                }
                else
                {
                    if (!m_HasStarted)
                    {
                        m_HasStarted = true;
                        Start(m_NetworkEventsApi);
                    }
                    else
                    {
                        OnResume();
                    }
                }
                
                PauseStateChangedEvent(m_IsPaused);
            }
        }
        
        /// <summary>
        /// Delegate for the <see cref="NetworkScenario.PauseStateChangedEvent"/> event.
        /// </summary>
        internal delegate void PauseStateChangedHandler(bool isPaused);
        
        /// <summary>
        /// Event triggered when the scenario is paused or resumed.
        /// </summary>
        internal event PauseStateChangedHandler PauseStateChangedEvent = delegate { };

        internal void InitializeScenario(INetworkEventsApi networkEventsApi, bool autoRun)
        {
            if (m_HasStarted)
            {
                return;
            }

            Analytic(autoRun);
            
            m_NetworkEventsApi = networkEventsApi;
            m_Initialized = true;

            if (autoRun)
            {
                m_HasStarted = true;
                Start(networkEventsApi);
                IsPaused = false;
            }
        }

        void Analytic(bool autoRun)
        {
#if UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            ScenarioClassType scenarioClassType;
            switch (this)
            {
                case NetworkScenarioBehaviour:
                    scenarioClassType = ScenarioClassType.NetworkScenarioBehaviour;
                    break;
                case NetworkScenarioTask:
                    scenarioClassType = ScenarioClassType.NetworkScenarioTask;
                    break;
                default:
                    scenarioClassType = ScenarioClassType.NetworkScenario;
                    break;
            }
            EditorAnalytics.SendAnalytic(new ScenarioInitializedAnalytic(autoRun, scenarioClassType));
#endif
        }

        /// <summary>
        /// Starts running the underlying network scenario.
        /// </summary>
        /// <param name="networkEventsApi">API to trigger network simulation events.</param>
        public abstract void Start(INetworkEventsApi networkEventsApi);

        /// <summary>
        /// Disposes the scenario.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Implement to define custom behaviour to be called when the scenario is paused.
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// Implement to define custom behaviour to be called when the scenario is resumed.
        /// </summary>
        protected virtual void OnResume() { }
    }

    /// <summary>
    /// Base class to use a MonoBehaviour-style frame update loop with network scenarios.
    /// </summary>
    public abstract class NetworkScenarioBehaviour : NetworkScenario
    {
        internal void UpdateScenario(float deltaTime)
        {
            if (IsPaused)
            {
                return;
            }

            Update(deltaTime);
        }

        /// <summary>
        /// Method called on every frame to determine what the scenario should run since the last frame.
        /// Only called when the scenario is not paused, so there is no need to manually check it in
        /// the implementation.
        /// </summary>
        /// <param name="deltaTime">The time elapsed since last frame.</param>
        protected abstract void Update(float deltaTime);
    }

    /// <summary>
    /// Base class to use network scenarios with asynchronous Tasks.
    /// </summary>
    public abstract class NetworkScenarioTask : NetworkScenario
    {
        readonly CancellationTokenSource m_Cancellation = new();

        /// <inheritdoc />
        public override void Start(INetworkEventsApi networkEventsApi)
        {
            Run(networkEventsApi, m_Cancellation.Token).Forget();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (m_Cancellation.IsCancellationRequested == false)
            {
                m_Cancellation.Cancel();
            }

            m_Cancellation?.Dispose();
        }

        /// <summary>
        /// Starts running the underlying network scenario.
        /// Make sure to handle when <see cref="NetworkScenario.IsPaused"/> and the cancellation token.
        /// </summary>
        /// <param name="networkEventsApi">API to trigger network simulation events.</param>
        /// <param name="cancellationToken">Cancellation token to handle cancellation requests
        /// to the underlying task.</param>
        /// <returns>Task simulating the scenario</returns>
        protected abstract Task Run(INetworkEventsApi networkEventsApi, CancellationToken cancellationToken);
    }
}
