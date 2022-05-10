using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace SyncUtil
{
    [System.Serializable]
    public class FieldData
    {
        public string name;
        public SyncParam.Mode mode;

        public string Key { get; protected set; }
        FieldInfo _fieldInfo;

        bool? _needSerialize;
        bool NeedSerialize => _needSerialize ?? (_needSerialize = !SyncParamManager.instance.IsTypeSupported(_fieldInfo.FieldType)).Value;

        public void Init(Object target)
        {
            Key = $"{target.name}/{name}"; // TODO: case _target object has multi instance

            var t = target.GetType();
            while (t != null && _fieldInfo == null)
            {
                _fieldInfo = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                t = t.BaseType;
            }
            Assert.IsNotNull(_fieldInfo, "Can't get field. [" + target.GetType() + "." + name + "]");
        }

        public object GetValue(Object target)
        {
            var ret = _fieldInfo.GetValue(target);
            if ( NeedSerialize )
            {
                ret = JsonUtility.ToJson(ret);
            }

            return ret;
        }

        public void SetValue(Object target, object value)
        {
            if (NeedSerialize)
            {
                value = JsonUtility.FromJson(value as string, _fieldInfo.FieldType);
            }
            _fieldInfo.SetValue(target, value);
        }
    }


}