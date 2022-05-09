using UnityEngine;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(SceneSelectorForExample))]
    public class NetworkManagerControllerSample : NetworkManagerController
    {
        public string networkAddress = "localhost";
        public int networkPort = 7777;
        public BootType bootType = BootType.Manual;
        public bool autoConnect = true;
        public float autoConnectInterval = 10f;

        public override string NetworkAddress { get { return networkAddress; } }
        public override int NetworkPort { get { return networkPort; } }
        public override BootType Boot { get { return bootType; } }
        public override bool AutoConnect { get { return autoConnect; } }
        public override float AutoConnectInterval { get { return autoConnectInterval; } }

        SceneSelectorForExample _sceneSelector;

        public override void Start()
        {
            base.Start();
            _sceneSelector = GetComponent<SceneSelectorForExample>();
        }

        protected override void OnGUINetworkSetting()
        {
            _sceneSelector.DebugMenu();

            networkAddress = GUIUtil.Field(networkAddress, "Host Address");
            networkPort = GUIUtil.Field(networkPort, "Host Port");
            
            DebugMenuInternal();
        }

        protected override void DebugMenuInternal()
        {
            autoConnect = GUIUtil.Field(autoConnect, "AutoConnect");
            autoConnectInterval = GUIUtil.Field(autoConnectInterval, "AutoConnectInterval");
        }
    }
}
