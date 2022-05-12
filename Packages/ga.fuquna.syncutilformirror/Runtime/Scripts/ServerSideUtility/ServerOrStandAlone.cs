using UnityEngine;

namespace SyncUtil
{
    /// <summary>
    /// Children are inactive at the client
    /// </summary>
    public class ServerOrStandAlone : MonoBehaviour
    {
        public void Awake()
        {
            var active = SyncNet.IsServerOrStandAlone;

            for (var i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(active);
            }
        }
    }
}