using UnityEngine;

public class PaloseboCamera : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerMover;

    [Header("Gameplay Camera Position")]
    [Tooltip("Create an empty GameObject called GameplayCameraPoint and place it at your desired starting camera angle.")]
    [SerializeField] private Transform gameplayCameraPoint;

    [Header("Camera Follow")]
    [SerializeField] private float smoothTime = 0.15f;

    [Tooltip("Extra vertical adjustment while following.")]
    [SerializeField] private float verticalOffset = 0f;

    [Header("Follow Down")]
    [SerializeField] private bool followPlayerDown = true;

    // Original/title camera
    private Vector3 titlePosition;
    private Quaternion titleRotation;

    // Gameplay camera
    private Vector3 gameplayStartPosition;
    private Quaternion gameplayRotation;

    // Player starting height
    private float gameplayStartPlayerY;

    private float yVelocity;

    private bool following = false;
    private bool initialized = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        InitializeCamera();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    private void InitializeCamera()
    {
        if (initialized)
            return;

        // Save current camera position for Title/Instruction.
        titlePosition = transform.position;
        titleRotation = transform.rotation;

        if (playerMover != null)
        {
            gameplayStartPlayerY = playerMover.position.y;
        }

        yVelocity = 0f;
        following = false;

        initialized = true;

        Debug.Log("PALOSOBO CAMERA: INITIALIZED");
    }

    // =========================================================
    // FOLLOW PLAYER
    // =========================================================

    private void LateUpdate()
    {
        if (!initialized)
            return;

        if (!following)
            return;

        if (playerMover == null)
            return;

        // How far player climbed/slipped from starting point.
        float playerYDifference =
            playerMover.position.y -
            gameplayStartPlayerY;

        float targetY =
            gameplayStartPosition.y +
            playerYDifference +
            verticalOffset;

        // Optional: prevent camera from moving below
        // the original gameplay starting height.
        if (!followPlayerDown)
        {
            targetY =
                Mathf.Max(
                    targetY,
                    gameplayStartPosition.y
                );
        }

        float newY =
            Mathf.SmoothDamp(
                transform.position.y,
                targetY,
                ref yVelocity,
                smoothTime
            );

        // IMPORTANT:
        // X and Z remain exactly at the gameplay camera position.
        Vector3 newPosition =
            gameplayStartPosition;

        newPosition.y = newY;

        transform.position = newPosition;

        // IMPORTANT:
        // Never change the gameplay camera angle.
        transform.rotation = gameplayRotation;
    }

    // =========================================================
    // START GAMEPLAY CAMERA
    // =========================================================

    public void StartFollowing()
    {
        if (!initialized)
            InitializeCamera();

        if (playerMover == null)
        {
            Debug.LogError(
                "PALOSOBO CAMERA: PlayerMover is not assigned!"
            );

            return;
        }

        // =====================================================
        // MOVE TO THE CAMERA ANGLE YOU CREATED
        // =====================================================

        if (gameplayCameraPoint != null)
        {
            transform.position =
                gameplayCameraPoint.position;

            transform.rotation =
                gameplayCameraPoint.rotation;
        }

        // Save this EXACT gameplay camera position/rotation.
        gameplayStartPosition =
            transform.position;

        gameplayRotation =
            transform.rotation;

        // Save player's Y exactly when gameplay begins.
        gameplayStartPlayerY =
            playerMover.position.y;

        yVelocity = 0f;

        following = true;

        Debug.Log(
            "PALOSOBO CAMERA: GAMEPLAY CAMERA STARTED"
        );
    }

    // =========================================================
    // STOP FOLLOW
    // =========================================================

    public void StopFollowing()
    {
        following = false;
        yVelocity = 0f;

        Debug.Log(
            "PALOSOBO CAMERA: FOLLOW STOPPED"
        );
    }

    // =========================================================
    // RESET TO TITLE CAMERA
    // =========================================================

    public void ResetCamera()
    {
        if (!initialized)
            InitializeCamera();

        following = false;
        yVelocity = 0f;

        transform.position =
            titlePosition;

        transform.rotation =
            titleRotation;

        Debug.Log(
            "PALOSOBO CAMERA: RESET TO TITLE"
        );
    }

    // =========================================================
    // CHECK
    // =========================================================

    public bool IsFollowing()
    {
        return following;
    }
}