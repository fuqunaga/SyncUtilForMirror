using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

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
            public int stepCount;
            public string hash;
        }
        
        public struct RequestHashMessage : NetworkMessage
        {
            public int stepCount;
        }
        
        #endregion
        
        
        private readonly Dictionary<int, Dictionary<int, string>> _connectionIdToHashOfStepCount = new();


        #region Server

        private ConsistencyData _lastConsistency = new()
        {
            stepCount = -1,
            consistency = LockStepConsistency.NotCheckYet
        };

        
        [Server]
        public void OnStartServer()
        {
            NetworkServer.ReplaceHandler<HashMessage>((conn, msg) =>
            {
                if ( !_connectionIdToHashOfStepCount.TryGetValue(msg.stepCount, out var connectionIdToHash))
                {
                    Debug.LogWarning($"Invalid hash message received. connectionId:{conn.connectionId} stepCount:{msg.stepCount}");
                    return;
                }
                
                connectionIdToHash[conn.connectionId] = msg.hash;
            });
        }

        [Server]
        public ConsistencyData GetLastConsistencyData() => _lastConsistency;

        [Server]
        public void StartCheckConsistency(MonoBehaviour behaviour, int targetStepCount, float timeOut = 10f)
        {
            if (_connectionIdToHashOfStepCount.ContainsKey(targetStepCount)) return;
            behaviour.StartCoroutine(CheckConsistencyCoroutine(targetStepCount, timeOut));
        }

        protected IEnumerator CheckConsistencyCoroutine(int targetStepCount, float timeOut)
        {
            using var _ = DictionaryPool<int, string>.Get(out var connectionIdToHash);
            _connectionIdToHashOfStepCount[targetStepCount] = connectionIdToHash;
            
            _lastConsistency.stepCount = targetStepCount;
            _lastConsistency.consistency = LockStepConsistency.Checking;

            NetworkServer.SendToAll(new RequestHashMessage() { stepCount = targetStepCount });
            var time = Time.time;

            yield return new WaitUntil(() => ((Time.time - time) > timeOut) || IsReplyMessagesComplete());


            if (IsReplyMessagesComplete())
            {
                _lastConsistency.consistency = (connectionIdToHash.Values.Distinct().Count() == 1) ? LockStepConsistency.Match : LockStepConsistency.NotMatch;
            }
            else
            {
                _lastConsistency.consistency = LockStepConsistency.TimeOut;
            }
            
            _connectionIdToHashOfStepCount.Remove(targetStepCount);

            bool IsReplyMessagesComplete() => connectionIdToHash.Count == NetworkServer.connections.Count;
        }
        
        #endregion
        

        #region Client
        
        protected int checkConsistencyStepCount = -1;
        
        public void OnStartClient()
        {
            NetworkClient.ReplaceHandler<RequestHashMessage>((msg) =>
            {
                checkConsistencyStepCount = msg.stepCount;
            });
        }


        [Client]
        private static void ReturnCheckConsistency(int stepCount, string hash)
        {
            NetworkClient.Send(new HashMessage()
            {
                stepCount = stepCount,
                hash = hash
            });
        }
        #endregion

   
        public void Step(int stepCountClient, Func<Task<string>> getHashFuncAsync)
        {
            if (checkConsistencyStepCount < 0) return;
            
            Assert.IsTrue(stepCountClient <= checkConsistencyStepCount, $"{stepCountClient} <= {checkConsistencyStepCount}");
            
            if (stepCountClient == checkConsistencyStepCount)
            {
                checkConsistencyStepCount = -1;
                Assert.IsNotNull(getHashFuncAsync, $"{nameof(ILockStep)}.{nameof(ILockStep.GetHashFuncAsync)} must be set.");
                CheckHash(stepCountClient, getHashFuncAsync);
            }
        }

        private static async void CheckHash(int stepCount, Func<Task<string>> getHashFuncAsync)
        {
            var hash = await getHashFuncAsync();
            ReturnCheckConsistency(stepCount, hash);
        }
    }
}