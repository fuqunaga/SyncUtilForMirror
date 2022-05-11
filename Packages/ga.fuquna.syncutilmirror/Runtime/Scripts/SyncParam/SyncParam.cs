using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil
{
    public class SyncParam : SyncParamBase
    {
        [FormerlySerializedAs("_target")]
        public Object target;
        
        [FormerlySerializedAs("_fields")]
        public List<FieldBinder.FieldData> fields = new();

        protected override List<IParamBinder> CreateParamBinders() => fields.Select(data => FieldBinder.Create(data, target)).ToList();
    }
}