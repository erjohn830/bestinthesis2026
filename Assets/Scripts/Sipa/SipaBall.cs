using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SipaBall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform visual;

    [Header("Bounce Height")]
    [Tooltip("Minimum height the Pato can reach after a kick.")]
    [SerializeField] private float minimumBounceHeight = 100f;

    [Tooltip("Maximum height the Pato can reach after a kick.")]
    [SerializeField] private float maximumBounceHeight = 130f;

    [Header("Bounce Timing")]
    [Tooltip("Minimum time before reaching the top of the bounce.")]
    [SerializeField] private float minimumTimeToPeak = 0.55f;

    [Tooltip("Maximum time before reaching the top of the bounce.")]
    [SerializeField] private float maximumTimeToPeak = 0.70f;

    [Header("Left / Right Movement")]
    [Tooltip("Minimum horizontal speed after a kick.")]
    [SerializeField] private float minimumSideSpeed = 35f;

    [Tooltip("Maximum horizontal speed after a kick.")]
    [SerializeField] private float maximumSideSpeed = 55f;

    [Tooltip("Minimum horizontal speed. Prevents straight-up kicks.")]
    [SerializeField] private float minimumRequiredSideSpeed = 25f;

    [Header("Side Boundaries")]
    [Tooltip("Optional left boundary Transform.")]
    [SerializeField] private Transform leftBoundary;

    [Tooltip("Optional right boundary Transform.")]
    [SerializeField] private Transform rightBoundary;

    [Tooltip("Used if Left Boundary is not assigned.")]
    [SerializeField] private float leftLimit = -100f;

    [Tooltip("Used if Right Boundary is not assigned.")]
    [SerializeField] private float rightLimit = 100f;

    [Tooltip("Distance before the edge where the Pato is redirected.")]
    [SerializeField] private float edgePadding = 5f;

    [Tooltip("Horizontal speed when redirected from an edge.")]
    [SerializeField] private float edgeReturnSpeed = 45f;

    [Header("Falling")]
    [SerializeField] private float maximumFallingSpeed = 450f;

    [Header("Kick")]
    [SerializeField] private float kickCooldown = 0.12f;

    [Header("Visual Rotation")]
    [SerializeField] private bool rotateVisual = true;
    [SerializeField] private float visualRotationSpeed = 300f;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private float customGravity = 100f;
    private float lastKickTime = -100f;

    private bool gameplayEnabled = false;
    private bool frozen = true;

    private float lockedZ;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;

        lockedZ = startingPosition.z;

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (!gameplayEnabled || frozen)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 velocity = rb.linearVelocity;

        // Apply our custom gravity.
        velocity.y -= customGravity * Time.fixedDeltaTime;

        // Limit falling speed.
        if (velocity.y < -maximumFallingSpeed)
            velocity.y = -maximumFallingSpeed;

        // Never allow almost-straight vertical movement
        // while the Pato is actively moving.
        if (Mathf.Abs(velocity.x) > 0.01f &&
            Mathf.Abs(velocity.x) < minimumRequiredSideSpeed)
        {
            velocity.x =
                Mathf.Sign(velocity.x) *
                minimumRequiredSideSpeed;
        }

        rb.linearVelocity = velocity;

        KeepInsideSideBoundaries();
        KeepOnGameplayPlane();
    }


    // =========================================================
    // VISUAL ROTATION
    // =========================================================

    private void Update()
    {
        if (!gameplayEnabled || frozen)
            return;

        if (!rotateVisual || visual == null)
            return;

        if (rb == null)
            return;

        if (rb.linearVelocity.sqrMagnitude < 0.1f)
            return;

        visual.Rotate(
            Vector3.forward,
            visualRotationSpeed * Time.deltaTime,
            Space.Self
        );
    }


    // =========================================================
    // KICK
    // =========================================================

    public bool TryKick(Vector3 kickPosition)
    {
        if (!gameplayEnabled)
            return false;

        if (frozen)
            return false;

        if (Time.time < lastKickTime + kickCooldown)
            return false;

        lastKickTime = Time.time;

        // -------------------------
        // REALISTIC BOUNCE
        // -------------------------

        float bounceHeight =
            Random.Range(
                minimumBounceHeight,
                maximumBounceHeight
            );

        float timeToPeak =
            Random.Range(
                minimumTimeToPeak,
                maximumTimeToPeak
            );

        // h = 1/2 g t²
        customGravity =
            (2f * bounceHeight) /
            (timeToPeak * timeToPeak);

        // v = g * t
        float upwardSpeed =
            customGravity * timeToPeak;


        // -------------------------
        // LEFT OR RIGHT
        // -------------------------

        float sideSpeed =
            Random.Range(
                minimumSideSpeed,
                maximumSideSpeed
            );

        sideSpeed =
            Mathf.Max(
                sideSpeed,
                minimumRequiredSideSpeed
            );


        float leftX = GetLeftBoundary();
        float rightX = GetRightBoundary();

        float currentX = transform.position.x;

        float direction;


        // If Pato is near LEFT edge,
        // force it RIGHT.
        if (currentX <= leftX + edgePadding)
        {
            direction = 1f;
        }

        // If Pato is near RIGHT edge,
        // force it LEFT.
        else if (currentX >= rightX - edgePadding)
        {
            direction = -1f;
        }

        // Otherwise randomly go left or right.
        else
        {
            direction =
                Random.value < 0.5f
                    ? -1f
                    : 1f;
        }


        // -------------------------
        // APPLY VELOCITY
        // -------------------------

        rb.linearVelocity = new Vector3(
            sideSpeed * direction,
            upwardSpeed,
            0f
        );

        // Rigidbody is kinematic in some parts of your game,
        // so only reset angular velocity if physics is active.
        if (!rb.isKinematic)
            rb.angularVelocity = Vector3.zero;


        // -------------------------
        // SCORE
        // -------------------------

        if (SipaManager.Instance != null)
        {
            SipaManager.Instance.RegisterSuccessfulKick();

            Debug.Log(
                "SIPA SUCCESS! Score: " +
                SipaManager.Instance.Score
            );
        }
        else
        {
            Debug.LogWarning(
                "SipaBall cannot find SipaManager.Instance."
            );
        }

        return true;
    }


    // =========================================================
    // EDGE CONTROL
    // =========================================================

    private void KeepInsideSideBoundaries()
    {
        float leftX = GetLeftBoundary();
        float rightX = GetRightBoundary();

        Vector3 position = rb.position;
        Vector3 velocity = rb.linearVelocity;


        // =========================
        // LEFT EDGE
        // =========================

        if (position.x <= leftX)
        {
            position.x = leftX + 0.1f;

            velocity.x =
                Mathf.Abs(
                    Mathf.Max(
                        Mathf.Abs(velocity.x),
                        edgeReturnSpeed
                    )
                );
        }


        // =========================
        // RIGHT EDGE
        // =========================

        else if (position.x >= rightX)
        {
            position.x = rightX - 0.1f;

            velocity.x =
                -Mathf.Abs(
                    Mathf.Max(
                        Mathf.Abs(velocity.x),
                        edgeReturnSpeed
                    )
                );
        }


        rb.position = position;
        rb.linearVelocity = velocity;
    }


    private float GetLeftBoundary()
    {
        if (leftBoundary != null)
            return leftBoundary.position.x;

        return leftLimit;
    }


    private float GetRightBoundary()
    {
        if (rightBoundary != null)
            return rightBoundary.position.x;

        return rightLimit;
    }


    // =========================================================
    // KEEP PATO IN 2D GAMEPLAY PLANE
    // =========================================================

    private void KeepOnGameplayPlane()
    {
        Vector3 position = rb.position;

        position.z = lockedZ;

        rb.position = position;

        Vector3 velocity = rb.linearVelocity;

        velocity.z = 0f;

        rb.linearVelocity = velocity;
    }


    // =========================================================
    // RELEASE PATO
    // =========================================================

    public void ReleasePato()
    {
        if (!gameplayEnabled)
            return;

        frozen = false;

        rb.isKinematic = false;
        rb.useGravity = false;

        Debug.Log("Pato released.");
    }


    // =========================================================
    // FREEZE AT START
    // =========================================================

    public void FreezeAtStart()
    {
        frozen = true;

        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;

        if (!rb.isKinematic)
            rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;
    }


    // Compatibility method for your manager.
    public void FreezeBall()
    {
        FreezeAtStart();
    }


    // =========================================================
    // ENABLE GAMEPLAY
    // =========================================================

    public void SetGameplayEnabled(bool enabled)
    {
        gameplayEnabled = enabled;

        if (!enabled)
        {
            frozen = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                if (!rb.isKinematic)
                    rb.angularVelocity = Vector3.zero;
            }
        }
    }


    // Compatibility with your existing SipaManager.
    public void SetGameActive(bool active)
    {
        SetGameplayEnabled(active);
    }


    // =========================================================
    // STOP BALL
    // =========================================================

    public void StopBall()
    {
        gameplayEnabled = false;
        frozen = true;

        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;

        if (!rb.isKinematic)
            rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;
    }


    // =========================================================
    // RESET BALL
    // =========================================================

    public void ResetBall()
    {
        if (rb == null)
            return;

        frozen = true;

        rb.isKinematic = true;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;

        transform.position = startingPosition;
        transform.rotation = startingRotation;

        rb.position = startingPosition;
        rb.rotation = startingRotation;

        lockedZ = startingPosition.z;

        lastKickTime = -100f;
    }


    // =========================================================
    // OPTIONAL RESET WITH POSITION
    // =========================================================

    public void ResetBall(
        Vector3 resetPosition,
        Quaternion resetRotation)
    {
        if (rb == null)
            return;

        frozen = true;

        rb.isKinematic = true;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;

        transform.position = resetPosition;
        transform.rotation = resetRotation;

        rb.position = resetPosition;
        rb.rotation = resetRotation;

        lockedZ = resetPosition.z;

        lastKickTime = -100f;
    }
}