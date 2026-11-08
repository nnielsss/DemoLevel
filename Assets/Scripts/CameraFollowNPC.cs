
using UnityEngine;

public class CameraFollowNPC : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -10f);
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);

    [Header("Smooth Movement")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Bepaal waar de camera moet staan.
        Vector3 desiredPosition = target.position + offset;

        // Laat de camera soepel naar de NPC bewegen.
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Laat de camera naar de NPC kijken.
        Vector3 lookPosition = target.position + lookOffset;
        Vector3 direction = lookPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
