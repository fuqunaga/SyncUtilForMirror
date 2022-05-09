using Mirror;
using UnityEngine;

#pragma warning disable 0618

namespace SyncUtil.Example
{
    [RequireComponent(typeof(ILockStep))]
    public class LockStepExample : LockStepExampleBase
    {
        public struct Msg : NetworkMessage
        {
            public Vector3 force;
        }

        GameObject _sphere;
        Vector3 velocity;
        public float damping = 0.9f;
        public float forceMax = 0.1f;

        protected override void Start()
        {
            base.Start();

            _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphere.transform.SetParent(transform);

            IniteLockStepCallbacks();
        }

        void IniteLockStepCallbacks()
        {
            var lockStep = GetComponent<ILockStep>();
            lockStep.GetDataFunc = () =>
            {
                return new Msg()
                {
                    force = Random.insideUnitSphere * forceMax
                };
            };

            lockStep.StepFunc = (stepCount, reader) =>
            {
                if (_stepEnable)
                {
                    var msg = reader.Read<Msg>();
                    Step(msg.force);
                }
                return _stepEnable;
            };

            lockStep.OnMissingCatchUpServer = () =>
            {
                Debug.Log("OnMissingCatchUp at Server. NetworkManager.Shutdown() will be called.");
                return true;
            };
            lockStep.OnMissingCatchUpClient = () => Debug.Log("OnMissingCatchUp at Client. Server will disconnect.");

            lockStep.GetHashFunc = () =>
            {
                return _sphere.transform.position.ToString(".00000");
            };
        }


        void Step(Vector3 force)
        {
            velocity += force;
            velocity *= damping;
            var trans = _sphere.transform;
            trans.position = trans.position + velocity;
        }
    }
}
