using System.Collections.Generic;
using System.Linq;
using Mirror;
using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    [RequireComponent(typeof(RosettaUIRoot))]
    public class CommonMenu : MonoBehaviour
    {
        private ClientHeartBeat _clientHeartBeat;

        private void Start()
        {
            _clientHeartBeat = FindObjectOfType<ClientHeartBeat>();
            
            var root = GetComponent<RosettaUIRoot>();
            root.Build(CreateElement());
        }

        private Element CreateElement()
        {
            return UI.Window(nameof(SyncUtil),
                UI.Page(
                    UI.DynamicElementOnStatusChanged(
                        () => SyncNet.IsActive,
                        active => active ? OnlineMenu() : OfflineMenu()
                    )
                )
            );
        }


        private Element OnlineMenu()
        {
            return UI.Column(
                UI.FieldReadOnly("Status",
                    () => SyncNet.IsHost
                        ? "Host"
                        : (SyncNet.IsServer ? "Server" : (SyncNet.IsClient ? "Client" : "StandAlone"))),
                UI.Button("Disconnect", () => NetworkManager.singleton.StopHost()),
                UI.Space().SetHeight(20f),
                Times()
            );

            Element Times()
            {
                return UI.Fold(nameof(Times),
                    UI.FieldReadOnly($"{nameof(SyncNet)}.{nameof(SyncNet.Time)}",
                        () => $"{SyncNet.Time:0.000}"),
                    UI.FieldReadOnly($"{nameof(SyncNet)}.{nameof(SyncNet.NetworkTime)}",
                        () => $"{SyncNet.NetworkTime:0.000}"),
                    ClientHeartBeatInfo()
                );

                Element ClientHeartBeatInfo()
                {
                    if (!SyncNet.IsServer || _clientHeartBeat == null) return null;

                    return UI.Column(
                        UI.Row(
                            UI.Label("Clients"),
                            UI.Space(),
                            UI.Field("currentFrame", () => Time.frameCount)
                        ),
                        UI.Box(
                            UI.DynamicElementOnStatusChanged(
                                () => GetConnectionIdsWithoutHost().Count(),
                                _ => UI.Column(
                                    GetConnectionIdsWithoutHost()
                                        .Select((connectionId, idx) => UI.Label(() =>
                                        {
                                            var address = NetworkServer.connections.TryGetValue(connectionId, out var conn)
                                                ? conn.address
                                                : "No connection";
                                            
                                            var info = _clientHeartBeat.GetHeartBeatInfo(connectionId);
                                            return $"{idx} Addr:{address} ConnId:{connectionId} {info}";
                                        }))
                                )
                            ).SetMinWidth(500f)
                        )
                    );

                    IEnumerable<int> GetConnectionIdsWithoutHost() =>
                        NetworkServer.connections.Keys.Where(id => id != 0);
                }
            }
        }

        private static Element OfflineMenu()
        {
            return UI.Column(
                UI.FieldIfObjectFound<SceneSelectorForExample>(),
                UI.FieldIfObjectFound<NetworkManagerControllerExample>()
            );
        }

    }
}