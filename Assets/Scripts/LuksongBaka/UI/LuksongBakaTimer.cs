using UnityEngine;
using TMPro;

public class LuksongBakaTimer : MonoBehaviour
{
    [Header("Jump Timer")]
    [Tooltip("Seconds allowed to press Jump")]
    public float jumpTime = 7f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("References")]
    public LevelManager levelManager;
    public TimingBar timingBar;
    public PlayerController player;

    private float currentTime;
    private bool running = false;
    private bool timeExpired = false;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (!running)
            return;

        if (timeExpired)
            return;

        // IMPORTANT:
        // Timing Bar uses slow motion,
        // so use REAL-TIME seconds.
        currentTime -= Time.unscaledDeltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            UpdateTimerUI();

            TimerExpired();

            return;
        }

        UpdateTimerUI();
    }

    // =====================================
    // START JUMP TIMER
    // =====================================

    public void StartTimer()
    {
        currentTime = jumpTime;

        running = true;
        timeExpired = false;

        UpdateTimerUI();

        Debug.Log(
            "JUMP TIMER STARTED: "
            + jumpTime
        );
    }

    // =====================================
    // STOP TIMER
    // =====================================

    public void StopTimer()
    {
        running = false;

        UpdateTimerUI();
    }

    // =====================================
    // RESET TIMER
    // =====================================

    public void ResetTimer()
    {
        running = false;
        timeExpired = false;

        currentTime = jumpTime;

        UpdateTimerUI();
    }

    // =====================================
    // TIME EXPIRED
    // =====================================

    void TimerExpired()
    {
        if (timeExpired)
            return;

        timeExpired = true;
        running = false;

        Debug.Log(
            "JUMP TIME EXPIRED!"
        );

        // Return from slow motion
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Hide Timing Bar
        if (timingBar != null)
        {
            timingBar.Deactivate();
        }

        // Stop Player
        if (player != null)
        {
            player.StopPlayer();

            // Optional Lose animation
            player.PlayLose();
        }

        // GAME OVER
        if (levelManager != null)
        {
            levelManager.LoseGame();
        }
    }

    // =====================================
    // UI
    // =====================================

    void UpdateTimerUI()
    {
        if (timerText == null)
            return;

        int seconds =
            Mathf.CeilToInt(
                currentTime
            );

        timerText.text =
            "Time: " + seconds;
    }

    // =====================================
    // GETTERS
    // =====================================

    public bool IsRunning()
    {
        return running;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
}