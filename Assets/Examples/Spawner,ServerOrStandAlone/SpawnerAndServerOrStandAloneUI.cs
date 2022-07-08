using RosettaUI;

namespace SyncUtil.Example
{
    public class SpawnerAndServerOrStandAloneUI : ExampleUIBase
    {
        protected override Element CreateElement()
        {
            return ExampleTemplate(
                    @"
<b>Spawner</b>

Register prefabs to NetworkManager's spawn prefabs.
Spawn prefabs when the server is started.


<b>ServerOrStandAlone</b>

Deactivate child GameObjects when server or standalone.
"
            );
        }
    }
}