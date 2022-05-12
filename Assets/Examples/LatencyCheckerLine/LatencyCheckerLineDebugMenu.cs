using System.Collections;
using UnityEngine;

namespace SyncUtil.Example
{
    public class LatencyCheckerLineDebugMenu : MonoBehaviour
    {
        LatencyCheckerLine _latencyCheckerLine;

        private void OnEnable()
        {
            DebugMenuForExample.Instance.onGUI += DebugMenu;
        }

        private void OnDisable()
        {
            DebugMenuForExample.Instance.onGUI -= DebugMenu;
        }

        void Start()
        {
            _latencyCheckerLine = FindObjectOfType<LatencyCheckerLine>();
            StartCoroutine(SetLatencyCheckerLineEnable());
        }


        IEnumerator SetLatencyCheckerLineEnable()
        {
            yield return new WaitForEndOfFrame();
            foreach(var data in _latencyCheckerLine.DataList)
            {
                data.enable = true;
            }
        }

        // Update is called once per frame
        void DebugMenu()
        {
            _latencyCheckerLine.DebugMenu();
        }
    }
}