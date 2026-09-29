using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 6f;
    public float gravity = -25f;

    [Header("Points")]
    public Transform startPoint;

    [Tooltip("Drag Taya here")]
    public Transform runTarget;

    [Header("Visual Character")]
    public Transform visualPivot;

    [Tooltip("0 = normal, 180 = reverse imported model")]
    public float visualYawOffset = 0f;

    [Tooltip("Adjust only if model feet are below/above road")]
    public float visualHeightOffset = 0.9f;

    [Header("Animator")]
    public Animator anim;

    [Header("Animation Names")]
    public string idleAnimation = "Idle";
    public string runAnimation = "Run";

    public string level1Jump = "Lv1Jump";
    public string level1Fail = "Lv1Fail";

    public string level2Jump = "Lv2Jump";
    public string level2Fail = "Lv2Fail";

    public string level3Jump = "Lv3Jump";
    public string level3Fail = "Lv3Fail";

    public string winAnimation = "Win";
    public string loseAnimation = "Lose";

    [Header("UI")]
    public GameObject jumpButton;

    private CharacterController controller;

    private bool autoRun = false;
    private bool waitingForJump = false;
    private bool scriptedJump = false;

    private float verticalVelocity = -2f;

    // Allows JumpJudge to read the Animator
    public Animator PlayerAnimator
    {
        get { return anim; }
    }

    void Awake()
    {
        controller =
            GetComponent<CharacterController>();

        if (anim == null)
        {
            anim =
                GetComponentInChildren<Animator>();
        }

        if (anim != null)
        {
            // IMPORTANT:
            // Animation controls body pose only.
            // Our synchronized code controls movement.
            anim.applyRootMotion = false;
        }
    }

    void Start()
    {
        if (jumpButton != null)
        {
            jumpButton.SetActive(false);
        }

        ApplyVisualDirection();

        PlayIdle();
    }

    void Update()
    {
        // Scripted jump controls position.
        if (scriptedJump)
            return;

        if (controller == null)
            return;

        if (!controller.enabled)
            return;

        Vector3 movement =
            Vector3.zero;

        // =====================================
        // AUTO RUN
        // =====================================

        if (autoRun &&
            runTarget != null)
        {
            Vector3 direction =
                runTarget.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude >
                0.01f)
            {
                direction.Normalize();

                transform.rotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up
                    );

                movement =
                    direction *
                    runSpeed;
            }
        }

        // =====================================
        // GRAVITY
        // =====================================

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity +=
                gravity *
                Time.deltaTime;
        }

        movement.y =
            verticalVelocity;

        controller.Move(
            movement *
            Time.deltaTime
        );
    }

    void LateUpdate()
    {
        ApplyVisualDirection();
    }

    // =====================================
    // VISUAL MODEL
    // =====================================

    void ApplyVisualDirection()
    {
        if (visualPivot == null)
            return;

        visualPivot.localPosition =
            new Vector3(
                0f,
                visualHeightOffset,
                0f
            );

        visualPivot.localRotation =
            Quaternion.Euler(
                0f,
                visualYawOffset,
                0f
            );
    }

    // =====================================
    // SAFE ANIMATION
    // =====================================

    public bool PlayAnimation(
        string stateName,
        float fadeTime = 0.05f)
    {
        if (anim == null)
            return false;

        if (string.IsNullOrEmpty(stateName))
            return false;

        int hash =
            Animator.StringToHash(
                stateName
            );

        if (!anim.HasState(0, hash))
        {
            hash =
                Animator.StringToHash(
                    "Base Layer." +
                    stateName
                );
        }

        if (!anim.HasState(0, hash))
        {
            Debug.LogWarning(
                "PLAYER ANIMATION NOT FOUND: "
                + stateName
            );

            return false;
        }

        anim.CrossFade(
            hash,
            fadeTime,
            0
        );

        return true;
    }

    public string GetJumpAnimationName(
        int level,
        bool success)
    {
        if (level == 1)
        {
            return success ?
                level1Jump :
                level1Fail;
        }

        if (level == 2)
        {
            return success ?
                level2Jump :
                level2Fail;
        }

        if (level == 3)
        {
            return success ?
                level3Jump :
                level3Fail;
        }

        return "";
    }

    // =====================================
    // IDLE
    // =====================================

    public void PlayIdle()
    {
        autoRun = false;

        PlayAnimation(
            idleAnimation
        );
    }

    // =====================================
    // RUN
    // =====================================

    public void StartRun()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        waitingForJump = false;
        scriptedJump = false;

        autoRun = true;

        verticalVelocity = -2f;

        if (jumpButton != null)
        {
            jumpButton.SetActive(false);
        }

        FaceRunTarget();

        PlayAnimation(
            runAnimation
        );
    }

    // =====================================
    // FACE TAYA
    // =====================================

    public void FaceRunTarget()
    {
        if (runTarget == null)
            return;

        Vector3 direction =
            runTarget.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );
    }

    // =====================================
    // SLOW ZONE
    // =====================================

    public void EnterSlowZone()
    {
        if (waitingForJump)
            return;

        waitingForJump = true;
        autoRun = false;

        verticalVelocity = -2f;

        // Always face Taya.
        FaceRunTarget();

        PlayIdle();

        Time.timeScale = 0.35f;

        Time.fixedDeltaTime =
            0.02f *
            Time.timeScale;

        if (jumpButton != null)
        {
            jumpButton.SetActive(true);
        }

        Debug.Log(
            "PLAYER STOPPED AT SLOWZONE"
        );
    }

    // Compatibility with old scripts.
    public void EnterSlowZone(
        Transform unused)
    {
        EnterSlowZone();
    }

    // =====================================
    // BEGIN SCRIPTED ANIMATION JUMP
    // =====================================

    public void BeginScriptedJump()
    {
        autoRun = false;
        waitingForJump = false;
        scriptedJump = true;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (jumpButton != null)
        {
            jumpButton.SetActive(false);
        }

        if (controller != null)
        {
            controller.enabled = false;
        }

        FaceRunTarget();
    }

    // =====================================
    // END SCRIPTED JUMP
    // =====================================

    public void EndScriptedJump()
    {
        scriptedJump = false;

        verticalVelocity = -2f;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    // =====================================
    // LEVEL JUMP ANIMATION
    // =====================================

    public bool PerformLevelJump(
        int level,
        bool success)
    {
        string animationName =
            GetJumpAnimationName(
                level,
                success
            );

        bool played =
            PlayAnimation(
                animationName,
                0.03f
            );

        // Backup if fail animation is missing.
        if (!played && !success)
        {
            PlayAnimation(
                loseAnimation,
                0.03f
            );
        }

        return played;
    }

    // =====================================
    // RESET
    // =====================================

    public void ResetToStart()
    {
        if (startPoint == null)
        {
            Debug.LogError(
                "PLAYER START POINT NOT ASSIGNED!"
            );

            return;
        }

        TeleportTo(
            startPoint
        );

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        autoRun = false;
        scriptedJump = false;
        waitingForJump = false;

        FaceRunTarget();

        PlayIdle();
    }

    public void TeleportTo(
        Transform point)
    {
        if (point == null)
            return;

        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position =
            point.position;

        if (controller != null)
        {
            controller.enabled = true;
        }

        verticalVelocity = -2f;

        FaceRunTarget();
    }

    public void PrepareAtSpawn()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        autoRun = false;
        scriptedJump = false;
        waitingForJump = false;

        verticalVelocity = -2f;

        if (jumpButton != null)
        {
            jumpButton.SetActive(false);
        }

        FaceRunTarget();

        PlayIdle();
    }

    // =====================================
    // WIN / LOSE
    // =====================================

    public void PlayWin()
    {
        autoRun = false;
        scriptedJump = false;

        PlayAnimation(
            winAnimation
        );
    }

    public void PlayLose()
    {
        autoRun = false;
        scriptedJump = false;

        PlayAnimation(
            loseAnimation
        );
    }

    public void StopPlayer()
    {
        autoRun = false;
    }

    // =====================================
    // OLD JUMP METHOD
    // Keep for compatibility with fail code.
    // =====================================

    public IEnumerator JumpToPosition(
        Vector3 targetPosition,
        float height,
        float duration)
    {
        BeginScriptedJump();

        Vector3 startPosition =
            transform.position;

        targetPosition.y =
            startPosition.y;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t =
                elapsed /
                duration;

            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            float arc =
                4f *
                height *
                t *
                (1f - t);

            position.y += arc;

            transform.position =
                position;

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        transform.position =
            targetPosition;

        EndScriptedJump();
    }
}