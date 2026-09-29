using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PatinteroGameplayHUD : MonoBehaviour
{
    // =========================================================
    // HUD
    // =========================================================

    [Header("HUD")]

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private TMP_Text roundText;

    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private TMP_Text roleText;


    // =========================================================
    // INSTRUCTIONS
    // =========================================================

    [Header("INSTRUCTION PANEL")]

    [SerializeField]
    private GameObject instructionPanel;

    [SerializeField]
    private TMP_Text instructionTimerText;


    // =========================================================
    // COUNTDOWN
    // =========================================================

    [Header("COUNTDOWN IMAGE")]

    [SerializeField]
    private Image countdownImage;

    [SerializeField]
    private Sprite countdown3Sprite;

    [SerializeField]
    private Sprite countdown2Sprite;

    [SerializeField]
    private Sprite countdown1Sprite;

    [SerializeField]
    private Sprite countdownGoSprite;


    // =========================================================
    // CONTROLS
    // =========================================================

    [Header("CONTROL GROUPS")]

    [SerializeField]
    private GameObject attackerControls;

    [SerializeField]
    private GameObject sideGuardControls;

    [SerializeField]
    private GameObject middleGuardControls;


    // =========================================================
    // ROUND RESULT
    // =========================================================

    [Header("ROUND RESULT")]

    [SerializeField]
    private GameObject roundResultPanel;

    [SerializeField]
    private TMP_Text roundResultText;


    // =========================================================
    // MATCH RESULT
    // =========================================================

    [Header("MATCH RESULT")]

    [SerializeField]
    private GameObject matchResultPanel;

    [SerializeField]
    private TMP_Text matchResultText;


    // =========================================================
    // EXIT
    // =========================================================

    [Header("EXIT")]

    [SerializeField]
    private string mainSceneName =
        "Main";


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        HideAllControls();


        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }


        if (countdownImage != null)
        {
            countdownImage.gameObject
                .SetActive(false);
        }


        if (roundResultPanel != null)
        {
            roundResultPanel.SetActive(false);
        }


        if (matchResultPanel != null)
        {
            matchResultPanel.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        // =====================================================
        // IMPORTANT FIX
        //
        // Do not read Networked properties before Fusion
        // calls Spawned() on the Round Manager.
        // =====================================================

        if (
            match == null ||
            !match.NetworkReady)
        {
            HideAllControls();

            return;
        }


        UpdateBasicHUD(match);

        UpdateInstructionPanel(match);

        UpdateCountdown(match);

        UpdateControls(match);

        UpdateRoundResult(match);

        UpdateMatchResult(match);
    }


    // =========================================================
    // BASIC HUD
    // =========================================================

    private void UpdateBasicHUD(
        PatinteroGameplayRoundManager match)
    {
        // ROUND
        if (roundText != null)
        {
            roundText.text =
                "ROUND " +
                Mathf.Max(
                    1,
                    match.RoundNumber
                ) +
                " / 3";
        }


        // SCORE
        if (scoreText != null)
        {
            scoreText.text =
                "TEAM A " +
                match.TeamAScore +
                " - " +
                match.TeamBScore +
                " TEAM B";
        }


        // TIMER
        if (timerText != null)
        {
            float time =
                match.GetRoundTimeRemaining();


            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(time)
                );


            int minutes =
                totalSeconds / 60;

            int seconds =
                totalSeconds % 60;


            timerText.text =
                minutes.ToString("00") +
                ":" +
                seconds.ToString("00");
        }


        // ROLE
        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (
            player == null ||
            roleText == null)
        {
            return;
        }


        if (!player.IsDefender)
        {
            roleText.text =
                player.ReachedEnd
                    ? "ATTACKER - RETURN TO START!"
                    : "ATTACKER - REACH THE END!";

            return;
        }


        if (player.GuardLineIndex == 4)
        {
            roleText.text =
                "MIDDLE DEFENDER";
        }
        else
        {
            roleText.text =
                "DEFENDER";
        }
    }


    // =========================================================
    // INSTRUCTION PANEL
    // =========================================================

    private void UpdateInstructionPanel(
        PatinteroGameplayRoundManager match)
    {
        bool showing =
            match.CurrentPhase ==
            PatinteroGameplayPhase.Instructions;


        if (instructionPanel != null)
        {
            instructionPanel.SetActive(
                showing
            );
        }


        if (!showing)
        {
            return;
        }


        HideAllControls();


        if (instructionTimerText != null)
        {
            float remaining =
                match.GetPhaseTimeRemaining();


            int seconds =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        remaining
                    )
                );


            instructionTimerText.text =
                seconds.ToString();
        }
    }


    // =========================================================
    // COUNTDOWN IMAGE
    // =========================================================

    private void UpdateCountdown(
        PatinteroGameplayRoundManager match)
    {
        if (countdownImage == null)
        {
            return;
        }


        if (
            match.CurrentPhase !=
            PatinteroGameplayPhase.Countdown)
        {
            countdownImage.gameObject
                .SetActive(false);

            return;
        }


        HideAllControls();


        float remaining =
            match.GetPhaseTimeRemaining();


        Sprite spriteToShow;


        if (remaining > 3f)
        {
            spriteToShow =
                countdown3Sprite;
        }
        else if (remaining > 2f)
        {
            spriteToShow =
                countdown2Sprite;
        }
        else if (remaining > 1f)
        {
            spriteToShow =
                countdown1Sprite;
        }
        else
        {
            spriteToShow =
                countdownGoSprite;
        }


        if (spriteToShow == null)
        {
            countdownImage.gameObject
                .SetActive(false);

            return;
        }


        countdownImage.sprite =
            spriteToShow;

        countdownImage.gameObject
            .SetActive(true);
    }


    // =========================================================
    // CONTROLS
    // =========================================================

    private void UpdateControls(
        PatinteroGameplayRoundManager match)
    {
        if (
            match.CurrentPhase !=
            PatinteroGameplayPhase.Playing)
        {
            HideAllControls();

            return;
        }


        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (player == null)
        {
            HideAllControls();

            return;
        }


        // ATTACKER
        if (!player.IsDefender)
        {
            ShowControls(
                true,
                false,
                false
            );

            return;
        }


        // MIDDLE GUARD
        if (player.GuardLineIndex == 4)
        {
            ShowControls(
                false,
                false,
                true
            );

            return;
        }


        // SIDE GUARD
        ShowControls(
            false,
            true,
            false
        );
    }


    private void ShowControls(
        bool attacker,
        bool sideGuard,
        bool middleGuard)
    {
        if (attackerControls != null)
        {
            attackerControls.SetActive(
                attacker
            );
        }


        if (sideGuardControls != null)
        {
            sideGuardControls.SetActive(
                sideGuard
            );
        }


        if (middleGuardControls != null)
        {
            middleGuardControls.SetActive(
                middleGuard
            );
        }


        if (!sideGuard)
        {
            PatinteroMobileInput.SetGuardButton(
                PatinteroGuardMoveDirection.Left,
                false
            );


            PatinteroMobileInput.SetGuardButton(
                PatinteroGuardMoveDirection.Right,
                false
            );
        }


        if (!middleGuard)
        {
            PatinteroMobileInput.SetGuardButton(
                PatinteroGuardMoveDirection.Up,
                false
            );


            PatinteroMobileInput.SetGuardButton(
                PatinteroGuardMoveDirection.Down,
                false
            );
        }
    }


    private void HideAllControls()
    {
        ShowControls(
            false,
            false,
            false
        );
    }


    // =========================================================
    // ROUND RESULT
    // =========================================================

    private void UpdateRoundResult(
        PatinteroGameplayRoundManager match)
    {
        bool showing =
            match.CurrentPhase ==
            PatinteroGameplayPhase.RoundResult;


        if (roundResultPanel != null)
        {
            roundResultPanel.SetActive(
                showing
            );
        }


        if (
            !showing ||
            roundResultText == null)
        {
            return;
        }


        HideAllControls();


        string winner =
            match.LastRoundWinnerTeam == 0
                ? "TEAM A"
                : "TEAM B";


        string reason =
            "ROUND COMPLETE";


        switch (match.LastRoundReason)
        {
            case 1:

                reason =
                    "ATTACKERS RETURNED SAFELY!";

                break;


            case 2:

                reason =
                    "AN ATTACKER WAS TAGGED!";

                break;


            case 3:

                reason =
                    "TIME'S UP!";

                break;
        }


        roundResultText.text =
            winner +
            " WINS THE ROUND!\n" +
            reason;
    }


    // =========================================================
    // MATCH RESULT
    // =========================================================

    private void UpdateMatchResult(
        PatinteroGameplayRoundManager match)
    {
        bool showing =
            match.CurrentPhase ==
            PatinteroGameplayPhase.MatchOver;


        if (matchResultPanel != null)
        {
            matchResultPanel.SetActive(
                showing
            );
        }


        if (!showing)
        {
            return;
        }


        HideAllControls();


        if (matchResultText == null)
        {
            return;
        }


        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (player == null)
        {
            return;
        }


        bool playerWon =
            player.TeamId ==
            match.MatchWinnerTeam;


        matchResultText.text =
            playerWon
                ? "YOUR TEAM WINS!"
                : "YOUR TEAM LOSES!";
    }


    // =========================================================
    // QUIT
    // =========================================================

    public void QuitMatch()
    {
        StartCoroutine(
            QuitMatchRoutine()
        );
    }


    private IEnumerator QuitMatchRoutine()
    {
        NetworkRunner runner =
            null;


        if (
            PatinteroFusionManager.Instance !=
            null)
        {
            runner =
                PatinteroFusionManager
                    .Instance
                    .Runner;
        }


        if (
            runner != null &&
            runner.IsRunning)
        {
            var shutdown =
                runner.Shutdown();


            while (!shutdown.IsCompleted)
            {
                yield return null;
            }
        }


        PatinteroMobileInput.Reset();


        SceneManager.LoadScene(
            mainSceneName
        );
    }
}