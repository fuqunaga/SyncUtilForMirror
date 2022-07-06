using System.Collections.Generic;
using System.Linq;
using Mirror;
using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncParamPerformanceTest : NetworkBehaviour, IElementCreator
    {
        public GameObject prefab;
        public int objectCount = 100;
        private readonly List<SyncParamExample> _exampleUnits = new();
        private int _displayObjectIdx;
        
        public Element CreateElement(LabelElement label)
        {
            return UI.Column(
                UI.Field(() => objectCount),
                UI.Button("Spawn", GenerateObjectAndDisplayFirstObject),
                UI.Space().SetHeight(20f),
                UI.DynamicElementOnStatusChanged(
                    () => _exampleUnits.Count(),
                    count => UI.Slider("Display Object Index", 
                        readValue: () => _displayObjectIdx,
                        writeValue: ChangeShowGuiUnitIndex,
                        max: count-1)
                ),
                UI.Space().SetHeight(20f)
            );
        }

        void GenerateObjectAndDisplayFirstObject()
        {
            if (SyncNet.IsServer)
            {
                GenerateObjects(objectCount);
                ChangeShowGuiUnitIndex(0);
            }
        }
        

        [ClientRpc]
        private void GenerateObjects(int count)
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
            if (_displayObjectIdx < _exampleUnits.Count)
            {
                _exampleUnits[_displayObjectIdx].enabled = false;
            }

            _exampleUnits[index].enabled = true;
            _displayObjectIdx = index;
        }
    }
}