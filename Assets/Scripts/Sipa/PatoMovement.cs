using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PatoMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maximumMoveSpeed = 45f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 120f;

    [Header("Boundaries")]
    [SerializeField] private Transform leftBoundary;
    [SerializeField] private Transform rightBoundary;

    [Header("Animation")]
    [SerializeField] private SipaAnimation sipaAnimation;

    private Rigidbody rb;

    private float keyboardInput;
    private float mobileInput;
    private float currentMoveSpeed;

    private float originalY;
    private float originalZ;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private bool gameplayEnabled;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;

        originalY = transform.position.y;
        originalZ = transform.position.z;

        rb.useGravity = false;
        rb.isKinematic = true;

        rb.interpolation =
            RigidbodyInterpolation.Interpolate;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezePositionZ |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;
    }


    private void Update()
    {
        if (!gameplayEnabled)
        {
            keyboardInput = 0f;
            mobileInput = 0f;
            return;
        }

        if (SipaManager.Instance != null &&
            SipaManager.Instance.IsGameFinished)
        {
            keyboardInput = 0f;
            mobileInput = 0f;
            return;
        }

        // =====================================
        // NEW INPUT SYSTEM - KEYBOARD
        // =====================================

        keyboardInput = 0f;

        if (Keyboard.current != null)
        {
            bool leftPressed =
                Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed;

            bool rightPressed =
                Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed;


            if (leftPressed && !rightPressed)
            {
                keyboardInput = -1f;
            }
            else if (rightPressed && !leftPressed)
            {
                keyboardInput = 1f;
            }
        }


        if (Mathf.Abs(keyboardInput) > 0.01f)
        {
            if (SipaManager.Instance != null)
            {
                SipaManager.Instance
                    .NotifyPlayerAction();
            }
        }
    }


    private void FixedUpdate()
    {
        if (!gameplayEnabled)
            return;


        float input = Mathf.Clamp(
            keyboardInput + mobileInput,
            -1f,
            1f
        );


        float targetSpeed =
            input * maximumMoveSpeed;


        float speedChange =
            Mathf.Abs(input) > 0.01f
                ? acceleration
                : deceleration;


        currentMoveSpeed =
            Mathf.MoveTowards(
                currentMoveSpeed,
                targetSpeed,
                speedChange *
                Time.fixedDeltaTime
            );


        Vector3 nextPosition =
            rb.position;


        nextPosition.x +=
            currentMoveSpeed *
            Time.fixedDeltaTime;


        // =====================================
        // LEFT BOUNDARY
        // =====================================

        if (leftBoundary != null)
        {
            nextPosition.x =
                Mathf.Max(
                    nextPosition.x,
                    leftBoundary.position.x
                );
        }


        // =====================================
        // RIGHT BOUNDARY
        // =====================================

        if (rightBoundary != null)
        {
            nextPosition.x =
                Mathf.Min(
                    nextPosition.x,
                    rightBoundary.position.x
                );
        }


        nextPosition.y = originalY;
        nextPosition.z = originalZ;


        rb.MovePosition(
            nextPosition
        );


        // =====================================
        // ANIMATION
        // =====================================

        if (sipaAnimation != null)
        {
            float animationSpeed = 0f;

            if (maximumMoveSpeed > 0f)
            {
                animationSpeed =
                    Mathf.Abs(
                        currentMoveSpeed
                    ) /
                    maximumMoveSpeed;
            }


            sipaAnimation.SetMovementSpeed(
                animationSpeed
            );
        }
    }


    // =========================================
    // GAMEPLAY
    // =========================================

    public void SetGameplayEnabled(
        bool enabledState)
    {
        enabled = true;

        gameplayEnabled =
            enabledState;


        if (!gameplayEnabled)
        {
            StopInput();
        }
    }


    public void StopGameplayMovement()
    {
        gameplayEnabled = false;

        StopInput();
    }


    // =========================================
    // RESET
    // =========================================

    public void ResetMovement()
    {
        ResetMovement(
            startingPosition,
            startingRotation
        );
    }


    public void ResetMovement(
        Vector3 resetPosition,
        Quaternion resetRotation)
    {
        enabled = true;

        gameplayEnabled = false;

        StopInput();


        originalY =
            resetPosition.y;

        originalZ =
            resetPosition.z;


        if (rb != null)
        {
            rb.position =
                resetPosition;

            rb.rotation =
                resetRotation;
        }
        else
        {
            transform.position =
                resetPosition;

            transform.rotation =
                resetRotation;
        }
    }


    private void StopInput()
    {
        keyboardInput = 0f;
        mobileInput = 0f;

        currentMoveSpeed = 0f;


        if (sipaAnimation != null)
        {
            sipaAnimation
                .SetMovementSpeed(0f);
        }
    }


    // =========================================
    // MOBILE LEFT BUTTON
    // =========================================

    public void MoveLeft()
    {
        if (!gameplayEnabled)
            return;


        if (SipaManager.Instance != null)
        {
            SipaManager.Instance
                .NotifyPlayerAction();
        }


        mobileInput = -1f;
    }


    // =========================================
    // MOBILE RIGHT BUTTON
    // =========================================

    public void MoveRight()
    {
        if (!gameplayEnabled)
            return;


        if (SipaManager.Instance != null)
        {
            SipaManager.Instance
                .NotifyPlayerAction();
        }


        mobileInput = 1f;
    }


    // =========================================
    // MOBILE BUTTON RELEASE
    // =========================================

    public void StopMove()
    {
        mobileInput = 0f;
    }


    private void OnDisable()
    {
        StopInput();
    }
}