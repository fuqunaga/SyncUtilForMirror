using System;
using Mirror;
using UnityEngine;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(ILockStep))]
    public class LockStepExample : LockStepExampleBase
    {
        #region  Type Define

        [Serializable]
        public class LorentzParameter
        {
            public float p = 10f;
            public float r = 28f;
            public float b = 8/3f;
            [Range(0f, 0.1f)]
            public float timeScale = 0.005f;
        }
        
        public struct Msg : NetworkMessage
        {
            public LorentzParameter lorentzParameter;
            public bool resetPosition;
        }

        #endregion
        

        public GameObject spherePrefab;
        public LorentzParameter lorentzParameter;
        public bool needResetPosition;
        
        GameObject _sphere;

        protected void Start()
        {
            _sphere = Instantiate(spherePrefab, transform);
            InitLockStepCallbacks();
            
            Reset();
        }

        void InitLockStepCallbacks()
        {
            var lockStep = GetComponent<ILockStep>();
            lockStep.GetDataFunc = () =>
            {
                var msg = new Msg()
                {
                    lorentzParameter = lorentzParameter,
                    resetPosition = needResetPosition
                };
                
                needResetPosition = false;
                
                return msg;
            };

            lockStep.StepFunc = (_, reader) =>
            {
                if (stepEnable)
                {
                    var msg = reader.Read<Msg>();
                    Step(msg.resetPosition, msg.lorentzParameter);
                }
                return stepEnable;
            };

            lockStep.OnMissingCatchUpServer = () =>
            {
                Debug.Log("OnMissingCatchUp at Server. NetworkManager.Shutdown() will be called.");
                return true;
            };
            lockStep.OnMissingCatchUpClient = () => Debug.Log("OnMissingCatchUp at Client. Server will disconnect.");

            lockStep.GetHashFunc = () => _sphere.transform.position.ToString(".00000");
        }

        void Reset()
        {
            _sphere.transform.position = new Vector3(5f, 20f, 40f);
        }


        void Step(bool reset, LorentzParameter lp)
        {
            if (reset) Reset();

            var pos = _sphere.transform.position;
            _sphere.transform.position = LorentzAttractor(pos, lp);
        }


        
        /// <summary>
        /// Solve LorenzAttractor with Runge-Kutta method
        /// 
        /// https://en.wikipedia.org/wiki/Lorenz_system
        /// https://en.wikipedia.org/wiki/Runge%E2%80%93Kutta_methods
        ///
        /// https://qiita.com/POPPIN_FRIENDS/items/d41330782e3a7041cf93
        /// </summary>
        static Vector3 LorentzAttractor(Vector3 pos, LorentzParameter lp)
        {
            var k1 = lp.timeScale * Lorenz(pos);
            var k2 = lp.timeScale * Lorenz(pos + k1 * 0.5f);
            var k3 = lp.timeScale * Lorenz(pos + k2 * 0.5f);
            var k4 = lp.timeScale * Lorenz(pos + k3);

            var delta = (k1 + 2f * (k2 + k3) + k4) / 6f;

            return pos + delta;

            Vector3 Lorenz(Vector3 currentPos)
            {
                return new Vector3(
                    lp.p * (currentPos.y - currentPos.x),
                    currentPos.x * (lp.r - currentPos.z) - currentPos.y,
                    currentPos.x * currentPos.y - lp.b * currentPos.z
                );
            }
        }
    }
}
