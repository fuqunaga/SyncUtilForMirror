using UnityEngine;

namespace SyncUtil
{
    public class SyncGameObjectActive : SyncParamSingle<GameObject, bool>
    {
        protected override bool GetParam() => target.activeSelf;

        protected override void SetParam(bool value) => target.SetActive(value);
    }
}