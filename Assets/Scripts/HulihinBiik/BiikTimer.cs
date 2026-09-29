using TMPro;
using UnityEngine;

public class BiikTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float gameDuration = 300f;
    [SerializeField] private TMP_Text timerText;

    [Header("Timer Speed")]
    [Tooltip("1 = normal timer speed")]
    [SerializeField] private float timeScale = 1f;

    [Header("Manager")]
    [SerializeField] private BiikManager biikManager;

    private float remainingTime;
    private bool timerRunning;

    public float RemainingTime => remainingTime;
    public bool TimerRunning => timerRunning;

    private void Start()
    {
        remainingTime = gameDuration;
        timerRunning = false;

        UpdateTimerText();
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        remainingTime -=
            Time.deltaTime * timeScale;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;

            UpdateTimerText();

            if (biikManager != null)
            {
                biikManager.TimeUp();
            }

            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int minutes =
            Mathf.FloorToInt(
                remainingTime / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                remainingTime % 60f
            );

        timerText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }

    // =====================================================
    // STOP
    // =====================================================

    public void StopTimer()
    {
        timerRunning = false;
    }

    // =====================================================
    // RESUME
    // =====================================================

    public void ResumeTimer()
    {
        if (remainingTime > 0f)
        {
            timerRunning = true;
        }
    }

    // =====================================================
    // PREPARE REVIVE TIME
    //
    // Sets 01:00 but DOES NOT start yet.
    // Countdown happens first.
    // =====================================================

    public void PrepareReviveTime()
    {
        remainingTime = 60f;
        timerRunning = false;

        UpdateTimerText();

        Debug.Log(
            "Revive time prepared: 01:00"
        );
    }

    // =====================================================
    // START REVIVE TIMER
    // =====================================================

    public void StartReviveTimer()
    {
        remainingTime = 60f;
        timerRunning = true;

        UpdateTimerText();

        Debug.Log(
            "Revive timer STARTED: 01:00"
        );
    }

    // =====================================================
    // CUSTOM TIME
    // =====================================================

    public void SetTime(float seconds)
    {
        remainingTime =
            Mathf.Max(0f, seconds);

        UpdateTimerText();
    }
}