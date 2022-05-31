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

        public virtual string NetworkAddress { get; } = "localhost";
        public virtual int NetworkPort { get; } = 7777;
        public virtual BootType Boot { get; } = BootType.Manual;
        public virtual bool AutoConnect { get; } = true;
        public virtual float AutoConnectInterval { get; } = 10f;


        public ClientHeartBeat clientHeartBeat;
        
        SyncNetworkManager _networkManager;
        private KcpTransport _kcp;
        

        public virtual void Start()
        {
            _networkManager = GetComponent<SyncNetworkManager>();
            _kcp = GetComponent<KcpTransport>();
            clientHeartBeat ??= GetComponent<ClientHeartBeat>();

            if (Boot != BootType.Manual) StartNetwork(Boot);
        }

        void StartNetwork(BootType bootType)
        {
            Assert.IsFalse(bootType == BootType.Manual);
            StopAllCoroutines();

            IEnumerator routine = bootType switch
            {
                BootType.Host => StartConnectLoop(() => NetworkClient.active, () => _networkManager.StartHost()),
                BootType.Client => StartConnectLoop(() => NetworkClient.active, StartClient),
                BootType.Server => StartConnectLoop(() => NetworkServer.active, () => _networkManager.StartServer()),
                _ => null
            };

            StartCoroutine(routine);
        }

        void StartClient()
        {
            _networkManager.onClientError -= OnClientError;
            _networkManager.onClientError += OnClientError;

            _networkManager.StartClient();
        }

        void OnClientError(Exception _)
        {
            _networkManager.StopClient();
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
        }

        public virtual void OnGUI()
        {
            if (_networkManager != null && !_networkManager.isNetworkActive)
            {
                GUILayout.Label("SyncUtil Manual Boot");

                OnGUINetworkSetting();

#if false
                var mgr = _networkManager;

                mgr.useSimulator = GUILayout.Toggle(mgr.useSimulator, "Use Network Simulator");
                if (mgr.useSimulator)
                {
                    mgr.simulatedLatency = GUIUtil.Slider(mgr.simulatedLatency, 1, 400, "Latency[msec]");
                    mgr.packetLossPercentage = GUIUtil.Slider(mgr.packetLossPercentage, 0f, 20f, "PacketLoss[%]");
                }
                
                GUILayout.Space(16f);
#endif

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
        }

        protected virtual void OnGUINetworkSetting() { }
        protected virtual void OnNetworkStartByManual() { }

        protected void UpdateManager()
        {
            _networkManager.networkAddress = NetworkAddress;
            UpdateNetworkPort(NetworkPort);
        }

        protected virtual void UpdateNetworkPort(int port)
        {
            if (_kcp != null)
            {
                _kcp.Port = (ushort)port;
            }
        }

        GUIUtil.Fold _fold;
        public void DebugMenu()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("NetworkManagerController");
                GUILayout.Label(SyncNet.IsHost ? "Host" : (SyncNet.IsServer ? "Server" : (SyncNet.IsClient ? "Client" : "StandAlone")));
                if (SyncNet.IsActive)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        NetworkManager.singleton.StopHost();
                    }
                }
            }

            GUIUtil.Indent(() =>
            {

                DebugMenuInternal();

                if (_fold == null)
                {
                    _fold = new GUIUtil.Fold("Time Debug", () =>
                    {
                        GUILayout.Label($"{nameof(SyncNet)}.{nameof(SyncNet.Time)}: {SyncNet.Time:0.000}");
                        GUILayout.Label($"{nameof(SyncNet)}.{nameof(SyncNet.NetworkTime)}: {SyncNet.NetworkTime:0.000}");


                        if (SyncNet.IsServer && clientHeartBeat != null)
                        {
                            foreach (var connectionId in NetworkServer.connections.Keys)
                            {
                                var info = clientHeartBeat.GetHeartBeatInfo(connectionId);
                                if (info != null)
                                {
                                    GUILayout.Label($"ConnId: {connectionId}  {info}");
                                }
                            }
                        }
                    });
                }

                _fold.OnGUI();
            });
        }


        protected virtual void DebugMenuInternal() { }
    }
}
