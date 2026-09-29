using UnityEngine;

public class PatinteroCameraDirector : MonoBehaviour
{
    // =========================================================
    // INTRO CAMERA
    // =========================================================

    [Header("INTRO CAMERA POINTS")]
    [SerializeField] private Transform teamAStart;
    [SerializeField] private Transform teamAEnd;
    [SerializeField] private Transform teamBStart;
    [SerializeField] private Transform teamBEnd;

    [Header("INTRO SETTINGS")]
    [SerializeField] private float introDuration = 5f;

    // =========================================================
    // THIRD PERSON CAMERA
    // =========================================================

    [Header("THIRD PERSON CAMERA")]
    [SerializeField] private float followDistance = 4.5f;
    [SerializeField] private float followHeight = 1.8f;
    [SerializeField] private float lookHeight = 0.5f;
    [SerializeField] private float followSmoothness = 8f;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float targetSmoothness = 14f;

    // =========================================================
    // CAMERA COLLISION
    // =========================================================

    [Header("CAMERA COLLISION")]
    [SerializeField] private bool useCameraCollision = true;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private LayerMask collisionMask = ~0;

    // =========================================================
    // RUNTIME SMOOTHING
    // =========================================================

    private Vector3 smoothedTargetPosition;
    private bool gameplayCameraInitialized;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        gameplayCameraInitialized = false;

        if (teamAStart != null)
        {
            transform.position = teamAStart.position;
            transform.rotation = teamAStart.rotation;
        }
    }

    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;

        // Never read Networked round properties before Fusion
        // has finished spawning the round manager.
        if (match == null || !match.NetworkReady)
        {
            return;
        }

        if (match.CurrentPhase == PatinteroGameplayPhase.Intro)
        {
            gameplayCameraInitialized = false;
            UpdateIntroCamera(match);
            return;
        }

        UpdateGameplayCamera();
    }

    // =========================================================
    // INTRO CAMERA
    // =========================================================

    private void UpdateIntroCamera(
        PatinteroGameplayRoundManager match)
    {
        if (teamAStart == null ||
            teamAEnd == null ||
            teamBStart == null ||
            teamBEnd == null)
        {
            return;
        }

        float remaining =
            match.GetPhaseTimeRemaining();

        float elapsed = Mathf.Clamp(
            introDuration - remaining,
            0f,
            introDuration
        );

        float halfDuration = Mathf.Max(
            0.01f,
            introDuration * 0.5f
        );

        // Team A intro: left -> right.
        if (elapsed < halfDuration)
        {
            float t = Mathf.Clamp01(
                elapsed / halfDuration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(
                teamAStart.position,
                teamAEnd.position,
                t
            );

            transform.rotation = Quaternion.Slerp(
                teamAStart.rotation,
                teamAEnd.rotation,
                t
            );

            return;
        }

        // Team B intro: right -> left.
        float teamBElapsed =
            elapsed - halfDuration;

        float teamBT = Mathf.Clamp01(
            teamBElapsed / halfDuration
        );

        teamBT = Mathf.SmoothStep(
            0f,
            1f,
            teamBT
        );

        transform.position = Vector3.Lerp(
            teamBStart.position,
            teamBEnd.position,
            teamBT
        );

        transform.rotation = Quaternion.Slerp(
            teamBStart.rotation,
            teamBEnd.rotation,
            teamBT
        );
    }

    // =========================================================
    // THIRD PERSON CAMERA
    // =========================================================

    private void UpdateGameplayCamera()
    {
        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;

        if (player == null)
        {
            gameplayCameraInitialized = false;
            return;
        }

        Transform target = player.CameraTarget;

        if (target == null)
        {
            gameplayCameraInitialized = false;
            return;
        }

        Vector3 rawTargetPosition = target.position;

        // -----------------------------------------------------
        // YAW
        // -----------------------------------------------------

        float yaw;

        if (!player.IsDefender)
        {
            yaw =
                player.transform.eulerAngles.y +
                PatinteroMobileInput.CameraYaw;
        }
        else
        {
            yaw = player.transform.eulerAngles.y;
        }

        Quaternion yawRotation = Quaternion.Euler(
            0f,
            yaw,
            0f
        );

        Vector3 cameraForward =
            yawRotation * Vector3.forward;

        // -----------------------------------------------------
        // FIRST GAMEPLAY FRAME
        // -----------------------------------------------------

        if (!gameplayCameraInitialized)
        {
            smoothedTargetPosition =
                rawTargetPosition;

            Vector3 initialDesiredPosition =
                smoothedTargetPosition -
                cameraForward * followDistance +
                Vector3.up * followHeight;

            initialDesiredPosition = ApplyCameraCollision(
                smoothedTargetPosition,
                initialDesiredPosition
            );

            transform.position =
                initialDesiredPosition;

            // IMPORTANT:
            // This has a different name from the later
            // lookPosition variable. This fixes CS0136.
            Vector3 initialLookPosition =
                smoothedTargetPosition +
                Vector3.up * lookHeight;

            Vector3 initialLookDirection =
                initialLookPosition -
                transform.position;

            if (initialLookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(
                    initialLookDirection,
                    Vector3.up
                );
            }

            gameplayCameraInitialized = true;
            return;
        }

        // -----------------------------------------------------
        // SMOOTH TARGET
        // -----------------------------------------------------

        float targetLerp =
            1f - Mathf.Exp(
                -targetSmoothness * Time.deltaTime
            );

        smoothedTargetPosition = Vector3.Lerp(
            smoothedTargetPosition,
            rawTargetPosition,
            targetLerp
        );

        Vector3 desiredPosition =
            smoothedTargetPosition -
            cameraForward * followDistance +
            Vector3.up * followHeight;

        desiredPosition = ApplyCameraCollision(
            smoothedTargetPosition,
            desiredPosition
        );

        // -----------------------------------------------------
        // SMOOTH CAMERA POSITION
        // -----------------------------------------------------

        float positionLerp =
            1f - Mathf.Exp(
                -followSmoothness * Time.deltaTime
            );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionLerp
        );

        // -----------------------------------------------------
        // LOOK AT PLAYER
        // -----------------------------------------------------

        Vector3 lookPosition =
            smoothedTargetPosition +
            Vector3.up * lookHeight;

        Vector3 lookDirection =
            lookPosition -
            transform.position;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    lookDirection,
                    Vector3.up
                );

            float rotationLerp =
                1f - Mathf.Exp(
                    -rotationSmoothness * Time.deltaTime
                );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationLerp
            );
        }
    }

    // =========================================================
    // CAMERA COLLISION
    // =========================================================

    private Vector3 ApplyCameraCollision(
        Vector3 targetPosition,
        Vector3 desiredPosition)
    {
        if (!useCameraCollision)
        {
            return desiredPosition;
        }

        Vector3 from =
            targetPosition +
            Vector3.up * lookHeight;

        Vector3 direction =
            desiredPosition - from;

        float distance =
            direction.magnitude;

        if (distance <= 0.01f)
        {
            return desiredPosition;
        }

        direction.Normalize();

        if (Physics.SphereCast(
            from,
            collisionRadius,
            direction,
            out RaycastHit hit,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            return
                hit.point -
                direction * collisionRadius;
        }

        return desiredPosition;
    }

    // =========================================================
    // DEBUG PATH
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        if (teamAStart != null &&
            teamAEnd != null)
        {
            Gizmos.DrawLine(
                teamAStart.position,
                teamAEnd.position
            );
        }

        if (teamBStart != null &&
            teamBEnd != null)
        {
            Gizmos.DrawLine(
                teamBStart.position,
                teamBEnd.position
            );
        }
    }
}
