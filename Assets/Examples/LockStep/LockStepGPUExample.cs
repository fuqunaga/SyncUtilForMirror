using Mirror;
using UnityEngine;

#pragma warning disable 0618

namespace SyncUtil.Example
{
    [RequireComponent(typeof(LockStep), typeof(LifeGame))]
    public class LockStepGPUExample : LockStepExampleBase
    {
        public class Msg : NetworkMessage
        {
            public LifeGame.StepData data;
        }

        public float _resolutionScale = 0.5f;
        LifeGame _lifeGame;

        protected override void Start()
        {
            base.Start();
            _lifeGame = GetComponent<LifeGame>();
            LifeGameUpdater.Reset();

            InitLockStepCallbacks();
        }


        void InitLockStepCallbacks()
        {
            var lockStep = GetComponent<LockStep>();
            lockStep.GetDataFunc = () =>
            {
                return new Msg()
                {
                    data = LifeGameUpdater.CreateStepData(_resolutionScale)
                };
            };

            lockStep.StepFunc = (stepCount, reader) =>
            {
                if (_stepEnable)
                {
                    var msg = reader.Read<Msg>();
                    _lifeGame.Step(msg.data);
                }
                return _stepEnable;
            };

            lockStep.OnMissingCatchUpServer = () =>
            {
                Debug.Log("OnMissingCatchUp at Server. NetworkManager.StopHost() will be called.");
                return true;
            };
            lockStep.OnMissingCatchUpClient = () => Debug.Log("OnMissingCatchUp at Client. Server will disconnect.");

            lockStep.GetHashFunc = () => LockStepHelper.GenerateBufferHash<LifeGame.Data>(_lifeGame.readBufs);
        }
    }
}
