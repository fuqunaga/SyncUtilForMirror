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
        private int _displayItemIdx;
        
        public Element CreateElement(LabelElement label)
        {
            return UI.Column(
                UI.Field(() => objectCount),
                UI.Button("Spawn", GenerateObjectAndDisplayFirstObject),
                UI.Space().SetHeight(20f),
                UI.DynamicElementOnStatusChanged(
                    () => _exampleUnits.Count(),
                    count => UI.Slider("Display Item Index", 
                        readValue: () => _displayItemIdx,
                        writeValue: ChangeShowGuiUnitIndex,
                        max: count-1)
                ),
                UI.Space().SetHeight(20f),
                UI.DynamicElementIf(
                    () => 0 <= _displayItemIdx && _displayItemIdx < _exampleUnits.Count,
                    () => UI.Column(
                        UI.Label(() => $"Item {_displayItemIdx}"),
                        UI.Indent(
                            UI.Field(null, () => _exampleUnits[_displayItemIdx])
                        )
                    )
                )
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
            _displayItemIdx = index;
        }
    }
}