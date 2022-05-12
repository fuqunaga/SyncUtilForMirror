using System.Collections;
using UnityEngine;
using Mirror;

namespace SyncUtil
{
    /// <summary>
    /// Serverの Time.time と Time.timeScale を Client でも追従する
    /// 通信ラグがあるので Server のほうが先行している
    /// 厳密ではないが粗く StandAlone と動作、コードを変えたくない場合の用途で使う
    /// ほぼ同時刻が欲しい場合は NetworkTime.time を推奨
    /// </summary>
    public class SyncTime : MonoBehaviour
    {
        #region Singleton
        
        protected static SyncTime instance;
        public static SyncTime Instance => instance ? instance : (instance = (FindObjectOfType<SyncTime>() ?? (new GameObject("SyncTimeManager", typeof(SyncTime))).GetComponent<SyncTime>()));

        #endregion


        #region Type Define
        
        public struct SyncTimeMessage : NetworkMessage
        {
            public float time;
            public float timeScale;
        }
        
        #endregion

        public float Time => SyncNet.IsServerOrStandAlone ? UnityEngine.Time.time : _clientTime;


        float _clientTime;
        SyncTimeMessage? _lastMsg;

        public void Start()
        {
			if(SyncNetworkManager.Singleton == null)
			{
				Debug.LogWarning("SyncNetworkManager is not in scene");
				return;
			}
			// when a client starts
            SyncNetworkManager.Singleton.onStartClient += () =>
            {
                if (SyncNet.IsFollower)
                {
					// register a network handler function that caches the last time msg received
                    NetworkClient.RegisterHandler<SyncTimeMessage>((msg) =>
                    {
                        if (_lastMsg == null || msg.time > _lastMsg.Value.time)
                        {
                            _lastMsg = msg;
                        }
                    });

					// start coroutine that will process received network message
                    StopAllCoroutines();
                    StartCoroutine(UpdateTimeClient());
                }
                // Mirrors disconnect in case of unknown messages, so register to avoid them.
                else
                {
                    NetworkClient.RegisterHandler<SyncTimeMessage>(_ => { });
                }
            };
            // in case the server restarts, when the client next connects the server, make sure the client's last message is reset to null, 
            // otherwise in a rare case when the server app restarts, the server's lastMsg.time will be less than the client's lastMsg.time and time chnages on the server will not sync properly on the client.
            SyncNetworkManager.Singleton.onClientConnect += () =>
            {
                _lastMsg = null;
            };
        }

        public void Update()
        {
            if (SyncNet.IsServer)
            {
                NetworkServer.SendToAll(new SyncTimeMessage() { time = Time, timeScale = UnityEngine.Time.timeScale });
            }
        }
       

        IEnumerator UpdateTimeClient()
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                yield return waitForEndOfFrame;  // フレームの最後で_time更新

                if (_lastMsg is {} lastMsg)
                {
                    _clientTime = lastMsg.time;
                    UnityEngine.Time.timeScale = lastMsg.timeScale;
                }
            }
        }
    }
}