﻿using System;
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
        #region Singleton
        
        static SyncParamManager _instance;
        public static SyncParamManager Instance => (_instance != null) ? _instance : (_instance = FindObjectOfType<SyncParamManager>());

        #endregion

        
        private readonly SyncDictionary<string, byte[]> _syncDictionary = new();
        private readonly HashSet<string> _triggeredKey = new();


        #region Unity

        public void Awake()
        {
            syncInterval = 0f;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _syncDictionary.Callback += (_, key, _) =>  _triggeredKey.Add(key);
        }

        #endregion
        

        [ServerCallback]
        public void SetParam<T>(string key, T value)
        {
            using var writer = NetworkWriterPool.Get();
            
            var writeAction = Writer<T>.write;
            if (writeAction != null)
            {
                writeAction(writer, value);
            }
            else
            {
                var str = JsonUtility.ToJson(value);
                writer.Write(str);
            }

            var seg = writer.ToArraySegment();

            if (_syncDictionary.TryGetValue(key, out var bytes) && seg.Count == bytes.Length)
            {
                seg.CopyTo(bytes);
            }
            else
            {
                bytes = seg.ToArray();
            }

            _syncDictionary[key] = bytes;
        }
        
        public bool TryGetParam<T>(string key, out T value)
        {
            if (!_syncDictionary.TryGetValue(key, out var bytes))
            {
                value = default;
                return false;
            }

            using var reader = NetworkReaderPool.Get(bytes);
            
            var readFunc = Reader<T>.read;
            if (readFunc != null)
            {
                value = reader.Read<T>();
            }
            else
            {
                var str = reader.Read<string>();
                value = JsonUtility.FromJson<T>(str);
            }

            return true;
        }

        public bool TryGetParamTriggered<T>(string key, out T value)
        {
            if (_triggeredKey.Remove(key))
            {
                return TryGetParam(key, out value);
            }

            value = default;
            return false;
        }
    }
}
