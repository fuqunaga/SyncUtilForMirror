using UnityEngine;

namespace SyncUtil
{
    public class SyncBehaviourEnabled : SyncParamSingle<Behaviour, bool>
    {
        protected override bool GetParam() => target.enabled;

        protected override void SetParam(bool value) => target.enabled = value;
    }
}