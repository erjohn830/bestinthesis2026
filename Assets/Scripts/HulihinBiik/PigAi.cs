using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PigAI : MonoBehaviour
{
    private enum PigState
    {
        Roaming,
        Fleeing,
        Dashing,
        CornerEscaping,
        Caught,
        GameOver
    }

    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private Transform playerCatchPoint;
    [SerializeField] private Animator animator;

    // =====================================================
    // ARENA
    // =====================================================

    [Header("Arena Boundary")]
    [SerializeField] private float arenaHalfWidth = 13f;
    [SerializeField] private float arenaHalfLength = 18f;
    [SerializeField] private float boundaryPadding = 1.2f;

    // =====================================================
    // MOVEMENT
    // =====================================================

    [Header("Movement")]
    [SerializeField] private float roamingSpeed = 50f;
    [SerializeField] private float fleeingSpeed = 40f;
    [SerializeField] private float dashSpeed = 40f;
    [SerializeField] private float acceleration = 55f;
    [SerializeField] private float angularSpeed = 500f;

    // =====================================================
    // DETECTION
    // =====================================================

    [Header("Player Detection")]
    [SerializeField] private float awarenessDistance = 25f;
    [SerializeField] private float calmDistance = 28f;

    // =====================================================
    // FLEEING
    // =====================================================

    [Header("Fleeing")]
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float fleeRepathInterval = 0.38f;
    [SerializeField] private float sideDirectionStrength = 0.65f;
    [SerializeField] private float directionRandomness = 0.12f;

    // =====================================================
    // NATURAL FLEE MOVEMENT
    // =====================================================

    [Header("Natural Flee Movement")]

    [Tooltip("How long the pig keeps running on the same side.")]
    [SerializeField] private float sideCommitDuration = 2.2f;

    [Tooltip("Smoothness when changing flee direction.")]
    [SerializeField] private float fleeDirectionSmoothTime = 0.55f;

    [Tooltip("Maximum direction search angle.")]
    [SerializeField] private float fleeSearchAngle = 40f;

    // =====================================================
    // DASH
    // =====================================================

    [Header("Dash")]
    [SerializeField] private float dashTriggerDistance = 6f;
    [SerializeField] private float dashDistance = 12f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashCooldown = 20f;
    [SerializeField] private float dashSideAngle = 85f;

    // =====================================================
    // CORNER ESCAPE
    // =====================================================

    [Header("Corner / Stuck Escape")]
    [SerializeField] private float stuckCheckTime = 0.45f;
    [SerializeField] private float stuckMoveThreshold = 0.18f;
    [SerializeField] private float cornerEscapeSpeed = 55f;
    [SerializeField] private float cornerEscapeDistance = 10f;
    [SerializeField] private float cornerEscapeDuration = 0.75f;
    [SerializeField] private float cornerEscapeCooldown = 1f;
    [SerializeField] private float wallDetectDistance = 2.2f;

    // =====================================================
    // NEXT LEVEL ESCAPE
    // =====================================================

    [Header("Next Level Escape")]
    [SerializeField] private float nextLevelEscapeSpeed = 18f;
    [SerializeField] private float nextLevelEscapeDistance = 11f;
    [SerializeField] private float nextLevelEscapeDuration = 1f;

    // =====================================================
    // ROAMING
    // =====================================================

    [Header("Roaming")]
    [SerializeField] private float roamRadius = 12f;
    [SerializeField] private float minimumWaitTime = 0.7f;
    [SerializeField] private float maximumWaitTime = 1.2f;
    [SerializeField] private float destinationTolerance = 0.5f;

    // =====================================================
    // NAVIGATION
    // =====================================================

    [Header("Navigation")]
    [SerializeField] private float navMeshSearchDistance = 3f;
    [SerializeField] private int pointAttempts = 12;

    // =====================================================
    // SMOOTH ROTATION
    // =====================================================

    [Header("Smooth Movement / Rotation")]

    [Tooltip("How quickly the pig body rotates.")]
    [SerializeField] private float smoothTurnSpeed = 3.2f;

    [Tooltip("Smooths small NavMesh direction changes.")]
    [SerializeField] private float movementDirectionSmoothTime = 0.42f;

    [Tooltip("Pig will not rotate from tiny velocity changes.")]
    [SerializeField] private float minimumTurnVelocity = 0.15f;

    // =====================================================
    // ANIMATION
    // =====================================================

    [Header("Animation")]
    [SerializeField] private float animationDampTime = 0.22f;
    [SerializeField] private float dashAnimationSpeed = 1.35f;

    // =====================================================
    // PRIVATE VARIABLES
    // =====================================================

    private NavMeshAgent agent;

    private PigState state =
        PigState.Roaming;

    private float nextFleeRepathTime;
    private float nextDashAvailableTime;

    private float roamingWaitTimer;
    private float selectedWaitTime;

    private Vector3 lastStuckCheckPosition;

    private float stuckTimer;
    private float nextCornerEscapeTime;

    private bool cornerEscapeRunning;

    // Smooth body direction.
    private Vector3 smoothMoveDirection;
    private Vector3 smoothMoveDirectionVelocity;

    // Smooth flee direction.
    private Vector3 smoothFleeDirection;
    private Vector3 fleeDirectionVelocity;

    // Pig commits to left or right instead
    // of rapidly changing direction.
    private float committedSide = 1f;
    private float nextSideChangeTime;

    private bool initialized = false;

    // Starts paused during intro/tutorial.
    private bool pigPaused = true;

    // =====================================================
    // PUBLIC PROPERTIES
    // =====================================================

    public Transform PlayerCatchPoint
    {
        get
        {
            return playerCatchPoint;
        }
    }

    public bool IsCaught
    {
        get
        {
            return state == PigState.Caught;
        }
    }

    // =====================================================
    // START
    // =====================================================

    private IEnumerator Start()
    {
        agent =
            GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (player == null)
        {
            PlayerBiikMovement foundPlayer =
                FindFirstObjectByType<PlayerBiikMovement>();

            if (foundPlayer != null)
            {
                player =
                    foundPlayer.transform;
            }
        }

        // ---------------------------------------------
        // NAVMESH SETTINGS
        // ---------------------------------------------

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        /*
         * IMPORTANT:
         *
         * NavMeshAgent must NOT rotate the pig.
         * We rotate it ourselves smoothly.
         */
        agent.updateRotation = false;

        /*
         * Prevent hard braking when approaching
         * constantly changing flee destinations.
         */
        agent.autoBraking = false;

        smoothMoveDirection =
            transform.forward;

        smoothFleeDirection =
            transform.forward;

        ChooseNewCommittedSide();

        yield return null;

        if (!PlacePigOnNavMesh())
        {
            Debug.LogError(
                "Pig could not be placed on NavMesh.",
                this
            );

            yield break;
        }

        initialized = true;

        state =
            PigState.Roaming;

        lastStuckCheckPosition =
            transform.position;

        stuckTimer = 0f;

        if (!pigPaused)
        {
            ChooseRoamingDestination();
        }
        else
        {
            agent.isStopped = true;

            agent.ResetPath();

            agent.velocity =
                Vector3.zero;

            UpdateAnimation(0f);
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        // ---------------------------------------------
        // PAUSED
        // ---------------------------------------------

        if (pigPaused)
        {
            UpdateAnimation(0f);
            return;
        }

        // ---------------------------------------------
        // NAVMESH SAFETY
        // ---------------------------------------------

        if (!initialized ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            UpdateAnimation(0f);
            return;
        }

        // ---------------------------------------------
        // STOPPED STATES
        // ---------------------------------------------

        if (state == PigState.Caught ||
            state == PigState.GameOver)
        {
            UpdateAnimation(0f);
            return;
        }

        float distanceToPlayer =
            GetDistanceToPlayer();

        // =================================================
        // STUCK / CORNER CHECK
        // =================================================

        if (state == PigState.Fleeing ||
            state == PigState.Dashing)
        {
            UpdateStuckDetection(
                distanceToPlayer
            );

            if (cornerEscapeRunning)
            {
                UpdateSmoothRotation();
                UpdateAnimation(1f);
                return;
            }
        }
        else
        {
            lastStuckCheckPosition =
                transform.position;

            stuckTimer = 0f;
        }

        // =================================================
        // DASH
        // =================================================

        if (state != PigState.Dashing &&
            state != PigState.CornerEscaping &&
            distanceToPlayer <=
            dashTriggerDistance &&
            Time.time >= nextDashAvailableTime)
        {
            StartCoroutine(
                DashAwayRoutine()
            );

            return;
        }

        // =================================================
        // DETECT PLAYER
        // =================================================

        if (state != PigState.Dashing &&
            state != PigState.CornerEscaping)
        {
            // Player is close.
            if (distanceToPlayer <=
                awarenessDistance)
            {
                if (state !=
                    PigState.Fleeing)
                {
                    state =
                        PigState.Fleeing;

                    ChooseNewCommittedSide();

                    smoothFleeDirection =
                        GetBasicAwayDirection();

                    nextFleeRepathTime =
                        0f;
                }
            }

            // Player is far away.
            else if (distanceToPlayer >=
                     calmDistance)
            {
                if (state !=
                    PigState.Roaming)
                {
                    state =
                        PigState.Roaming;

                    roamingWaitTimer =
                        0f;

                    selectedWaitTime =
                        0f;

                    ChooseRoamingDestination();
                }
            }
        }

        // =================================================
        // MOVEMENT STATE
        // =================================================

        if (state ==
            PigState.Roaming)
        {
            UpdateRoaming();
        }
        else if (state ==
                 PigState.Fleeing)
        {
            UpdateFleeing();
        }

        // =================================================
        // SMOOTH BODY ROTATION
        // =================================================

        UpdateSmoothRotation();

        // =================================================
        // ANIMATION
        // =================================================

        float normalizedSpeed = 0f;

        if (dashSpeed > 0f)
        {
            normalizedSpeed =
                agent.velocity.magnitude /
                dashSpeed;
        }

        UpdateAnimation(
            normalizedSpeed
        );
    }

    // =====================================================
    // SMOOTH REALISTIC BODY ROTATION
    // =====================================================

    private void UpdateSmoothRotation()
    {
        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh ||
            pigPaused ||
            state == PigState.Caught ||
            state == PigState.GameOver)
        {
            return;
        }

        Vector3 velocity =
            agent.velocity;

        velocity.y = 0f;

        /*
         * Ignore tiny NavMesh corrections.
         *
         * Without this, even tiny velocity changes
         * can make the pig shake left/right.
         */
        if (velocity.magnitude <
            minimumTurnVelocity)
        {
            return;
        }

        Vector3 wantedDirection =
            velocity.normalized;

        // ---------------------------------------------
        // SMOOTH MOVEMENT DIRECTION FIRST
        // ---------------------------------------------

        smoothMoveDirection =
            Vector3.SmoothDamp(
                smoothMoveDirection,
                wantedDirection,
                ref smoothMoveDirectionVelocity,
                movementDirectionSmoothTime
            );

        smoothMoveDirection.y = 0f;

        if (smoothMoveDirection.sqrMagnitude <
            0.001f)
        {
            return;
        }

        smoothMoveDirection.Normalize();

        // ---------------------------------------------
        // TARGET BODY ROTATION
        // ---------------------------------------------

        Quaternion targetRotation =
            Quaternion.LookRotation(
                smoothMoveDirection,
                Vector3.up
            );

        float angle =
            Quaternion.Angle(
                transform.rotation,
                targetRotation
            );

        /*
         * Small turns are slower.
         * Large turns are slightly faster.
         *
         * This gives the pig body more weight.
         */
        float turnMultiplier =
            Mathf.Lerp(
                0.65f,
                1.15f,
                Mathf.InverseLerp(
                    0f,
                    120f,
                    angle
                )
            );

        /*
         * Exponential interpolation produces a
         * smoother result than a direct snap.
         */
        float smoothAmount =
            1f -
            Mathf.Exp(
                -smoothTurnSpeed *
                turnMultiplier *
                Time.deltaTime
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                smoothAmount
            );
    }

    // =====================================================
    // DISTANCE TO PLAYER
    // =====================================================

    private float GetDistanceToPlayer()
    {
        if (player == null)
        {
            return Mathf.Infinity;
        }

        Vector3 pigPosition =
            transform.position;

        Vector3 playerPosition =
            player.position;

        pigPosition.y = 0f;
        playerPosition.y = 0f;

        return Vector3.Distance(
            pigPosition,
            playerPosition
        );
    }

    // =====================================================
    // ROAMING
    // =====================================================

    private void UpdateRoaming()
    {
        agent.speed =
            roamingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        agent.autoBraking =
            false;

        if (agent.pathPending)
        {
            return;
        }

        bool reachedDestination =
            !agent.hasPath ||
            agent.remainingDistance <=
            Mathf.Max(
                agent.stoppingDistance,
                destinationTolerance
            );

        if (!reachedDestination)
        {
            roamingWaitTimer = 0f;
            return;
        }

        // ---------------------------------------------
        // SHORT NATURAL PAUSE
        // ---------------------------------------------

        if (selectedWaitTime <= 0f)
        {
            selectedWaitTime =
                Random.Range(
                    minimumWaitTime,
                    maximumWaitTime
                );
        }

        roamingWaitTimer +=
            Time.deltaTime;

        if (roamingWaitTimer >=
            selectedWaitTime)
        {
            roamingWaitTimer = 0f;
            selectedWaitTime = 0f;

            ChooseRoamingDestination();
        }
    }

    // =====================================================
    // CHOOSE ROAM DESTINATION
    // =====================================================

    private void ChooseRoamingDestination()
    {
        if (pigPaused ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh ||
            state == PigState.Caught ||
            state == PigState.GameOver)
        {
            return;
        }

        Vector3 center =
            arenaCenter != null
            ? arenaCenter.position
            : transform.position;

        for (int i = 0;
             i < pointAttempts;
             i++)
        {
            /*
             * Instead of choosing completely random
             * close points, choose a decent-distance
             * location so the pig makes longer runs.
             */
            Vector2 randomCircle =
                Random.insideUnitCircle *
                roamRadius;

            Vector3 candidate =
                new Vector3(
                    center.x +
                    randomCircle.x,

                    center.y,

                    center.z +
                    randomCircle.y
                );

            Vector3 destination;

            if (!TryFindReachablePoint(
                candidate,
                out destination))
            {
                continue;
            }

            float travelDistance =
                Vector3.Distance(
                    transform.position,
                    destination
                );

            // Avoid tiny movement paths.
            if (travelDistance < 3f)
            {
                continue;
            }

            agent.speed =
                roamingSpeed;

            agent.isStopped =
                false;

            agent.SetDestination(
                destination
            );

            return;
        }
    }

    // =====================================================
    // FLEEING
    // =====================================================

    private void UpdateFleeing()
    {
        agent.speed =
            fleeingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        agent.autoBraking =
            false;

        if (Time.time <
            nextFleeRepathTime)
        {
            return;
        }

        nextFleeRepathTime =
            Time.time +
            fleeRepathInterval;

        ChooseFleeDestination();
    }

    // =====================================================
    // BASIC AWAY DIRECTION
    // =====================================================

    private Vector3 GetBasicAwayDirection()
    {
        if (player == null)
        {
            return transform.forward;
        }

        Vector3 away =
            transform.position -
            player.position;

        away.y = 0f;

        if (away.sqrMagnitude <
            0.01f)
        {
            away =
                transform.forward;
        }

        return away.normalized;
    }

    // =====================================================
    // CHOOSE SIDE
    // =====================================================

    private void ChooseNewCommittedSide()
    {
        committedSide =
            Random.value < 0.5f
            ? -1f
            : 1f;

        nextSideChangeTime =
            Time.time +
            sideCommitDuration;
    }

    // =====================================================
    // NATURAL FLEE DESTINATION
    // =====================================================

    private void ChooseFleeDestination()
    {
        if (pigPaused ||
            player == null ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 away =
            GetBasicAwayDirection();

        // =================================================
        // KEEP RUNNING ON SAME SIDE
        // =================================================

        if (Time.time >=
            nextSideChangeTime)
        {
            /*
             * Most of the time, keep running
             * in the current curved direction.
             *
             * Occasionally switch sides.
             */
            if (Random.value < 0.25f)
            {
                committedSide *= -1f;
            }

            nextSideChangeTime =
                Time.time +
                sideCommitDuration;
        }

        Vector3 side =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized *
            committedSide;

        // =================================================
        // DESIRED RUNNING DIRECTION
        // =================================================

        Vector3 desiredDirection =
            away +
            side *
            sideDirectionStrength;

        /*
         * Very small randomness.
         *
         * This prevents the movement from looking
         * perfectly robotic without causing zig-zag.
         */
        desiredDirection +=
            new Vector3(
                Random.Range(
                    -directionRandomness,
                    directionRandomness
                ),
                0f,
                Random.Range(
                    -directionRandomness,
                    directionRandomness
                )
            );

        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude <
            0.01f)
        {
            desiredDirection =
                away;
        }

        desiredDirection.Normalize();

        // =================================================
        // SMOOTH FLEE DIRECTION
        // =================================================

        if (smoothFleeDirection.sqrMagnitude <
            0.01f)
        {
            smoothFleeDirection =
                desiredDirection;
        }

        smoothFleeDirection =
            Vector3.SmoothDamp(
                smoothFleeDirection,
                desiredDirection,
                ref fleeDirectionVelocity,
                fleeDirectionSmoothTime
            );

        smoothFleeDirection.y = 0f;

        if (smoothFleeDirection.sqrMagnitude <
            0.01f)
        {
            smoothFleeDirection =
                desiredDirection;
        }

        smoothFleeDirection.Normalize();

        float oldDistance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // =================================================
        // SEARCH AROUND CURRENT DIRECTION
        // =================================================

        float[] angleTests =
        {
            0f,
            10f,
            -10f,
            20f,
            -20f,
            30f,
            -30f,
            40f,
            -40f
        };

        for (int i = 0;
             i < angleTests.Length;
             i++)
        {
            float angle =
                Mathf.Clamp(
                    angleTests[i],
                    -fleeSearchAngle,
                    fleeSearchAngle
                );

            Vector3 testDirection =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                smoothFleeDirection;

            Vector3 candidate =
                transform.position +
                testDirection *
                fleeDistance;

            Vector3 destination;

            if (!TryFindReachablePoint(
                candidate,
                out destination))
            {
                continue;
            }

            float newDistance =
                Vector3.Distance(
                    destination,
                    player.position
                );

            /*
             * Do not select a path that takes the
             * pig closer to the player.
             */
            if (newDistance <
                oldDistance + 0.25f)
            {
                continue;
            }

            /*
             * Avoid extremely short paths because
             * they cause rapid turning.
             */
            if (Vector3.Distance(
                    transform.position,
                    destination) <
                2.5f)
            {
                continue;
            }

            agent.isStopped =
                false;

            agent.SetDestination(
                destination
            );

            return;
        }

        // =================================================
        // CURRENT SIDE BLOCKED
        // =================================================

        /*
         * If the current curved route is blocked,
         * switch once instead of repeatedly
         * changing directions.
         */
        committedSide *= -1f;

        nextSideChangeTime =
            Time.time +
            sideCommitDuration *
            0.7f;

        Vector3 fallbackSide =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized *
            committedSide;

        Vector3 fallbackDirection =
            away +
            fallbackSide * 0.5f;

        fallbackDirection.y = 0f;

        if (fallbackDirection.sqrMagnitude >
            0.01f)
        {
            fallbackDirection.Normalize();
        }

        Vector3 fallbackCandidate =
            transform.position +
            fallbackDirection *
            fleeDistance;

        Vector3 fallbackDestination;

        if (TryFindReachablePoint(
            fallbackCandidate,
            out fallbackDestination))
        {
            smoothFleeDirection =
                fallbackDirection;

            agent.isStopped =
                false;

            agent.SetDestination(
                fallbackDestination
            );
        }
    }

    // =====================================================
    // DASH
    // =====================================================

    private IEnumerator DashAwayRoutine()
    {
        if (player == null ||
            pigPaused)
        {
            yield break;
        }

        state =
            PigState.Dashing;

        nextDashAvailableTime =
            Time.time +
            dashCooldown;

        Vector3 away =
            GetBasicAwayDirection();

        Vector3 side =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized *
            committedSide;

        /*
         * Dash is still fast, but it follows
         * the pig's current escape side.
         */
        Vector3 direction =
            (
                away +
                side * 0.85f
            ).normalized;

        Vector3 candidate =
            transform.position +
            direction *
            dashDistance;

        Vector3 destination;

        // ---------------------------------------------
        // FIRST SIDE
        // ---------------------------------------------

        bool found =
            TryFindReachablePoint(
                candidate,
                out destination
            );

        // ---------------------------------------------
        // TRY OTHER SIDE
        // ---------------------------------------------

        if (!found)
        {
            committedSide *= -1f;

            side =
                Vector3.Cross(
                    Vector3.up,
                    away
                ).normalized *
                committedSide;

            direction =
                (
                    away +
                    side * 0.85f
                ).normalized;

            candidate =
                transform.position +
                direction *
                dashDistance;

            found =
                TryFindReachablePoint(
                    candidate,
                    out destination
                );
        }

        if (found)
        {
            agent.speed =
                dashSpeed;

            agent.acceleration =
                Mathf.Max(
                    acceleration,
                    90f
                );

            agent.angularSpeed =
                angularSpeed;

            agent.autoBraking =
                false;

            agent.isStopped =
                false;

            smoothFleeDirection =
                direction;

            agent.SetDestination(
                destination
            );

            if (animator != null)
            {
                animator.speed =
                    dashAnimationSpeed;
            }
        }

        yield return new WaitForSeconds(
            dashDuration
        );

        if (pigPaused ||
            state != PigState.Dashing)
        {
            yield break;
        }

        RestoreAnimatorSpeed();

        agent.speed =
            fleeingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        state =
            PigState.Fleeing;

        nextFleeRepathTime =
            Time.time + 0.15f;

        ChooseFleeDestination();
    }

    // =====================================================
    // STUCK DETECTION
    // =====================================================

    private void UpdateStuckDetection(
        float distanceToPlayer)
    {
        if (cornerEscapeRunning ||
            Time.time <
            nextCornerEscapeTime ||
            pigPaused ||
            player == null ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        if (distanceToPlayer >
            awarenessDistance)
        {
            lastStuckCheckPosition =
                transform.position;

            stuckTimer = 0f;

            return;
        }

        float moved =
            Vector3.Distance(
                transform.position,
                lastStuckCheckPosition
            );

        if (moved <=
            stuckMoveThreshold)
        {
            stuckTimer +=
                Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;

            lastStuckCheckPosition =
                transform.position;
        }

        bool nearBoundary =
            IsNearArenaBoundary();

        bool blocked =
            nearBoundary &&
            agent.hasPath &&
            !agent.pathPending &&
            agent.velocity.sqrMagnitude <
            0.08f;

        if ((stuckTimer >=
             stuckCheckTime &&
             nearBoundary) ||
            blocked)
        {
            stuckTimer = 0f;

            lastStuckCheckPosition =
                transform.position;

            StartCoroutine(
                CornerEscapeRoutine()
            );
        }
    }

    // =====================================================
    // NEAR ARENA WALL?
    // =====================================================

    private bool IsNearArenaBoundary()
    {
        if (arenaCenter == null)
        {
            return false;
        }

        float usableHalfWidth =
            Mathf.Max(
                0.1f,
                arenaHalfWidth -
                boundaryPadding
            );

        float usableHalfLength =
            Mathf.Max(
                0.1f,
                arenaHalfLength -
                boundaryPadding
            );

        Vector3 local =
            transform.position -
            arenaCenter.position;

        float distanceToXWall =
            usableHalfWidth -
            Mathf.Abs(local.x);

        float distanceToZWall =
            usableHalfLength -
            Mathf.Abs(local.z);

        return
            distanceToXWall <=
            wallDetectDistance ||
            distanceToZWall <=
            wallDetectDistance;
    }

    // =====================================================
    // CORNER ESCAPE
    // =====================================================

    private IEnumerator CornerEscapeRoutine()
    {
        if (cornerEscapeRunning ||
            player == null ||
            pigPaused ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            yield break;
        }

        cornerEscapeRunning = true;

        state =
            PigState.CornerEscaping;

        nextCornerEscapeTime =
            Time.time +
            cornerEscapeCooldown;

        agent.isStopped =
            false;

        agent.ResetPath();

        Vector3 bestDestination;

        bool found =
            TryFindBestCornerEscapePoint(
                out bestDestination
            );

        if (found)
        {
            agent.speed =
                Mathf.Max(
                    cornerEscapeSpeed,
                    fleeingSpeed
                );

            agent.acceleration =
                Mathf.Max(
                    acceleration,
                    90f
                );

            agent.angularSpeed =
                angularSpeed;

            agent.autoBraking =
                false;

            Vector3 direction =
                bestDestination -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude >
                0.01f)
            {
                smoothFleeDirection =
                    direction.normalized;
            }

            if (animator != null)
            {
                animator.speed =
                    Mathf.Max(
                        dashAnimationSpeed,
                        1.45f
                    );
            }

            agent.SetDestination(
                bestDestination
            );
        }
        else
        {
            Vector3 centerTarget =
                arenaCenter != null
                ? arenaCenter.position
                : transform.position +
                  transform.forward * 4f;

            Vector3 fallback;

            if (TryFindReachablePoint(
                centerTarget,
                out fallback))
            {
                agent.speed =
                    Mathf.Max(
                        cornerEscapeSpeed,
                        fleeingSpeed
                    );

                agent.acceleration =
                    Mathf.Max(
                        acceleration,
                        90f
                    );

                agent.isStopped =
                    false;

                agent.SetDestination(
                    fallback
                );
            }
        }

        float timer = 0f;

        while (timer <
               cornerEscapeDuration)
        {
            if (pigPaused ||
                state == PigState.Caught ||
                state == PigState.GameOver)
            {
                cornerEscapeRunning =
                    false;

                RestoreAnimatorSpeed();

                yield break;
            }

            UpdateSmoothRotation();

            timer +=
                Time.deltaTime;

            yield return null;
        }

        cornerEscapeRunning =
            false;

        RestoreAnimatorSpeed();

        if (pigPaused ||
            state == PigState.Caught ||
            state == PigState.GameOver)
        {
            yield break;
        }

        agent.speed =
            fleeingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        agent.autoBraking =
            false;

        state =
            PigState.Fleeing;

        lastStuckCheckPosition =
            transform.position;

        stuckTimer = 0f;

        ChooseNewCommittedSide();

        ChooseFleeDestination();
    }

    // =====================================================
    // FIND CORNER ESCAPE ROUTE
    // =====================================================

    private bool TryFindBestCornerEscapePoint(
        out Vector3 bestDestination)
    {
        bestDestination =
            transform.position;

        if (player == null)
        {
            return false;
        }

        Vector3 away =
            GetBasicAwayDirection();

        Vector3 sideA =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized;

        Vector3 sideB =
            -sideA;

        Vector3 wallSideA =
            GetBestWallTangent(1f);

        Vector3 wallSideB =
            -wallSideA;

        Vector3[] directions =
        {
            sideA,
            sideB,

            (away +
             sideA * 1.2f).normalized,

            (away +
             sideB * 1.2f).normalized,

            wallSideA,
            wallSideB,

            (wallSideA +
             away * 0.45f).normalized,

            (wallSideB +
             away * 0.45f).normalized
        };

        float bestScore =
            float.NegativeInfinity;

        bool found = false;

        for (int i = 0;
             i < directions.Length;
             i++)
        {
            Vector3 direction =
                directions[i];

            if (direction.sqrMagnitude <
                0.01f)
            {
                continue;
            }

            direction.y = 0f;
            direction.Normalize();

            Vector3 candidate =
                transform.position +
                direction *
                cornerEscapeDistance;

            Vector3 destination;

            if (!TryFindReachablePoint(
                candidate,
                out destination))
            {
                continue;
            }

            float distanceFromPlayer =
                Vector3.Distance(
                    destination,
                    player.position
                );

            float travelDistance =
                Vector3.Distance(
                    destination,
                    transform.position
                );

            float edgeClearance =
                GetArenaEdgeClearance(
                    destination
                );

            float score =
                distanceFromPlayer * 2.5f +
                travelDistance * 0.8f +
                edgeClearance * 1.5f;

            if (score > bestScore)
            {
                bestScore =
                    score;

                bestDestination =
                    destination;

                found = true;
            }
        }

        return found;
    }

    // =====================================================
    // WALL DIRECTION
    // =====================================================

    private Vector3 GetBestWallTangent(
        float directionSign)
    {
        if (arenaCenter == null)
        {
            return transform.right *
                   directionSign;
        }

        Vector3 local =
            transform.position -
            arenaCenter.position;

        float usableHalfWidth =
            Mathf.Max(
                0.1f,
                arenaHalfWidth -
                boundaryPadding
            );

        float usableHalfLength =
            Mathf.Max(
                0.1f,
                arenaHalfLength -
                boundaryPadding
            );

        float distanceToXWall =
            usableHalfWidth -
            Mathf.Abs(local.x);

        float distanceToZWall =
            usableHalfLength -
            Mathf.Abs(local.z);

        if (distanceToXWall <
            distanceToZWall)
        {
            return Vector3.forward *
                   directionSign;
        }

        return Vector3.right *
               directionSign;
    }

    // =====================================================
    // EDGE CLEARANCE
    // =====================================================

    private float GetArenaEdgeClearance(
        Vector3 position)
    {
        if (arenaCenter == null)
        {
            return 0f;
        }

        Vector3 local =
            position -
            arenaCenter.position;

        float usableHalfWidth =
            Mathf.Max(
                0.1f,
                arenaHalfWidth -
                boundaryPadding
            );

        float usableHalfLength =
            Mathf.Max(
                0.1f,
                arenaHalfLength -
                boundaryPadding
            );

        float xClearance =
            usableHalfWidth -
            Mathf.Abs(local.x);

        float zClearance =
            usableHalfLength -
            Mathf.Abs(local.z);

        return Mathf.Max(
            0f,
            Mathf.Min(
                xClearance,
                zClearance
            )
        );
    }

    // =====================================================
    // BEGIN CAUGHT
    // =====================================================

    public void BeginCaught()
    {
        StopAllCoroutines();

        cornerEscapeRunning =
            false;

        stuckTimer = 0f;

        state =
            PigState.Caught;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                true;

            agent.ResetPath();

            agent.velocity =
                Vector3.zero;
        }

        UpdateAnimation(0f);
    }

    // =====================================================
    // START NEXT LEVEL
    // =====================================================

    public void StartNextLevelFromCurrentPosition()
    {
        StopAllCoroutines();

        pigPaused = false;

        cornerEscapeRunning =
            false;

        stuckTimer = 0f;

        lastStuckCheckPosition =
            transform.position;

        state =
            PigState.Fleeing;

        RestoreAnimatorSpeed();

        ChooseNewCommittedSide();

        smoothFleeDirection =
            GetBasicAwayDirection();

        if (agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped =
            false;

        agent.ResetPath();

        StartCoroutine(
            NextLevelEscapeRoutine()
        );
    }

    // =====================================================
    // NEXT LEVEL ESCAPE
    // =====================================================

    private IEnumerator NextLevelEscapeRoutine()
    {
        if (player == null)
        {
            yield break;
        }

        Vector3 away =
            GetBasicAwayDirection();

        Vector3 side =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized *
            committedSide;

        Vector3 escapeDirection =
            (
                away +
                side * 0.65f
            ).normalized;

        Vector3 candidate =
            transform.position +
            escapeDirection *
            nextLevelEscapeDistance;

        Vector3 destination;

        agent.speed =
            Mathf.Max(
                nextLevelEscapeSpeed,
                dashSpeed
            );

        agent.acceleration =
            Mathf.Max(
                acceleration,
                90f
            );

        agent.angularSpeed =
            angularSpeed;

        agent.autoBraking =
            false;

        if (animator != null)
        {
            animator.speed =
                1.35f;
        }

        if (TryFindReachablePoint(
            candidate,
            out destination))
        {
            smoothFleeDirection =
                escapeDirection;

            agent.SetDestination(
                destination
            );
        }
        else
        {
            ChooseFleeDestination();
        }

        yield return new WaitForSeconds(
            nextLevelEscapeDuration
        );

        RestoreAnimatorSpeed();

        if (state !=
            PigState.Fleeing)
        {
            yield break;
        }

        agent.speed =
            fleeingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        ChooseFleeDestination();
    }

    // =====================================================
    // FAILED CATCH ESCAPE
    // =====================================================

    public void EscapeFromPlayer()
    {
        StopAllCoroutines();

        pigPaused = false;

        cornerEscapeRunning =
            false;

        stuckTimer = 0f;

        lastStuckCheckPosition =
            transform.position;

        state =
            PigState.Fleeing;

        RestoreAnimatorSpeed();

        ChooseNewCommittedSide();

        smoothFleeDirection =
            GetBasicAwayDirection();

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                false;
        }

        StartCoroutine(
            FailedCatchEscapeRoutine()
        );
    }

    private IEnumerator FailedCatchEscapeRoutine()
    {
        if (player == null)
        {
            yield break;
        }

        Vector3 away =
            GetBasicAwayDirection();

        Vector3 side =
            Vector3.Cross(
                Vector3.up,
                away
            ).normalized *
            committedSide;

        Vector3 direction =
            (
                away +
                side * 0.45f
            ).normalized;

        Vector3 candidate =
            transform.position +
            direction * 10f;

        Vector3 destination;

        agent.speed =
            Mathf.Max(
                18f,
                fleeingSpeed
            );

        agent.acceleration =
            Mathf.Max(
                acceleration,
                80f
            );

        agent.angularSpeed =
            angularSpeed;

        agent.autoBraking =
            false;

        if (animator != null)
        {
            animator.speed =
                1.35f;
        }

        if (TryFindReachablePoint(
            candidate,
            out destination))
        {
            smoothFleeDirection =
                direction;

            agent.SetDestination(
                destination
            );
        }

        yield return new WaitForSeconds(
            1f
        );

        RestoreAnimatorSpeed();

        if (state !=
            PigState.Fleeing)
        {
            yield break;
        }

        agent.speed =
            fleeingSpeed;

        agent.acceleration =
            acceleration;

        agent.angularSpeed =
            angularSpeed;

        ChooseFleeDestination();
    }

    // =====================================================
    // PAUSE PIG
    // =====================================================

    public void PausePig()
    {
        pigPaused = true;

        cornerEscapeRunning =
            false;

        stuckTimer = 0f;

        StopAllCoroutines();

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                true;

            agent.ResetPath();

            agent.velocity =
                Vector3.zero;
        }

        UpdateAnimation(0f);
    }

    // =====================================================
    // RESUME PIG
    // =====================================================

    public void ResumePig()
    {
        pigPaused = false;

        if (!initialized ||
            agent == null ||
            !agent.enabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped =
            false;

        ChooseNewCommittedSide();

        smoothMoveDirection =
            transform.forward;

        smoothFleeDirection =
            transform.forward;

        float distance =
            GetDistanceToPlayer();

        if (distance <=
            awarenessDistance)
        {
            state =
                PigState.Fleeing;

            ChooseFleeDestination();
        }
        else
        {
            state =
                PigState.Roaming;

            ChooseRoamingDestination();
        }
    }

    // =====================================================
    // WIN
    // =====================================================

    public void StopAfterWin()
    {
        StopAllCoroutines();

        pigPaused = true;

        state =
            PigState.Caught;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                true;

            agent.ResetPath();

            agent.velocity =
                Vector3.zero;
        }
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    public void StopForGameOver()
    {
        StopAllCoroutines();

        pigPaused = true;

        state =
            PigState.GameOver;

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped =
                true;

            agent.ResetPath();

            agent.velocity =
                Vector3.zero;
        }

        UpdateAnimation(0f);
    }

    // =====================================================
    // LEVEL DIFFICULTY
    // KEEPING YOUR FAST SPEEDS
    // =====================================================

    public void SetDifficulty(int level)
    {
        // =================================================
        // LEVEL 1
        // =================================================

        if (level <= 1)
        {
            // SAME FAST SPEED
            roamingSpeed = 50f;
            fleeingSpeed = 40f;
            dashSpeed = 40f;

            acceleration = 55f;
            angularSpeed = 500f;

            awarenessDistance = 25f;
            calmDistance = 28f;

            fleeDistance = 10f;

            /*
             * Repath is intentionally not extremely
             * fast because that caused zig-zagging.
             */
            fleeRepathInterval = 0.38f;

            sideDirectionStrength = 0.65f;
            directionRandomness = 0.12f;

            dashTriggerDistance = 6f;
            dashDistance = 12f;
            dashDuration = 1f;
            dashCooldown = 20f;
            dashSideAngle = 85f;

            cornerEscapeSpeed = 55f;
            cornerEscapeDistance = 10f;
            cornerEscapeDuration = 0.75f;

            nextLevelEscapeSpeed = 18f;
            nextLevelEscapeDistance = 11f;
            nextLevelEscapeDuration = 1f;

            roamRadius = 12f;
            minimumWaitTime = 0.7f;
            maximumWaitTime = 1.2f;
            destinationTolerance = 0.5f;

            navMeshSearchDistance = 3f;
            pointAttempts = 12;
        }

        // =================================================
        // LEVEL 2
        // =================================================

        else if (level == 2)
        {
            // SAME FAST SPEED
            roamingSpeed = 55f;
            fleeingSpeed = 47f;
            dashSpeed = 48f;

            acceleration = 65f;
            angularSpeed = 550f;

            awarenessDistance = 28f;
            calmDistance = 31f;

            fleeDistance = 12f;

            fleeRepathInterval = 0.34f;

            sideDirectionStrength = 0.72f;
            directionRandomness = 0.14f;

            dashTriggerDistance = 7f;
            dashDistance = 14f;
            dashDuration = 1f;
            dashCooldown = 15f;
            dashSideAngle = 90f;

            cornerEscapeSpeed = 62f;
            cornerEscapeDistance = 11f;
            cornerEscapeDuration = 0.8f;

            nextLevelEscapeSpeed = 24f;
            nextLevelEscapeDistance = 13f;
            nextLevelEscapeDuration = 1f;

            roamRadius = 13f;
            minimumWaitTime = 0.5f;
            maximumWaitTime = 1f;
            destinationTolerance = 0.45f;

            navMeshSearchDistance = 3f;
            pointAttempts = 14;
        }

        // =================================================
        // LEVEL 3
        // =================================================

        else
        {
            // SAME FAST SPEED
            roamingSpeed = 60f;
            fleeingSpeed = 55f;
            dashSpeed = 58f;

            acceleration = 75f;
            angularSpeed = 600f;

            awarenessDistance = 31f;
            calmDistance = 34f;

            fleeDistance = 14f;

            fleeRepathInterval = 0.30f;

            sideDirectionStrength = 0.8f;
            directionRandomness = 0.16f;

            dashTriggerDistance = 8f;
            dashDistance = 16f;
            dashDuration = 1.05f;
            dashCooldown = 10f;
            dashSideAngle = 95f;

            cornerEscapeSpeed = 70f;
            cornerEscapeDistance = 12f;
            cornerEscapeDuration = 0.85f;

            nextLevelEscapeSpeed = 30f;
            nextLevelEscapeDistance = 15f;
            nextLevelEscapeDuration = 1f;

            roamRadius = 14f;
            minimumWaitTime = 0.3f;
            maximumWaitTime = 0.8f;
            destinationTolerance = 0.4f;

            navMeshSearchDistance = 3f;
            pointAttempts = 16;
        }

        if (agent != null)
        {
            agent.speed =
                fleeingSpeed;

            agent.acceleration =
                acceleration;

            agent.angularSpeed =
                angularSpeed;

            // Keep manual smooth rotation.
            agent.updateRotation =
                false;

            agent.autoBraking =
                false;
        }

        Debug.Log(
            "Pig Difficulty = LEVEL " +
            level
        );
    }

    // =====================================================
    // FIND REACHABLE NAVMESH POINT
    // =====================================================

    private bool TryFindReachablePoint(
        Vector3 candidate,
        out Vector3 destination)
    {
        destination =
            transform.position;

        candidate =
            ClampInsideArena(
                candidate
            );

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            candidate,
            out hit,
            navMeshSearchDistance,
            agent.areaMask))
        {
            return false;
        }

        NavMeshPath path =
            new NavMeshPath();

        if (!agent.CalculatePath(
            hit.position,
            path))
        {
            return false;
        }

        if (path.status !=
            NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        destination =
            hit.position;

        return true;
    }

    // =====================================================
    // PLACE PIG ON NAVMESH
    // =====================================================

    private bool PlacePigOnNavMesh()
    {
        if (agent.isOnNavMesh)
        {
            return true;
        }

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            transform.position,
            out hit,
            navMeshSearchDistance,
            agent.areaMask))
        {
            return agent.Warp(
                hit.position
            );
        }

        return false;
    }

    // =====================================================
    // ANIMATION
    // =====================================================

    private void UpdateAnimation(
        float normalizedSpeed)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(
            "Speed",
            Mathf.Clamp01(
                normalizedSpeed
            ),
            animationDampTime,
            Time.deltaTime
        );
    }

    public void RestoreAnimatorSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    // =====================================================
    // SAFE ARENA POSITION
    // =====================================================

    public Vector3 GetSafeArenaPosition(
        Vector3 position)
    {
        return ClampInsideArena(
            position
        );
    }

    // =====================================================
    // CLAMP INSIDE ARENA
    // =====================================================

    private Vector3 ClampInsideArena(
        Vector3 position)
    {
        if (arenaCenter == null)
        {
            return position;
        }

        float minX =
            arenaCenter.position.x -
            arenaHalfWidth +
            boundaryPadding;

        float maxX =
            arenaCenter.position.x +
            arenaHalfWidth -
            boundaryPadding;

        float minZ =
            arenaCenter.position.z -
            arenaHalfLength +
            boundaryPadding;

        float maxZ =
            arenaCenter.position.z +
            arenaHalfLength -
            boundaryPadding;

        position.x =
            Mathf.Clamp(
                position.x,
                minX,
                maxX
            );

        position.z =
            Mathf.Clamp(
                position.z,
                minZ,
                maxZ
            );

        return position;
    }
}