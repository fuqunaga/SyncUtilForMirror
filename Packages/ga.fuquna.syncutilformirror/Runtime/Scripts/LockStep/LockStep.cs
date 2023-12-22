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

        public Func<NetworkMessage> GetDataFunc { set => getDataFunc = value; }
        public Func<int, NetworkReader, bool> StepFunc { set => stepFunc = value; }

        public Func<NetworkMessage> GetInitDataFunc { set => getInitDataFunc = value; }
        public Func<NetworkReader, bool> InitFunc { set => initFunc = value; }


        public Func<bool> OnMissingCatchUpServer { set => onMissingCatchUpServer = value; }
        public Action OnMissingCatchUpClient { set => onMissingCatchUpClient = value; }
        
        
        public Func<Task<string>> GetHashFuncAsync { set => getHashFuncAsync = value; }

        public ConsistencyChecker.ConsistencyData GetLastConsistencyData() => _consistencyChecker.GetLastConsistencyData();
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


        [FormerlySerializedAs("_dataNumMax")] 
        public int dataNumMax = 10000;
        
        [FormerlySerializedAs("_stepNumMaxPerFrame")] 
        public int stepNumMaxPerFrame = 10;


        private readonly SyncList<Data> _dataList = new();
        [SyncVar] private InitData _initData;

        private bool _sentInit;
        private bool _initialized;

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
            if (!_dataList.Any() || stepFunc == null) return;

            var idx = _dataList.Count-1;
            for (; idx>=0; --idx)
            {
                var step = _dataList[idx].stepCount;
                if (step == StepCountClient) break;
                if (step < StepCountClient) return;
            }

            if (idx >= 0)
            { 
                var firstStepCount = _dataList[idx].stepCount;
                if (firstStepCount > StepCountClient)
                {
                    Debug.LogWarning($"Wrong step count Expected[{StepCountClient}], min data's[{firstStepCount}]");
                    onMissingCatchUpClient?.Invoke();
                }
                else
                {
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

                    if (_initialized)
                    {
                        var limit = Mathf.Min(idx + stepNumMaxPerFrame, _dataList.Count);
                        for (; idx < limit; idx++)
                        {
                            var data = _dataList[idx];
                            Assert.IsTrue(StepCountClient == data.stepCount, $"stepCountClient[{StepCountClient}] data.stepCount[{data.stepCount}]");

                            using var reader = NetworkReaderPool.Get(data.bytes);
                            var isStepEnable = stepFunc(data.stepCount, reader);
                            if (!isStepEnable) break;

                            _consistencyChecker.Step(StepCountClient, getHashFuncAsync);
                            StepCountClient++;
                        }
                    }
                }
            }
        }
    }
}