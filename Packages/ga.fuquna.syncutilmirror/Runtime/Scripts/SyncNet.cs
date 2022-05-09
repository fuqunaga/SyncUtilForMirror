using UnityEngine;
using Mirror;

namespace SyncUtil
{
    public static class SyncNet
    {
        /// <summary>
        /// クライアントである判定になるように偽装する
        /// あくまでSyncNetの判定のみに作用するのでちゃんとしたクライアントとしては動作せず注意が必要
        /// </summary>
        public static bool IsDummyFollower { get; set; }

        public static bool IsServer => NetworkServer.active && !IsDummyFollower;
        public static bool IsClient => NetworkClient.active || IsDummyFollower;
        public static bool IsHost => IsServer && IsClient;
        public static bool IsStandAlone => !IsServer && !IsClient;

        public static bool IsServerOrStandAlone => IsServer || !IsClient;

        // Not Server but Client.
        // warn Host: isServer == isClient == true
        public static bool IsFollower => !IsServerOrStandAlone;

        public static bool IsActive { get { var nm = NetworkManager.singleton; return (nm != null) && nm.isNetworkActive; } }

        public static void Spawn(GameObject go)
        {
            if (IsServer) NetworkServer.Spawn(go);
        }

        public static void Destroy(GameObject go)
        {
            if (IsServer) NetworkServer.Destroy(go);
            if (IsServerOrStandAlone) Object.Destroy(go);
        }


        public static float Time => SyncTime.Instance.Time;
        public static double NetworkTime => Mirror.NetworkTime.time;
    }
}