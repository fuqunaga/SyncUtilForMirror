using System;
using System.Collections;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SyncUtil
{
    [Serializable]
    public class SyncNetworkManager : NetworkManagerWithHookAction
    {
        public static SyncNetworkManager Singleton => singleton as SyncNetworkManager;

        public bool enableLogServer = false;
        public bool enableLogClient = true;

#if UNITY_EDITOR
        [HideInInspector] public bool checkPlayerPrefab = true;

        [ContextMenu("Enable CheckPlayerPrefab")]
        void EnableCheckPlayerPrefab() => checkPlayerPrefab = true;
        
        public override void OnValidate()
        {
            base.OnValidate();
            CheckPlayerPrefab();
        }
        
        void CheckPlayerPrefab()
        {
            if (Application.isPlaying) return;
            
            if (playerPrefab == null && checkPlayerPrefab)
            {
                EditorGUIUtility.PingObject(this);
                
                // いきなりダイアログが出ると何きっかけで出てきてるのかわかりにくいので少し時間を置く
                // yield return new WaitForSeconds(3f);
                var delayCount = 3 * 60;

                EditorApplication.update += DelayShowDialog;

                void DelayShowDialog()
                {
                    if (Application.isPlaying || this == null)
                    {
                        EditorApplication.update -= DelayShowDialog;
                        return;
                    }
                    
                    delayCount--;
                    if (delayCount >= 0) return;
                    EditorApplication.update -= DelayShowDialog;

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
                            var guids = AssetDatabase.FindAssets("EmptyPlayer",
                                new[] {"Packages/ga.fuquna.syncutilformirror"});

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
            if (enableLogServer) Log($"Server Disconnect  connection ID {conn.connectionId}");
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