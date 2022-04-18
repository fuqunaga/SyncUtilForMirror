using System;
using UnityEngine;
using Mirror;

namespace SyncUtil
{
    public class NetworkManagerWithHookAction : NetworkManager
    {
        #region Server side

        public event Action onStartServer;
        public override void OnStartServer()
        {
            base.OnStartServer();
            onStartServer?.Invoke();
        }

        public event Action<NetworkConnectionToClient> onServerConnect;
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            onServerConnect?.Invoke(conn);
        }

        public event Action<NetworkConnectionToClient> onServerDisconnect;
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            onServerDisconnect?.Invoke(conn);
        }

        #endregion


        #region Client side

        public event Action onStartClient;
        public override void OnStartClient()
        {
            base.OnStartClient();
            onStartClient?.Invoke();
        }

        public event Action<NetworkConnection> onClientConnect;
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            onClientConnect?.Invoke(conn);
        }


        public event Action<Exception> onClientError;
        public override void OnClientError(Exception exception)
        {
            base.OnClientError(exception);
            onClientError?.Invoke(exception);
        }

        public event Action<NetworkConnection> onClientDicconnect;
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            onClientDicconnect?.Invoke(conn);
        }

        #endregion
    }

    [Serializable]
    public class SyncNetworkManager : NetworkManagerWithHookAction
    {
        public static new SyncNetworkManager singleton => NetworkManager.singleton as SyncNetworkManager;

        public bool enableLogServer = false;
        public bool enableLogClient = true;

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

        public override void OnClientConnect(NetworkConnection conn)
        {
            if (enableLogClient) Log($"Client connection ID: {conn.connectionId}  has connected to the server");
            base.OnClientConnect(conn);
        }


        public override void OnClientError(Exception exception)
        {
            if (enableLogClient) LogError($"Client Error: {exception.Message}");
            base.OnClientError(exception);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            if (enableLogClient) Log($"Client Disconeect  connection ID {conn.connectionId}");
            base.OnClientDisconnect(conn);
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