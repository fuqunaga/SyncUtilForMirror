using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    public abstract class SyncParamBase : MonoBehaviour
    {
        private List<IParamBinder> _paramBinders;

        
        #region Unity
        
        protected virtual void Start()
        {
            _paramBinders = CreateParamBinders();
        }

        [ClientCallback]
        protected virtual void Update()
        {
            if (SyncNet.IsFollower)
            {
                foreach (var field in _paramBinders)
                {
                    field.ReceiveParam();
                }
            }
        }

        [ServerCallback]
        protected virtual void LateUpdate()
        {
            if (SyncNet.IsServer)
            {
                foreach (var field in _paramBinders)
                {
                    field.SendParam();
                }
            }
        }
        
        #endregion

        
        protected abstract List<IParamBinder> CreateParamBinders();
    }
}