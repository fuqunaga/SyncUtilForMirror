using System.Linq;
using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class ExampleUIBase : MonoBehaviour
    {
        public Vector2 windowPosition = Vector2.up * 200f;

        private Element _window;
        
        private void OnEnable()
        {
            var element = CreateElement();
            if (element == null) return;

            _window = UI.Window(
                UI.Label(gameObject.scene.name),
                element
            ).SetPosition(windowPosition);

            var root = FindFirstObjectByType<RosettaUIRoot>();
            root.Build(_window);
        }

        private void OnDisable()
        {
            _window?.DetachView();
        }

        protected virtual Element CreateElement()
        {
            return null;
        }

        protected static Element ExampleTemplate(string description, params Element[] elements)
        {
            return UI.Page(
                elements.Prepend(
                    UI.Box(
                        UI.Page(
                            UI.Label(description)
                        )
                    )
                )
            );
        }
    }
}