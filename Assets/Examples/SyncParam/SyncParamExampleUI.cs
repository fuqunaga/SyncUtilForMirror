using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncParamExampleUI : MonoBehaviour
    {
        public Vector2 windowPosition;

        private void Start()
        {
            var window = UI.Window(
                UI.Label(nameof(SyncParamExample)),
                UI.Page(
                    UI.FieldIfObjectFound<SyncParamExample>()
                    )
            ).SetPosition(windowPosition).Open();

            var root = FindObjectOfType<RosettaUIRoot>();
            root.Build(window);
        }
    }
}