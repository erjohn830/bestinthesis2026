using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BiikManager : MonoBehaviour
{
    private enum GameState
    {
        Playing,
        Catching,
        Won,
        Lost
    }

    // ==================================================
    // CHARACTERS
    // ==================================================

    [Header("Characters")]
    [SerializeField] private PlayerBiikMovement playerMovement;
    [SerializeField] private PlayerCatch playerCatch;
    [SerializeField] private PigAI pigAI;

    // ==================================================
    // SYSTEMS
    // ==================================================

    [Header("Systems")]
    [SerializeField] private CameraControllerBiik cameraController;
    [SerializeField] private ButtonMashManager buttonMashManager;
    [SerializeField] private BiikTimer gameTimer;

    // ==================================================
    // LEVEL
    // ==================================================

    [Header("Level System")]
    [SerializeField] private BiikLevelManager levelManager;

    // ==================================================
    // COMBINED CATCH
    // ==================================================

    [Header("Combined Catch")]
    [SerializeField] private CombinedCatchController combinedCatch;
    [SerializeField] private Transform combinedCatchSpawnPoint;

    [SerializeField] private GameObject playerVisual;
    [SerializeField] private GameObject pigVisual;

    // ==================================================
    // GAMEPLAY UI
    // ==================================================

    [Header("Playing UI")]
    [SerializeField] private GameObject joystick;

    // ==================================================
    // REVIVE COUNTDOWN
    // ==================================================

    [Header("Revive Countdown")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private Image countdownImage;

    [Header("Countdown Images")]
    [SerializeField] private Sprite countdown3Image;
    [SerializeField] private Sprite countdown2Image;
    [SerializeField] private Sprite countdown1Image;
    [SerializeField] private Sprite countdownGoImage;

    [Header("Countdown Timing")]
    [SerializeField] private float countdownNumberTime = 1f;
    [SerializeField] private float countdownGoTime = 0.8f;

    [Header("Countdown Animation")]
    [SerializeField] private float countdownStartScale = 1.35f;
    [SerializeField] private float countdownScaleSpeed = 8f;

    // ==================================================
    // RESULT PANELS
    // ==================================================

    [Header("Result Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    // ==================================================
    // NEXT LEVEL
    // ==================================================

    [Header("Next Level")]
    [SerializeField] private float nextLevelDelay = 1f;
    [SerializeField] private float catchProtectionTime = 2f;

    // ==================================================
    // SCENES
    // ==================================================

    [Header("Scenes")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private string homeSceneName = "Menu";

    // ==================================================
    // PRIVATE
    // ==================================================

    private GameState state = GameState.Playing;

    private Vector3 currentCatchPosition;
    private Quaternion currentCatchRotation;

    // ==================================================
    // START
    // ==================================================

    private void Start()
    {
        state = GameState.Playing;

        // ------------------------------------------
        // Find references automatically
        // ------------------------------------------

        if (playerMovement == null)
        {
            playerMovement =
                FindFirstObjectByType<PlayerBiikMovement>();
        }

        if (playerCatch == null)
        {
            playerCatch =
                FindFirstObjectByType<PlayerCatch>();
        }

        if (pigAI == null)
        {
            pigAI =
                FindFirstObjectByType<PigAI>();
        }

        if (cameraController == null)
        {
            cameraController =
                FindFirstObjectByType<CameraControllerBiik>();
        }

        if (buttonMashManager == null)
        {
            buttonMashManager =
                FindFirstObjectByType<ButtonMashManager>();
        }

        if (gameTimer == null)
        {
            gameTimer =
                FindFirstObjectByType<BiikTimer>();
        }

        if (levelManager == null)
        {
            levelManager =
                FindFirstObjectByType<BiikLevelManager>();
        }

        // ------------------------------------------
        // Starting UI
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.gameObject.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        /*
         * IMPORTANT:
         *
         * Do NOT disable CountdownPanel here.
         *
         * Your HulihinBiikIntroManager also uses the
         * SAME CountdownPanel for the normal
         * 3-2-1-GO starting countdown.
         *
         * IntroManager controls it when the scene starts.
         */
    }

    // ==================================================
    // CATCH START
    // ==================================================

    public void TryCatchPig(PigAI caughtPig)
    {
        if (state != GameState.Playing)
        {
            return;
        }

        if (caughtPig == null)
        {
            return;
        }

        if (playerMovement == null)
        {
            return;
        }

        state = GameState.Catching;

        pigAI = caughtPig;

        // Save the exact safe catch location.
        currentCatchPosition =
            pigAI.GetSafeArenaPosition(
                pigAI.transform.position
            );

        currentCatchRotation =
            pigAI.transform.rotation;

        // ------------------------------------------
        // Stop Player
        // ------------------------------------------

        playerMovement.StopMovement();

        // ------------------------------------------
        // Catch Pig
        // ------------------------------------------

        pigAI.BeginCaught();

        // ------------------------------------------
        // Disable CatchButton
        // ------------------------------------------

        if (playerCatch != null)
        {
            playerCatch.DisableDetection();
        }

        // ------------------------------------------
        // Hide joystick
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(false);
        }

        // ------------------------------------------
        // Hide normal characters
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(false);
        }

        if (pigVisual != null)
        {
            pigVisual.SetActive(false);
        }

        // ------------------------------------------
        // Combined catch animation
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.gameObject.SetActive(true);

            combinedCatch.transform.position =
                currentCatchPosition;

            combinedCatch.transform.rotation =
                currentCatchRotation;

            combinedCatch.PlayCatching();
        }

        // ------------------------------------------
        // Catch camera
        // ------------------------------------------

        if (cameraController != null &&
            combinedCatch != null)
        {
            cameraController.StartCombinedCatchView(
                combinedCatch.transform
            );
        }

        // ------------------------------------------
        // Button mash
        // ------------------------------------------

        if (buttonMashManager != null)
        {
            buttonMashManager.StartMash(
                CatchSuccess,
                CatchFailed
            );
        }
    }

    // ==================================================
    // CATCH SUCCESS
    // ==================================================

    private void CatchSuccess()
    {
        if (state != GameState.Catching)
        {
            return;
        }

        // Level 1 / Level 2.
        if (levelManager != null &&
            !levelManager.IsLastLevel)
        {
            StartCoroutine(
                ContinueFromCatchPosition()
            );

            return;
        }

        // Final level.
        FinalGameWin();
    }

    // ==================================================
    // NEXT LEVEL
    // ==================================================

    private IEnumerator ContinueFromCatchPosition()
    {
        state = GameState.Catching;

        // Keep catch animation visible briefly.
        yield return new WaitForSeconds(
            nextLevelDelay
        );

        // ------------------------------------------
        // Remove combined catch
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.ResumeAnimation();
            combinedCatch.gameObject.SetActive(false);
        }

        // ------------------------------------------
        // Show normal characters
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (pigVisual != null)
        {
            pigVisual.SetActive(true);
        }

        // ------------------------------------------
        // Advance level
        // ------------------------------------------

        if (levelManager != null)
        {
            levelManager.GoToNextLevel();
        }

        // ------------------------------------------
        // Player
        // ------------------------------------------

        if (playerMovement != null)
        {
            playerMovement.RestoreAnimatorSpeed();
            playerMovement.ReturnToMovementAnimation();

            playerMovement.StopMovement();
        }

        // ------------------------------------------
        // Camera
        // ------------------------------------------

        if (cameraController != null)
        {
            cameraController.ReturnToNormalView();
        }

        // ------------------------------------------
        // Pig starts new level
        // ------------------------------------------

        if (pigAI != null)
        {
            pigAI.RestoreAnimatorSpeed();

            pigAI.StartNextLevelFromCurrentPosition();
        }

        // ------------------------------------------
        // Knock player away
        // ------------------------------------------

        if (playerMovement != null &&
            pigAI != null)
        {
            playerMovement.KnockAwayFrom(
                pigAI.transform.position
            );
        }

        // ------------------------------------------
        // Show joystick
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(true);
        }

        yield return new WaitForSeconds(1.2f);

        state = GameState.Playing;

        // Explicitly unlock player after knockback.
        if (playerMovement != null)
        {
            playerMovement.ResumeMovement();
        }

        // Catch protection.
        yield return new WaitForSeconds(
            catchProtectionTime
        );

        // ------------------------------------------
        // Reset CatchButton
        // ------------------------------------------

        if (playerCatch != null &&
            state == GameState.Playing)
        {
            playerCatch.ResetForNextLevel();

            Debug.Log(
                "Catch system READY for next level."
            );
        }
    }

    // ==================================================
    // FAILED CATCH
    // ==================================================

    private void CatchFailed()
    {
        if (state != GameState.Catching)
        {
            return;
        }

        state = GameState.Playing;

        // ------------------------------------------
        // Remove combined catch
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.ResumeAnimation();
            combinedCatch.gameObject.SetActive(false);
        }

        // ------------------------------------------
        // Show characters
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (pigVisual != null)
        {
            pigVisual.SetActive(true);
        }

        // ------------------------------------------
        // Pig escapes
        // ------------------------------------------

        if (pigAI != null)
        {
            pigAI.RestoreAnimatorSpeed();
            pigAI.EscapeFromPlayer();
        }

        // ------------------------------------------
        // Player knockback
        // ------------------------------------------

        if (playerMovement != null &&
            pigAI != null)
        {
            playerMovement.RestoreAnimatorSpeed();

            playerMovement.KnockAwayFrom(
                pigAI.transform.position
            );
        }

        // ------------------------------------------
        // Camera
        // ------------------------------------------

        if (cameraController != null)
        {
            cameraController.ReturnToNormalView();
        }

        // ------------------------------------------
        // Joystick
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(true);
        }

        Invoke(
            nameof(EnableCatchDetection),
            2.2f
        );
    }

    // ==================================================
    // ENABLE CATCH AFTER FAILED CATCH
    // ==================================================

    private void EnableCatchDetection()
    {
        if (state != GameState.Playing)
        {
            return;
        }

        if (playerMovement != null)
        {
            playerMovement.ResumeMovement();
        }

        if (playerCatch != null)
        {
            playerCatch.ResetCatch();
        }
    }

    // ==================================================
    // FINAL WIN
    // ==================================================

    private void FinalGameWin()
    {
        state = GameState.Won;

        CancelInvoke(
            nameof(EnableCatchDetection)
        );

        // ------------------------------------------
        // Stop Pig
        // ------------------------------------------

        if (pigAI != null)
        {
            pigAI.StopAfterWin();
        }

        // ------------------------------------------
        // Stop Timer
        // ------------------------------------------

        if (gameTimer != null)
        {
            gameTimer.StopTimer();
        }

        // ------------------------------------------
        // Remove combined catch
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.ResumeAnimation();
            combinedCatch.gameObject.SetActive(false);
        }

        // ------------------------------------------
        // Player visible
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        // Pig captured.
        if (pigVisual != null)
        {
            pigVisual.SetActive(false);
        }

        // ------------------------------------------
        // Win animation
        // ------------------------------------------

        if (playerMovement != null)
        {
            playerMovement.RestoreAnimatorSpeed();
            playerMovement.PlayWinAnimation();
        }

        // ------------------------------------------
        // Win camera
        // ------------------------------------------

        if (cameraController != null)
        {
            cameraController.ShowWinCamera();
        }

        // ------------------------------------------
        // Hide controls
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(false);
        }

        Invoke(
            nameof(ShowWinPanel),
            2.5f
        );
    }

    private void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    // ==================================================
    // TIME UP / LOSE
    // ==================================================

    public void TimeUp()
    {
        if (state == GameState.Won ||
            state == GameState.Lost)
        {
            return;
        }

        state = GameState.Lost;

        CancelInvoke(
            nameof(EnableCatchDetection)
        );

        StopAllCoroutines();

        // ------------------------------------------
        // Stop button mash
        // ------------------------------------------

        if (buttonMashManager != null)
        {
            buttonMashManager.StopMash();
        }

        // ------------------------------------------
        // Remove combined catch
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.ResumeAnimation();
            combinedCatch.gameObject.SetActive(false);
        }

        // ------------------------------------------
        // Show normal models
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (pigVisual != null)
        {
            pigVisual.SetActive(true);
        }

        // ------------------------------------------
        // Disable CatchButton
        // ------------------------------------------

        if (playerCatch != null)
        {
            playerCatch.DisableDetection();
        }

        // ------------------------------------------
        // Player lose
        // ------------------------------------------

        if (playerMovement != null)
        {
            playerMovement.StopMovement();
            playerMovement.RestoreAnimatorSpeed();
            playerMovement.PlayLoseAnimation();
        }

        // ------------------------------------------
        // Pig stop
        // ------------------------------------------

        if (pigAI != null)
        {
            pigAI.RestoreAnimatorSpeed();
            pigAI.StopForGameOver();
        }

        // ------------------------------------------
        // Camera
        // ------------------------------------------

        if (cameraController != null)
        {
            cameraController.ReturnToNormalView();
        }

        // ------------------------------------------
        // Hide joystick
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(false);
        }

        // ------------------------------------------
        // Show lose panel
        // ------------------------------------------

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    // ==================================================
    // REWARDED AD REVIVE
    // ==================================================

    public void ReviveFromRewardedAd()
    {
        if (state != GameState.Lost)
        {
            Debug.LogWarning(
                "Revive ignored because game is not Lost."
            );

            return;
        }

        Debug.Log(
            "REWARDED AD REVIVE STARTING..."
        );

        // Make sure Unity isn't paused.
        Time.timeScale = 1f;

        CancelInvoke(
            nameof(EnableCatchDetection)
        );

        StopAllCoroutines();

        StartCoroutine(
            ReviveCountdownRoutine()
        );
    }

    // ==================================================
    // REVIVE COUNTDOWN
    // ==================================================

    private IEnumerator ReviveCountdownRoutine()
    {
        // Keep game locked while countdown is running.
        state = GameState.Lost;

        // ------------------------------------------
        // Hide result panels
        // ------------------------------------------

        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        // ------------------------------------------
        // Remove combined catch
        // ------------------------------------------

        if (combinedCatch != null)
        {
            combinedCatch.ResumeAnimation();
            combinedCatch.gameObject.SetActive(false);
        }

        // ------------------------------------------
        // Show normal player/pig
        // ------------------------------------------

        if (playerVisual != null)
        {
            playerVisual.SetActive(true);
        }

        if (pigVisual != null)
        {
            pigVisual.SetActive(true);
        }

        // ------------------------------------------
        // Hide joystick during countdown
        // ------------------------------------------

        if (joystick != null)
        {
            joystick.SetActive(false);
        }

        // ------------------------------------------
        // Disable CatchButton during countdown
        // ------------------------------------------

        if (playerCatch != null)
        {
            playerCatch.DisableDetection();
        }

        // ------------------------------------------
        // Camera
        // ------------------------------------------

        if (cameraController != null)
        {
            cameraController.ReturnToNormalView();
        }

        // ------------------------------------------
        // Keep player stopped
        // ------------------------------------------

        if (playerMovement != null)
        {
            playerMovement.RestoreAnimatorSpeed();

            playerMovement.ReturnToMovementAnimation();

            playerMovement.StopMovement();
        }

        // ------------------------------------------
        // Keep pig stopped
        // ------------------------------------------

        if (pigAI != null)
        {
            pigAI.RestoreAnimatorSpeed();

            pigAI.PausePig();
        }

        // ------------------------------------------
        // Prepare 01:00
        // ------------------------------------------

        if (gameTimer != null)
        {
            gameTimer.PrepareReviveTime();
        }

        // ==================================================
        // SHOW COUNTDOWN PANEL
        // ==================================================

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }

        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(true);
        }

        // ==================================================
        // 3
        // ==================================================

        yield return StartCoroutine(
            ShowReviveCountdownImage(
                countdown3Image,
                countdownNumberTime
            )
        );

        // ==================================================
        // 2
        // ==================================================

        yield return StartCoroutine(
            ShowReviveCountdownImage(
                countdown2Image,
                countdownNumberTime
            )
        );

        // ==================================================
        // 1
        // ==================================================

        yield return StartCoroutine(
            ShowReviveCountdownImage(
                countdown1Image,
                countdownNumberTime
            )
        );

        // ==================================================
        // GO
        // ==================================================

        yield return StartCoroutine(
            ShowReviveCountdownImage(
                countdownGoImage,
                countdownGoTime
            )
        );

        // ==================================================
        // HIDE COUNTDOWN
        // ==================================================

        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }

        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }

        // ==================================================
        // IMPORTANT:
        // GAME IS NOW ACTIVE
        // ==================================================

        Time.timeScale = 1f;

        state = GameState.Playing;

        // ==================================================
        // RESTORE PLAYER
        // ==================================================

        if (playerMovement != null)
        {
            playerMovement.RestoreAnimatorSpeed();

            playerMovement.ReturnToMovementAnimation();

            // IMPORTANT:
            // StopMovement() was called when the player
            // lost and during the countdown.
            //
            // This explicitly unlocks movement again.
            playerMovement.ResumeMovement();

            Debug.Log(
                "REVIVE: PLAYER MOVEMENT ENABLED"
            );
        }

        // ==================================================
        // RESTORE PIG
        // ==================================================

        if (pigAI != null)
        {
            pigAI.RestoreAnimatorSpeed();

            /*
             * IMPORTANT:
             *
             * StopForGameOver() placed PigAI into
             * its GameOver state.
             *
             * This changes the pig back into its
             * active next-level/flee state WITHOUT
             * changing BiikLevelManager's current level.
             */
            pigAI.StartNextLevelFromCurrentPosition();

            /*
             * Explicitly resume the NavMeshAgent/AI
             * after resetting the state.
             */
            pigAI.ResumePig();

            Debug.Log(
                "REVIVE: PIG MOVEMENT ENABLED"
            );
        }

        // ==================================================
        // RESET CATCH SYSTEM
        // ==================================================

        if (playerCatch != null)
        {
            playerCatch.ResetForNextLevel();

            Debug.Log(
                "REVIVE: CATCH SYSTEM ENABLED"
            );
        }

        // ==================================================
        // SHOW JOYSTICK
        // ==================================================

        if (joystick != null)
        {
            joystick.SetActive(true);
        }

        // ==================================================
        // START EXTRA 1 MINUTE
        // ==================================================

        if (gameTimer != null)
        {
            gameTimer.StartReviveTimer();
        }

        Debug.Log(
            "REVIVE COMPLETE - PLAYER AND PIG ACTIVE - 01:00"
        );
    }

    // ==================================================
    // REVIVE COUNTDOWN IMAGE
    // ==================================================

    private IEnumerator ShowReviveCountdownImage(
        Sprite sprite,
        float duration)
    {
        if (countdownImage == null)
        {
            Debug.LogError(
                "BiikManager Countdown Image is not assigned!"
            );

            yield return new WaitForSecondsRealtime(
                duration
            );

            yield break;
        }

        if (sprite == null)
        {
            Debug.LogWarning(
                "A BiikManager countdown sprite is missing!"
            );

            yield return new WaitForSecondsRealtime(
                duration
            );

            yield break;
        }

        countdownImage.sprite = sprite;
        countdownImage.preserveAspect = true;

        countdownImage.gameObject.SetActive(true);

        // Start slightly large.
        countdownImage.transform.localScale =
            Vector3.one * countdownStartScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            countdownImage.transform.localScale =
                Vector3.Lerp(
                    countdownImage.transform.localScale,
                    Vector3.one,
                    countdownScaleSpeed *
                    Time.unscaledDeltaTime
                );

            yield return null;
        }

        countdownImage.transform.localScale =
            Vector3.one;
    }

    // ==================================================
    // RESTART
    // ==================================================

    public void RestartGame()
    {
        Debug.Log(
            "Restarting Hulihin Biik..."
        );

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void TryAgain()
    {
        RestartGame();
    }

    // ==================================================
    // NEXT SCENE
    // ==================================================

    public void NextLevel()
    {
        if (string.IsNullOrWhiteSpace(
            nextSceneName))
        {
            Debug.LogWarning(
                "Next Scene Name is empty."
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            nextSceneName
        );
    }

    // ==================================================
    // HOME
    // ==================================================

    public void GoHome()
    {
        if (string.IsNullOrWhiteSpace(
            homeSceneName))
        {
            Debug.LogWarning(
                "Home Scene Name is empty."
            );

            return;
        }

        Time.timeScale = 1f;

        Debug.Log(
            "Going Home: " +
            homeSceneName
        );

        SceneManager.LoadScene(
            homeSceneName
        );
    }
}