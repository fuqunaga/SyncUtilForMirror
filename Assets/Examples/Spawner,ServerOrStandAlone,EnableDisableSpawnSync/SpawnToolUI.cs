using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SpawnerAndServerOrStandAloneUI : ExampleUIBase
    {
        public GameObject enableDisableSpawnSyncObject;
        
        protected override Element CreateElement()
        {
            return UI.Column(
                ExampleTemplate(
                    @"
<b>Spawner</b>

Register prefabs to NetworkManager's spawn prefabs.
Spawn prefabs when the server is started.


<b>ServerOrStandAlone</b>

Deactivate child GameObjects when server or standalone.


<b>EnableDisableSpawnSync</b>

Spawns the object when it is enabled and unspawns it when it is disabled.
"
                ),
                UI.Field("EnableDisableSpawnSync Object activeSelf", 
                    () => enableDisableSpawnSyncObject.activeSelf,
                    v => enableDisableSpawnSyncObject.SetActive(v))
            );
        }
    }
}