using UnityEngine;

public class TeleportOnCollision : MonoBehaviour
{
    [SerializeField] Transform _teleportTo;

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position = _teleportTo.position + transform.up * 100.0f;
    }
}
