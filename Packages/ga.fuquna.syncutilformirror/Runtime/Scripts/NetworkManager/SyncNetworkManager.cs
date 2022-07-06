using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace SyncUtil
{
    [Serializable]
    [ExecuteAlways]
    public class SyncNetworkManager : NetworkManagerWithHookAction
    {
        public static SyncNetworkManager Singleton => singleton as SyncNetworkManager;

        public bool enableLogServer = false;
        public bool enableLogClient = true;

        
#if UNITY_EDITOR
        [HideInInspector] public bool checkPlayerPrefab = true;


        public override void Start()
        {
            base.Start();
            CheckPlayerPrefab();
        }


        void CheckPlayerPrefab()
        {
            if (Application.isPlaying) return;
            
            StartCoroutine(CheckPlayerPrefabCoroutine());
        }
        
        IEnumerator CheckPlayerPrefabCoroutine()
        {
            if (playerPrefab == null && checkPlayerPrefab)
            {
                // EditorGUIUtility.PingObject(this);
                Selection.activeObject = this;

                yield return null;
                
                var response = EditorUtility.DisplayDialogComplex(
                    $"{nameof(SyncNetworkManager)} scene:[{gameObject.scene.name}]",
                    "Mirror requires a PlayerPrefab to spawn objects.\n\nDo you want to set the SyncUtil's default PlayerPrefab to the SyncNetworkManager\nand turn on AutoCreatePlayer?",
                    "Ok",
                    "Cancel",
                    "Don't ask again"
                );

                switch (response)
                {
                    case 0:
                        var guids = AssetDatabase.FindAssets("EmptyPlayer", new[] {"Packages/ga.fuquna.syncutilformirror"});
                
                        foreach (var path in guids.Select(AssetDatabase.GUIDToAssetPath))
                        {
                            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            if (go != null)
                            {
                                playerPrefab = go;
                                autoCreatePlayer = true;
                                
                                EditorUtility.SetDirty(this);
                                break;
                            }
                        }
                        break;
                    
                    case 2:
                        checkPlayerPrefab = false;
                        break;
                }
            }
        }
#endif
        

        #region Server side

        public override void OnStartServer()
        {
            if (enableLogServer) Log("Server networking logic is starting");
            base.OnStartServer();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (enableLogServer) Log($"Server connection ID: {conn.connectionId}  has connected to the server");
            base.OnServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (enableLogServer) Log($"Server Disconeect  connection ID {conn.connectionId}");
            base.OnServerDisconnect(conn);
        }

        #endregion


        #region Client side

        public override void OnStartClient()
        {
            if (enableLogClient) Log("Client networking logic is starting");
            base.OnStartClient();
        }

        public override void OnClientConnect()
        {
            if (enableLogClient) Log($"Client connection ID: {NetworkClient.connection.connectionId}  has connected to the server");
            base.OnClientConnect();
        }


        public override void OnClientError(Exception exception)
        {
            if (enableLogClient) LogError($"Client Error: {exception.Message}");
            base.OnClientError(exception);
        }

        public override void OnClientDisconnect()
        {
            if (enableLogClient) Log($"Client Disconnect  connection ID {NetworkClient.connection.connectionId}");
            base.OnClientDisconnect();
        }

        #endregion


        protected virtual void Log(string log)
        {
            Debug.Log($"{DateTime.Now} {log}");
        }

        protected virtual void LogError(string log)
        {
            Debug.LogError($"{DateTime.Now} {log}");
        }
    }
}