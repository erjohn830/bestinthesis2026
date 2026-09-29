using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PaloseboManager : MonoBehaviour
{
    public static PaloseboManager Instance;

    [Header("Player")]
    [SerializeField] private PaloseboPlayer player;

    [Header("Gameplay Systems")]
    [SerializeField] private PaloseboBalance balanceSystem;

    [Header("Camera")]
    [SerializeField] private PaloseboCamera paloseboCamera;

    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private GameObject finalClimbPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Countdown")]
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite number3;
    [SerializeField] private Sprite number2;
    [SerializeField] private Sprite number1;
    [SerializeField] private Sprite goImage;

    [Header("Countdown Time")]
    [SerializeField] private float numberDuration = 1f;
    [SerializeField] private float goDuration = 0.7f;

    [Header("Result Delay")]
    [SerializeField] private float resultDelay = 1.2f;

    [Header("Scenes")]
    [SerializeField] private string homeSceneName;
    [SerializeField] private string nextSceneName;

    public bool GameStarted { get; private set; }
    public bool GameFinished { get; private set; }

    private Coroutine countdownRoutine;
    private Coroutine resultRoutine;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Time.timeScale = 1f;

        GameStarted = false;
        GameFinished = false;

        SetPanel(titlePanel, true);
        SetPanel(instructionPanel, false);
        SetPanel(countdownPanel, false);

        // IMPORTANT:
        // Balance bar + buttons remain hidden until GO.
        SetPanel(gameplayUI, false);

        SetPanel(finalClimbPanel, false);
        SetPanel(winPanel, false);
        SetPanel(losePanel, false);

        if (player != null)
            player.SetGameplay(false);

        if (balanceSystem != null)
            balanceSystem.StopGameplay();

        if (paloseboCamera != null)
        {
            paloseboCamera.StopFollowing();
            paloseboCamera.ResetCamera();
        }

        Debug.Log("PALOSOBO MANAGER: READY");
    }

    // =========================================================
    // TITLE
    // =========================================================

    public void PlayFromTitle()
    {
        if (GameFinished)
            return;

        SetPanel(titlePanel, false);
        SetPanel(instructionPanel, false);

        BeginCountdown();
    }

    public void OpenInstruction()
    {
        if (GameFinished)
            return;

        SetPanel(titlePanel, false);
        SetPanel(instructionPanel, true);
        SetPanel(gameplayUI, false);

        if (paloseboCamera != null)
            paloseboCamera.StopFollowing();
    }

    public void BackToTitle()
    {
        if (GameFinished)
            return;

        StopCountdown();

        SetPanel(instructionPanel, false);
        SetPanel(countdownPanel, false);
        SetPanel(gameplayUI, false);
        SetPanel(titlePanel, true);

        if (player != null)
            player.SetGameplay(false);

        if (balanceSystem != null)
            balanceSystem.StopGameplay();

        if (paloseboCamera != null)
        {
            paloseboCamera.StopFollowing();
            paloseboCamera.ResetCamera();
        }
    }

    // =========================================================
    // INSTRUCTION -> GAME
    // =========================================================

    public void StartFromInstruction()
    {
        if (GameFinished)
            return;

        SetPanel(instructionPanel, false);

        BeginCountdown();
    }

    // =========================================================
    // COUNTDOWN
    // =========================================================

    private void BeginCountdown()
    {
        StopCountdown();

        countdownRoutine = StartCoroutine(StartCountdown());
    }

    private void StopCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
    }

    private IEnumerator StartCountdown()
    {
        GameStarted = false;
        GameFinished = false;

        SetPanel(gameplayUI, false);
        SetPanel(countdownPanel, true);
        SetPanel(winPanel, false);
        SetPanel(losePanel, false);

        if (player != null)
            player.SetGameplay(false);

        if (balanceSystem != null)
            balanceSystem.StopGameplay();

        // Camera must stay at starting view during countdown.
        if (paloseboCamera != null)
        {
            paloseboCamera.StopFollowing();
            paloseboCamera.ResetCamera();
        }

        if (countdownImage != null)
        {
            if (number3 != null)
            {
                countdownImage.sprite = number3;
                yield return new WaitForSeconds(numberDuration);
            }

            if (number2 != null)
            {
                countdownImage.sprite = number2;
                yield return new WaitForSeconds(numberDuration);
            }

            if (number1 != null)
            {
                countdownImage.sprite = number1;
                yield return new WaitForSeconds(numberDuration);
            }

            if (goImage != null)
            {
                countdownImage.sprite = goImage;
                yield return new WaitForSeconds(goDuration);
            }
        }

        SetPanel(countdownPanel, false);

        // Balance bar only appears NOW.
        SetPanel(gameplayUI, true);

        GameStarted = true;
        GameFinished = false;

        // Start player FIRST.
        if (player != null)
            player.SetGameplay(true);

        // Then balance system.
        if (balanceSystem != null)
            balanceSystem.StartGameplay();

        // Then camera follows the PlayerMover.
        if (paloseboCamera != null)
            paloseboCamera.StartFollowing();

        countdownRoutine = null;

        Debug.Log("PALOSOBO GAME STARTED!");
    }

    // =========================================================
    // FINAL CLIMB UI
    // =========================================================

    public void ShowFinalClimb(bool show)
    {
        if (finalClimbPanel != null)
            finalClimbPanel.SetActive(show);
    }

    // =========================================================
    // WIN
    // =========================================================

    public void Win()
    {
        if (GameFinished)
            return;

        Debug.Log("PALOSOBO MANAGER: WIN!");

        GameFinished = true;
        GameStarted = false;

        StopCountdown();

        if (balanceSystem != null)
            balanceSystem.StopGameplay();

        if (paloseboCamera != null)
            paloseboCamera.StopFollowing();

        SetPanel(gameplayUI, false);
        SetPanel(finalClimbPanel, false);

        if (player != null)
            player.PlayWinAnimation();

        if (resultRoutine != null)
            StopCoroutine(resultRoutine);

        resultRoutine = StartCoroutine(ShowWinPanel());
    }

    private IEnumerator ShowWinPanel()
    {
        yield return new WaitForSeconds(resultDelay);

        SetPanel(winPanel, true);

        resultRoutine = null;
    }

    // =========================================================
    // LOSE
    // =========================================================

    public void Lose()
    {
        if (GameFinished)
            return;

        Debug.Log("PALOSOBO MANAGER: LOSE!");

        GameFinished = true;
        GameStarted = false;

        StopCountdown();

        if (balanceSystem != null)
            balanceSystem.StopGameplay();

        if (paloseboCamera != null)
            paloseboCamera.StopFollowing();

        SetPanel(gameplayUI, false);
        SetPanel(finalClimbPanel, false);

        if (player != null)
            player.Lose();

        if (resultRoutine != null)
            StopCoroutine(resultRoutine);

        resultRoutine = StartCoroutine(ShowLosePanel());
    }

    private IEnumerator ShowLosePanel()
    {
        yield return new WaitForSeconds(resultDelay);

        SetPanel(losePanel, true);

        resultRoutine = null;
    }

    // =========================================================
    // BUTTONS
    // =========================================================

    public void TryAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void Home()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
        }
        else
        {
            Debug.LogWarning(
                "PALOSOBO: Home Scene Name is empty."
            );
        }
    }

    public void Next()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning(
                "PALOSOBO: Next Scene Name is empty."
            );
        }
    }

    // =========================================================
    // PANEL HELPER
    // =========================================================

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}