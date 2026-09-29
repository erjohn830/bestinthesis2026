using UnityEngine;

public class PaloseboBalance : MonoBehaviour
{
    [Header("UI REFERENCES")]
    [SerializeField] private RectTransform balanceBar;
    [SerializeField] private RectTransform safeZone;
    [SerializeField] private RectTransform zoneIndicator;

    [Header("PLAYER")]
    [SerializeField] private PaloseboPlayer player;

    // =========================================================
    // RANDOM ROAMING
    // =========================================================

    [Header("RANDOM ROAMING")]

    // IMPORTANT:
    // New variable name so Unity will NOT keep
    // your old Roam Speed = 4 value.
    [SerializeField] private float randomRoamSpeed = 55f;

    [Tooltip("Shortest time before changing/randomizing target.")]
    [SerializeField] private float minRoamTime = 0.8f;

    [Tooltip("Longest time before changing/randomizing target.")]
    [SerializeField] private float maxRoamTime = 2.0f;

    [Tooltip("Distance considered close enough to target.")]
    [SerializeField] private float targetTolerance = 5f;

    [Header("RANDOMNESS")]

    [Tooltip("Chance that the indicator chooses a new target before reaching the old one.")]
    [Range(0f, 1f)]
    [SerializeField] private float earlyDirectionChangeChance = 0.35f;

    // =========================================================
    // SPAM BUTTONS
    // =========================================================

    [Header("SPAM BUTTONS")]

    [Tooltip("How far ONE Blue tap pushes the indicator left.")]
    [SerializeField] private float tapPushDistance = 35f;

    [Tooltip("Extra push when the indicator is far from center.")]
    [SerializeField] private float dangerTapBonus = 10f;

    // =========================================================
    // FAST FALL
    // =========================================================

    [Header("FAST FALL EDGE")]

    [Range(0.5f, 0.95f)]
    [SerializeField] private float fastFallStart = 0.78f;

    // =========================================================
    // START
    // =========================================================

    [Header("START")]
    [SerializeField] private bool startAtCenter = true;

    // =========================================================
    // PRIVATE
    // =========================================================

    private float indicatorX;

    private float leftLimit;
    private float rightLimit;

    private float roamTargetX;
    private float roamTimer;

    private bool gameplayActive = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        RecalculateLimits();
        ResetBalance();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!gameplayActive)
            return;

        if (balanceBar == null ||
            safeZone == null ||
            zoneIndicator == null ||
            player == null)
        {
            return;
        }

        RecalculateLimits();

        RandomRoam();

        ClampIndicator();

        UpdateIndicator();

        UpdatePlayerMovement();
    }

    // =========================================================
    // CALCULATE BAR LIMITS
    // =========================================================

    private void RecalculateLimits()
    {
        if (balanceBar == null ||
            zoneIndicator == null)
        {
            return;
        }

        float halfBar =
            balanceBar.rect.width * 0.5f;

        float halfIndicator =
            zoneIndicator.rect.width * 0.5f;

        leftLimit =
            -halfBar + halfIndicator;

        rightLimit =
            halfBar - halfIndicator;
    }

    // =========================================================
    // RANDOM ROAMING
    // =========================================================

    private void RandomRoam()
    {
        roamTimer -= Time.deltaTime;

        float distance =
            Mathf.Abs(indicatorX - roamTargetX);

        bool reachedTarget =
            distance <= targetTolerance;

        bool timerExpired =
            roamTimer <= 0f;

        if (reachedTarget || timerExpired)
        {
            ChooseNewRandomTarget();
        }

        // Smooth movement.
        indicatorX =
            Mathf.MoveTowards(
                indicatorX,
                roamTargetX,
                randomRoamSpeed * Time.deltaTime
            );
    }

    // =========================================================
    // RANDOM TARGET
    // =========================================================

    private void ChooseNewRandomTarget()
    {
        if (rightLimit <= leftLimit)
            return;

        float oldTarget =
            roamTargetX;

        // Choose anywhere across the whole bar.
        roamTargetX =
            Random.Range(
                leftLimit,
                rightLimit
            );

        // Occasionally force a bigger move so it doesn't
        // keep choosing almost the same location.
        if (Mathf.Abs(roamTargetX - indicatorX) <
            (rightLimit - leftLimit) * 0.18f)
        {
            if (Random.value < 0.65f)
            {
                if (indicatorX >= 0f)
                {
                    roamTargetX =
                        Random.Range(
                            leftLimit,
                            0f
                        );
                }
                else
                {
                    roamTargetX =
                        Random.Range(
                            0f,
                            rightLimit
                        );
                }
            }
        }

        roamTimer =
            Random.Range(
                minRoamTime,
                maxRoamTime
            );

        // Sometimes shorten the timer to make the movement
        // unexpectedly change direction.
        if (Random.value <
            earlyDirectionChangeChance)
        {
            roamTimer *=
                Random.Range(
                    0.45f,
                    0.75f
                );
        }
    }

    // =========================================================
    // BLUE BUTTON
    // =========================================================

    public void PressBlue()
    {
        if (!gameplayActive)
            return;

        float push =
            GetTapPushAmount();

        // BLUE pushes indicator LEFT.
        indicatorX -= push;

        ClampIndicator();

        UpdateIndicator();

        Debug.Log("BLUE TAP");
    }

    // =========================================================
    // RED BUTTON
    // =========================================================

    public void PressRed()
    {
        if (!gameplayActive)
            return;

        float push =
            GetTapPushAmount();

        // RED pushes indicator RIGHT.
        indicatorX += push;

        ClampIndicator();

        UpdateIndicator();

        Debug.Log("RED TAP");
    }

    // =========================================================
    // TAP STRENGTH
    // =========================================================

    private float GetTapPushAmount()
    {
        float maximumDistance =
            Mathf.Max(
                Mathf.Abs(leftLimit),
                Mathf.Abs(rightLimit)
            );

        if (maximumDistance <= 0f)
            return tapPushDistance;

        float normalizedDistance =
            Mathf.Abs(indicatorX) /
            maximumDistance;

        // Slightly stronger correction when
        // indicator is dangerously far away.
        return tapPushDistance +
               dangerTapBonus *
               normalizedDistance;
    }

    // =========================================================
    // LIMIT INDICATOR
    // =========================================================

    private void ClampIndicator()
    {
        indicatorX =
            Mathf.Clamp(
                indicatorX,
                leftLimit,
                rightLimit
            );
    }

    // =========================================================
    // UPDATE INDICATOR
    // =========================================================

    private void UpdateIndicator()
    {
        if (zoneIndicator == null)
            return;

        Vector2 position =
            zoneIndicator.anchoredPosition;

        position.x =
            indicatorX;

        zoneIndicator.anchoredPosition =
            position;
    }

    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

    private void UpdatePlayerMovement()
    {
        if (player == null ||
            safeZone == null ||
            zoneIndicator == null)
        {
            return;
        }

        // =====================================================
        // CHECK ACTUAL GREEN UI AREA
        // =====================================================

        Vector3 indicatorWorld =
            zoneIndicator.TransformPoint(
                zoneIndicator.rect.center
            );

        Vector3 safeLocal =
            safeZone.InverseTransformPoint(
                indicatorWorld
            );

        float safeHalfWidth =
            safeZone.rect.width * 0.5f;

        bool insideGreen =
            safeLocal.x >= -safeHalfWidth &&
            safeLocal.x <= safeHalfWidth;

        // =====================================================
        // GREEN = CLIMB
        // =====================================================

        if (insideGreen)
        {
            player.SetClimbState(
                PaloseboPlayer.ClimbState.Climb
            );

            return;
        }

        // =====================================================
        // DISTANCE TO EDGE
        // =====================================================

        float maximumDistance =
            Mathf.Max(
                Mathf.Abs(leftLimit),
                Mathf.Abs(rightLimit)
            );

        float normalizedDistance = 0f;

        if (maximumDistance > 0f)
        {
            normalizedDistance =
                Mathf.Abs(indicatorX) /
                maximumDistance;
        }

        // =====================================================
        // FAR BLUE / RED = FAST DOWN
        // =====================================================

        if (normalizedDistance >= fastFallStart)
        {
            player.SetClimbState(
                PaloseboPlayer.ClimbState.FastSlip
            );

            return;
        }

        // =====================================================
        // NORMAL BLUE / RED = DOWN
        // =====================================================

        player.SetClimbState(
            PaloseboPlayer.ClimbState.Slip
        );
    }

    // =========================================================
    // COMPATIBILITY
    // =========================================================

    public void SetLeftPressed(bool pressed)
    {
        if (pressed)
            PressBlue();
    }

    public void SetRightPressed(bool pressed)
    {
        if (pressed)
            PressRed();
    }

    public void LeftDown()
    {
        PressBlue();
    }

    public void LeftUp()
    {
        // Spam button: nothing happens on release.
    }

    public void RightDown()
    {
        PressRed();
    }

    public void RightUp()
    {
        // Spam button: nothing happens on release.
    }

    // =========================================================
    // START GAMEPLAY
    // =========================================================

    public void StartGameplay()
    {
        RecalculateLimits();

        gameplayActive = true;

        ResetBalance();

        Debug.Log(
            "PALOSOBO BALANCE: STARTED"
        );
    }

    // Compatibility with older Manager.
    public void StartGame()
    {
        StartGameplay();
    }

    // =========================================================
    // STOP GAMEPLAY
    // =========================================================

    public void StopGameplay()
    {
        gameplayActive = false;

        Debug.Log(
            "PALOSOBO BALANCE: STOPPED"
        );
    }

    // Compatibility with older Manager.
    public void StopGame()
    {
        StopGameplay();
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetBalance()
    {
        RecalculateLimits();

        if (startAtCenter)
        {
            indicatorX = 0f;
        }
        else
        {
            indicatorX =
                Random.Range(
                    leftLimit,
                    rightLimit
                );
        }

        ChooseNewRandomTarget();

        UpdateIndicator();
    }
}