using System;
using Mirror;
using RosettaUI;

namespace SyncUtil.Example
{
    public class LockStepUI : ExampleUIBase
    {
        public LockStep lockStep;
        
        protected override Element CreateElement()
        {
            return ExampleTemplate(
                    @"Deterministic LockStep framework.

1. Attach the LockStep component to the target GameObject.
2. Set LockStep.GetDataFunc/StepFunc.
    GetDataFunc will call on the server. Returns a message required for step.
    SetFunc will call on the client. Step based on message.
3. Spawn the GameObject because LockStep inherits NetworkBehaviour.",
                    SyncNet.IsServer
                        ? new[]
                        {
                            UI.FieldReadOnly("Connection Count", () => NetworkServer.connections.Count),
                            UI.DynamicElementIf(
                                () => NetworkServer.connections.Count >= 2,
                                () => UI.Row(
                                    UI.Button("CheckConsistency", () => lockStep.StartCheckConsistency()),
                                    UI.DynamicElementOnStatusChanged(
                                        () => lockStep.GetLastConsistencyData(),
                                        data => UI.FieldReadOnly($"Step{data.stepCount}", () => data.consistency)
                                    )
                                )
                            )
                        }
                        : Array.Empty<Element>()
            );
        }
    }
}