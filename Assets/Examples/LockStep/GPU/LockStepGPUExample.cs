using System.Threading;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(LockStep), typeof(LifeGame))]
    public class LockStepGPUExample : LockStepExampleBase
    {
        // `public` for code generation. 
        // ReSharper disable once MemberCanBePrivate.Global
        public class Msg : NetworkMessage
        {
            public LifeGame.StepData data;
        }
        
        
        public float resolutionScale = 0.5f;
        private LifeGame _lifeGame;

        protected void Start()
        {
            _lifeGame = GetComponent<LifeGame>();
            LifeGameUpdater.Reset();

            InitLockStepCallbacks();
        }


        private void InitLockStepCallbacks()
        {
            var lockStep = GetComponent<LockStep>();
            lockStep.GetDataFunc = () => new Msg()
            {
                data = LifeGameUpdater.CreateStepData(resolutionScale)
            };

            lockStep.StepFunc = (_, reader) =>
            {
                if (!stepEnable) return stepEnable;
                
                var msg = reader.Read<Msg>();
                _lifeGame.Step(msg.data);
                return stepEnable;
            };

            lockStep.OnMissingCatchUpServer = () =>
            {
                Debug.Log("OnMissingCatchUp at Server. NetworkManager.StopHost() will be called.");
                return true;
            };
            lockStep.OnMissingCatchUpClient = () => Debug.Log("OnMissingCatchUp at Client. Server will disconnect.");

            lockStep.GetHashFuncAsync = () => LockStepHelper.GenerateBufferHashAsync(_lifeGame.ReadBuffer);
        }
    }
}
