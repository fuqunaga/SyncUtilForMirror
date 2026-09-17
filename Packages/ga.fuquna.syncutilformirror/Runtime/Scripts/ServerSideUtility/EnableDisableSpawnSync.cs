using Mirror;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// OnEnable/OnDisableのタイミングでSpawn/UnSpawnを行う
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class EnableDisableSpawnSync : MonoBehaviour
    {
        [SerializeField] private bool enableSpawn = true;
        [SerializeField] private bool enableUnSpawn = true;
        private NetworkIdentity _networkIdentity;


        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsSpawned
        {
            get
            {
                if (_networkIdentity == null)
                {
                    TryGetComponent(out _networkIdentity);
                }

                return _networkIdentity != null && _networkIdentity.netId != 0;
            }
        }
        
        
        private void OnEnable()
        {
            if (enableSpawn && !IsSpawned)
            {
                // シーンオブジェクトでかつサーバーがロード中の場合はこのあとで自動的にSpawnされるのでSpawnしない
                var willBeSpawnedByServer = (_networkIdentity != null && _networkIdentity.sceneId != 0 && NetworkServer.isLoadingScene);
                if (willBeSpawnedByServer)
                {
                    return;
                }
                
                SyncNet.Spawn(gameObject);
            }
        }

        private void OnDisable()
        {
            if (enableUnSpawn && IsSpawned)
            {
                SyncNet.UnSpawn(gameObject);
            }
        }
    }
}