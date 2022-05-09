using System;
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

        public event Action onClientConnect;
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            onClientConnect?.Invoke();
        }


        public event Action<Exception> onClientError;
        public override void OnClientError(Exception exception)
        {
            base.OnClientError(exception);
            onClientError?.Invoke(exception);
        }

        public event Action onClientDisconnect;
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            onClientDisconnect?.Invoke();
        }

        #endregion
    }
}