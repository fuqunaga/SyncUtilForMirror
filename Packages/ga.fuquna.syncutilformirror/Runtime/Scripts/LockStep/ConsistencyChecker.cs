using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;

namespace SyncUtil
{
    public class ConsistencyChecker
    {
        #region Type Define
        
        public struct ConsistencyData
        {
            public int stepCount;
            public LockStepConsistency consistency;
        }

                
        public struct HashMessage : NetworkMessage
        {
            public string value;
        }
        
        public struct RequestHashMessage : NetworkMessage
        {
            public int value;
        }
        
        #endregion
        
        
        #region Server

        ConsistencyData _lastConsistency = new()
        {
            stepCount = -1,
            consistency = LockStepConsistency.NotCheckYet
        };

        public Dictionary<int, string> ConnectionIdToHash { get; } = new();
        protected bool IsCompleteConnectionIdToHash => ConnectionIdToHash.Count == NetworkServer.connections.Count;

        
        [Server]
        public void OnStartServer()
        {
            NetworkServer.RegisterHandler<HashMessage>((conn, msg) =>
            {
                ConnectionIdToHash[conn.connectionId] = msg.value;
            });
        }

        [Server]
        public ConsistencyData GetLastConsistencyData() => _lastConsistency;

        [Server]
        public void StartCheckConsistency(MonoBehaviour behaviour, int targetStepCount, float timeOut = 10f) => behaviour.StartCoroutine(CheckConsistencyCoroutine(targetStepCount, timeOut));

        protected IEnumerator CheckConsistencyCoroutine(int targetStepCount, float timeOut)
        {
            ConnectionIdToHash.Clear();
            
            _lastConsistency.stepCount = targetStepCount;
            _lastConsistency.consistency = LockStepConsistency.Checking;

            NetworkServer.SendToAll(new RequestHashMessage() {value = targetStepCount});
            var time = Time.time;

            yield return new WaitUntil(() => ((Time.time - time) > timeOut) || IsCompleteConnectionIdToHash);


            if (IsCompleteConnectionIdToHash)
            {
                _lastConsistency.consistency = (ConnectionIdToHash.Values.Distinct().Count() == 1) ? LockStepConsistency.Match : LockStepConsistency.NotMatch;
            }
            else
            {
                _lastConsistency.consistency = LockStepConsistency.TimeOut;
            }
        }
        #endregion



        #region Client
        
        protected int checkConsistencyStepCount = -1;
        
        public void OnStartClient()
        {
            NetworkClient.RegisterHandler<RequestHashMessage>((msg) =>
            {
                checkConsistencyStepCount = msg.value;
            });
        }


        [Client]
        protected void ReturnCheckConsistency(string hash)
        {
            NetworkClient.Send(new HashMessage() { value = hash });
        }
        #endregion

        public void Update(int stepCountClient, Func<string> getHashFunc)
        {
            if (checkConsistencyStepCount < 0) return;
            Assert.IsTrue(stepCountClient <= checkConsistencyStepCount);
            
            if (stepCountClient == checkConsistencyStepCount)
            {
                ReturnCheckConsistency(getHashFunc());
                checkConsistencyStepCount = -1;
            }
        }
    }
}