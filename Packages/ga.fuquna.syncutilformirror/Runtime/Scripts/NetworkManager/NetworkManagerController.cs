using System;
using System.Collections;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;

namespace SyncUtil
{
    [RequireComponent(typeof(SyncNetworkManager))]
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
       
        protected SyncNetworkManager networkManager;
        private KcpTransport _kcp;
        

        public virtual void Start()
        {
            networkManager = GetComponent<SyncNetworkManager>();
            _kcp = GetComponent<KcpTransport>();

            if (Boot != BootType.Manual) StartNetwork(Boot);
        }

        public void StartNetwork(BootType bootType)
        {
            Assert.IsFalse(bootType == BootType.Manual);
            StopAllCoroutines();

            IEnumerator routine = bootType switch
            {
                BootType.Host => StartConnectLoop(() => NetworkClient.active, () => networkManager.StartHost()),
                BootType.Client => StartConnectLoop(() => NetworkClient.active, StartClient),
                BootType.Server => StartConnectLoop(() => NetworkServer.active, () => networkManager.StartServer()),
                _ => null
            };

            StartCoroutine(routine);
        }

        void StartClient()
        {
            networkManager.onClientError -= OnClientError;
            networkManager.onClientError += OnClientError;

            networkManager.StartClient();
        }

        void OnClientError(Exception _)
        {
            networkManager.StopClient();
        }


        IEnumerator StartConnectLoop(Func<bool> isActiveFunc, Action startFunc)
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
            if (showManualBootMenu && networkManager != null && !networkManager.isNetworkActive)
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
            networkManager.networkAddress = NetworkAddress;
            UpdateNetworkPort(NetworkPort);
        }

        protected virtual void UpdateNetworkPort(int port)
        {
            if (_kcp != null)
            {
                _kcp.Port = (ushort)port;
            }
        }
    }
}
