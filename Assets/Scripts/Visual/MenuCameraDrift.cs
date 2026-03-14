using UnityEngine;

public class MenuCameraDrift : MonoBehaviour
{
    [SerializeField] private Vector3 driftAmplitude = new(0.25f, 0.1f, 0.25f);
    [SerializeField] private float driftSpeed = 0.18f;
    [SerializeField] private Vector3 lookAtOffset = new(0f, -0.1f, 0f);

    private Vector3 _startPos;
    private Quaternion _startRot;

    private void Awake()
    {
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

    private void Update()
    {
        float t = Time.time * driftSpeed;
        transform.position = _startPos + new Vector3(
            Mathf.Sin(t) * driftAmplitude.x,
            Mathf.Sin(t * 1.2f) * driftAmplitude.y,
            Mathf.Cos(t * 0.85f) * driftAmplitude.z);

        Quaternion driftLook = Quaternion.LookRotation((_startPos + lookAtOffset) - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(_startRot, driftLook, 0.45f);
    }
}
