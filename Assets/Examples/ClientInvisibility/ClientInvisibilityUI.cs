using System.Collections.Generic;
using System.Linq;
using Mirror;
using RosettaUI;
using UnityEngine;

namespace SyncUtil.Example
{
    public class ClientInvisibilityUI : ExampleUIBase
    {
        public List<NetworkIdentity> prefabs;
        private readonly Dictionary<NetworkIdentity, Stack<NetworkIdentity>> _instances = new();

        protected override Element CreateElement()
        {
            return ExampleTemplate(
                    @"
Specify the invisibility per Client to a GameObject.

1. Put ClientName to a GameObject.
2. Set the client name to the ClientName.myName.
3. Put ClientInvisibilityManagement to the same GameObject.
4. Attach ClientInvisibility component to the target GameObject.
5. Set ClientInvisibility's Invisible Client Name List.
6. The target GameObject will not spawn on the client with the specified name.
",
                    UI.HelpBox(@"
Spawn messages will be executed
that are received before ClientInvisibilityManagement.Awake().
",
                        HelpBoxType.Warning),
                    SyncNet.IsServerOrStandAlone
                        ? UI.Column(
                            prefabs.Select((prefab, i) => UI.Row(
                                UI.Label(prefab.name, LabelType.Prefix),
                                UI.Button("Spawn", () => SpawnObject(prefab, i)),
                                UI.Button("Destroy", () => DestroyObject(prefab))
                                    .RegisterUpdateCallback(e => e.SetInteractable(GetInstanceStack(prefab).Any()))
                                )
                            )
                        )
                        : UI.DynamicElementIfObjectFound<ClientName>(clientName => UI.Field(() => clientName.myName))
            );
        }

        Stack<NetworkIdentity> GetInstanceStack(NetworkIdentity prefab)
        {
            if (!_instances.TryGetValue(prefab, out var stack))
            {
                _instances[prefab] = stack = new();
            }

            return stack;
        }

        void SpawnObject(NetworkIdentity prefab, float y)
        {
            var stack = GetInstanceStack(prefab);
            var ni = Instantiate(prefab);
            var go = ni.gameObject;
            go.transform.position = new Vector3(
               stack.Count - 3f,
                -y,
                0f
            );

            SyncNet.Spawn(go);
            
            stack.Push(ni);
        }

        void DestroyObject(NetworkIdentity prefab)
        {
            var stack = GetInstanceStack(prefab);
            if (stack.TryPop(out var ni))
            {
                SyncNet.Destroy(ni.gameObject);
            }
        }
    }
}