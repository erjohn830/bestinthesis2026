using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SipaManager : MonoBehaviour
{
    public static SipaManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int targetScore = 10;

    [Header("Player")]
    [SerializeField] private SipaAnimation playerAnimation;
    [SerializeField] private PatoMovement playerMovement;

    [Header("Pato")]
    [SerializeField] private SipaBall sipaBall;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameplayControls;

    [Header("Title Panel")]
    [SerializeField] private GameObject titlePanel;

    [Header("Instruction")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private SipaInstructionManager instructionManager;

    [Header("Countdown")]
    [SerializeField] private GameObject countdownPanel;

    [Tooltip("Drag NUMBER 3 Image here")]
    [SerializeField] private GameObject countdown3Image;

    [Tooltip("Drag NUMBER 2 Image here")]
    [SerializeField] private GameObject countdown2Image;

    [Tooltip("Drag NUMBER 1 Image here")]
    [SerializeField] private GameObject countdown1Image;

    [SerializeField] private float countdownImageDuration = 1f;

    [Header("Waiting To Start")]
    [Tooltip("TOUCH ANY BUTTON TO START")]
    [SerializeField] private GameObject touchToStartText;

    [Header("Win Panel")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winFinalScoreText;

    [Header("Lose Panel")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text loseFinalScoreText;

    [Header("Result Delay")]
    [SerializeField] private float resultPanelDelay = 1.2f;


    private int score = 0;

    private bool gameStarted = false;
    private bool gameFinished = false;
    private bool playerWon = false;

    private bool countdownRunning = false;
    private bool waitingForFirstPlayerAction = false;


    public bool IsGameStarted => gameStarted;
    public bool IsGameFinished => gameFinished;
    public bool IsGameOver => gameFinished;

    public int Score => score;


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

        score = 0;

        gameStarted = false;
        gameFinished = false;
        playerWon = false;
        countdownRunning = false;
        waitingForFirstPlayerAction = false;

        HideEverything();
        UpdateScoreUI();

        if (titlePanel != null)
            titlePanel.SetActive(true);

        SetGameplayActive(false);

        if (sipaBall != null)
        {
            sipaBall.ResetBall();
            sipaBall.FreezeAtStart();
        }
    }


    // =========================================================
    // UI
    // =========================================================

    private void HideEverything()
    {
        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        HideCountdownImages();

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        if (gameplayControls != null)
            gameplayControls.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }


    private void HideCountdownImages()
    {
        if (countdown3Image != null)
            countdown3Image.SetActive(false);

        if (countdown2Image != null)
            countdown2Image.SetActive(false);

        if (countdown1Image != null)
            countdown1Image.SetActive(false);
    }


    // =========================================================
    // PLAY
    // =========================================================

    public void PlayFromTitle()
    {
        if (countdownRunning)
            return;

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        StartCountdown();
    }


    // =========================================================
    // INSTRUCTIONS
    // =========================================================

    public void OpenInstructionsFromTitle()
    {
        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(true);

        if (gameplayControls != null)
            gameplayControls.SetActive(false);

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        SetGameplayActive(false);

        if (instructionManager != null)
            instructionManager.OpenInstructions();
    }


    public void StartGameAfterInstructions()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (instructionManager != null)
            instructionManager.RestoreAllTargets();

        StartCountdown();
    }


    public void StartGameFromInstructions()
    {
        StartGameAfterInstructions();
    }


    public void CloseInstructionAndStartCountdown()
    {
        StartGameAfterInstructions();
    }


    // =========================================================
    // RETURN TO TITLE
    // =========================================================

    public void ReturnToTitle()
    {
        StopAllCoroutines();
        CancelInvoke();

        Time.timeScale = 1f;

        score = 0;

        gameStarted = false;
        gameFinished = false;
        playerWon = false;
        countdownRunning = false;
        waitingForFirstPlayerAction = false;

        UpdateScoreUI();

        HideEverything();

        if (titlePanel != null)
            titlePanel.SetActive(true);

        SetGameplayActive(false);

        if (playerMovement != null)
        {
            playerMovement.StopGameplayMovement();
            playerMovement.ResetMovement();
        }

        if (sipaBall != null)
        {
            sipaBall.ResetBall();
            sipaBall.FreezeAtStart();
        }

        if (playerAnimation != null)
            playerAnimation.ResetToIdle();
    }


    // =========================================================
    // COUNTDOWN
    // =========================================================

    private void StartCountdown()
    {
        if (countdownRunning)
            return;

        StopAllCoroutines();

        StartCoroutine(CountdownRoutine());
    }


    private IEnumerator CountdownRoutine()
    {
        countdownRunning = true;

        gameStarted = false;
        gameFinished = false;
        playerWon = false;
        waitingForFirstPlayerAction = false;

        SetGameplayActive(false);

        if (gameplayControls != null)
            gameplayControls.SetActive(false);

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (sipaBall != null)
        {
            sipaBall.SetGameplayEnabled(false);
            sipaBall.FreezeAtStart();
        }

        HideCountdownImages();

        if (countdownPanel != null)
            countdownPanel.SetActive(true);


        // =====================
        // 3
        // =====================

        if (countdown3Image != null)
            countdown3Image.SetActive(true);

        yield return new WaitForSeconds(countdownImageDuration);

        if (countdown3Image != null)
            countdown3Image.SetActive(false);


        // =====================
        // 2
        // =====================

        if (countdown2Image != null)
            countdown2Image.SetActive(true);

        yield return new WaitForSeconds(countdownImageDuration);

        if (countdown2Image != null)
            countdown2Image.SetActive(false);


        // =====================
        // 1
        // =====================

        if (countdown1Image != null)
            countdown1Image.SetActive(true);

        yield return new WaitForSeconds(countdownImageDuration);

        if (countdown1Image != null)
            countdown1Image.SetActive(false);


        // =====================
        // START GAME
        // =====================

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        HideCountdownImages();

        countdownRunning = false;

        gameStarted = true;
        gameFinished = false;
        playerWon = false;

        if (gameplayControls != null)
            gameplayControls.SetActive(true);

        if (playerMovement != null)
            playerMovement.SetGameplayEnabled(true);

        if (sipaBall != null)
        {
            // Ball is allowed to play,
            // but remains frozen until first button press.
            sipaBall.SetGameplayEnabled(true);
            sipaBall.FreezeAtStart();
        }

        waitingForFirstPlayerAction = true;

        if (touchToStartText != null)
            touchToStartText.SetActive(true);

        if (playerAnimation != null)
            playerAnimation.ResetToIdle();
    }


    // =========================================================
    // FIRST PLAYER ACTION
    // =========================================================

    public void NotifyPlayerAction()
    {
        if (!gameStarted)
            return;

        if (gameFinished)
            return;

        if (!waitingForFirstPlayerAction)
            return;

        waitingForFirstPlayerAction = false;

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        if (sipaBall != null)
            sipaBall.ReleasePato();
    }


    // =========================================================
    // GAMEPLAY
    // =========================================================

    private void SetGameplayActive(bool active)
    {
        if (playerMovement != null)
            playerMovement.SetGameplayEnabled(active);

        if (sipaBall != null)
            sipaBall.SetGameplayEnabled(active);
    }


    // =========================================================
    // SUCCESSFUL KICK
    // =========================================================

    public void RegisterSuccessfulKick()
    {
        if (!gameStarted)
        {
            Debug.LogWarning(
                "Kick detected but game has not started."
            );

            return;
        }

        if (gameFinished)
            return;

        score++;

        UpdateScoreUI();

        Debug.Log(
            "SUCCESSFUL KICK! SCORE = " +
            score +
            "/" +
            targetScore
        );

        if (score >= targetScore)
            WinGame();
    }


    // =========================================================
    // SCORE
    // =========================================================

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Score: " +
                score +
                " / " +
                targetScore;
        }
    }


    // =========================================================
    // WIN
    // =========================================================

    public void WinGame()
    {
        if (!gameStarted)
            return;

        if (gameFinished)
            return;

        gameFinished = true;
        gameStarted = false;
        playerWon = true;
        waitingForFirstPlayerAction = false;

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        StopGameplay();

        if (playerAnimation != null)
            playerAnimation.PlayWinAnimation();

        Invoke(
            nameof(ShowWinPanel),
            resultPanelDelay
        );
    }


    private void ShowWinPanel()
    {
        if (!playerWon)
            return;

        if (winFinalScoreText != null)
        {
            winFinalScoreText.text =
                "Final Score: " + score;
        }

        if (winPanel != null)
            winPanel.SetActive(true);
    }


    // =========================================================
    // LOSE
    // =========================================================

    public void LoseGame()
    {
        if (!gameStarted)
            return;

        if (gameFinished)
            return;

        gameFinished = true;
        gameStarted = false;
        playerWon = false;
        waitingForFirstPlayerAction = false;

        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        StopGameplay();

        if (playerAnimation != null)
            playerAnimation.PlayLoseAnimation();

        Invoke(
            nameof(ShowLosePanel),
            resultPanelDelay
        );
    }


    public void GameOver()
    {
        LoseGame();
    }


    private void ShowLosePanel()
    {
        if (playerWon)
            return;

        if (loseFinalScoreText != null)
        {
            loseFinalScoreText.text =
                "Final Score: " + score;
        }

        if (losePanel != null)
            losePanel.SetActive(true);
    }


    // =========================================================
    // STOP GAMEPLAY
    // =========================================================

    private void StopGameplay()
    {
        if (gameplayControls != null)
            gameplayControls.SetActive(false);

        if (playerMovement != null)
            playerMovement.StopGameplayMovement();

        if (sipaBall != null)
            sipaBall.StopBall();
    }


    // =========================================================
    // TRY AGAIN
    // =========================================================

    public void TryAgain()
    {
        CancelInvoke();
        StopAllCoroutines();

        Time.timeScale = 1f;

        // NEW GAME = reset score.
        score = 0;

        gameStarted = false;
        gameFinished = false;
        playerWon = false;
        countdownRunning = false;
        waitingForFirstPlayerAction = false;

        UpdateScoreUI();

        PrepareRestart();

        StartCountdown();
    }


    // =========================================================
    // CONTINUE AFTER AD
    // =========================================================

    public void ContinueAfterAd()
    {
        CancelInvoke();
        StopAllCoroutines();

        Time.timeScale = 1f;

        // IMPORTANT:
        // DO NOT RESET SCORE HERE.
        //
        // Example:
        // Player loses at 3/10.
        // Watches advertisement.
        // Continues at 3/10.

        gameStarted = false;
        gameFinished = false;
        playerWon = false;
        countdownRunning = false;
        waitingForFirstPlayerAction = false;

        PrepareRestart();

        // Keep previous score visible.
        UpdateScoreUI();

        StartCountdown();
    }


    // =========================================================
    // PREPARE RESTART
    // =========================================================

    private void PrepareRestart()
    {
        if (touchToStartText != null)
            touchToStartText.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (countdownPanel != null)
            countdownPanel.SetActive(false);

        HideCountdownImages();

        if (gameplayControls != null)
            gameplayControls.SetActive(false);


        // Reset player.
        if (playerAnimation != null)
            playerAnimation.ResetToIdle();

        if (playerMovement != null)
        {
            playerMovement.StopGameplayMovement();
            playerMovement.ResetMovement();
        }


        // Reset Pato to original position.
        if (sipaBall != null)
        {
            sipaBall.StopBall();
            sipaBall.ResetBall();
            sipaBall.FreezeAtStart();
        }
    }


    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public void NextLevel()
    {
        if (!playerWon)
            return;

        int currentScene =
            SceneManager.GetActiveScene().buildIndex;

        int nextScene = currentScene + 1;

        if (nextScene <
            SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning(
                "No next Sipa scene found."
            );
        }
    }


    // =========================================================
    // HOME
    // =========================================================

    public void Home()
    {
        ReturnToTitle();
    }
}