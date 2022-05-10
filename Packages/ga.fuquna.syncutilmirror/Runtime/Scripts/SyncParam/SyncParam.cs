using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SyncUtil
{
    public class SyncParam : MonoBehaviour
    {
        public enum Mode
        {
            Sync,
            Trigger
        }

        [FormerlySerializedAs("_target")]
        public Object target;

        [FormerlySerializedAs("_fields")]
        public List<FieldData> fields = new();

        public void Start()
        {
            foreach(var field in fields)
            {
                field.Init(target);
            }
        }

        void LateUpdate()
        {
            var mgr = SyncParamManager.instance;
            if (mgr != null)
            {
                if (SyncNet.IsServer)
                {
                    foreach(var field in fields)
                    {
                        mgr.UpdateParam(field.Key, field.GetValue(target));
                    }
                }

                if (SyncNet.IsFollower)
                {
                    foreach(var field in fields)
                    {
                        var obj = mgr.GetParam(field.Key, field.mode == Mode.Trigger);
                        if (obj != null)
                        {
                            field.SetValue(target, obj);
                        }
                    }
                }
            }
        }

    }
}