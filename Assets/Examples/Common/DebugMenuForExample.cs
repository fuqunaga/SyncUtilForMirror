using System;
using UnityEngine;

namespace SyncUtil.Example
{
    public class DebugMenuForExample : MonoBehaviour
    {
        #region Singleton
        
        static DebugMenuForExample _instance;
        public static DebugMenuForExample Instance => (_instance != null ? _instance : (_instance = FindObjectOfType<DebugMenuForExample>()));
        
        #endregion

        NetworkManagerController _networkManagerController;

        public event Action onGUI;


        void Start()
        {
            _networkManagerController = FindObjectOfType<NetworkManagerController>();
        }

        private void OnGUI()
        {
            if (SyncNet.IsActive)
            {
                _networkManagerController.DebugMenu();

                onGUI?.Invoke();
            }
        }
    }
}