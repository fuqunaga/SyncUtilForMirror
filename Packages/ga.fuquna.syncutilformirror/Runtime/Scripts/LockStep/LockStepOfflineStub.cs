using System;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// LockStepOffline
    /// A class that provides a lockstep interface when offline
    /// </summary>
    public class LockStepOfflineStub : MonoBehaviour, ILockStep
    {
        public static readonly ConsistencyChecker.ConsistencyData ConsistencyData = new();

        public int stepCount;

        protected Func<NetworkMessage> getDataFunc;
        protected Func<int, NetworkReader, bool> stepFunc;

        protected Func<NetworkMessage> getInitDataFunc;
        protected Func<NetworkReader, bool> initFunc;



        #region Unity

        bool _firstStep = true;

        private void Update()
        {
            if (getDataFunc != null)
            {
                if ( _firstStep )
                {
                    _firstStep = false;
                    if ( getInitDataFunc !=null)
                    {
                        var initMsg = getInitDataFunc();
                        using var initReader = NetworkReaderPool.Get(initMsg.ToBytes());
                        initFunc(initReader);
                    }

                }

                var msg = getDataFunc();
                using var reader = NetworkReaderPool.Get(msg.ToBytes());
                if (stepFunc(stepCount, reader))
                {
                    stepCount++;
                }
            }
        }

        #endregion


        #region  ILockStep

        public Func<NetworkMessage> GetDataFunc { set => getDataFunc = value; }
        public Func<int, NetworkReader, bool> StepFunc { set => stepFunc = value; }
        public Func<NetworkMessage> GetInitDataFunc { set => getInitDataFunc = value; }
        public Func<NetworkReader, bool> InitFunc { set => initFunc = value; }

        public Func<bool> OnMissingCatchUpServer { set { } }
        public Action OnMissingCatchUpClient { set { } }
        public Func<string> GetHashFunc { set { } }

        public ConsistencyChecker.ConsistencyData GetLastConsistencyData()
        {
            return ConsistencyData;
        }

        public void StartCheckConsistency()
        {
        }

        public int StepCountServer => stepCount;
        public int StepCountClient => stepCount;
        
        #endregion
    }
}