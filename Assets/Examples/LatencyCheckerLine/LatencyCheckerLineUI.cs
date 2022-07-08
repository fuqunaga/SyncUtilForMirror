using System.Linq;
using RosettaUI;

namespace SyncUtil.Example
{
    public class LatencyCheckerLineUI : ExampleUIBase
    {
        public LatencyCheckerLine latencyCheckerLine;
        
        protected override Element CreateElement()
        {
            return ExampleTemplate(
                @"Display SyncNet.Time/SyncNet.NetworkTime as straight-line positions. 
 So to visually compare differences among clients.

1. Attach DebugDraw,LatencyCheckerLine component to the Camera GameObject.
2. Set LatencyCheckerLine.timeEnable,networkTimeEnable == true to display lines.

Support only legacy rendering pipeline currently. URP, HDRP is not.
",
                UI.Field(null, () => latencyCheckerLine),
                UI.DynamicElementOnStatusChanged(
                    () => latencyCheckerLine.DataList.Count,
                    _ => UI.Column(
                        latencyCheckerLine.DataList.Select(data => UI.Fold(data.camera.name,
                                UI.Field(nameof(data.enable), () => data.enable),
                                UI.Field(nameof(data.mode), () => data.mode)
                            ).Open()
                        )
                    )
                )
            );
        }
    }
}