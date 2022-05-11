using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil
{
    public class SyncParam : MonoBehaviour
    {
        public enum Mode
        {
            Sync,    // The client value is always overwritten by the server value
            Trigger　// The client value is only overwritten when server values change
        }

        [FormerlySerializedAs("_target")]
        public Object target;

        [FormerlySerializedAs("_fields")]
        public List<FieldBinder.FieldData> fields = new();

        private List<FieldBinder> _fieldBinders;
        

        public void Start()
        {
            _fieldBinders = fields.Select(data => FieldBinder.Create(data, target)).ToList();
        }

        [ClientCallback]
        private void Update()
        {
            if (SyncNet.IsFollower)
            {
                foreach (var field in _fieldBinders)
                {
                    field.ReceiveParam();
                }
            }
        }

        [ServerCallback]
        void LateUpdate()
        {
            if (SyncNet.IsServer)
            {
                foreach (var field in _fieldBinders)
                {
                    field.SendParam();
                }
            }
        }
    }
}