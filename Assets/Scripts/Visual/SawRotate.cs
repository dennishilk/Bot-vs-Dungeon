using UnityEngine;

public class SawRotate : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 260f;

    private void Update()
    {
        transform.Rotate(axis.normalized, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
