using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncParamPerformanceTest : NetworkBehaviour
    {
        public GameObject prefab;
        public int objectCount = 100;
        private readonly List<SyncParamExample> _exampleUnits = new();
        private int showGuiUnitIdx;
        
        private void OnEnable()
        {
            if (SyncNet.IsServer)
            {
                DebugMenuForExample.Instance.onGUI += DebugMenu;
            }
        }

        private void OnDisable()
        {
            if (SyncNet.IsServer)
            {
                var menu = DebugMenuForExample.Instance;
                if (menu != null)
                {
                    menu.onGUI -= DebugMenu;
                }
            }
        }
        
        public void DebugMenu()
        {
            GUILayout.Label(nameof(SyncParamPerformanceTest));
            GUIUtil.Indent(() =>
            {
                objectCount = GUIUtil.Field(objectCount, nameof(objectCount));
                if (GUILayout.Button("Spawn"))
                {
                    SpawnObjects(objectCount);
                    ChangeShowGuiUnitIndex(0);
                }

                if (_exampleUnits.Any())
                {
                    var idx = GUIUtil.Slider(showGuiUnitIdx, 0, _exampleUnits.Count-1, nameof(showGuiUnitIdx));
                    if (idx != showGuiUnitIdx)
                    {
                        ChangeShowGuiUnitIndex(idx);
                    }
                }
            });
        }

        [ClientRpc]
        private void SpawnObjects(int count)
        {
            foreach (var unit in _exampleUnits)
            {
                Destroy(unit.gameObject);
            }
         
            _exampleUnits.Clear();

            for (var i = 0; i < count; ++i)
            {
                var go = Instantiate(prefab, transform, true);
                go.name += i;
                var unit = go.GetComponent<SyncParamExample>();
                unit.enabled = false; // disable gui
                
                _exampleUnits.Add(unit);
            }
        }

        [ClientRpc]
        void ChangeShowGuiUnitIndex(int index)
        {
            if (showGuiUnitIdx < _exampleUnits.Count)
            {
                _exampleUnits[showGuiUnitIdx].enabled = false;
            }

            _exampleUnits[index].enabled = true;
            showGuiUnitIdx = index;
        }
    }
}