using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// Client 単位で可視判定.
    /// </summary>
    [RequireComponent(typeof(ClientNameBase))]
    public class ClientInvisibilityManagement : InterestManagement
    {
        private ClientNameBase _clientNameBase;

        protected ClientNameBase ClientName => _clientNameBase ??= GetComponent<ClientNameBase>();

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver) => true;
        public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
        {
            var manager = ClientName;
            var invisibleClientNameList = identity.GetComponent<ClientInvisibility>()?.invisibleClientNameList;

            foreach (var conn in NetworkServer.connections.Values)
            {
                var clientName = manager.GetClientName(conn);
                if (invisibleClientNameList == null || !invisibleClientNameList.Contains(clientName))
                {
                    newObservers.Add(conn);
                }
            }
        }
    }
}