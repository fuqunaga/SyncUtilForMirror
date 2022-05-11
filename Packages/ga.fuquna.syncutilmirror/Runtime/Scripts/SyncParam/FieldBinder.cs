using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace SyncUtil
{
    public abstract class FieldBinder
    {
        #region Static
        
        public static FieldBinder Create(FieldData data, Object target)
        {
            var t = target.GetType();
            var name = data.name;

            FieldInfo fi = null;
            while (t != null && fi == null)
            {
                fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                t = t.BaseType;
            }

            Assert.IsNotNull(fi, "Can't get field. [" + target.GetType() + "." + name + "]");

            var binderType = typeof(FieldBinder<>).MakeGenericType(fi.FieldType);
            
            
            return (FieldBinder) Activator.CreateInstance(binderType, data, target);
        }
        
        #endregion
        
        
        [Serializable]
        public class FieldData
        {
            [Tooltip("field name of target")] public string name;
            public SyncParam.Mode mode;
        }

        public abstract void SendParam();

        public abstract void ReceiveParam();
    }

    
    
    public class FieldBinder<T> : FieldBinder
    {
        // 値を比較できるか
        // - クラスは参照比較なので駄目
        // - IsValueType ではメンバーにクラスを持つ構造体があるので駄目
        // - unmanaged でもポインタが含まれる場合があるが気にしない
        private static readonly bool IsEquatable = typeof(T).IsUnManaged() || typeof(T) == typeof(string);

        
        private FieldData _data;
        private readonly Object _target;
        private bool _firstSend = true;
        private T _prevValue;
        
        
        private Func<T> _getValueFunc;
        private Func<T> GetValueFunc => _getValueFunc ??= CreateGetValueFunc();
        
        private Action<T> _setValueFunc;
        private  Action<T> SetValueFunc => _setValueFunc ??= CreateSetValueAction();
        
        public string Key { get; protected set; }

        
        public FieldBinder(FieldData data, Object target)
        {
            _data = data;
            _target = target;

            Key = $"{target.name}/{data.name}"; // TODO: case _target object has multi instance
        }

        Func<T> CreateGetValueFunc()
        {
            // () => _target.[_data.name]
            return Expression.Lambda<Func<T>>(
                Expression.PropertyOrField(
                    Expression.Constant(_target),
                    _data.name
                )
            ).Compile();
        }

        Action<T> CreateSetValueAction()
        {
            // (t) => _target.[_data.name] = t;
            var arg = Expression.Parameter(typeof(T));
            return Expression.Lambda<Action<T>>(
                Expression.Assign(
                    Expression.PropertyOrField(
                        Expression.Constant(_target),
                        _data.name
                    ),
                    arg
                ),
                arg
            ).Compile();
        }
        

        [Server]
        public override void SendParam()
        {
            var value = GetValueFunc();

            if (!IsEquatable || !Equals(value, _prevValue) || _firstSend)
            {
                var mgr = SyncParamManager.Instance;
                if (mgr != null)
                {
                    mgr.SetParam(Key, value);
                    _prevValue = value;
                    _firstSend = false;
                }
            }
        }


        [Client]
        public override void ReceiveParam()
        {
            var mgr = SyncParamManager.Instance;
            if (mgr == null) return;
            
            switch (_data.mode)
            {
                case SyncParam.Mode.Sync:
                {
                    if (mgr.TryGetParam<T>(Key, out var value))
                    {
                        SetValueFunc(value);
                    }
                    else
                    {
                        Debug.LogError($"Key[{Key}] is not found.");
                    }
                }
                    break;

                case SyncParam.Mode.Trigger:
                {
                    if (mgr.TryGetParamTriggered<T>(Key, out var value))
                    {
                        SetValueFunc(value);
                    }
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    
    // check unmanaged trick
    // https://stackoverflow.com/questions/53968920/how-do-i-check-if-a-type-fits-the-unmanaged-constraint-in-c
    public static class UnmanagedTypeExtensions
    {
        class U<T> where T : unmanaged { }
        public static bool IsUnManaged(this Type t)
        {
            try { var gt = typeof(U<>).MakeGenericType(t); return true; }
            catch (Exception){ return false; }
        }
    }
}