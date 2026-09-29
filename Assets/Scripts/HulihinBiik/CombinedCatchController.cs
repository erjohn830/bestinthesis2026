using UnityEngine;

public class CombinedCatchController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string catchingState = "Catching";

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void PlayCatching()
    {
        if (animator == null)
        {
            Debug.LogError("Combined Catch Animator is missing.", this);
            return;
        }

        animator.speed = 1f;
        animator.Play(catchingState, 0, 0f);
    }

    public void FreezeCatchPose(float normalizedTime = 0.95f)
    {
        if (animator == null)
        {
            return;
        }

        animator.Play(catchingState, 0, normalizedTime);
        animator.speed = 0f;
    }

    public void ResumeAnimation()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }
}