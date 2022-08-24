using System;
using UnityEngine;
using Mirror;
using Object = UnityEngine.Object;

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

        public static bool IsActive
        {
            get
            {
                var nm = NetworkManager.singleton;
                return (nm != null) && nm.isNetworkActive;
            }
        }

        public static void Spawn(GameObject go, Space transformSpace = Space.Self)
        {
            if (IsServer)
            {
                // NetworkServer.Spawn() はTransformのローカル座標を使用します。
                // https://github.com/vis2k/Mirror/pull/875
                //
                // クライアント上ではgoのヒエラルキーの親子関係は維持されずルート直下に置かれます。
                // transformSpace == Space.Worldのときは
                // 一時的にTransformのローカル座標をワールド座標の値にすることで、ワールド座標を維持したままクライアント上にSpawnさせます。
                var syncWorldSpace = transformSpace == Space.World;

                TransformCache? transformCache = syncWorldSpace
                    ? new TransformCache(go.transform)
                    : null;

                if (syncWorldSpace)
                {
                    var trans = go.transform;
                    trans.localPosition = trans.position;
                    trans.localRotation = trans.rotation;
                    trans.localScale = trans.lossyScale;
                }

                NetworkServer.Spawn(go);
                
                transformCache?.Revert();
            }
        }

        public static void Destroy(GameObject go)
        {
            if (IsServer) NetworkServer.Destroy(go);
            if (IsServerOrStandAlone) Object.Destroy(go);
        }


        public static float Time => SyncTime.Instance.Time;
        public static double NetworkTime => Mirror.NetworkTime.time;


        struct TransformCache
        {
            private readonly Transform _transform;
            readonly Vector3 _localPosition;
            readonly Quaternion _localRotation;
            readonly Vector3 _localScale;

            public TransformCache(Transform transform)
            {
                _transform = transform;

                _localPosition = transform.localPosition;
                _localRotation = transform.localRotation;
                _localScale = transform.localScale;
            }

            public void Revert()
            {
                _transform.localPosition = _localPosition;
                _transform.localRotation = _localRotation;
                _transform.localScale = _localScale;
            }
        }
    }
}