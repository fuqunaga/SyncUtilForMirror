using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;

namespace SyncUtil
{
    /// <summary>
    /// Spawn the registered Prefabs at the server
    /// Also register to NetworkManager on the editor
    /// </summary>
    [ExecuteAlways]
    public class Spawner : MonoBehaviour
    {
        [FormerlySerializedAs("_prefabs")]
        public List<NetworkIdentity> prefabs = new();

#if UNITY_EDITOR
        // Auto register to SpawnPrefabs on Editor
        NetworkManager _networkManager;
        private void Update()
        {
            if (!Application.isPlaying)
            {
                var nm = _networkManager ??= FindObjectOfType<NetworkManager>();
                if (nm != null)
                {
                    var diffGo = prefabs.Where(ni => ni != null).Select(ni => ni.gameObject).Except(nm.spawnPrefabs);
                    nm.spawnPrefabs.AddRange(diffGo);
                }
            }
        }
#endif

        private void Start()
        {
            if (Application.isPlaying)
            {
                if (SyncNet.IsServer)
                {
                    StartCoroutine(DelaySpawn());
                }
            }
        }

        IEnumerator DelaySpawn()
        {
            yield return new WaitUntil(() => NetworkServer.active);

            foreach(var prefab in prefabs)
            {
                var go = Instantiate(prefab.gameObject, transform, true);
                SyncNet.Spawn(go);
            }
        }
    }
}