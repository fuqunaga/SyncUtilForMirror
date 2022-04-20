using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// Client 単位で可視判定.
    /// </summary>
    [RequireComponent(typeof(ClientNameManagerBase))]
    public class ClientInvisibilityManagement : InterestManagement
    {
        private ClientNameManagerBase _clientNameManagerBase;

        protected ClientNameManagerBase ClientNameManager => _clientNameManagerBase ??= GetComponent<ClientNameManagerBase>();

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver) => true;
        public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
        {
            var manager = ClientNameManager;
            var invisibleClientNameList = identity.GetComponent<ClientInvisibility>()?.invisibleClientNameList;

            foreach (var conn in NetworkServer.connections.Values)
            {
                var clientName = manager.GetClientName(conn);
                if (invisibleClientNameList != null && !invisibleClientNameList.Contains(clientName))
                {
                    newObservers.Add(conn);
                }
            }
        }
    }
}