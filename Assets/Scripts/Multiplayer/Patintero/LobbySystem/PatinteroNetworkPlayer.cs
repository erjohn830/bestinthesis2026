using Fusion;
using UnityEngine;

public enum PatinteroRole
{
    Runner,
    HorizontalGuard,
    CenterGuard
}

public enum PatinteroRunnerState
{
    GoingForward,
    Returning,
    Completed,
    Tagged
}


[RequireComponent(
    typeof(CharacterController))]
[RequireComponent(
    typeof(NetworkTransform))]
public class PatinteroNetworkPlayer :
    NetworkBehaviour
{
    public static PatinteroNetworkPlayer Local;

    [Header("MOVEMENT")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float rotationSpeed = 12f;

    [Header("REFERENCES")]
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private NetworkTransform networkTransform;

    [SerializeField]
    private Animator animator;


    [Networked]
    public int TeamId { get; set; }

    [Networked]
    public int TeamIndex { get; set; }

    [Networked]
    public PatinteroRole Role
    {
        get;
        set;
    }

    [Networked]
    public PatinteroRunnerState RunnerState
    {
        get;
        set;
    }

    [Networked]
    public float GuardLineValue
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool IsMoving
    {
        get;
        set;
    }


    private void Awake()
    {
        if (controller == null)
        {
            controller =
                GetComponent<
                    CharacterController>();
        }

        if (networkTransform == null)
        {
            networkTransform =
                GetComponent<
                    NetworkTransform>();
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<
                    Animator>();
        }
    }


    public void InitializeBeforeSpawn(
        int team,
        int teamIndex,
        PatinteroRole role,
        float lineValue)
    {
        TeamId = team;
        TeamIndex = teamIndex;
        Role = role;
        GuardLineValue = lineValue;

        RunnerState =
            PatinteroRunnerState
                .GoingForward;

        IsMoving = false;
    }


    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Local = this;

            PatinteroCameraFollow camera =
                FindFirstObjectByType<
                    PatinteroCameraFollow>();

            if (camera != null)
            {
                camera.SetTarget(
                    transform
                );
            }
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (!GetInput(
            out PatinteroInputData input))
        {
            return;
        }

        Vector3 direction =
            new Vector3(
                input.Move.x,
                0f,
                input.Move.y
            );

        if (Role ==
            PatinteroRole.HorizontalGuard)
        {
            direction.z = 0f;
        }
        else if (Role ==
            PatinteroRole.CenterGuard)
        {
            direction.x = 0f;
        }

        bool canMove =
            PatinteroMatchManager.Instance != null &&
            PatinteroMatchManager.Instance
                .CanPlayersMove;

        if (RunnerState ==
                PatinteroRunnerState.Tagged ||
            RunnerState ==
                PatinteroRunnerState.Completed)
        {
            canMove = false;
        }

        if (!canMove)
        {
            direction =
                Vector3.zero;
        }

        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        Vector3 movement =
            direction *
            moveSpeed;

        // Small downward movement keeps
        // CharacterController grounded.
        movement.y = -2f;

        controller.Move(
            movement *
            Runner.DeltaTime
        );

        // Force guards to remain
        // exactly on their assigned line.
        Vector3 position =
            transform.position;

        if (Role ==
            PatinteroRole.HorizontalGuard)
        {
            position.z =
                GuardLineValue;

            transform.position =
                position;
        }

        if (Role ==
            PatinteroRole.CenterGuard)
        {
            position.x =
                GuardLineValue;

            transform.position =
                position;
        }

        if (direction.sqrMagnitude >
            0.01f)
        {
            Quaternion target =
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    target,
                    rotationSpeed *
                    Runner.DeltaTime
                );
        }

        if (HasStateAuthority)
        {
            IsMoving =
                direction.sqrMagnitude >
                0.01f;
        }
    }


    public override void Render()
    {
        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                IsMoving
            );
        }
    }


    public void ServerPrepareRound(
        PatinteroRole role,
        float lineValue,
        Transform spawn)
    {
        if (!HasStateAuthority)
            return;

        Role = role;

        GuardLineValue =
            lineValue;

        IsMoving = false;

        if (role ==
            PatinteroRole.Runner)
        {
            RunnerState =
                PatinteroRunnerState
                    .GoingForward;
        }
        else
        {
            RunnerState =
                PatinteroRunnerState
                    .GoingForward;
        }

        networkTransform.Teleport(
            spawn.position,
            spawn.rotation
        );
    }


    public void ServerSetRunnerState(
        PatinteroRunnerState state)
    {
        if (!HasStateAuthority)
            return;

        RunnerState = state;

        if (state ==
                PatinteroRunnerState.Tagged ||
            state ==
                PatinteroRunnerState.Completed)
        {
            IsMoving = false;
        }
    }
}