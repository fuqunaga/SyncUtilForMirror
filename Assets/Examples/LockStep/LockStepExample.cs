using Mirror;
using UnityEngine;

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
        Vector3 _velocity;
        public float damping = 0.9f;
        public float forceMax = 0.1f;

        protected override void Start()
        {
            base.Start();

            _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphere.transform.SetParent(transform);

            InitLockStepCallbacks();
        }

        void InitLockStepCallbacks()
        {
            var lockStep = GetComponent<ILockStep>();
            lockStep.GetDataFunc = () => new Msg()
            {
                force = Random.insideUnitSphere * forceMax
            };

            lockStep.StepFunc = (_, reader) =>
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

            lockStep.GetHashFunc = () => _sphere.transform.position.ToString(".00000");
        }


        void Step(Vector3 force)
        {
            _velocity += force;
            _velocity *= damping;
            var trans = _sphere.transform;
            trans.position += _velocity;
        }
    }
}
