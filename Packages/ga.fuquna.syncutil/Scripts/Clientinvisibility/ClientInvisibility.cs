using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// Client 単位で可視判定.
    /// SpawnするObjectにAttachすることで、特定のClientには生成しないといったフィルタリングができる
    /// </summary>
    public class ClientInvisibility : NetworkBehaviour
    {
        public List<string> invisibleClientName;

#if true
        private void Start()
        {
            Debug.LogError($"{nameof(ClientInvisibility)} does not work on Mirror.");
        }
#else
        public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initialize)
        {
            var manager = ClientNameManager.Instance;
            if (manager == null)
            {
                return false;
            }

            var connections = NetworkServer.connections;

            for(var i=0; i<connections.Count; ++i)
            {
                var conn = connections[i];

                var clientName = manager.GetClientName(conn);
                if (!invisibleClientName.Contains(clientName))
                {
                    observers.Add(conn);
                }
            }

            return true;
        }
#endif
    }
}