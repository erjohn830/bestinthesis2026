using UnityEngine;

public class MainMenuCameraController : MonoBehaviour
{
    [Header("Camera Positions")]
    [SerializeField] private Transform middlePosition;
    [SerializeField] private Transform storyModePosition;
    [SerializeField] private Transform multiplayerPosition;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 4f;

    private Transform targetPosition;

    private void Start()
    {
        if (middlePosition == null)
        {
            Debug.LogError("Middle Position is not assigned.");
            return;
        }

        targetPosition = middlePosition;
        transform.SetPositionAndRotation(
            middlePosition.position,
            middlePosition.rotation
        );
    }

    private void Update()
    {
        if (targetPosition == null)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition.position,
            moveSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetPosition.rotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void MoveToStoryMode()
    {
        if (storyModePosition != null)
            targetPosition = storyModePosition;
    }

    public void MoveToMultiplayer()
    {
        if (multiplayerPosition != null)
            targetPosition = multiplayerPosition;
    }

    public void MoveToMiddle()
    {
        if (middlePosition != null)
            targetPosition = middlePosition;
    }
}