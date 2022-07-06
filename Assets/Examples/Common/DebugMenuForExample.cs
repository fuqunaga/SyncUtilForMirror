using System;
using Mirror;
using UnityEngine;

namespace SyncUtil.Example
{
    public class DebugMenuForExample : MonoBehaviour
    {
        #region Singleton
        
        static DebugMenuForExample _instance;
        public static DebugMenuForExample Instance => (_instance != null ? _instance : (_instance = FindObjectOfType<DebugMenuForExample>()));
        
        #endregion

        private ClientHeartBeat _clientHeartBeat;

        public event Action onGUI;


        void Start()
        {
            _clientHeartBeat = FindObjectOfType<ClientHeartBeat>();
        }

        private void OnGUI()
        {
            if (SyncNet.IsActive)
            {
                CommonMenu();

                onGUI?.Invoke();
            }
        }

        GUIUtil.Fold _fold;
        private void CommonMenu()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("SyncNet");
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


                        if (SyncNet.IsServer && _clientHeartBeat != null)
                        {
                            foreach (var connectionId in NetworkServer.connections.Keys)
                            {
                                var info = _clientHeartBeat.GetHeartBeatInfo(connectionId);
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