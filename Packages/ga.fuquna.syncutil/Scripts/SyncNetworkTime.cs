using UnityEngine;
using Mirror;

namespace SyncUtil
{
    public class SyncNetworkTime : MonoBehaviour
    {
        #region singleton
        protected static SyncNetworkTime _instance;
        public static SyncNetworkTime Instance { get { return _instance ?? (_instance = (FindObjectOfType<SyncNetworkTime>() ?? (new GameObject("SyncNetworkTime", typeof(SyncNetworkTime))).GetComponent<SyncNetworkTime>())); } }
        #endregion

        #region type define
        public struct NetworkTimeMessage : NetworkMessage
        {
            public double time;
        }
        #endregion

        double _offset;

        protected float realTime => Time.realtimeSinceStartup;

        public double time => realTime + _offset;

        public void Start()
        {
            var networkManager = SyncNetworkManager.singleton;


            networkManager.onServerConnect += (conn) =>
            {
                NetworkServer.SendToReady(conn.identity, new NetworkTimeMessage() { time = realTime });
            };


            networkManager.onStartClient += () =>
            {
                if (SyncNet.isFollower)
                {
                    NetworkClient.RegisterHandler<NetworkTimeMessage>((msg) =>
                    {
                        _offset = msg.time - realTime;
                    });
                }
            };
        }
    }
}