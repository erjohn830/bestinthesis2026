using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControllerBiik : MonoBehaviour
{
    [Header("Normal Follow")]
    [SerializeField] private Transform player;

    [SerializeField] private Vector3 normalOffset =
        new Vector3(0f, 14f, -13f);

    [SerializeField] private Vector3 normalLookOffset =
        new Vector3(0f, 1f, 0f);

    [SerializeField] private float normalFollowSpeed = 4f;
    [SerializeField] private float normalRotationSpeed = 5f;
    [SerializeField] private float normalFieldOfView = 55f;

    [Header("Combined Catch View")]
    [SerializeField] private Vector3 catchOffset =
        new Vector3(0f, 3.5f, -7f);

    [SerializeField] private float catchFieldOfView = 50f;
    [SerializeField] private float catchMoveSpeed = 5f;
    [SerializeField] private float catchRotationSpeed = 6f;

    [Header("Win View")]
    [SerializeField] private Transform winCameraPoint;
    [SerializeField] private Transform winLookTarget;

    [SerializeField] private float winMoveSpeed = 4f;
    [SerializeField] private float winRotationSpeed = 5f;
    [SerializeField] private float winFieldOfView = 45f;

    private Camera cameraComponent;

    private Transform combinedCatchRoot;

    private bool catchViewActive;
    private bool showingWinCamera;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        if (player == null)
        {
            PlayerBiikMovement foundPlayer =
                FindFirstObjectByType<PlayerBiikMovement>();

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
        }

        if (winLookTarget == null)
        {
            winLookTarget = player;
        }
    }

    private void LateUpdate()
    {
        if (showingWinCamera)
        {
            UpdateWinView();
            return;
        }

        if (catchViewActive &&
            combinedCatchRoot != null)
        {
            UpdateCombinedCatchView();
            return;
        }

        UpdateNormalFollow();
    }

    private void UpdateNormalFollow()
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetPoint =
            player.position + normalLookOffset;

        Vector3 desiredPosition =
            player.position + normalOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            normalFollowSpeed * Time.deltaTime
        );

        LookSmoothlyAt(
            targetPoint,
            normalRotationSpeed
        );

        cameraComponent.fieldOfView = Mathf.Lerp(
            cameraComponent.fieldOfView,
            normalFieldOfView,
            normalFollowSpeed * Time.deltaTime
        );
    }

    private void UpdateCombinedCatchView()
    {
        Vector3 center =
            GetCombinedCatchCenter();

        Vector3 desiredPosition =
            center + catchOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            catchMoveSpeed * Time.deltaTime
        );

        LookSmoothlyAt(
            center,
            catchRotationSpeed
        );

        cameraComponent.fieldOfView = Mathf.Lerp(
            cameraComponent.fieldOfView,
            catchFieldOfView,
            catchMoveSpeed * Time.deltaTime
        );
    }

    private Vector3 GetCombinedCatchCenter()
    {
        if (combinedCatchRoot == null)
        {
            return transform.position;
        }

        Renderer[] renderers =
            combinedCatchRoot.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return combinedCatchRoot.position;
        }

        Bounds bounds =
            renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }

        Vector3 center = bounds.center;

        // Aim slightly higher than exact middle
        // so Player/Pig stay centered visually.
        center.y += 0.4f;

        return center;
    }

    private void UpdateWinView()
    {
        if (winCameraPoint == null)
        {
            showingWinCamera = false;
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            winCameraPoint.position,
            winMoveSpeed * Time.deltaTime
        );

        Vector3 lookPosition;

        if (winLookTarget != null)
        {
            lookPosition =
                winLookTarget.position +
                Vector3.up * 1.2f;
        }
        else
        {
            lookPosition =
                winCameraPoint.position +
                winCameraPoint.forward;
        }

        LookSmoothlyAt(
            lookPosition,
            winRotationSpeed
        );

        cameraComponent.fieldOfView = Mathf.Lerp(
            cameraComponent.fieldOfView,
            winFieldOfView,
            winMoveSpeed * Time.deltaTime
        );
    }

    private void LookSmoothlyAt(
        Vector3 targetPoint,
        float speed)
    {
        Vector3 direction =
            targetPoint - transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            speed * Time.deltaTime
        );
    }

    public void StartCombinedCatchView(
        Transform combinedModel)
    {
        if (combinedModel == null)
        {
            Debug.LogWarning(
                "CombinedCatch is missing.",
                this
            );

            return;
        }

        showingWinCamera = false;

        combinedCatchRoot =
            combinedModel;

        catchViewActive = true;
    }

    public void ReturnToNormalView()
    {
        showingWinCamera = false;
        catchViewActive = false;

        combinedCatchRoot = null;
    }

    public void ShowWinCamera()
    {
        catchViewActive = false;
        combinedCatchRoot = null;

        showingWinCamera = true;
    }

    public void StopWinCamera()
    {
        showingWinCamera = false;
    }
}