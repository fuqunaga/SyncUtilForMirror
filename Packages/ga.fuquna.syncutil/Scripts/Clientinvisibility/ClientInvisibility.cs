using System.Collections.Generic;
using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// Client 単位で可視判定.
    /// SpawnするObjectにAttachすることで、特定のClientには生成しないといったフィルタリングができる
    /// </summary>
    public class ClientInvisibility : MonoBehaviour
    {
        public List<string> invisibleClientNameList;
    }
}