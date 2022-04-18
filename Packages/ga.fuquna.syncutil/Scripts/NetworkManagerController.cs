﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace SyncUtil
{
    [RequireComponent(typeof(SyncNetworkManager))]
    public class NetworkManagerController : MonoBehaviour
    {
        #region TypeDefine
        public enum BootType
        {
            Manual,
            Host,
            Client,
            Server
        }
        #endregion

        public virtual string _networkAddress { get; } = "localhost";
        public virtual int _networkPort { get; } = 7777;
        public virtual BootType _bootType { get; } = BootType.Manual;
        public virtual bool _autoConnect { get; } = true;
        public virtual float _autoConnectInterval { get; } = 10f;


        SyncNetworkManager _networkManager;


        public virtual void Start()
        {
            _networkManager = GetComponent<SyncNetworkManager>();
            Assert.IsTrue(_networkManager);

            if (_bootType != BootType.Manual) StartNetwork(_bootType);
        }

        void StartNetwork(BootType bootType)
        {
            Assert.IsFalse(bootType == BootType.Manual);
            StopAllCoroutines();

            IEnumerator routine = null;
            var mgr = _networkManager;
            switch (bootType)
            {
                case BootType.Host: routine = StartConnectLoop(() => mgr.client != null, () => mgr.StartHost()); break;
                case BootType.Client: routine = StartConnectLoop(() => mgr.client != null, StartClient); break;
                case BootType.Server: routine = StartConnectLoop(() => NetworkServer.active, () => mgr.StartServer()); break;
            }

            StartCoroutine(routine);
        }

        void StartClient()
        {
            _networkManager.onClientError -= OnClientError;
            _networkManager.onClientError += OnClientError;

            _networkManager.StartClient();
        }

        void OnClientError(NetworkConnection conn, int errorCode)
        {
            _networkManager.StopClient();
        }


        IEnumerator StartConnectLoop(Func<bool> isActiveFunc, Action startFunc)
        {
            while (true)
            {
                //yield return new WaitForEndOfFrame(); // Wait for set NetworkManager callback at Start()
                yield return new WaitForSeconds(0.1f);

                UpdateManager();
                if (!isActiveFunc())
                {
                    startFunc();
                }
                yield return new WaitUntil(() => _autoConnect);
                yield return new WaitWhile(isActiveFunc);
                yield return new WaitForSeconds(_autoConnectInterval);
            }
        }

        public virtual void OnGUI()
        {
            if (_networkManager != null && !_networkManager.isNetworkActive)
            {
                GUILayout.Label("SyncUtil Manual Boot");

                var mgr = _networkManager;

                OnGUINetworkSetting();

                mgr.useSimulator = GUILayout.Toggle(mgr.useSimulator, "Use Network Simulator");
                if (mgr.useSimulator)
                {
                    mgr.simulatedLatency = GUIUtil.Slider(mgr.simulatedLatency, 1, 400, "Latency[msec]");
                    mgr.packetLossPercentage = GUIUtil.Slider(mgr.packetLossPercentage, 0f, 20f, "PacketLoss[%]");
                }

                GUILayout.Space(16f);

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
            _networkManager.networkAddress = _networkAddress;
            _networkManager.networkPort = _networkPort;
        }

        GUIUtil.Fold _fold;
        public void DebugMenu()
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("NetworkManagerController");
                GUILayout.Label(SyncNet.isHost ? "Host" : (SyncNet.isServer ? "Server" : (SyncNet.isClient ? "Client" : "StandAlone")));
                if (SyncNet.isActive)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        NetworkManager.Shutdown();
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
                        GUILayout.Label(string.Format("SyncTime: {0:0.000}", SyncNet.time));
                        //GUILayout.Label(string.Format("Network.time Synced/Orig: {0:0.000} / {1:0.000}", SyncNet.networkTime, Network.time));

                        LatencyChecker.Instance._conectionLatencyTable.ToList().ForEach(pair =>
                        {
                            var data = pair.Value;
                            GUILayout.Label(string.Format("ConnId: {0}  Latency: {1:0.000} Average:{2:0.000} " + (data._recieved ? "✔" : ""), pair.Key, data.Last, data.average));
                        });
                    });
                }

                _fold.OnGUI();
            });
        }


        protected virtual void DebugMenuInternal() { }
    }
}
