using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public LuksongBakaTimer gameTimer;

    public SlowZoneTrigger slowZone;
    public TimingBar timingBar;

    [Header("Gameplay UI")]
    public GameObject gameplayHUD;

    [Header("Countdown")]
    public GameObject countdownPanel;
    public Image countdownImage;

    [Header("Countdown Sprites")]
    public Sprite number3;
    public Sprite number2;
    public Sprite number1;
    public Sprite goImage;

    [Header("Countdown Duration")]
    public float numberDuration = 1f;
    public float goDuration = 0.7f;

    public bool GameplayStarted
    {
        get;
        private set;
    }

    private bool starting = false;

    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        GameplayStarted = false;

        LockGameplay();

        ResetPlayerToSpawn();

        if (gameTimer != null)
        {
            gameTimer.ResetTimer();
        }

        HideCountdown();
    }

    // =====================================
    // MENU / TUTORIAL
    // =====================================

    public void LockGameplay()
    {
        StopAllCoroutines();

        starting = false;
        GameplayStarted = false;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (player != null)
        {
            player.StopPlayer();
        }

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        if (timingBar != null)
        {
            timingBar.Deactivate();
        }

        HideCountdown();
    }

    // =====================================
    // ALWAYS USE REAL START POINT
    // =====================================

    public void ResetPlayerToSpawn()
    {
        if (player == null)
            return;

        player.ResetToStart();
        player.PrepareAtSpawn();
    }

    // =====================================
    // START GAME
    // =====================================

    public void ConfirmTaya()
    {
        if (starting)
            return;

        starting = true;
        GameplayStarted = false;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        ResetPlayerToSpawn();

        if (slowZone != null)
        {
            slowZone.ResetZone();
        }

        if (timingBar != null)
        {
            timingBar.Deactivate();
        }

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(true);
        }

        if (gameTimer != null)
        {
            gameTimer.ResetTimer();
        }

        StartCoroutine(
            CountdownRoutine()
        );
    }

    IEnumerator CountdownRoutine()
    {
        if (player != null)
        {
            player.StopPlayer();
        }

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        ShowCountdown(number3);

        yield return new WaitForSecondsRealtime(
            numberDuration
        );

        ShowCountdown(number2);

        yield return new WaitForSecondsRealtime(
            numberDuration
        );

        ShowCountdown(number1);

        yield return new WaitForSecondsRealtime(
            numberDuration
        );

        ShowCountdown(goImage);

        yield return new WaitForSecondsRealtime(
            goDuration
        );

        HideCountdown();

        // SlowZone is allowed now.
        GameplayStarted = true;

        if (player != null)
        {
            player.StartRun();
        }

        starting = false;
    }

    // =====================================
    // COUNTDOWN IMAGE
    // =====================================

    void ShowCountdown(Sprite sprite)
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        if (countdownImage == null)
        {
            Debug.LogError(
                "COUNTDOWN IMAGE NOT ASSIGNED!"
            );

            return;
        }

        if (sprite == null)
        {
            Debug.LogError(
                "COUNTDOWN SPRITE NOT ASSIGNED!"
            );

            countdownImage.enabled =
                false;

            return;
        }

        countdownImage.enabled = true;
        countdownImage.sprite = sprite;
        countdownImage.preserveAspect = true;
    }

    void HideCountdown()
    {
        if (countdownImage != null)
        {
            countdownImage.enabled = false;
        }

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
    }
}