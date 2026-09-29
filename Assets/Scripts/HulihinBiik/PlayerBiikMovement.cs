using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerBiikMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maximumSpeed = 5.2f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float deceleration = 18f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float inputDeadZone = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDampTime = 0.12f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Failed Catch Knockback")]
    [SerializeField] private float knockbackDistance = 4.5f;
    [SerializeField] private float knockbackHeight = 1.8f;
    [SerializeField] private float knockbackDuration = 0.65f;
    [SerializeField] private float recoveryDelay = 0.7f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 horizontalVelocity;

    private float verticalVelocity;
    private bool canMove = false;
    private bool isBeingKnockedBack;

    public bool CanMove => canMove;
    public bool IsBeingKnockedBack => isBeingKnockedBack;
    public float MaximumSpeed => maximumSpeed;

    public float CurrentSpeed
    {
        get
        {
            Vector3 flatVelocity = horizontalVelocity;
            flatVelocity.y = 0f;
            return flatVelocity.magnitude;
        }
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
        UpdateMovementAnimation();
    }

    // Called by Player Input because the action is named "Move".
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void HandleMovement()
    {
        if (isBeingKnockedBack)
        {
            return;
        }

        Vector3 desiredDirection = Vector3.zero;

        if (canMove && moveInput.magnitude >= inputDeadZone)
        {
            desiredDirection = GetCameraRelativeDirection(moveInput);
        }

        Vector3 desiredVelocity =
            desiredDirection * maximumSpeed;

        float changeSpeed;

        if (desiredVelocity.sqrMagnitude >
            horizontalVelocity.sqrMagnitude)
        {
            changeSpeed = acceleration;
        }
        else
        {
            changeSpeed = deceleration;
        }

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            desiredVelocity,
            changeSpeed * Time.deltaTime
        );

        Vector3 movement = horizontalVelocity;
        movement.y = verticalVelocity;

        characterController.Move(
            movement * Time.deltaTime
        );

        if (canMove &&
            horizontalVelocity.sqrMagnitude > 0.02f)
        {
            RotateTowards(horizontalVelocity);
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (cameraTransform == null)
        {
            return new Vector3(
                input.x,
                0f,
                input.y
            ).normalized;
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        if (right.sqrMagnitude < 0.001f)
        {
            right = Vector3.right;
        }

        forward.Normalize();
        right.Normalize();

        return (
            forward * input.y +
            right * input.x
        ).normalized;
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleGravity()
    {
        if (isBeingKnockedBack)
        {
            return;
        }

        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null)
        {
            return;
        }

        float normalizedSpeed = 0f;

        if (maximumSpeed > 0f &&
            canMove &&
            !isBeingKnockedBack)
        {
            normalizedSpeed =
                CurrentSpeed / maximumSpeed;
        }

        animator.SetFloat(
            "Speed",
            Mathf.Clamp01(normalizedSpeed),
            animationDampTime,
            Time.deltaTime
        );
    }

    public void StopMovement()
    {
        canMove = false;
        moveInput = Vector2.zero;
        horizontalVelocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    public void ResumeMovement()
    {
        if (!isBeingKnockedBack)
        {
            canMove = true;
        }
    }

    public void FacePosition(Vector3 worldPosition)
    {
        Vector3 direction =
            worldPosition - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }
  
    public void MoveToCatchPoint(
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        bool wasEnabled =
            characterController.enabled;

        characterController.enabled = false;

        transform.position = worldPosition;
        transform.rotation = worldRotation;

        characterController.enabled = wasEnabled;
    }

    public void PlayHoldAnimation()
    {
        StopMovement();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("HoldPig", 0, 0f);
        }
    }

    public void ReturnToMovementAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            animator.CrossFade(
                "Movement",
                0.15f
            );

            animator.SetFloat("Speed", 0f);
        }
    }
    
    public void SetMaximumSpeed(float newSpeed)
    {
        maximumSpeed = newSpeed;
    }

    public void PlayWinAnimation()
    {
        StopMovement();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.CrossFade("Win", 0.12f);
        }
    }

    public void PlayLoseAnimation()
    {
        StopMovement();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.CrossFade("Lose", 0.12f);
        }
    }

    public void FreezeHoldPose(
        float normalizedTime = 0.95f)
    {
        if (animator == null)
        {
            return;
        }

        animator.Play(
            "HoldPig",
            0,
            normalizedTime
        );

        animator.speed = 0f;
    }

    public void RestoreAnimatorSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void KnockAwayFrom(Vector3 pigPosition)
    {
        if (isBeingKnockedBack)
        {
            return;
        }

        StartCoroutine(
            KnockbackRoutine(pigPosition)
        );
    }

    private IEnumerator KnockbackRoutine(
        Vector3 pigPosition)
    {
        isBeingKnockedBack = true;
        canMove = false;

        moveInput = Vector2.zero;
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetFloat("Speed", 0f);
            animator.CrossFade("Lose", 0.08f);
        }

        Vector3 startPosition =
            transform.position;

        Vector3 awayDirection =
            transform.position - pigPosition;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.01f)
        {
            awayDirection = -transform.forward;
        }

        awayDirection.Normalize();

        Vector3 finalPosition =
            startPosition +
            awayDirection * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / knockbackDuration
            );

            Vector3 horizontalPosition =
                Vector3.Lerp(
                    startPosition,
                    finalPosition,
                    progress
                );

            float arcHeight =
                Mathf.Sin(progress * Mathf.PI) *
                knockbackHeight;

            Vector3 targetPosition =
                horizontalPosition +
                Vector3.up * arcHeight;

            Vector3 movement =
                targetPosition -
                transform.position;

            characterController.Move(movement);

            yield return null;
        }

        yield return new WaitForSeconds(
            recoveryDelay
        );

        verticalVelocity = groundedGravity;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.CrossFade(
                "Movement",
                0.15f
            );

            animator.SetFloat("Speed", 0f);
        }

        isBeingKnockedBack = false;
        canMove = true;
    }
}