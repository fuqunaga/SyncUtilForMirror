using System;
using Mirror;
using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(SceneSelectorForExample))]
    public class NetworkManagerControllerExample : NetworkManagerController, IElementCreator
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

        private SceneSelectorForExample _sceneSelector;

        private SceneSelectorForExample SceneSelector =>
            _sceneSelector != null ? _sceneSelector : _sceneSelector = GetComponent<SceneSelectorForExample>();
        

        protected override void OnGUINetworkSetting()
        {
            SceneSelector.DebugMenu();

            networkAddress = GUIUtil.Field(networkAddress, "Host Address");
            networkPort = GUIUtil.Field(networkPort, "Host Port");
            autoConnect = GUIUtil.Field(autoConnect, "AutoConnect");
            autoConnectInterval = GUIUtil.Field(autoConnectInterval, "AutoConnectInterval");
        }

        public Element CreateElement(LabelElement label)
        {
            return UI.Column(
                UI.Dropdown("Scene",
                    () => SceneSelector.idx,
                    SceneSelector.onlineSceneNames),
                UI.Field(() => networkAddress),
                UI.Field(() => networkPort),
                UI.Field(() => autoConnect),
                UI.Field(() => autoConnectInterval),
                UI.Button(nameof(BootType.Host), () => StartNetwork(BootType.Host)),
                UI.Button(nameof(BootType.Server), () => StartNetwork(BootType.Server)),
                UI.Button(nameof(BootType.Client), () => StartNetwork(BootType.Client))
            );
        }
    }
}
