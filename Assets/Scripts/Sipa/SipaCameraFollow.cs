using UnityEngine;

public class SipaCameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform pato;

    [Header("Camera Limits")]
    [SerializeField] private Transform leftCameraLimit;
    [SerializeField] private Transform rightCameraLimit;

    [Header("Follow Settings")]
    [Tooltip("0 means follow only Player. 0.5 means center between Player and Pato.")]
    [Range(0f, 1f)]
    [SerializeField] private float patoInfluence = 0.35f;

    [SerializeField] private float smoothTime = 0.18f;

    [Tooltip("Maximum distance the Pato can pull the camera away from the Player.")]
    [SerializeField] private float maximumPatoCameraOffset = 60f;

    [Header("Vertical Follow")]
    [SerializeField] private bool followPatoHeight = false;

    [Range(0f, 1f)]
    [SerializeField] private float verticalPatoInfluence = 0.15f;

    [SerializeField] private float minimumCameraY;
    [SerializeField] private float maximumCameraY;

    private Vector3 originalCameraPosition;
    private Vector3 smoothVelocity;

    private void Awake()
    {
        originalCameraPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = originalCameraPosition;

        float targetX = player.position.x;

        if (pato != null)
        {
            float patoOffsetFromPlayer =
                pato.position.x - player.position.x;

            patoOffsetFromPlayer = Mathf.Clamp(
                patoOffsetFromPlayer,
                -maximumPatoCameraOffset,
                maximumPatoCameraOffset
            );

            targetX += patoOffsetFromPlayer * patoInfluence;
        }

        if (leftCameraLimit != null &&
            rightCameraLimit != null)
        {
            targetX = Mathf.Clamp(
                targetX,
                leftCameraLimit.position.x,
                rightCameraLimit.position.x
            );
        }

        targetPosition.x = targetX;

        if (followPatoHeight && pato != null)
        {
            float targetY = Mathf.Lerp(
                originalCameraPosition.y,
                pato.position.y,
                verticalPatoInfluence
            );

            targetPosition.y = Mathf.Clamp(
                targetY,
                minimumCameraY,
                maximumCameraY
            );
        }
        else
        {
            targetPosition.y = originalCameraPosition.y;
        }

        // Preserve the original camera depth.
        targetPosition.z = originalCameraPosition.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothVelocity,
            smoothTime
        );
    }
}