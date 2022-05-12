using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SyncUtil
{
    /// <summary>
    /// ClientのConnectionに任意の名前をつけ、サーバー側で区別できるようにする
    /// Client側からの自己申告性
    /// 区別するにはClientごとにNameの値を変える必要があるので注意
    /// </summary>
    public class ClientNameManager : ClientNameManagerBase
    {
        public string myName;
        protected override string Name => myName;
    }


    public abstract class ClientNameManagerBase : MonoBehaviour
    {
        #region Type Define

        public struct Message : NetworkMessage
        {
            public string name;
        }

        #endregion


        #region Static

        static ClientNameManagerBase _instance;

        public static ClientNameManagerBase Instance => _instance != null ? _instance : _instance = FindObjectOfType<ClientNameManagerBase>();

        #endregion


        readonly Dictionary<NetworkConnection, string> _nameDic = new();

        protected abstract string Name { get; }

        protected virtual void Start()
        {
            if (SyncNet.IsServer)
            {
                NetworkServer.RegisterHandler<Message>(OnReceiveConnectionIdentity, false);

                var manager = SyncNetworkManager.Singleton;
                manager.onServerDisconnect += (conn) => _nameDic.Remove(conn);
            }

            if (SyncNet.IsClient)
            {
                var message = new Message() { name = Name };
                NetworkClient.Send(message);
            }
        }


        #region Server


        void OnReceiveConnectionIdentity(NetworkConnectionToClient conn, Message msg)
        {
            _nameDic[conn] = msg.name;
        }

        public string GetClientName(NetworkConnection conn)
        {
            _nameDic.TryGetValue(conn, out var ret);
            return ret;
        }

        #endregion
    }
}