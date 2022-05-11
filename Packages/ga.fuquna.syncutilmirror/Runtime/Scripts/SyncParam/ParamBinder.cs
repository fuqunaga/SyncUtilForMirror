using System;
using Mirror;
using UnityEngine;

namespace SyncUtil
{
    public class ParamBinder<T> : IParamBinder
    {
        // 値を比較できるか
        // - クラスは参照比較なので駄目
        // - IsValueType ではメンバーにクラスを持つ構造体があるので駄目
        // - unmanaged でもポインタが含まれる場合があるが気にしない
        private static readonly bool IsEquatable = typeof(T).IsUnManaged() || typeof(T) == typeof(string);

        private readonly string _key;
        private readonly SyncParamMode _mode;
        
        private readonly Func<T> _getValueFunc;
        private readonly Action<T> _setValueFunc;
        
        private bool _firstSend = true;
        private T _prevValue;
        
        
        public ParamBinder(string key, SyncParamMode mode,  Func<T> getValueFunc, Action<T> setValueFunc)
        {
            _key = key;
            _mode = mode;
            _getValueFunc = getValueFunc;
            _setValueFunc = setValueFunc;
        }

        [Server]
        public void SendParam()
        {
            var value = _getValueFunc();

            if (!IsEquatable || !Equals(value, _prevValue) || _firstSend)
            {
                var mgr = SyncParamManager.Instance;
                if (mgr != null)
                {
                    mgr.SetParam(_key, value);
                    _prevValue = value;
                    _firstSend = false;
                }
            }
        }


        [Client]
        public void ReceiveParam()
        {
            var mgr = SyncParamManager.Instance;
            if (mgr == null) return;
            
            switch (_mode)
            {
                case SyncParamMode.Sync:
                {
                    if (mgr.TryGetParam<T>(_key, out var value))
                    {
                        _setValueFunc(value);
                    }
                    else
                    {
                        Debug.LogError($"Key[{_key}] is not found.");
                    }
                }
                    break;

                case SyncParamMode.Trigger:
                {
                    if (mgr.TryGetParamTriggered<T>(_key, out var value))
                    {
                        _setValueFunc(value);
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