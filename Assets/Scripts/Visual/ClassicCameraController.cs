using UnityEngine;

public class ClassicCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform focusTarget;
    [SerializeField] private Vector3 focusOffset = new(0f, 0.4f, 0f);

    [Header("Isometric Framing")]
    [SerializeField] private float distance = 16f;
    [SerializeField] private float height = 13f;
    [SerializeField] private float yaw = 45f;
    [SerializeField] private float pitch = 38f;

    [Header("Smoothing")]
    [SerializeField] private float moveSmooth = 6f;
    [SerializeField] private float rotateSmooth = 9f;

    private Vector3 _velocity;

    private void LateUpdate()
    {
        if (focusTarget == null)
        {
            return;
        }

        Vector3 lookPoint = focusTarget.position + focusOffset;
        Quaternion isoRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 back = isoRotation * Vector3.back;

        Vector3 desiredPos = lookPoint + back * distance + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _velocity, 1f / Mathf.Max(0.01f, moveSmooth));

        Quaternion desiredRot = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * rotateSmooth);
    }
}
