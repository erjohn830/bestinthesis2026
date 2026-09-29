using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PatinteroMatchUI :
    MonoBehaviour
{
    // =========================================================
    // GAME UI
    // =========================================================

    [Header("GAME UI")]
    [SerializeField]
    private TMP_Text timerText;


    [SerializeField]
    private TMP_Text roundText;


    [SerializeField]
    private TMP_Text scoreText;


    [SerializeField]
    private TMP_Text roleText;


    [SerializeField]
    private TMP_Text countdownText;


    // =========================================================
    // CONTROLS
    // =========================================================

    [Header("CONTROLS")]
    [SerializeField]
    private GameObject controlsRoot;


    [SerializeField]
    private GameObject turnButton;


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
        if (roundResultPanel != null)
        {
            roundResultPanel.SetActive(
                false
            );
        }


        if (matchResultPanel != null)
        {
            matchResultPanel.SetActive(
                false
            );
        }


        if (countdownText != null)
        {
            countdownText.gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        if (match == null)
        {
            return;
        }


        UpdateBasicUI(
            match
        );


        UpdateControls(
            match
        );


        UpdateCountdown(
            match
        );


        UpdateRoundResult(
            match
        );


        UpdateMatchResult(
            match
        );
    }


    // =========================================================
    // BASIC UI
    // =========================================================

    private void UpdateBasicUI(
        PatinteroGameplayRoundManager match)
    {
        // -----------------------------------------------------
        // ROUND
        // -----------------------------------------------------

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


        // -----------------------------------------------------
        // SCORE
        // -----------------------------------------------------

        if (scoreText != null)
        {
            scoreText.text =
                "TEAM A " +
                match.TeamAScore +
                " - " +
                match.TeamBScore +
                " TEAM B";
        }


        // -----------------------------------------------------
        // TIMER
        // -----------------------------------------------------

        if (timerText != null)
        {
            float time =
                match.GetRoundTimeRemaining();


            int minutes =
                Mathf.FloorToInt(
                    time / 60f
                );


            int seconds =
                Mathf.FloorToInt(
                    time % 60f
                );


            timerText.text =
                minutes.ToString("00") +
                ":" +
                seconds.ToString("00");
        }


        // -----------------------------------------------------
        // ROLE
        // -----------------------------------------------------

        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (
            roleText != null &&
            player != null)
        {
            if (player.IsDefender)
            {
                roleText.text =
                    "DEFENDER - GUARD YOUR LINE";
            }
            else
            {
                if (player.ReachedEnd)
                {
                    roleText.text =
                        "ATTACKER - RETURN TO START!";
                }
                else
                {
                    roleText.text =
                        "ATTACKER - REACH THE END!";
                }
            }
        }
    }


    // =========================================================
    // CONTROLS
    // =========================================================

    private void UpdateControls(
        PatinteroGameplayRoundManager match)
    {
        bool playing =
            match.CurrentPhase ==
            PatinteroGameplayPhase.Playing;


        if (controlsRoot != null)
        {
            controlsRoot.SetActive(
                playing
            );
        }


        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (turnButton != null)
        {
            bool showTurnButton =
                playing &&
                player != null &&
                player.IsDefender;


            turnButton.SetActive(
                showTurnButton
            );
        }
    }


    // =========================================================
    // COUNTDOWN
    // =========================================================

    private void UpdateCountdown(
        PatinteroGameplayRoundManager match)
    {
        if (countdownText == null)
        {
            return;
        }


        // -----------------------------------------------------
        // 3 2 1
        // -----------------------------------------------------

        if (
            match.CurrentPhase ==
            PatinteroGameplayPhase.Countdown)
        {
            int number =
                Mathf.CeilToInt(
                    match.GetPhaseTimeRemaining()
                );


            countdownText.gameObject
                .SetActive(true);


            countdownText.text =
                Mathf.Max(
                    1,
                    number
                )
                .ToString();


            return;
        }


        // -----------------------------------------------------
        // SWITCH SIDE
        // -----------------------------------------------------

        if (
            match.CurrentPhase ==
            PatinteroGameplayPhase.SwitchingSides)
        {
            countdownText.gameObject
                .SetActive(true);


            countdownText.text =
                "SWITCH SIDES";


            return;
        }


        countdownText.gameObject
            .SetActive(false);
    }


    // =========================================================
    // ROUND RESULT
    // =========================================================

    private void UpdateRoundResult(
        PatinteroGameplayRoundManager match)
    {
        bool show =
            match.CurrentPhase ==
            PatinteroGameplayPhase.RoundResult;


        if (roundResultPanel != null)
        {
            roundResultPanel.SetActive(
                show
            );
        }


        if (!show ||
            roundResultText == null)
        {
            return;
        }


        string winner =
            match.LastRoundWinnerTeam == 0
                ? "TEAM A"
                : "TEAM B";


        string reason =
            "ROUND COMPLETE";


        if (match.LastRoundReason == 1)
        {
            reason =
                "ATTACKERS RETURNED SAFELY!";
        }


        if (match.LastRoundReason == 2)
        {
            reason =
                "AN ATTACKER WAS TAGGED!";
        }


        if (match.LastRoundReason == 3)
        {
            reason =
                "TIME'S UP!";
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
        bool show =
            match.CurrentPhase ==
            PatinteroGameplayPhase.MatchOver;


        if (matchResultPanel != null)
        {
            matchResultPanel.SetActive(
                show
            );
        }


        if (!show ||
            matchResultText == null)
        {
            return;
        }


        PatinteroGameplayPlayer player =
            PatinteroGameplayPlayer.Local;


        if (player == null)
        {
            return;
        }


        bool winner =
            player.TeamId ==
            match.MatchWinnerTeam;


        matchResultText.text =
            winner
                ? "YOUR TEAM WINS!"
                : "YOUR TEAM LOSES!";
    }


    // =========================================================
    // QUIT MATCH
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
            PatinteroFusionManager.Instance != null)
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
            var shutdownTask =
                runner.Shutdown();


            while (!shutdownTask.IsCompleted)
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