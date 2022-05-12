using UnityEngine;
using Mirror;

namespace SyncUtil
{
    public interface IInstanceRandom
    {
        CustomRandom Rand { get; }
    }
}
