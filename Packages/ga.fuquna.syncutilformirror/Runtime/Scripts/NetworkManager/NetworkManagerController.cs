using System;
using System.Collections;
using System.Diagnostics;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace SyncUtil
{
    public class NetworkManagerController : MonoBehaviour
    {
        #region Type Define
        
        public enum BootType
        {
            Manual,
            Host,
            Client,
            Server
        }
        
        #endregion

        public virtual string NetworkAddress => "localhost";
        public virtual int NetworkPort => 7777;
        public virtual BootType Boot => BootType.Manual;
        public virtual bool AutoConnect => true;
        public virtual float AutoConnectInterval => 10f;


        public bool showManualBootMenu = true;
        public bool dontDestroyOnLoad = true;

        public BootType? StartedBootType { get; private set; }
        
        private static SyncNetworkManager NetworkManager => SyncNetworkManager.Singleton;
        

        public virtual void Start()
        {
            CheckWarningAutoConnect();
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (Boot != BootType.Manual) StartNetwork(Boot);
        }

        /// <summary>
        /// For AutoConnect to work
        /// Check NetworkManagerController and NetworkManager are attached different GameObject.
        /// NetworkManager will be destroyed when network is offline even if dontDestroyOnLoad==true
        /// https://github.com/vis2k/Mirror/pull/2582
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        private void CheckWarningAutoConnect()
        {
            var networkManager = Mirror.NetworkManager.singleton; 
            if (!AutoConnect || networkManager == null) return;

            if (!string.IsNullOrEmpty(networkManager.offlineScene))
            {
                if (networkManager.gameObject == gameObject)
                {
                    Debug.LogWarning(
                        $"For AutoConnect to work, attach {nameof(NetworkManagerController)} to a different GameObject than the one {nameof(NetworkManager)} attached.\n" +
                        $"{nameof(NetworkManager)} will be destroyed when the network goes offline even if dontDestroyOnLoad==true");
                }

                if (!dontDestroyOnLoad)
                {
                    Debug.LogWarning(
                        $"For AutoConnect to work, set {nameof(NetworkManagerController)}.{nameof(dontDestroyOnLoad)}==true");
                }
                
            }
        }

        public void StartNetwork(BootType bootType)
        {
            Assert.IsFalse(bootType == BootType.Manual);
            StopAllCoroutines();

            var routine = bootType switch
            {
                BootType.Host => StartConnectLoop(() => NetworkClient.active, () => NetworkManager.StartHost()),
                BootType.Client => StartConnectLoop(() => NetworkClient.active, StartClient),
                BootType.Server => StartConnectLoop(() => NetworkServer.active, () => NetworkManager.StartServer()),
                _ => null
            };

            StartCoroutine(routine);

            StartedBootType = bootType;
        }

        private static void StartClient()
        {
            NetworkManager.onClientError -= OnClientError;
            NetworkManager.onClientError += OnClientError;

            NetworkManager.StartClient();
            
            void OnClientError(TransportError error, string reason)
            {
                NetworkManager.StopClient();
            }
        }


        private IEnumerator StartConnectLoop(Func<bool> isActiveFunc, Action startFunc)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);

                UpdateManager();
                if (!isActiveFunc())
                {
                    startFunc();
                }
                yield return new WaitUntil(() => AutoConnect);
                yield return new WaitWhile(isActiveFunc);
                yield return new WaitForSeconds(AutoConnectInterval);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public virtual void OnGUI()
        {
            if (showManualBootMenu && NetworkManager != null && !NetworkManager.isNetworkActive)
            {
                ManualBootGUI();
            }
        }

        public void ManualBootGUI()
        {
            GUILayout.Label("SyncUtil Manual Boot");

            OnGUINetworkSetting();

            GUILayout.Label("Boot Type (Manual. once only):");
            if (GUILayout.Button("Host (client & server)"))
            {
                OnNetworkStartByManual();
                StartNetwork(BootType.Host);
            }

            if (GUILayout.Button("Client"))
            {
                OnNetworkStartByManual();
                StartNetwork(BootType.Client);
            }

            if (GUILayout.Button("Server"))
            {
                OnNetworkStartByManual();
                StartNetwork(BootType.Server);
            }
        }
        

        protected virtual void OnGUINetworkSetting() { }
        protected virtual void OnNetworkStartByManual() { }

        protected void UpdateManager()
        {
            NetworkManager.networkAddress = NetworkAddress;
            UpdateNetworkPort(NetworkPort);
        }

        protected virtual void UpdateNetworkPort(int port)
        {
            var kcpTransport = Transport.active as KcpTransport;
            if (kcpTransport != null)
            {
                kcpTransport.Port = (ushort)port;
            }
        }
    }
}
