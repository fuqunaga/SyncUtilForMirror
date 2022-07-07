using RosettaUI;

namespace SyncUtil.Example
{
    public class SyncParamExampleUI : ExampleUIBase
    {
        public SyncParamExample syncParamExample;
        protected override Element CreateElement()
        {
            return UI.Page(
                UI.Box(
                    UI.Label(@"Sync non-NetworkBehaviour parameters.

1. Put SyncParamManager at the scene.
2. Attach SyncParam component to a GameObject.
3. Set target parameters at the SyncParam on the Inspector.
"
                    )
                    ),
                UI.Field(null, () => syncParamExample)
            );
        }
    }
}