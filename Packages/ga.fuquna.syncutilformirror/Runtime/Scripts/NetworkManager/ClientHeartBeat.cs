using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

namespace SyncUtil
{
    /// <summary>
    /// Information sent from the client to the server every frame
    /// </summary>
    [RequireComponent(typeof(NetworkManagerWithHookAction))]
    public class ClientHeartBeat : MonoBehaviour
    {
        public struct Message : NetworkMessage
        {
            public double rtt;
        }

        public class HeartBeatInfo
        {
            public static int maxMessageCount = 10;
            
            public int ReceivedFrameCount { get; protected set; }
            public readonly Queue<Message> messages = new();

            public void Add(Message message)
            {
                ReceivedFrameCount = Time.frameCount;
                
                messages.Enqueue(message);
                while (messages.Count > maxMessageCount)
                {
                    messages.Dequeue();
                }
            }

            public double Rtt => messages.LastOrDefault().rtt;
            public double RttAverage => messages.Any() ? messages.Average(m => m.rtt) : 0;
            public bool IsReceivedAtCurrentFrame => ReceivedFrameCount == Time.frameCount;
            
            
            public override string ToString()
            {
                return $"rtt:{Rtt:0.000} rttAverage:{RttAverage:0.000} receivedFrame:{ReceivedFrameCount}";
            }
        }

        
        private readonly Dictionary<int, HeartBeatInfo> _connectionIdToInfo = new();

        
        #region Unity
        
        public void Start()
        {
            SyncNetworkManager.Singleton.onStartServer += () =>
            {
                NetworkServer.RegisterHandler<Message>((conn, message) =>
                {
                    var id = conn.connectionId;
                    if (!_connectionIdToInfo.TryGetValue(id, out var info))
                    {
                        info = new HeartBeatInfo();
                        _connectionIdToInfo[id] = info;
                    }

                    info.Add(message);
                });
            };

            SyncNetworkManager.Singleton.onServerDisconnect += (conn) =>
            {
                _connectionIdToInfo.Remove(conn.connectionId);
            };
        }

        [ClientCallback]
        public void Update()
        {
            if (SyncNet.IsFollower)
            {
                if (NetworkClient.isConnected)
                {
                    NetworkClient.Send(CreateHeartBeatInfo());
                }
            }
        }

        #endregion

        public HeartBeatInfo GetHeartBeatInfo(int connectionId)
        {
            _connectionIdToInfo.TryGetValue(connectionId, out var info);
            return info;
        } 
        
        
        static Message CreateHeartBeatInfo()
        {
            return new Message()
            {
                rtt = NetworkTime.rtt
            };
        }
    }
}