using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SyncUtil
{
    [ExecuteAlways]
    public class OnlineOfflineSceneLoadHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        [FormerlySerializedAs("_autoUnloadOnline")]
        public bool autoUnloadOnline = true;

        [FormerlySerializedAs("_autoUnloadOffline")]
        public bool autoUnloadOffline = true;

        [FormerlySerializedAs("_autoLoadOnline")]
        public bool autoLoadOnline = true;

        [FormerlySerializedAs("_autoLoadOffline")]
        public bool autoLoadOffline = true;

        static OnlineOfflineSceneLoadHelper _instance;

        static OnlineOfflineSceneLoadHelper Instance => (_instance != null)
            ? _instance
            : (_instance = FindObjectOfType<OnlineOfflineSceneLoadHelper>());

        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        static void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode && (Instance != null))
            {
                Instance.UnloadScenes();
            }
        }

        void UnloadScenes()
        {
            var nm = FindObjectOfType<NetworkManager>(); // singleton maybe not ready.
            Assert.IsNotNull(nm);

            var scenes = Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(s => (autoUnloadOnline && (s.name == nm.onlineScene)) ||
                            (autoUnloadOffline && (s.name == nm.offlineScene)))
                .Where(s => s.isLoaded);


            foreach (var scene in scenes)
            {
                if (scene.isDirty)
                {
                    EditorSceneManager.SaveScene(scene);
                }

                SceneManager.UnloadSceneAsync(scene);
            }
        }

        void Start()
        {
            if (Application.isPlaying) return;
            var nm = FindObjectOfType<NetworkManager>();
            Assert.IsNotNull(nm);


            Debug.Log(string.Join("\n", EditorBuildSettings.scenes.Select(s => s.path)));

            var scenePaths = new[]
                {
                    autoLoadOnline ? nm.onlineScene : "",
                    autoLoadOffline ? nm.offlineScene : ""
                }
                .Where(path => !string.IsNullOrEmpty(path));

            foreach (var scenePath in scenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
        }
#endif
    }
}
