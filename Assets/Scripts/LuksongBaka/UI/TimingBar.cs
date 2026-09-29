using UnityEngine;

public class TimingBar : MonoBehaviour
{
    [Header("UI")]
    public RectTransform marker;
    public RectTransform yellowZone;

    [Header("Movement")]
    public float speed = 350f;
    public float movementLimit = 300f;

    [Header("References")]
    public JumpJudge judge;
    public LuksongBakaTimer jumpTimer;

    private float direction = 1f;
    private bool active = false;

    void Update()
    {
        if (!active)
            return;

        if (marker == null)
            return;

        // Use unscaled time because
        // gameplay is slowed to 0.35.
        marker.anchoredPosition +=
            Vector2.right *
            speed *
            direction *
            Time.unscaledDeltaTime;

        // RIGHT
        if (marker.anchoredPosition.x >=
            movementLimit)
        {
            marker.anchoredPosition =
                new Vector2(
                    movementLimit,
                    marker.anchoredPosition.y
                );

            direction = -1f;
        }

        // LEFT
        else if (
            marker.anchoredPosition.x <=
            -movementLimit)
        {
            marker.anchoredPosition =
                new Vector2(
                    -movementLimit,
                    marker.anchoredPosition.y
                );

            direction = 1f;
        }
    }

    // =====================================
    // ACTIVATE
    // =====================================

    public void Activate()
    {
        if (marker == null)
            return;

        direction = 1f;

        marker.anchoredPosition =
            new Vector2(
                -movementLimit,
                marker.anchoredPosition.y
            );

        active = true;

        gameObject.SetActive(true);

        Debug.Log(
            "TIMING BAR ACTIVATED"
        );
    }

    // =====================================
    // DEACTIVATE
    // =====================================

    public void Deactivate()
    {
        active = false;

        gameObject.SetActive(false);
    }

    // =====================================
    // JUMP BUTTON
    // =====================================

    public void CheckTiming()
    {
        if (!active)
            return;

        // PLAYER PRESSED JUMP:
        // Stop 7-second timer immediately.
        if (jumpTimer != null)
        {
            jumpTimer.StopTimer();
        }

        if (marker == null ||
            yellowZone == null)
        {
            Debug.LogError(
                "Marker or YellowZone missing!"
            );

            return;
        }

        Vector3[] yellowCorners =
            new Vector3[4];

        yellowZone.GetWorldCorners(
            yellowCorners
        );

        float yellowLeft =
            yellowCorners[0].x;

        float yellowRight =
            yellowCorners[2].x;

        float markerCenter =
            marker.position.x;

        bool success =
            markerCenter >= yellowLeft &&
            markerCenter <= yellowRight;

        Debug.Log(
            "TIMING RESULT = "
            + success
            + " | Marker: "
            + markerCenter
            + " | Yellow: "
            + yellowLeft
            + " - "
            + yellowRight
        );

        Deactivate();

        if (judge == null)
            return;

        if (success)
        {
            judge.PerfectJump();
        }
        else
        {
            judge.BadJump();
        }
    }
}