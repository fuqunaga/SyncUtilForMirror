using UnityEngine;

public class HideInHierarchy : MonoBehaviour
{
    private void Awake()
    {
        gameObject.hideFlags |= HideFlags.HideInHierarchy;
    }
}
