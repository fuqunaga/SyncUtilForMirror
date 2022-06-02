using UnityEngine;
using Mirror;

namespace SyncUtil
{
    /// <summary>
    /// Random, which returns the same value on server and client
    /// </summary>
    public class InstanceRandom : NetworkBehaviour, IInstanceRandom
    {
        [SyncVar]
        protected int seed;

        [ServerCallback]
        public void Awake()
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        CustomRandom _rand;
        public CustomRandom Rand => _rand ??= new CustomRandom(seed);
    }
}
