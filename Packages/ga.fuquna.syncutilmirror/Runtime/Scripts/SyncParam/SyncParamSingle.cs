using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil
{
    public abstract class SyncParamSingle<TTarget, TValue> : SyncParamBase
        where TTarget : Object
    {
        [FormerlySerializedAs("_target")] 
        public TTarget target;
        
        [FormerlySerializedAs("_mode")] 
        public SyncParamMode mode;
        
        protected override List<IParamBinder> CreateParamBinders()
        {
            var key = $"{GetType()}/{target.name}/{target.GetType()}";
            
            return new[] {
                (IParamBinder)new ParamBinder<TValue>(
                    key,
                    mode,
                    GetParam,
                    SetParam
                    )
            }.ToList();
        }

        protected abstract TValue GetParam();
        protected abstract void SetParam(TValue value);
    }
}