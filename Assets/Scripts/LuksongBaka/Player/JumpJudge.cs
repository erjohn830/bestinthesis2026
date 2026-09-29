using UnityEngine;
using System.Collections;

public class JumpJudge : MonoBehaviour
{
    [Header("Main References")]
    public PlayerController player;
    public Transform taya;

    public LevelManager levelManager;
    public SlowZoneTrigger slowZone;
    public GameFlowManager gameFlow;

    // =====================================
    // SUCCESSFUL JUMP SYNC
    // =====================================

    [Header("Animation Sync")]

    [Tooltip(
        "At what percentage of the animation should the Player be directly above Taya? 0.5 = 50%"
    )]
    [Range(0.2f, 0.8f)]
    public float tayaCrossTime = 0.52f;

    [Tooltip(
        "How high above Taya's world position should PlayerRoot be at the crossing?"
    )]
    public float level1Clearance = 1.4f;

    public float level2Clearance = 1.8f;

    public float level3Clearance = 2.2f;

    [Tooltip(
        "How far behind Taya should the Player land?"
    )]
    public float landingDistance = 2.5f;

    [Tooltip(
        "Move the crossing slightly behind Taya's center."
    )]
    public float tayaBackOffset = 0.2f;

    // =====================================
    // FAIL
    // =====================================

    [Header("Failed Jump")]
    public float failMoveDistance = 0.6f;
    public float failHeight = 0.25f;
    public float failDuration = 0.55f;

    // =====================================
    // DELAYS
    // =====================================

    [Header("Delays")]
    public float landingDelay = 0.5f;
    public float nextLevelDelay = 0.8f;

    public float winAnimationTime = 2f;
    public float loseAnimationTime = 1.5f;

    private bool resolvingJump = false;

    // =====================================
    // PERFECT
    // =====================================

    public void PerfectJump()
    {
        if (resolvingJump)
            return;

        StartCoroutine(
            PerfectJumpRoutine()
        );
    }

    // =====================================
    // BAD
    // =====================================

    public void BadJump()
    {
        if (resolvingJump)
            return;

        StartCoroutine(
            BadJumpRoutine()
        );
    }

    // =====================================
    // SUCCESS
    // =====================================

    IEnumerator PerfectJumpRoutine()
    {
        resolvingJump = true;

        int level =
            levelManager.currentLevel;

        // Restore normal speed before jump.
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        player.BeginScriptedJump();

        string jumpState =
            player.GetJumpAnimationName(
                level,
                true
            );

        bool animationPlayed =
            player.PerformLevelJump(
                level,
                true
            );

        if (!animationPlayed)
        {
            Debug.LogError(
                "Cannot perform jump because animation state is missing: "
                + jumpState
            );

            player.EndScriptedJump();

            resolvingJump = false;

            yield break;
        }

        Animator animator =
            player.PlayerAnimator;

        if (animator == null)
        {
            Debug.LogError(
                "Player Animator is not assigned!"
            );

            player.EndScriptedJump();

            resolvingJump = false;

            yield break;
        }

        // Wait until CrossFade actually enters
        // the jump state.
        float waitTimer = 0f;

        while (
            !IsCurrentState(
                animator,
                jumpState
            ) &&
            waitTimer < 1f)
        {
            waitTimer +=
                Time.deltaTime;

            yield return null;
        }

        if (!IsCurrentState(
            animator,
            jumpState))
        {
            Debug.LogError(
                "Animator never entered state: "
                + jumpState
            );

            player.EndScriptedJump();

            resolvingJump = false;

            yield break;
        }

        // =================================
        // CALCULATE JUMP POINTS
        // =================================

        Vector3 start =
            player.transform.position;

        Vector3 direction =
            taya.position -
            start;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
        {
            direction =
                player.transform.forward;
        }

        direction.Normalize();

        // Point slightly behind Taya.
        Vector3 tayaBackPoint =
            taya.position +
            direction *
            tayaBackOffset;

        // Keep road/base height.
        tayaBackPoint.y =
            start.y;

        Vector3 landingPoint =
            taya.position +
            direction *
            landingDistance;

        landingPoint.y =
            start.y;

        float clearance =
            GetClearance(level);

        // =================================
        // DRIVE POSITION USING
        // ANIMATOR NORMALIZED TIME
        // =================================

        float lastNormalizedTime = 0f;

        while (true)
        {
            AnimatorStateInfo info =
                animator.GetCurrentAnimatorStateInfo(
                    0
                );

            // If Animator transitions,
            // use the next state's info
            // when it is our jump animation.
            if (animator.IsInTransition(0))
            {
                AnimatorStateInfo next =
                    animator.GetNextAnimatorStateInfo(
                        0
                    );

                if (StateMatches(
                    next,
                    jumpState))
                {
                    info = next;
                }
            }

            float normalizedTime =
                Mathf.Clamp01(
                    info.normalizedTime
                );

            lastNormalizedTime =
                normalizedTime;

            Vector3 position =
                CalculateSyncedJumpPosition(
                    start,
                    tayaBackPoint,
                    landingPoint,
                    normalizedTime,
                    clearance
                );

            player.transform.position =
                position;

            if (normalizedTime >= 0.99f)
                break;

            yield return null;
        }

        player.transform.position =
            landingPoint;

        player.EndScriptedJump();

        player.StopPlayer();

        levelManager.AddScore(100);

        yield return new WaitForSeconds(
            landingDelay
        );

        // =================================
        // LEVEL 3 FINISHED
        // =================================

        if (level >=
            levelManager.maxLevel)
        {
            levelManager.PauseGameTimer();

            player.PlayWin();

            yield return new WaitForSeconds(
                winAnimationTime
            );

            levelManager.WinGame();

            resolvingJump = false;

            yield break;
        }

        // =================================
        // NEXT LEVEL
        // =================================

        levelManager.NextLevel();

        yield return new WaitForSeconds(
            nextLevelDelay
        );

        if (gameFlow != null)
        {
            gameFlow.ResetPlayerToSpawn();
        }
        else
        {
            player.ResetToStart();
        }

        if (slowZone != null)
        {
            slowZone.ResetZone();
        }

        yield return new WaitForSeconds(
            0.25f
        );

        player.StartRun();

        resolvingJump = false;
    }

    // =====================================
    // SYNCHRONIZED JUMP POSITION
    // =====================================

    Vector3 CalculateSyncedJumpPosition(
        Vector3 start,
        Vector3 tayaPoint,
        Vector3 landing,
        float animationTime,
        float clearance)
    {
        Vector3 horizontalPosition;

        // =================================
        // FIRST HALF:
        // Player → Taya
        // =================================

        if (animationTime <= tayaCrossTime)
        {
            float t =
                animationTime /
                tayaCrossTime;

            // Smooth movement.
            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            horizontalPosition =
                Vector3.Lerp(
                    start,
                    tayaPoint,
                    t
                );
        }

        // =================================
        // SECOND HALF:
        // Taya → Landing
        // =================================

        else
        {
            float t =
                (animationTime -
                tayaCrossTime) /
                (1f -
                tayaCrossTime);

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            horizontalPosition =
                Vector3.Lerp(
                    tayaPoint,
                    landing,
                    t
                );
        }

        // =================================
        // HEIGHT
        //
        // Peak happens EXACTLY when
        // animation reaches tayaCrossTime.
        // =================================

        float height01;

        if (animationTime <=
            tayaCrossTime)
        {
            height01 =
                animationTime /
                tayaCrossTime;
        }
        else
        {
            height01 =
                1f -
                (
                    animationTime -
                    tayaCrossTime
                ) /
                (
                    1f -
                    tayaCrossTime
                );
        }

        height01 =
            Mathf.Clamp01(
                height01
            );

        // Smooth rise and fall.
        height01 =
            Mathf.SmoothStep(
                0f,
                1f,
                height01
            );

        horizontalPosition.y =
            start.y +
            clearance *
            height01;

        return horizontalPosition;
    }

    // =====================================
    // FAIL
    // =====================================

    IEnumerator BadJumpRoutine()
    {
        resolvingJump = true;

        levelManager.PauseGameTimer();

        int level =
            levelManager.currentLevel;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        player.PerformLevelJump(
            level,
            false
        );

        Vector3 start =
            player.transform.position;

        Vector3 direction =
            taya.position -
            start;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
        {
            direction =
                player.transform.forward;
        }

        direction.Normalize();

        Vector3 failPosition =
            start +
            direction *
            failMoveDistance;

        failPosition.y =
            start.y;

        yield return player.StartCoroutine(
            player.JumpToPosition(
                failPosition,
                failHeight,
                failDuration
            )
        );

        player.StopPlayer();

        // Allow fail animation to be visible.
        yield return new WaitForSeconds(
            0.8f
        );

        player.PlayLose();

        yield return new WaitForSeconds(
            loseAnimationTime
        );

        levelManager.LoseGame();

        resolvingJump = false;
    }

    // =====================================
    // LEVEL HEIGHT
    // =====================================

    float GetClearance(int level)
    {
        if (level == 1)
            return level1Clearance;

        if (level == 2)
            return level2Clearance;

        return level3Clearance;
    }

    // =====================================
    // ANIMATOR STATE HELPERS
    // =====================================

    bool IsCurrentState(
        Animator animator,
        string stateName)
    {
        AnimatorStateInfo current =
            animator.GetCurrentAnimatorStateInfo(
                0
            );

        if (StateMatches(
            current,
            stateName))
        {
            return true;
        }

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo next =
                animator.GetNextAnimatorStateInfo(
                    0
                );

            if (StateMatches(
                next,
                stateName))
            {
                return true;
            }
        }

        return false;
    }

    bool StateMatches(
        AnimatorStateInfo info,
        string stateName)
    {
        return
            info.IsName(stateName) ||
            info.IsName(
                "Base Layer." +
                stateName
            );
    }
}