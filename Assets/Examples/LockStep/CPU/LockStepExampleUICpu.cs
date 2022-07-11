using RosettaUI;

namespace SyncUtil.Example
{
    public class LockStepExampleUICpu : LockStepExampleUIBase
    {
        protected override Element ExtraUIOnServer()
        {
            var lockStepExample = lockStepExampleBase as LockStepExample;
            
            return UI.Column(
                UI.Field("Lorentz Attractor", () => lockStepExample.lorentzParameter),
                UI.Button("ResetPosition", () => lockStepExample.needResetPosition = true)
            );
        }
    }
}