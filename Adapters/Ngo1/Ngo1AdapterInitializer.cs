using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Common;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Unity.Multiplayer.Tools.Adapters.Ngo1
{
    static class Ngo1AdapterInitializer
    {
        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeAdapter()
        {
            InitializeAdapterAsync().Forget();
        }

        static async Task InitializeAdapterAsync()
        {
            var networkManager = await GetNetworkManagerAsync();
            var ngo1Adapter = new Ngo1Adapter(networkManager);
            NetworkAdapters.AddAdapter(ngo1Adapter);

#if UNITY_NETCODE_GAMEOBJECTS_2_1_0_ABOVE
            // We need the OnInstantiated callback because the NetworkManager could get destroyed and recreated when we change scenes
            // OnInstantiated is called in Awake, and the GetNetworkManagerAsync only returns at least after OnEnable
            // therefore the initialization is not called twice
            NetworkManager.OnInstantiated += async _ =>
            {
                // We need to wait for the NetworkTickSystem to be ready as well
                var newNetworkManager = await GetNetworkManagerAsync();
                ngo1Adapter.ReplaceNetworkManager(newNetworkManager);
            };
            NetworkManager.OnDestroying += _ =>
            {
                ngo1Adapter.Deinitialize();
            };
#endif

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeStateChange)
            {
                if (playModeStateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                    ngo1Adapter.Deinitialize();
                    NetworkAdapters.RemoveAdapter(ngo1Adapter);
                }
            }
#endif
        }

        static async Task<NetworkManager> GetNetworkManagerAsync()
        {
            while (NetworkManager.Singleton == null || NetworkManager.Singleton.NetworkTickSystem == null)
            {
                await Task.Yield();
            }

            return NetworkManager.Singleton;
        }
    }
}
