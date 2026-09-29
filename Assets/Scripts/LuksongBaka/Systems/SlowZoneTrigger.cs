using UnityEngine;

public class SlowZoneTrigger : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public TimingBar timingBar;
    public GameFlowManager gameFlow;
    public LuksongBakaTimer jumpTimer;

    private bool activated = false;

    void OnTriggerEnter(Collider other)
    {
        TryActivate(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryActivate(other);
    }

    void TryActivate(Collider other)
    {
        if (activated)
            return;

        // Don't trigger during menu/countdown.
        if (gameFlow != null &&
            !gameFlow.GameplayStarted)
        {
            return;
        }

        PlayerController hitPlayer =
            other.GetComponentInParent
            <PlayerController>();

        if (hitPlayer == null)
            return;

        activated = true;

        PlayerController actualPlayer =
            player != null
            ? player
            : hitPlayer;

        // =====================================
        // STOP PLAYER
        // =====================================

        actualPlayer.EnterSlowZone();

        // =====================================
        // SHOW TIMING BAR
        // =====================================

        if (timingBar != null)
        {
            timingBar.Activate();
        }

        // =====================================
        // START 7 SECOND JUMP TIMER
        // =====================================

        if (jumpTimer != null)
        {
            jumpTimer.StartTimer();
        }

        Debug.Log(
            "SLOWZONE ACTIVATED"
        );
    }

    // =====================================
    // RESET FOR NEXT LEVEL
    // =====================================

    public void ResetZone()
    {
        activated = false;

        if (timingBar != null)
        {
            timingBar.Deactivate();
        }

        if (jumpTimer != null)
        {
            jumpTimer.ResetTimer();
        }
    }
}