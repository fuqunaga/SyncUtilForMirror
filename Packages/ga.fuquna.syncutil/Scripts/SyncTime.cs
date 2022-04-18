using System.Collections;
using UnityEngine;
using Mirror;

namespace SyncUtil
{
    public class SyncTime : MonoBehaviour
    {
        #region singleton
        protected static SyncTime _instance;
        public static SyncTime Instance { get { return _instance ?? (_instance = (FindObjectOfType<SyncTime>() ?? (new GameObject("SyncTimeManager", typeof(SyncTime))).GetComponent<SyncTime>())); } }
        #endregion

        #region type define
        public struct SyncTimeMessage : NetworkMessage
        {
            public float time;
            public float timeScale;
        }
        #endregion

        public float time
        {
            get { return SyncNet.isServerOrStandAlone ? Time.time : _clientTime; }
        }


        float _clientTime;
        SyncTimeMessage? _lastMsg;

        public void Start()
        {
			if(SyncNetworkManager.singleton == null)
			{
				Debug.LogWarning("SyncNetworkManager is not in scene");
				return;
			}
			// when a client starts
            SyncNetworkManager.singleton.onStartClient += () =>
            {
				// if it is a slave
                if (SyncNet.isFollower)
                {
					// register a network handler function that caches the last time msg recieved
                    NetworkClient.RegisterHandler<SyncTimeMessage>((msg) =>
                    {
                        if (_lastMsg == null || msg.time > _lastMsg.Value.time)
                        {
                            _lastMsg = msg;
                        }
                    });

					// start coroutine that will proces recieved network message
                    StopAllCoroutines();
                    StartCoroutine(UpdateTimeClient());
                }
            };
            // in case the server restarts, when the client next connects the server, make sure the client's last message is reset to null, 
            // otherwise in a rare case when the server app restarts, the server's lastMsg.time will be less than the client's lastMsg.time and time chnages on the server will not sync properly on the client.
            SyncNetworkManager.singleton.onClientConnect += (networkConn) =>
            {
                _lastMsg = null;
            };
        }

        public void Update()
        {
            if (SyncNet.isServer)
            {
                NetworkServer.SendToAll(new SyncTimeMessage() { time = time, timeScale = Time.timeScale });
            }
        }

       

        IEnumerator UpdateTimeClient()
        {
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                yield return waitForEndOfFrame;  // フレームの最後で_time更新

                if (_lastMsg is {} lastMsg)
                {
                    _clientTime = lastMsg.time;
                    Time.timeScale = lastMsg.timeScale;
                }
            }
        }
    }
}