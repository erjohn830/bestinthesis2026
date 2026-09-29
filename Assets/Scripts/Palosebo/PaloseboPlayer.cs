using UnityEngine;

public class PaloseboPlayer : MonoBehaviour
{
    public enum ClimbState
    {
        Climb,
        Slip,
        FastSlip
    }

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Tooltip("Optional. Assign the child PaloseboPlayer visual.")]
    [SerializeField] private Transform characterVisual;

    [Header("Bamboo Points")]
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private Transform winPoint;

    [Header("Movement Speed")]
    [SerializeField] private float climbSpeed = 8f;
    [SerializeField] private float slipSpeed = 3f;
    [SerializeField] private float fastSlipSpeed = 7f;

    [Header("Movement Smoothness")]
    [SerializeField] private float acceleration = 16f;
    [SerializeField] private float deceleration = 12f;

    private ClimbState currentState = ClimbState.Slip;

    private bool gameplayActive = false;
    private bool finished = false;

    private float currentVerticalSpeed = 0f;

    private float bottomY;
    private float winY;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (bottomPoint != null)
            bottomY = bottomPoint.position.y;
        else
            bottomY = transform.position.y;

        if (winPoint != null)
            winY = winPoint.position.y;

        ResetAnimator();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!gameplayActive)
            return;

        if (finished)
            return;

        HandleMovement();
        HandleAnimation();
        CheckWin();
    }

    // =========================================================
    // GAMEPLAY
    // =========================================================

    public void SetGameplay(bool active)
    {
        gameplayActive = active;

        if (active)
        {
            finished = false;
            currentVerticalSpeed = 0f;

            ResetAnimator();
        }
        else
        {
            currentVerticalSpeed = 0f;

            if (animator != null)
            {
                animator.SetBool("IsClimbing", false);
                animator.SetBool("IsSlipping", false);
            }
        }
    }

    public bool IsGameplayActive()
    {
        return gameplayActive;
    }

    // =========================================================
    // CALLED BY BALANCE SYSTEM
    // =========================================================

    public void SetClimbState(ClimbState newState)
    {
        if (!gameplayActive)
            return;

        if (finished)
            return;

        currentState = newState;
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        float targetSpeed = 0f;

        switch (currentState)
        {
            // GREEN
            case ClimbState.Climb:

                targetSpeed = climbSpeed;

                break;


            // NORMAL BLUE / RED
            case ClimbState.Slip:

                targetSpeed = -slipSpeed;

                break;


            // EDGE OF BLUE / RED
            case ClimbState.FastSlip:

                targetSpeed = -fastSlipSpeed;

                break;
        }

        // Smooth speed changes.
        float speedChangeRate;

        if (Mathf.Abs(targetSpeed) >
            Mathf.Abs(currentVerticalSpeed))
        {
            speedChangeRate = acceleration;
        }
        else
        {
            speedChangeRate = deceleration;
        }

        currentVerticalSpeed =
            Mathf.MoveTowards(
                currentVerticalSpeed,
                targetSpeed,
                speedChangeRate * Time.deltaTime
            );

        Vector3 position =
            transform.position;

        position.y +=
            currentVerticalSpeed *
            Time.deltaTime;

        // Never go below BottomPoint.
        if (bottomPoint != null)
        {
            position.y =
                Mathf.Max(
                    position.y,
                    bottomY
                );
        }

        // IMPORTANT:
        // Never climb above WinPoint.
        if (winPoint != null)
        {
            position.y =
                Mathf.Min(
                    position.y,
                    winY
                );
        }

        transform.position =
            position;
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        bool climbing =
            currentState == ClimbState.Climb;

        bool slipping =
            currentState == ClimbState.Slip ||
            currentState == ClimbState.FastSlip;

        animator.SetBool(
            "IsClimbing",
            climbing
        );

        animator.SetBool(
            "IsSlipping",
            slipping
        );
    }

    // =========================================================
    // WIN
    // =========================================================

    private void CheckWin()
    {
        if (winPoint == null)
            return;

        if (transform.position.y <
            winY - 0.01f)
            return;

        // Force player exactly onto WinPoint.
        Vector3 position =
            transform.position;

        position.y =
            winY;

        transform.position =
            position;

        currentVerticalSpeed = 0f;

        gameplayActive = false;
        finished = true;

        if (animator != null)
        {
            animator.SetBool(
                "IsClimbing",
                false
            );

            animator.SetBool(
                "IsSlipping",
                false
            );
        }

        Debug.Log(
            "PALOSOBO: REACHED WIN POINT!"
        );

        if (PaloseboManager.Instance != null)
        {
            PaloseboManager.Instance.Win();
        }
    }

    // =========================================================
    // WIN ANIMATION
    // =========================================================

    public void PlayWinAnimation()
    {
        gameplayActive = false;
        finished = true;

        currentVerticalSpeed = 0f;

        if (animator != null)
        {
            animator.SetBool(
                "IsClimbing",
                false
            );

            animator.SetBool(
                "IsSlipping",
                false
            );

            animator.ResetTrigger("Lose");
            animator.SetTrigger("Win");
        }
    }

    // =========================================================
    // LOSE
    // =========================================================

    public void Lose()
    {
        gameplayActive = false;
        finished = true;

        currentVerticalSpeed = 0f;

        if (animator != null)
        {
            animator.SetBool(
                "IsClimbing",
                false
            );

            animator.SetBool(
                "IsSlipping",
                false
            );

            animator.ResetTrigger("Win");
            animator.SetTrigger("Lose");
        }
    }

    // =========================================================
    // RESET PLAYER
    // =========================================================

    public void ResetPlayer()
    {
        gameplayActive = false;
        finished = false;

        currentVerticalSpeed = 0f;

        if (bottomPoint != null)
        {
            Vector3 position =
                transform.position;

            position.y =
                bottomY;

            transform.position =
                position;
        }

        ResetAnimator();
    }

    // =========================================================
    // ANIMATOR RESET
    // =========================================================

    private void ResetAnimator()
    {
        if (animator == null)
            return;

        animator.SetBool(
            "IsClimbing",
            false
        );

        animator.SetBool(
            "IsSlipping",
            false
        );

        animator.ResetTrigger("Win");
        animator.ResetTrigger("Lose");
    }
}