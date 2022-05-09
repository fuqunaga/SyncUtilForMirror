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

        public override string NetworkAddress => networkAddress;
        public override int NetworkPort => networkPort;
        public override BootType Boot => bootType;
        public override bool AutoConnect => autoConnect;
        public override float AutoConnectInterval => autoConnectInterval;

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
