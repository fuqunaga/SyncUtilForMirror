using RosettaUI;

namespace SyncUtil.Example
{
    public class InstanceRandomUI : ExampleUIBase
    {
        protected override Element CreateElement()
        {
            return ExampleTemplate(
                    @"Deterministic random per instance.

1. Attach the InstanceRandom component to the target GameObject.
2. Spawn the GameObject because of InstanceRandom inherits NetworkBehaviour.
3. Use InstanceRandom.CustomRandom from script. It will return same value for all clients."
            );
        }
    }
}