using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class SyncParamExampleUI : MonoBehaviour
    {
        public Vector2 windowPosition;

        private Element _window;
        
        private void OnEnable()
        {
            _window = UI.Window(
                UI.Label(gameObject.scene.name),
                UI.Page(
                    UI.FieldIfObjectFound<SyncParamPerformanceTest>(),
                    UI.FieldIfObjectFound<SyncParamExample>()
                    )
            ).SetPosition(windowPosition).Open();

            var root = FindObjectOfType<RosettaUIRoot>();
            root.Build(_window);
        }

        private void OnDisable()
        {
            _window.Destroy();
        }
    }
}