using System.Collections.Generic;
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
        private HashSet<string> _triggeredKey;

        public IEnumerable<string> SyncedParamNames => _syncDictionary.Keys;
       
        
        #region Unity

        public void Awake()
        {
            syncInterval = 0f;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _triggeredKey = new(_syncDictionary.Keys);
#if MIRROR_90_OR_NEWER
            _syncDictionary.OnChange += (_, key, _) => _triggeredKey.Add(key);
#else
            _syncDictionary.Callback += (_, key, _) => _triggeredKey.Add(key);
#endif
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
                var str = JsonUtilityEx.ToJson(value);
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
                value = JsonUtilityEx.FromJson<T>(str);
            }

            return true;
        }

        public bool TryGetParamTriggered<T>(string key, out T value)
        {
            if (_triggeredKey?.Remove(key) ?? false)
            {
                return TryGetParam(key, out value);
            }

            value = default;
            return false;
        }
    }
}
