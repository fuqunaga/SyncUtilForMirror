using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using Mirror;

namespace SyncUtil
{
    public class SyncParamManager : NetworkBehaviour
    {
        #region Type Define
        
        public struct KeyObj { public string key; public object _value; }


        public struct KeyBool { public string key; public bool _value; }
        public struct KeyInt { public string key; public int _value; }
        public struct KeyUInt { public string key; public uint _value; }
        public struct KeyFloat { public string key; public float _value; }
        public struct KeyString { public string key; public string _value; }
        public struct KeyVector2 { public string key; public Vector2 _value; }
        public struct KeyVector3 { public string key; public Vector3 _value; }
        public struct KeyVector4 { public string key; public Vector4 _value; }


        public interface ISyncListKeyObj
        {
            int Count { get; }
            void Add(string key, object obj);
            void Set(int idx, object obj);
            KeyObj Get(int idx);
            void SetCallback(Action<int, int> callback);
        }

        public class SyncListKeyBool    : SyncList<KeyBool>,    ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyInt     : SyncList<KeyInt>,     ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyUInt    : SyncList<KeyUInt>,    ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyFloat   : SyncList<KeyFloat>,   ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyString  : SyncList<KeyString>,  ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyVector2 : SyncList<KeyVector2>, ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyVector3 : SyncList<KeyVector3>, ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }
        public class SyncListKeyVector4 : SyncList<KeyVector4>, ISyncListKeyObj { public void Add(string key, object obj) { this._Add(key, obj); } public KeyObj Get(int idx) { return this._Get(idx); } public void Set(int idx, object obj) { this._Set(idx, obj); } public void SetCallback(Action<int, int> callback){ this._SetCallback(callback); } }

        public class TypeAndIdx
        {
            public Type type;
            public int idx;
        }
        #endregion

        #region singleton
        static SyncParamManager _instance;
        public static SyncParamManager instance { get { return (_instance != null) ? _instance : (_instance = FindObjectOfType<SyncParamManager>()); } }
        #endregion

        #region sync
        readonly SyncListKeyBool _syncListKeyBool = new SyncListKeyBool();
        readonly SyncListKeyInt _syncListKeyInt = new SyncListKeyInt();
        readonly SyncListKeyUInt _syncListKeyUInt = new SyncListKeyUInt();
        readonly SyncListKeyFloat _syncListKeyFloat = new SyncListKeyFloat();
        readonly SyncListKeyString _syncListKeyString = new SyncListKeyString();
        readonly SyncListKeyVector2 _syncListKeyVector2 = new SyncListKeyVector2();
        readonly SyncListKeyVector3 _syncListKeyVector3 = new SyncListKeyVector3();
        readonly SyncListKeyVector4 _syncListKeyVector4 = new SyncListKeyVector4();
        #endregion

        Dictionary<Type, ISyncListKeyObj> typeToSyncList;
        Dictionary<string, TypeAndIdx> keyToTypeIdx = new Dictionary<string, TypeAndIdx>();

        HashSet<string> triggerdKey = new HashSet<string>();

        public bool IsTypeSupported(Type type) => typeToSyncList.ContainsKey(type) || type.IsEnum;

        public void Awake()
        {
            typeToSyncList = new Dictionary<Type, ISyncListKeyObj>()
            {
                { typeof(bool),    _syncListKeyBool    },
                { typeof(int),     _syncListKeyInt     },
                { typeof(uint),    _syncListKeyUInt    },
                { typeof(float),   _syncListKeyFloat   },
                { typeof(string),  _syncListKeyString  },
                { typeof(Vector2), _syncListKeyVector2 },
                { typeof(Vector3), _syncListKeyVector3 },
                { typeof(Vector4), _syncListKeyVector4 }
            };

            syncInterval = 0f;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.SpawnObjects();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            _syncListKeyBool.Callback += (op, idx, _, _) => { triggerdKey.Add(_syncListKeyBool.Get(idx).key); };
            typeToSyncList.Values.ToList().ForEach(list =>
            {
                list.SetCallback((op, idx) => {
                    triggerdKey.Add(list.Get(idx).key);
                });
            });
        }


        [ServerCallback]
        public void UpdateParam(string key, object val)
        {
            var type = val.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
                val = Convert.ChangeType(val, type);
            }

            if (keyToTypeIdx.TryGetValue(key, out var ti))
            {
                var iSynList = typeToSyncList[type];
                iSynList.Set(ti.idx, val);
            }
            else {
                if (typeToSyncList != null)
                {
                    Assert.IsTrue(typeToSyncList.ContainsKey(type), string.Format("type [{0}] is not supported.", type));

                    var iSynList = typeToSyncList[type];
                    var idx = iSynList.Count;
                    iSynList.Add(key, val);
                    keyToTypeIdx[key] = new TypeAndIdx() { type = type, idx = idx };
                }
            }
        }


        public object GetParam(string key, bool triggeredOnly = false)
        {
            if (!triggeredOnly || triggerdKey.Contains(key))
            {
                triggerdKey.Remove(key);
                
                var allList = typeToSyncList.Values.ToList();
                for (var iList = 0; iList < allList.Count; ++iList)
                {
                    var list = allList[iList];
                    for (var i = 0; i < list.Count; ++i)
                    {
                        var ko = list.Get(i);
                        if (ko.key == key)
                        {
                            return ko._value;
                        }
                    }
                }

                Assert.IsTrue(true, key + " was not found.");
            }
            return null;
        }
    }


    public static class SyncListExtenion
    {
        public class KVField
        {
            public FieldInfo keyField;
            public FieldInfo valueField;
        }

        static Dictionary<Type, KVField> _typeToField = new Dictionary<Type, KVField>();
        static KVField GetField(Type type)
        {
            KVField kvField;
            if (!_typeToField.TryGetValue(type, out kvField))
            {
                _typeToField[type] = kvField = new KVField()
                {
                    keyField = type.GetField("key"),
                    valueField = type.GetField("_value")
                };
            }
            return kvField;
        }


        static T CreateInstance<T>(string key, object obj)
        {
            var ret = Activator.CreateInstance(typeof(T));
            var kvField = GetField(typeof(T));
            kvField.keyField.SetValue(ret, key);
            kvField.valueField.SetValue(ret, obj);
            return (T)ret;
        }

        public static void _Add<T>(this SyncList<T> sl, string key, object obj)
            where T : struct
        {
            sl.Add(CreateInstance<T>(key, obj));
        }

        public static void _Set<T>(this SyncList<T> sl, int idx, object obj)
            where T : struct
        {
            var kvField = GetField(typeof(T));
            if (false == kvField.valueField.GetValue(sl[idx]).Equals(obj))
            {
                var key = (string)kvField.keyField.GetValue(sl[idx]);
                sl[idx] = CreateInstance<T>(key, obj);
            }
        }

        public static SyncParamManager.KeyObj _Get<T>(this SyncList<T> sl, int idx)
            where T : struct
        {
            var kvField = GetField(typeof(T));
            return new SyncParamManager.KeyObj()
            {
                key = (string)kvField.keyField.GetValue(sl[idx]),
                _value = kvField.valueField.GetValue(sl[idx])
            };
        }

        public static void _SetCallback<T>(this SyncList<T> sl, Action<int, int> callback)
            where T : struct
        {
            sl.Callback += (op,idx, _, _) => callback((int)op, idx); 
        }
    }
}
