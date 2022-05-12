#define INCLUDE_UPDATE

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

namespace SyncUtil
{

    /// <summary>
    /// 以下の時間を計測し、この半分をServerの値がClientに反映される時間（Latency）として計測する
    /// 
    /// INCLUDE_UPDATE defined
    /// (Server)Send -> (Client)Recieve -> (Client)Update,Send -> (Server)Recieve -> (Server)Update
    /// 
    /// INCLUDE_UPATE not defined
    /// (Server)Send -> (Client)Recieve,Send -> (Server)Recieve
    /// </summary>
    public class LatencyChecker : MonoBehaviour
    {
        #region singleton
        protected static LatencyChecker _instance;
        public static LatencyChecker Instance { get { return _instance ?? (_instance = (FindObjectOfType<LatencyChecker>() ?? (new GameObject("LatencyChecker", typeof(LatencyChecker))).GetComponent<LatencyChecker>())); } }
        #endregion

        #region type define
        public struct LatencyMessage : NetworkMessage
        {
            public float serverTime;
        }

        public class Data
        {
            static int _queueNum = 8;
            Queue<float> _latencies = new Queue<float>();
            public bool _recieved;

            public void Add(float latency)
            {
                _latencies.Enqueue(latency);
                while (_latencies.Count > _queueNum) _latencies.Dequeue();
            }

            public float Last { get { return _latencies.LastOrDefault(); } }
            public float average { get { return _latencies.Average(); } }
        }
        #endregion


        public Dictionary<int, Data> _conectionLatencyTable = new Dictionary<int, Data>();

#if INCLUDE_UPDATE
        Dictionary<int, LatencyMessage> _conectionLatencyPool = new Dictionary<int, LatencyMessage>();
        LatencyMessage? _lastMsg;
#endif

        protected float time => Time.realtimeSinceStartup;

        public void Start()
        {
            SyncNetworkManager.Singleton.onStartServer += () =>
            {
                NetworkServer.RegisterHandler<LatencyMessage>((conn, msg) =>
                {
#if INCLUDE_UPDATE
                    _conectionLatencyPool[conn.connectionId] = msg;
#else
                    UpdateTable(nmsg.conn.connectionId, nmsg.ReadMessage<LatencyMessage>());
#endif
                });
            };

            SyncNetworkManager.Singleton.onServerDisconnect += (conn) =>
            {
                _conectionLatencyTable.Remove(conn.connectionId);
            };

            SyncNetworkManager.Singleton.onStartClient += () =>
            {
                if (SyncNet.IsFollower)
                {
                    NetworkClient.RegisterHandler<LatencyMessage>((msg) =>
                    {
#if INCLUDE_UPDATE
                        _lastMsg = msg;
#else
                        client.Send(CustomMsgType.Latency, msg.ReadMessage<LatencyMessage>());
#endif
                    });

                }
                // Mirrors disconnect in case of unknown messages, so register to avoid them.
                else
                {
                    NetworkClient.RegisterHandler<LatencyMessage>(_ => { });
                }
            };
        }

        public void Update()
        {
            if (SyncNet.IsServer)
            {
                foreach(var d in _conectionLatencyTable.Values)
                {
                    d._recieved = false;
                };
                
                NetworkServer.SendToAll(new LatencyMessage() { serverTime = time });


#if INCLUDE_UPDATE
                if (_conectionLatencyPool.Any())
                {
                    foreach(var pair in _conectionLatencyPool)
                    {
                        UpdateTable(pair.Key, pair.Value);
                    }
                    _conectionLatencyPool.Clear();
                }
#endif
            }

#if INCLUDE_UPDATE
            if (SyncNet.IsFollower)
            {
                if (_lastMsg != null && NetworkClient.isConnected)
                {
                    NetworkClient.Send(_lastMsg.Value);
                    _lastMsg = null;
                }
            }
#endif      
        }

        void UpdateTable(int connectionId, LatencyMessage lmsg)
        {
            Data data;
            if (!_conectionLatencyTable.TryGetValue(connectionId, out data))
            {
                _conectionLatencyTable[connectionId] = data = new Data();
            }

            var latency = (float)(time - lmsg.serverTime) * 0.5f;
            data.Add(latency);
            data._recieved = true;
        }
    }
}