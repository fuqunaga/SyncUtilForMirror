﻿using System;
using UnityEngine;
using Mirror;

#pragma warning disable 0618

namespace SyncUtil
{
    /// <summary>
    /// LockStepOffline
    /// A class that provides a lockstep interface when offline
    /// </summary>
    public class LockStepOfflineStub : MonoBehaviour, ILockStep
    {
        public static LockStep.ConsistencyData consistencyData = new LockStep.ConsistencyData();

        public int stepCount;

        protected Func<NetworkMessage> _getDataFunc;
        protected Func<int, NetworkReader, bool> _stepFunc;

        protected Func<NetworkMessage> _getInitDataFunc;
        protected Func<NetworkReader, bool> _initFunc;



        #region Unity

        bool firstStep = true;

        private void Update()
        {
            if (_getDataFunc != null)
            {
                if ( firstStep )
                {
                    firstStep = false;
                    if ( _getInitDataFunc !=null)
                    {
                        var initMsg = _getInitDataFunc();
                        var w = new NetworkWriter();
                        w.Write(initMsg);
                        _initFunc(new NetworkReader(w.ToArraySegment()));
                    }

                }

                var msg = _getDataFunc();

                var writer = new NetworkWriter();
                writer.Write(msg);
                if (_stepFunc(stepCount, new NetworkReader(writer.ToArraySegment())))
                {
                    stepCount++;
                }
            }
        }

        #endregion


        #region  Override

        public Func<NetworkMessage> getDataFunc { set { _getDataFunc = value; } }
        public Func<int, NetworkReader, bool> stepFunc { set { _stepFunc = value; } }
        public Func<NetworkMessage> getInitDataFunc { set { _getInitDataFunc = value; } }
        public Func<NetworkReader, bool> initFunc { set { _initFunc = value; } }

        public Func<bool> onMissingCatchUpServer { set { } }
        public Action onMissingCatchUpClient { set { } }
        public Func<string> getHashFunc { set { } }

        public LockStep.ConsistencyData GetLastConsistencyData()
        {
            return consistencyData;
        }

        public void StartCheckConsistency()
        {
        }


        public int stepCountServer => stepCount;
        public int stepCountClient => stepCount;
        #endregion
    }
}