using System.Linq;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace SyncUtil
{
    [CustomEditor(typeof(SyncParamManager), true)]
    public class SyncParamManagerEditor : NetworkBehaviourInspector
    {
        private bool _folding;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("For Debug", EditorStyles.boldLabel);

            _folding = EditorGUILayout.Foldout(_folding, "Synced params");
            if( _folding )
            {
                var syncParamManager = (SyncParamManager) target;

                GUI.enabled = false;
                foreach (var paramName in syncParamManager.SyncedParamNames.OrderBy(s => s))
                {
                    EditorGUILayout.TextField(paramName);
                }
                GUI.enabled = true;
            }
        }
    }
}