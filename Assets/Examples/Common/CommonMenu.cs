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
        
        void Start()
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
                        IsNetworkActive,
                        active => active ? OnlineMenu() : OfflineMenu()
                    )
                )
            );

            Element OnlineMenu()
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
                            UI.Label("Clients"),
                            UI.Box(
                                UI.DynamicElementOnStatusChanged(
                                    () => GetConnectionIdsWithoutHost().Count(),
                                    _ => UI.Column(
                                        GetConnectionIdsWithoutHost()
                                            .Select(connectionId => UI.Label(() =>
                                            {
                                                var info = _clientHeartBeat.GetHeartBeatInfo(connectionId);
                                                return $"ConnId: {connectionId} {info}";
                                            }))
                                    )
                                ).SetWidth(500f)
                            )
                        );

                        IEnumerable<int> GetConnectionIdsWithoutHost() => NetworkServer.connections.Keys.Where(id => id != 0);
                    }
                }
            }

            static Element OfflineMenu()
            {
                return UI.FieldIfObjectFound<NetworkManagerControllerExample>();
            }

            static bool IsNetworkActive()
            {
                var manager = NetworkManager.singleton;
                return manager != null & manager.isNetworkActive;
            }
        }
    }
}