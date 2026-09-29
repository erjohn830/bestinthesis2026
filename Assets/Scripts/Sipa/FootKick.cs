using UnityEngine;

public class FootKick : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float kickCooldown = 0.20f;

    private SipaBall currentBall;
    private float nextKickTime;

    private void OnTriggerEnter(Collider other)
    {
        SipaBall ball = other.GetComponentInParent<SipaBall>();

        if (ball == null)
            return;

        currentBall = ball;

        // Automatically kick when the foot physically touches the Pato.
        KickBall();
    }

    private void OnTriggerStay(Collider other)
    {
        // Keep track of the Pato while it is touching the foot.
        SipaBall ball = other.GetComponentInParent<SipaBall>();

        if (ball != null)
            currentBall = ball;
    }

    private void OnTriggerExit(Collider other)
    {
        SipaBall ball = other.GetComponentInParent<SipaBall>();

        if (ball == currentBall)
            currentBall = null;
    }

    // =========================================================
    // CALLED BY SIPAANIMATION
    // =========================================================
    public void KickBall()
    {
        if (Time.time < nextKickTime)
            return;

        if (SipaManager.Instance != null)
        {
            if (!SipaManager.Instance.IsGameStarted)
                return;

            if (SipaManager.Instance.IsGameFinished)
                return;
        }

        if (currentBall == null)
        {
            Debug.Log("Kick pressed, but Pato is not touching the foot.");
            return;
        }

        nextKickTime = Time.time + kickCooldown;

        currentBall.TryKick(transform.position);
    }

    // Keep this for compatibility with older versions.
    public bool TryKickBall()
    {
        if (currentBall == null)
            return false;

        if (Time.time < nextKickTime)
            return false;

        if (SipaManager.Instance != null)
        {
            if (!SipaManager.Instance.IsGameStarted ||
                SipaManager.Instance.IsGameFinished)
            {
                return false;
            }
        }

        nextKickTime = Time.time + kickCooldown;

        return currentBall.TryKick(transform.position);
    }

    public void ResetKick()
    {
        currentBall = null;
        nextKickTime = 0f;
    }
}