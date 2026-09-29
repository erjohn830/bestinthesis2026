using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(10000)]
public class TutorialCameraFocus : MonoBehaviour
{
    [Header("Main Camera")]
    public Camera targetCamera;

    [Header("Gameplay Camera")]
    public CameraFollow gameplayCameraFollow;

    [Header("Fixed Camera Points")]
    public Transform menuCameraPoint;
    public Transform playerCameraPoint;
    public Transform tayaCameraPoint;

    [Header("Field Of View")]
    public float menuFOV = 60f;
    public float playerFOV = 50f;
    public float tayaFOV = 45f;

    [Header("Transition")]
    public float transitionDuration = 0.7f;

    private Coroutine cameraRoutine;

    private bool tutorialCameraActive = false;
    private bool transitioning = false;

    private Transform currentLockedPoint;
    private float currentLockedFOV = 60f;

    void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    // =========================================
    // KEEP TUTORIAL CAMERA LOCKED
    // =========================================

    void LateUpdate()
    {
        if (!tutorialCameraActive)
            return;

        if (transitioning)
            return;

        if (targetCamera == null)
            return;

        if (currentLockedPoint == null)
            return;

        targetCamera.transform.position =
            currentLockedPoint.position;

        targetCamera.transform.rotation =
            currentLockedPoint.rotation;

        targetCamera.fieldOfView =
            currentLockedFOV;
    }

    // =========================================
    // ENTER TUTORIAL MODE
    // =========================================

    public void EnterTutorialMode()
    {
        tutorialCameraActive = true;

        if (gameplayCameraFollow != null)
        {
            gameplayCameraFollow.enabled = false;
        }
    }

    // =========================================
    // TITLE / MENU CAMERA
    // =========================================

    public void StartMenuCamera()
    {
        EnterTutorialMode();

        SnapToPoint(
            menuCameraPoint,
            menuFOV
        );
    }

    // =========================================
    // START OF INSTRUCTION
    //
    // THIS IS THE MISSING FUNCTION
    // =========================================

    public void StartInstructionCamera()
    {
        EnterTutorialMode();

        // Start instruction directly
        // on the Player camera angle.
        SnapToPoint(
            playerCameraPoint,
            playerFOV
        );
    }

    // =========================================
    // PLAYER PAGE
    // =========================================

    public void FocusPlayer()
    {
        EnterTutorialMode();

        MoveToPoint(
            playerCameraPoint,
            playerFOV
        );
    }

    // =========================================
    // TAYA PAGE
    // =========================================

    public void FocusTaya()
    {
        EnterTutorialMode();

        MoveToPoint(
            tayaCameraPoint,
            tayaFOV
        );
    }

    // =========================================
    // TIMING BAR / JUMP / SCORE / TIMER
    //
    // KEEP CURRENT CAMERA VIEW
    // =========================================

    public void HoldCurrentView()
    {
        EnterTutorialMode();

        // Do nothing.
        // Camera stays on current point.
    }

    // =========================================
    // OPTIONAL OLD FUNCTION
    //
    // Keeps compatibility with older code.
    // =========================================

    public void ResetToDefault()
    {
        EnterTutorialMode();

        MoveToPoint(
            menuCameraPoint,
            menuFOV
        );
    }

    // =========================================
    // START REAL GAME CAMERA
    // =========================================

    public void StartGameplayCamera()
    {
        StopCurrentTransition();

        tutorialCameraActive = false;
        transitioning = false;
        currentLockedPoint = null;

        if (targetCamera != null)
        {
            targetCamera.fieldOfView =
                menuFOV;
        }

        if (gameplayCameraFollow != null)
        {
            gameplayCameraFollow.enabled =
                true;

            gameplayCameraFollow
                .SnapToTarget();
        }
    }

    // =========================================
    // SNAP CAMERA
    // =========================================

    void SnapToPoint(
        Transform point,
        float fov)
    {
        if (point == null ||
            targetCamera == null)
        {
            return;
        }

        StopCurrentTransition();

        currentLockedPoint = point;
        currentLockedFOV = fov;

        targetCamera.transform.position =
            point.position;

        targetCamera.transform.rotation =
            point.rotation;

        targetCamera.fieldOfView =
            fov;
    }

    // =========================================
    // SMOOTH MOVE
    // =========================================

    void MoveToPoint(
        Transform point,
        float fov)
    {
        if (point == null ||
            targetCamera == null)
        {
            return;
        }

        StopCurrentTransition();

        currentLockedPoint = point;
        currentLockedFOV = fov;

        cameraRoutine =
            StartCoroutine(
                MoveCameraRoutine(
                    point,
                    fov
                )
            );
    }

    // =========================================
    // CAMERA TRANSITION
    // =========================================

    IEnumerator MoveCameraRoutine(
        Transform point,
        float targetFOV)
    {
        transitioning = true;

        Vector3 startPosition =
            targetCamera.transform.position;

        Quaternion startRotation =
            targetCamera.transform.rotation;

        float startFOV =
            targetCamera.fieldOfView;

        Vector3 targetPosition =
            point.position;

        Quaternion targetRotation =
            point.rotation;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            float t =
                elapsed /
                transitionDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            targetCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            targetCamera.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            targetCamera.fieldOfView =
                Mathf.Lerp(
                    startFOV,
                    targetFOV,
                    t
                );

            elapsed +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        targetCamera.transform.position =
            targetPosition;

        targetCamera.transform.rotation =
            targetRotation;

        targetCamera.fieldOfView =
            targetFOV;

        transitioning = false;
        cameraRoutine = null;
    }

    // =========================================
    // STOP OLD TRANSITION
    // =========================================

    void StopCurrentTransition()
    {
        if (cameraRoutine != null)
        {
            StopCoroutine(cameraRoutine);
            cameraRoutine = null;
        }

        transitioning = false;
    }
}