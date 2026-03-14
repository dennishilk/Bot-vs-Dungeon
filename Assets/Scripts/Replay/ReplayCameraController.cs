using UnityEngine;

public class ReplayCameraController : MonoBehaviour
{
    [SerializeField] private Camera replayCamera;
    [SerializeField] private Transform gameplayCamera;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float followHeight = 6f;
    [SerializeField] private float followDistance = 6f;
    [SerializeField] private float defaultZoom = 60f;
    [SerializeField] private float highlightZoom = 42f;
    [SerializeField] private float slowMotionFactor = 0.35f;
    [SerializeField] private float cameraShakeDamping = 0.8f;

    private CameraBehaviorType _activeBehavior = CameraBehaviorType.FollowBot;
    private Vector3 _focusPoint;
    private Vector3 _velocity;
    private bool _cinematicModeEnabled;

    public bool CinematicModeEnabled => _cinematicModeEnabled;

    public void SetCinematicMode(bool enabled)
    {
        _cinematicModeEnabled = enabled;
        if (replayCamera != null)
        {
            replayCamera.enabled = enabled;
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.gameObject.SetActive(!enabled);
        }

        if (!enabled)
        {
            Time.timeScale = 1f;
        }
    }

    public void SetBehavior(CameraBehaviorType behavior, Vector3 focusPoint, bool slowMotion)
    {
        _activeBehavior = behavior;
        _focusPoint = focusPoint;

        if (replayCamera != null)
        {
            replayCamera.fieldOfView = Mathf.Lerp(replayCamera.fieldOfView, slowMotion ? highlightZoom : defaultZoom, 0.75f);
        }

        Time.timeScale = slowMotion ? slowMotionFactor : 1f;
    }

    public void SetFocusPoint(Vector3 focusPoint)
    {
        _focusPoint = focusPoint;
    }

    private void LateUpdate()
    {
        if (!_cinematicModeEnabled || replayCamera == null)
        {
            return;
        }

        Vector3 targetPosition = ComputeTargetPosition(_activeBehavior, _focusPoint);
        replayCamera.transform.position = Vector3.SmoothDamp(
            replayCamera.transform.position,
            targetPosition,
            ref _velocity,
            1f / Mathf.Max(0.01f, moveSpeed));

        Vector3 lookTarget = _focusPoint + Vector3.up * 0.7f;
        Quaternion targetRotation = Quaternion.LookRotation((lookTarget - replayCamera.transform.position).normalized, Vector3.up);
        replayCamera.transform.rotation = Quaternion.Slerp(replayCamera.transform.rotation, targetRotation, Time.unscaledDeltaTime * rotationSpeed);

        _velocity *= cameraShakeDamping;
    }

    private Vector3 ComputeTargetPosition(CameraBehaviorType behavior, Vector3 focus)
    {
        return behavior switch
        {
            CameraBehaviorType.ZoomToTrap => focus + new Vector3(0f, followHeight * 0.75f, -followDistance * 0.5f),
            CameraBehaviorType.FocusOnDeath => focus + new Vector3(0f, followHeight * 0.6f, -followDistance * 0.35f),
            CameraBehaviorType.WideDungeonView => focus + new Vector3(0f, followHeight * 1.6f, -followDistance * 1.35f),
            CameraBehaviorType.GoalCelebrationShot => focus + new Vector3(-followDistance * 0.4f, followHeight * 0.7f, -followDistance * 0.45f),
            CameraBehaviorType.IntroPan => focus + new Vector3(-followDistance * 0.9f, followHeight * 1.1f, -followDistance * 0.9f),
            _ => focus + new Vector3(0f, followHeight, -followDistance)
        };
    }
}
