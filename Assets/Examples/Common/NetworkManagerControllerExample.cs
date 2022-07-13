using RosettaUI;

namespace SyncUtil.Example
{
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


        public Element CreateElement(LabelElement label)
        {
            return UI.Column(
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
