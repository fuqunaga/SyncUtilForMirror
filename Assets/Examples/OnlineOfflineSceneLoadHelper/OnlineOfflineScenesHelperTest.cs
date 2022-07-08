using UnityEngine;

namespace SyncUtil.Example
{
    /// <summary>
    /// this logs will not be called when it is in online/offline scene
    /// </summary>
    public class OnlineOfflineScenesHelperTest : MonoBehaviour
    {
        string SceneName => gameObject.scene.name;

        private void Awake()
        {
            Debug.Log("Awake: " + SceneName);
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable" + SceneName);
        }

        private void Start()
        {
            Debug.Log("Start" + SceneName);
        }
    }

}