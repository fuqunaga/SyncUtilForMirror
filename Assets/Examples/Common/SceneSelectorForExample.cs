using System.Collections.Generic;
using System.Linq;
using Mirror;
using RosettaUI;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SyncUtil.Example
{
    public class SceneSelectorForExample : MonoBehaviour, IElementCreator
    {
#if UNITY_EDITOR
        public List<SceneAsset> onlineScenes = new();
#endif

        [FormerlySerializedAs("_idx")]
        public int idx;

        [FormerlySerializedAs("_onlineSceneNames")]
        [HideInInspector]
        public string[] onlineSceneNames;

        private void Start()
        {
            UpdateOnlineScene();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            var next = onlineScenes.Where(s => s != null).Select(s => s.name).ToArray();
            if (!next.SequenceEqual(onlineSceneNames))
            {
                onlineSceneNames = next;
                UpdateOnlineScene();
            }
        }
#endif

        public void DebugMenu()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Scene: ");

                var newIdx = GUILayout.SelectionGrid(idx, onlineSceneNames, 1);
                if (newIdx != idx)
                {
                    idx = newIdx;
                    UpdateOnlineScene();
                }
            }
        }

        void UpdateOnlineScene()
        {
            if (onlineSceneNames != null && onlineSceneNames.Any())
            {
                var nm = FindFirstObjectByType<NetworkManager>();
                nm.onlineScene = onlineSceneNames[Mathf.Min(onlineSceneNames.Length - 1, idx)];
            }
        }

        public Element CreateElement(LabelElement label)
        {
            return UI.Dropdown(label,
                () => idx,
                onlineSceneNames
            ).RegisterValueChangeCallback(UpdateOnlineScene);
        }
    }
}