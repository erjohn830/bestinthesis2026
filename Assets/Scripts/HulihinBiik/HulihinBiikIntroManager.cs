using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HulihinBiikIntroManager : MonoBehaviour
{
    [Header("PANELS")]
    [SerializeField] private GameObject demoPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private GameObject gameplayUI;

    // =====================================================
    // COUNTDOWN - IMAGE VERSION
    // =====================================================

    [Header("COUNTDOWN IMAGE")]
    [SerializeField] private Image countdownImage;

    [Header("COUNTDOWN SPRITES")]
    [SerializeField] private Sprite countdown3Image;
    [SerializeField] private Sprite countdown2Image;
    [SerializeField] private Sprite countdown1Image;
    [SerializeField] private Sprite countdownGoImage;

    [Header("COUNTDOWN TIMING")]
    [SerializeField] private float numberDuration = 1f;
    [SerializeField] private float goDuration = 0.8f;

    [Header("COUNTDOWN ANIMATION")]
    [SerializeField] private float startScale = 1.35f;
    [SerializeField] private float scaleSpeed = 8f;

    // =====================================================
    // GAME SYSTEMS
    // =====================================================

    [Header("GAME SYSTEMS")]
    [SerializeField] private PlayerBiikMovement playerMovement;
    [SerializeField] private PigAI pigAI;
    [SerializeField] private BiikTimer gameTimer;

    private bool demoFinished = false;
    private bool tutorialFinished = false;
    private bool countdownRunning = false;
    private bool gameStarted = false;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        demoFinished = false;
        tutorialFinished = false;
        countdownRunning = false;
        gameStarted = false;

        // Starting UI.
        SetPanel(demoPanel, true);
        SetPanel(tutorialPanel, false);
        SetPanel(countdownPanel, false);
        SetPanel(gameplayUI, false);

        // Hide countdown image.
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }

        // Lock gameplay.
        StopGameSystems();
    }

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // Lock again after other Start() methods.
        StopGameSystems();
    }

    // =====================================================
    // DEMO
    // =====================================================

    public void FinishDemo()
    {
        if (demoFinished)
        {
            return;
        }

        demoFinished = true;

        SetPanel(demoPanel, false);
        SetPanel(tutorialPanel, true);
        SetPanel(countdownPanel, false);
        SetPanel(gameplayUI, false);

        StopGameSystems();

        Debug.Log(
            "Demo Finished. Tutorial Started."
        );
    }

    // =====================================================
    // TUTORIAL
    // =====================================================

    public void FinishTutorialAndStartCountdown()
    {
        // Cannot skip Demo.
        if (!demoFinished)
        {
            Debug.LogWarning(
                "Finish the Demo first."
            );

            return;
        }

        if (tutorialFinished ||
            countdownRunning ||
            gameStarted)
        {
            return;
        }

        tutorialFinished = true;

        StartCountdown();
    }

    // =====================================================
    // START COUNTDOWN
    // =====================================================

    private void StartCountdown()
    {
        if (!demoFinished ||
            !tutorialFinished)
        {
            Debug.LogWarning(
                "Demo and Tutorial must be completed first."
            );

            return;
        }

        if (countdownRunning)
        {
            return;
        }

        countdownRunning = true;

        StopAllCoroutines();

        StartCoroutine(
            CountdownRoutine()
        );
    }

    // =====================================================
    // COUNTDOWN ROUTINE
    // =====================================================

    private IEnumerator CountdownRoutine()
    {
        // Hide instructions.
        SetPanel(demoPanel, false);
        SetPanel(tutorialPanel, false);

        // Show gameplay background/UI.
        SetPanel(gameplayUI, true);

        // Show countdown panel.
        SetPanel(countdownPanel, true);

        // Show countdown image.
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(true);
        }

        // Gameplay remains locked.
        StopGameSystems();

        // =================================================
        // 3
        // =================================================

        yield return StartCoroutine(
            ShowCountdownImage(
                countdown3Image,
                numberDuration
            )
        );

        // =================================================
        // 2
        // =================================================

        yield return StartCoroutine(
            ShowCountdownImage(
                countdown2Image,
                numberDuration
            )
        );

        // =================================================
        // 1
        // =================================================

        yield return StartCoroutine(
            ShowCountdownImage(
                countdown1Image,
                numberDuration
            )
        );

        // =================================================
        // GO
        // =================================================

        yield return StartCoroutine(
            ShowCountdownImage(
                countdownGoImage,
                goDuration
            )
        );

        // =================================================
        // COUNTDOWN FINISHED
        // =================================================

        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }

        SetPanel(countdownPanel, false);

        countdownRunning = false;

        StartActualGame();
    }

    // =====================================================
    // SHOW COUNTDOWN IMAGE
    // =====================================================

    private IEnumerator ShowCountdownImage(
        Sprite sprite,
        float duration)
    {
        if (countdownImage == null)
        {
            Debug.LogError(
                "Countdown Image is NOT assigned!"
            );

            yield return new WaitForSeconds(duration);
            yield break;
        }

        if (sprite == null)
        {
            Debug.LogWarning(
                "One of the countdown sprites is missing!"
            );

            yield return new WaitForSeconds(duration);
            yield break;
        }

        // Change the image.
        countdownImage.sprite = sprite;

        // Prevent stretching.
        countdownImage.preserveAspect = true;

        countdownImage.gameObject.SetActive(true);

        // Start slightly bigger.
        countdownImage.transform.localScale =
            Vector3.one * startScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Smoothly shrink toward normal size.
            countdownImage.transform.localScale =
                Vector3.Lerp(
                    countdownImage.transform.localScale,
                    Vector3.one,
                    scaleSpeed * Time.deltaTime
                );

            yield return null;
        }

        // Reset scale.
        countdownImage.transform.localScale =
            Vector3.one;
    }

    // =====================================================
    // ACTUAL GAME
    // =====================================================

    private void StartActualGame()
    {
        if (!demoFinished ||
            !tutorialFinished)
        {
            Debug.LogWarning(
                "GAME BLOCKED: Demo or Tutorial is not complete."
            );

            StopGameSystems();

            return;
        }

        if (countdownRunning)
        {
            return;
        }

        gameStarted = true;

        SetPanel(demoPanel, false);
        SetPanel(tutorialPanel, false);
        SetPanel(countdownPanel, false);
        SetPanel(gameplayUI, true);

        // =================================================
        // PLAYER START
        // =================================================

        if (playerMovement != null)
        {
            playerMovement.ResumeMovement();
        }

        // =================================================
        // PIG START
        // =================================================

        if (pigAI != null)
        {
            pigAI.ResumePig();
        }

        // =================================================
        // TIMER START
        // =================================================

        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();
        }

        Debug.Log(
            "HULIHIN BIIK GAMEPLAY STARTED!"
        );
    }

    // =====================================================
    // LOCK GAMEPLAY
    // =====================================================

    private void StopGameSystems()
    {
        if (playerMovement != null)
        {
            playerMovement.StopMovement();
        }

        if (pigAI != null)
        {
            pigAI.PausePig();
        }

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }
    }

    // =====================================================
    // PANEL HELPER
    // =====================================================

    private void SetPanel(
        GameObject panel,
        bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}