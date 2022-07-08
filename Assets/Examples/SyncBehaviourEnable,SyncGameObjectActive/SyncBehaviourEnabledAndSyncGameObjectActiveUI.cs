using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncBehaviourEnabledAndSyncGameObjectActiveUI : ExampleUIBase
    {
        public Behaviour cubeAutRotY;
        public GameObject sphereGameObject;
        protected override Element CreateElement()
        {
            return ExampleTemplate(
                    @"
Sync Behaviour.enabled, GameObject.active.

1. Put SyncParamManager at the scene.
2. Attach SyncBehaviourEnabled/SyncGameObjectActive component to a GameObject.
3. Set target at the SyncBehaviourEnabled/SyncGameObjectActive on the Inspector.
",
                    UI.Field(() => cubeAutRotY.enabled),
                    UI.Field(() => sphereGameObject.activeSelf, active => sphereGameObject.SetActive(active))
            );
        }
    }
}