using UnityEngine;
using TMPro;

public class LuksongBakaTutorialUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject quickGuidePanel;
    public GameObject instructionPanel;
    public GameObject gameplayHUD;

    [Header("Managers")]
    public GameFlowManager gameFlow;
    public TutorialCameraFocus cameraFocus;

    [Header("Real Characters")]
    public GameObject playerObject;
    public GameObject tayaObject;

    [Header("Instruction Arrows")]
    public GameObject playerArrow;
    public GameObject tayaArrow;
    public GameObject timingBarArrow;
    public GameObject jumpButtonArrow;
    public GameObject scoreArrow;
    public GameObject timerArrow;

    [Header("Tutorial Objects")]
    public GameObject tutorialTimingBar;
    public GameObject tutorialJumpButton;
    public GameObject tutorialScore;
    public GameObject tutorialTimer;

    [Header("Subtitle")]
    public TMP_Text subtitleText;

    [Header("Gameplay HUD")]
    public GameObject scorePanel;

    [Header("Instruction Buttons")]
    public GameObject backButton;
    public GameObject previousButton;
    public GameObject nextButton;
    public GameObject startButton;

    private int currentStep = 0;

    private const int lastStep = 5;

    void Start()
    {
        ShowTitle();
    }

    // =========================================
    // TITLE
    // =========================================

    public void ShowTitle()
    {
        currentStep = 0;

        // STOP GAME ONLY.
        // DO NOT TELEPORT PLAYER.
        if (gameFlow != null)
        {
            gameFlow.LockGameplay();
        }

        if (titlePanel != null)
            titlePanel.SetActive(true);

        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        HideTutorialObjects();

        // Show actual characters
        if (playerObject != null)
            playerObject.SetActive(true);

        if (tayaObject != null)
            tayaObject.SetActive(true);

        if (cameraFocus != null)
        {
            cameraFocus.StartMenuCamera();
        }
    }

    // =========================================
    // QUICK GUIDE
    // =========================================

    public void ShowQuickGuide()
    {
        // DO NOT teleport Player.
        if (gameFlow != null)
        {
            gameFlow.LockGameplay();
        }

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(true);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        HideTutorialObjects();

        if (cameraFocus != null)
        {
            cameraFocus.StartMenuCamera();
        }
    }

    // =========================================
    // OPEN INSTRUCTION
    // =========================================

    public void ShowInstruction()
    {
        // Stop gameplay but keep Player
        // at the tutorial position.
        if (gameFlow != null)
        {
            gameFlow.LockGameplay();
        }

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(true);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        currentStep = 0;

        if (cameraFocus != null)
        {
            cameraFocus.StartInstructionCamera();
        }

        UpdateInstruction();
    }

    // =========================================
    // NEXT
    // =========================================

    public void NextInstruction()
    {
        if (currentStep < lastStep)
        {
            currentStep++;

            UpdateInstruction();
        }
    }

    // =========================================
    // PREVIOUS
    // =========================================

    public void PreviousInstruction()
    {
        if (currentStep > 0)
        {
            currentStep--;

            UpdateInstruction();
        }
    }

    // =========================================
    // UPDATE INSTRUCTION
    // =========================================

    void UpdateInstruction()
    {
        HideTutorialObjects();

        UpdateButtons();

        // =====================================
        // PAGE 1 - PLAYER
        // =====================================

        if (currentStep == 0)
        {
            if (playerObject != null)
            {
                playerObject.SetActive(true);
            }

            if (playerArrow != null)
            {
                playerArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.FocusPlayer();
            }

            SetSubtitle(
                "Ikaw ang manlalaro. Tatakbo ang iyong karakter papunta sa Taya kapag nagsimula ang laro."
            );
        }

        // =====================================
        // PAGE 2 - TAYA
        // =====================================

        else if (currentStep == 1)
        {
            if (tayaObject != null)
            {
                tayaObject.SetActive(true);
            }

            if (tayaArrow != null)
            {
                tayaArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.FocusTaya();
            }

            SetSubtitle(
                "Ito ang Taya. Habang tumataas ang level, magbabago ang posisyon nito at magiging mas mahirap itong lampasan."
            );
        }

        // =====================================
        // PAGE 3 - TIMING BAR
        // =====================================

        else if (currentStep == 2)
        {
            if (tutorialTimingBar != null)
            {
                tutorialTimingBar.SetActive(true);
            }

            if (timingBarArrow != null)
            {
                timingBarArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.HoldCurrentView();
            }

            SetSubtitle(
                "Kapag malapit ka na sa Taya, lalabas ang Timing Bar. Hintayin ang marker na pumasok sa yellow zone."
            );
        }

        // =====================================
        // PAGE 4 - JUMP BUTTON
        // =====================================

        else if (currentStep == 3)
        {
            if (tutorialJumpButton != null)
            {
                tutorialJumpButton.SetActive(true);
            }

            if (jumpButtonArrow != null)
            {
                jumpButtonArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.HoldCurrentView();
            }

            SetSubtitle(
                "Pindutin ang Jump Button habang nasa yellow zone ang marker upang makagawa ng tamang pagtalon."
            );
        }

        // =====================================
        // PAGE 5 - SCORE / LEVEL
        // =====================================

        else if (currentStep == 4)
        {
            if (tutorialScore != null)
            {
                tutorialScore.SetActive(true);
            }

            if (scoreArrow != null)
            {
                scoreArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.HoldCurrentView();
            }

            SetSubtitle(
                "Bawat matagumpay na pagtalon ay nagbibigay ng puntos. Kapag matagumpay, lilipat ka sa susunod na level."
            );
        }

        // =====================================
        // PAGE 6 - TIMER
        // =====================================

        else if (currentStep == 5)
        {
            if (tutorialTimer != null)
            {
                tutorialTimer.SetActive(true);
            }

            if (timerArrow != null)
            {
                timerArrow.SetActive(true);
            }

            if (cameraFocus != null)
            {
                cameraFocus.HoldCurrentView();
            }

            SetSubtitle(
                "Tapusin ang lahat ng 3 level bago maubos ang oras. Isang maling pagtalon lamang ay Game Over."
            );
        }
    }

    // =========================================
    // BUTTON VISIBILITY
    // =========================================

    void UpdateButtons()
    {
        bool firstPage =
            currentStep == 0;

        bool finalPage =
            currentStep == lastStep;

        // PAGE 1:
        // BACK
        if (backButton != null)
        {
            backButton.SetActive(
                firstPage
            );
        }

        // PAGE 2 - 6:
        // PREVIOUS
        if (previousButton != null)
        {
            previousButton.SetActive(
                !firstPage
            );
        }

        // PAGE 1 - 5:
        // NEXT
        if (nextButton != null)
        {
            nextButton.SetActive(
                !finalPage
            );
        }

        // PAGE 6:
        // START GAME
        if (startButton != null)
        {
            startButton.SetActive(
                finalPage
            );
        }
    }

    // =========================================
    // HIDE TUTORIAL OBJECTS
    // =========================================

    void HideTutorialObjects()
    {
        if (playerObject != null)
            playerObject.SetActive(false);

        if (tayaObject != null)
            tayaObject.SetActive(false);

        if (playerArrow != null)
            playerArrow.SetActive(false);

        if (tayaArrow != null)
            tayaArrow.SetActive(false);

        if (timingBarArrow != null)
            timingBarArrow.SetActive(false);

        if (jumpButtonArrow != null)
            jumpButtonArrow.SetActive(false);

        if (scoreArrow != null)
            scoreArrow.SetActive(false);

        if (timerArrow != null)
            timerArrow.SetActive(false);

        if (tutorialTimingBar != null)
            tutorialTimingBar.SetActive(false);

        if (tutorialJumpButton != null)
            tutorialJumpButton.SetActive(false);

        if (tutorialScore != null)
            tutorialScore.SetActive(false);

        if (tutorialTimer != null)
            tutorialTimer.SetActive(false);
    }

    // =========================================
    // SUBTITLE
    // =========================================

    void SetSubtitle(string message)
    {
        if (subtitleText != null)
        {
            subtitleText.text =
                message;
        }
    }

    // =========================================
    // START ACTUAL GAME
    // =========================================

    public void StartGame()
    {
        // =====================================
        // HIDE MENU / INSTRUCTION
        // =====================================

        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (quickGuidePanel != null)
            quickGuidePanel.SetActive(false);

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        HideTutorialObjects();

        // =====================================
        // SHOW REAL CHARACTERS
        // =====================================

        if (playerObject != null)
            playerObject.SetActive(true);

        if (tayaObject != null)
            tayaObject.SetActive(true);

        // =====================================
        // SHOW GAMEPLAY SCOREBOARD
        // =====================================

        if (gameplayHUD != null)
        {
            gameplayHUD.SetActive(true);
        }

        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
        }

        // =====================================
        // CAMERA
        // =====================================

        if (cameraFocus != null)
        {
            cameraFocus.StartGameplayCamera();
        }

        // =====================================
        // START GAME
        // =====================================

        if (gameFlow != null)
        {
            gameFlow.ConfirmTaya();
        }
    }
}