using System;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace SyncUtil
{
    public class LockStep : NetworkBehaviour, ILockStep
    {
        #region Type Define

        public struct Data
        {
            public int stepCount;
            public byte[] bytes;
        }

        public struct InitData
        {
            public bool sent;
            public byte[] bytes;
        }

        #endregion;


        #region ILockStep

        public Func<NetworkMessage> GetDataFunc
        {
            set => getDataFunc = value;
        }

        public Func<int, NetworkReader, bool> StepFunc
        {
            set => stepFunc = value;
        }

        public Func<NetworkMessage> GetInitDataFunc
        {
            set => getInitDataFunc = value;
        }

        public Func<NetworkReader, bool> InitFunc
        {
            set => initFunc = value;
        }


        public Func<bool> OnMissingCatchUpServer
        {
            set => onMissingCatchUpServer = value;
        }

        public Action OnMissingCatchUpClient
        {
            set => onMissingCatchUpClient = value;
        }


        public Func<Task<string>> GetHashFuncAsync
        {
            set => getHashFuncAsync = value;
        }

        public ConsistencyChecker.ConsistencyData GetLastConsistencyData() =>
            _consistencyChecker.GetLastConsistencyData();

        public void StartCheckConsistency() => _consistencyChecker.StartCheckConsistency(this, StepCountServer + 10);

        public int StepCountServer { get; protected set; }
        public int StepCountClient { get; protected set; }

        #endregion


        protected Func<NetworkMessage> getDataFunc;
        protected Func<int, NetworkReader, bool> stepFunc;

        protected Func<NetworkMessage> getInitDataFunc;
        protected Func<NetworkReader, bool> initFunc;

        protected Func<bool> onMissingCatchUpServer;
        protected Action onMissingCatchUpClient;
        protected Func<Task<string>> getHashFuncAsync;


        [FormerlySerializedAs("_dataNumMax")] public int dataNumMax = 10000;

        [FormerlySerializedAs("_stepNumMaxPerFrame")]
        public int stepNumMaxPerFrame = 10;

        [Tooltip("遅延ステップ数\nクライアント側でこのステップ数の実行を遅らせる。最新のStepDataが来なくてもバッファがある限りステップ動作させることで画面の停止をある程度防ぐ")]
        public int delayStep;

        [Tooltip("遅延ステップを消費していい間隔。遅延ステップを常に進めるとサーバーが処理落ちしていた場合など補充されない場合がありバッファがすぐに枯渇してしまう。ゆっくりステップ実行を進める")]
        public float processDelayStepInterval = 0.1f;

        private readonly SyncList<Data> _dataList = new();
        [SyncVar] private InitData _initData;

        private bool _sentInit;
        private bool _initialized;
        private float _lastProcessDelayStepTime;

        private readonly ConsistencyChecker _consistencyChecker = new();


        #region Unity

        protected void Start()
        {
            syncInterval = 0f;

            var nm = SyncNetworkManager.Singleton;
            nm.onServerConnect += OnServerConnect;
        }

        protected void OnDestroy()
        {
            var nm = SyncNetworkManager.Singleton;
            if (nm != null)
            {
                nm.onServerConnect -= OnServerConnect;
            }
        }

        protected virtual void Update()
        {
            if (SyncNet.IsServer)
            {
                SendLockStep();
            }

            if (SyncNet.IsClient)
            {
                Step();
            }
        }

        #endregion


        protected virtual void OnServerConnect(NetworkConnection conn)
        {
            var isMissingFirstData = _dataList.Any() && _dataList.First().stepCount > 0;
            if (!isMissingFirstData) return;

            var doStopHost = onMissingCatchUpServer?.Invoke() ?? false;
            if (doStopHost)
            {
                NetworkManager.singleton.StopHost();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _consistencyChecker.OnStartServer();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _consistencyChecker.OnStartClient();
        }


        protected virtual void SendLockStep()
        {
            if (getDataFunc == null) return;

            if (!_sentInit)
            {
                var initData = new InitData() { sent = true };

                var initMsg = getInitDataFunc?.Invoke();
                if (initMsg != null)
                {
                    initData.bytes = initMsg.ToBytes();
                }

                _initData = initData;
                _sentInit = true;
            }

            var msg = getDataFunc();
            if (msg == null) return;

            _dataList.Add(new Data() { stepCount = StepCountServer, bytes = msg.ToBytes() });
            if (_dataList.Count > dataNumMax) _dataList.RemoveAt(0);
            ++StepCountServer;
        }


        private void Step()
        {
            var dataListCount = _dataList.Count;
            if (dataListCount <= delayStep || stepFunc == null) return;

            // まだ新しいステップのデータが届いていないので終了
            if (_dataList.Last().stepCount < StepCountClient) return;

            // _dataListを後ろから検索してStepCountClientと一致するステップ数のインデックスを求める
            var idx = dataListCount - 1;
            for (; idx >= 0; --idx)
            {
                var step = _dataList[idx].stepCount;
                
                // 一致するステップ数のデータが見つかった
                if (step == StepCountClient) break;
                
                // 一致するステップ数のデータが見つかる前にStepCountClientより小さいステップ数のデータが見つかった
                if (step < StepCountClient)
                {
                    Debug.LogWarning($"A smaller step count[{step}] than StepCountClient[{StepCountClient}] was found before data with a matching step count was found.");
                    onMissingCatchUpClient?.Invoke();
                    return;
                }
            }

            // _dataListの中にStepCountClientより先のデータしかない
            if (idx < 0)
            {
                Debug.LogWarning($"Wrong step count Expected[{StepCountClient}], min data's[{_dataList[0].stepCount}]");
                onMissingCatchUpClient?.Invoke();
                return;
            }


            //　初期化
            if (!_initialized)
            {
                if (initFunc != null)
                {
                    if (!_initData.sent)
                    {
                        // InitData is NOT reach to this client yet
                        return;
                    }

                    using var reader = NetworkReaderPool.Get(_initData.bytes);
                    _initialized = initFunc(reader);
                }
                else
                {
                    _initialized = true;
                }
            }


            if (!_initialized) return;
            
            // Step実行
            // delayStep分は残して最大stepNumMaxPerFrame回実行してよい
            // 一回も実行できないならdelayStepを無視して１回実行できる
            var limit = Mathf.Min(idx + stepNumMaxPerFrame, _dataList.Count - delayStep);
            if ( limit <= idx && Time.time - _lastProcessDelayStepTime > processDelayStepInterval)
            {
                limit = Mathf.Min(idx + 1, _dataList.Count);
                _lastProcessDelayStepTime = Time.time;
            }
            
            for (; idx < limit; idx++)
            {
                var data = _dataList[idx];
                Assert.IsTrue(StepCountClient == data.stepCount,
                    $"stepCountClient[{StepCountClient}] data.stepCount[{data.stepCount}]");

                using var reader = NetworkReaderPool.Get(data.bytes);
                var isStepEnable = stepFunc(data.stepCount, reader);
                if (!isStepEnable) break;

                _consistencyChecker.Step(StepCountClient, getHashFuncAsync);
                StepCountClient++;
            }
        }
    }
}