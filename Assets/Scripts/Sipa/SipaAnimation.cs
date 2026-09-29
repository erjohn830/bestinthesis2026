using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SipaAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FootKick footKick;


    [Header("Animator Parameters")]
    [SerializeField] private string kickTriggerName = "kick";
    [SerializeField] private string winTriggerName = "win";
    [SerializeField] private string loseTriggerName = "lose";
    [SerializeField] private string speedParameterName = "Speed";


    [Header("Animator State")]
    [Tooltip("Exact name of your idle state.")]
    [SerializeField] private string idleStateName = "Armature|idle";


    [Header("Kick")]
    [SerializeField] private float kickCooldown = 0.25f;


    private Animator animator;

    private bool animationEnabled = true;
    private bool kicking = false;

    private float lastKickTime = -999f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        animator = GetComponent<Animator>();


        if (footKick == null)
        {
            footKick = GetComponentInChildren<FootKick>(true);
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ResetToIdle();
    }


    // =========================================================
    // KICK BUTTON
    // =========================================================

    public void Kick()
    {
        if (!animationEnabled)
            return;


        if (SipaManager.Instance != null)
        {
            if (!SipaManager.Instance.IsGameStarted)
                return;

            if (SipaManager.Instance.IsGameFinished)
                return;


            // IMPORTANT:
            // If this is the first action after countdown,
            // release the Pato.
            SipaManager.Instance.NotifyPlayerAction();
        }


        if (Time.time < lastKickTime + kickCooldown)
            return;


        lastKickTime = Time.time;

        kicking = true;


        if (animator != null)
        {
            ResetAnimatorTriggers();

            animator.SetTrigger(kickTriggerName);
        }


        Debug.Log("KICK BUTTON PRESSED");
    }


    // =========================================================
    // OPTIONAL BUTTON NAME
    //
    // This lets you use either:
    //
    // SipaAnimation -> Kick()
    //
    // OR
    //
    // SipaAnimation -> RequestKick()
    //
    // in your UI button.
    // =========================================================

    public void RequestKick()
    {
        Kick();
    }


    // =========================================================
    // ANIMATION EVENT
    //
    // If your kick animation has an Animation Event,
    // set the event function to:
    //
    // HitBall
    //
    // This checks the FootKick at the exact contact frame.
    // =========================================================

    public void HitBall()
    {
        if (!animationEnabled)
            return;


        if (SipaManager.Instance != null)
        {
            if (!SipaManager.Instance.IsGameStarted)
                return;

            if (SipaManager.Instance.IsGameFinished)
                return;
        }


        if (footKick != null)
        {
            footKick.KickBall();
        }
        else
        {
            Debug.LogWarning(
                "SipaAnimation: FootKick reference is missing."
            );
        }
    }


    // =========================================================
    // TRY KICK BALL
    //
    // Kept for compatibility with older Animation Events.
    // =========================================================

    public void TryKickBall()
    {
        HitBall();
    }


    // =========================================================
    // MOVEMENT SPEED
    // =========================================================

    public void SetMovementSpeed(float speed)
    {
        if (animator == null)
            return;

        if (!animationEnabled)
            return;


        animator.SetFloat(
            speedParameterName,
            Mathf.Abs(speed)
        );
    }


    // =========================================================
    // WIN
    // =========================================================

    public void PlayWinAnimation()
    {
        if (animator == null)
            return;


        StopAllCoroutines();

        animationEnabled = true;
        kicking = false;


        animator.enabled = true;

        ResetAnimatorTriggers();

        animator.SetFloat(
            speedParameterName,
            0f
        );

        animator.SetTrigger(
            winTriggerName
        );


        Debug.Log("WIN ANIMATION");
    }


    // =========================================================
    // LOSE
    // =========================================================

    public void PlayLoseAnimation()
    {
        if (animator == null)
            return;


        StopAllCoroutines();

        animationEnabled = true;
        kicking = false;


        animator.enabled = true;

        ResetAnimatorTriggers();

        animator.SetFloat(
            speedParameterName,
            0f
        );

        animator.SetTrigger(
            loseTriggerName
        );


        Debug.Log("LOSE ANIMATION");
    }


    // =========================================================
    // RESET TO IDLE
    // =========================================================

    public void ResetToIdle()
    {
        StopAllCoroutines();


        animationEnabled = true;
        kicking = false;

        lastKickTime = -999f;


        if (animator == null)
            animator = GetComponent<Animator>();


        if (animator == null)
            return;


        animator.enabled = true;

        ResetAnimatorTriggers();


        if (!string.IsNullOrEmpty(speedParameterName))
        {
            animator.SetFloat(
                speedParameterName,
                0f
            );
        }


        if (!string.IsNullOrEmpty(idleStateName))
        {
            animator.Play(
                idleStateName,
                0,
                0f
            );
        }


        animator.Update(0f);
    }


    // =========================================================
    // ENABLE / DISABLE ANIMATION
    // =========================================================

    public void SetAnimationEnabled(bool enabledState)
    {
        animationEnabled = enabledState;


        if (animator != null)
        {
            animator.enabled = enabledState;
        }
    }


    // =========================================================
    // RESET TRIGGERS
    // =========================================================

    private void ResetAnimatorTriggers()
    {
        if (animator == null)
            return;


        if (!string.IsNullOrEmpty(kickTriggerName))
            animator.ResetTrigger(kickTriggerName);

        if (!string.IsNullOrEmpty(winTriggerName))
            animator.ResetTrigger(winTriggerName);

        if (!string.IsNullOrEmpty(loseTriggerName))
            animator.ResetTrigger(loseTriggerName);
    }


    // =========================================================
    // ANIMATION EVENT
    //
    // Put this at the END of the kick animation if needed.
    // =========================================================

    public void KickFinished()
    {
        kicking = false;
    }
}