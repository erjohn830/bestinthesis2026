using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeDuration = 30f;
    private float currentTime;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Hearts")]
    public GameObject[] hearts;

    [Header("Lose Panel")]
    public GameObject losePanel;

    private int currentHearts;

    void Start()
    {
        ResetForNewLevel();
    }

    void Update()
    {
        RunTimer();
        UpdateUI();
    }

    void RunTimer()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            LoseHeart();
            ResetTimer(); // ✅ use the new function here
        }
    }

    // ✅ NEW: simple timer reset (ONLY resets time)
    public void ResetTimer()
    {
        currentTime = timeDuration;
    }

    // ⭐ FULL reset (used for new level)
    public void ResetForNewLevel()
    {
        currentTime = timeDuration;

        currentHearts = hearts.Length;

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(true);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void LoseHeart()
    {
        if (currentHearts > 0)
        {
            currentHearts--;
            hearts[currentHearts].SetActive(false);

            // reset timer after losing heart
            ResetTimer();

            if (currentHearts == 0)
            {
                GameOver();
            }
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(currentTime).ToString();
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}