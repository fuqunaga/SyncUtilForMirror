﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SyncUtil.Example
{
    public class PreRenderingDebugMenu : MonoBehaviour
    {
        PreRenderingWithLatencyCheckerSample _preRendering;
        LatencyCheckerLine _laytencyCheckerLine;


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
            _preRendering = FindObjectOfType<PreRenderingWithLatencyCheckerSample>();

            _laytencyCheckerLine = FindObjectOfType<LatencyCheckerLine>();
            StartCoroutine(SetLaytencyCheckerLineEnable());
        }


        IEnumerator SetLaytencyCheckerLineEnable()
        {
            yield return new WaitForEndOfFrame();
            _laytencyCheckerLine.Datas.ForEach(data => data.enable = true);

        }

        // Update is called once per frame
        void DebugMenu()
        {
            _preRendering.DebugMenu();
            _laytencyCheckerLine.DebugMenu();
        }
    }
}