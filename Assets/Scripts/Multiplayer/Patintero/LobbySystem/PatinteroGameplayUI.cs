using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatinteroGameplayUI :
    MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text roundText;
    public TMP_Text timerText;
    public TMP_Text roleText;
    public TMP_Text objectiveText;

    [Header("COUNTDOWN")]
    public GameObject countdownPanel;
    public TMP_Text countdownText;

    [Header("ROUND RESULT")]
    public GameObject roundResultPanel;
    public TMP_Text roundResultText;

    [Header("MATCH RESULT")]
    public GameObject matchResultPanel;
    public TMP_Text matchResultText;

    public Button playAgainButton;
    public Button returnToLobbyButton;


    private void Update()
    {
        PatinteroMatchManager manager =
            PatinteroMatchManager.Instance;

        if (manager == null)
            return;

        scoreText.text =
            "TEAM A " +
            manager.TeamAWins +
            " - " +
            manager.TeamBWins +
            " TEAM B";

        roundText.text =
            "ROUND " +
            manager.RoundNumber;


        float time =
            manager
                .GetRoundTimeRemaining();

        int seconds =
            Mathf.CeilToInt(time);

        int minutes =
            seconds / 60;

        seconds %= 60;

        timerText.text =
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");


        UpdateRole(manager);

        UpdateCountdown(manager);

        UpdateRoundResult(manager);

        UpdateMatchResult(manager);
    }


    private void UpdateRole(
        PatinteroMatchManager manager)
    {
        PatinteroNetworkPlayer local =
            PatinteroNetworkPlayer.Local;

        if (local == null)
            return;

        if (local.Role ==
            PatinteroRole.Runner)
        {
            roleText.text =
                "ROLE: RUNNER";

            switch (
                local.RunnerState)
            {
                case
                PatinteroRunnerState
                    .GoingForward:

                    objectiveText.text =
                        "Reach the End Zone!";
                    break;

                case
                PatinteroRunnerState
                    .Returning:

                    objectiveText.text =
                        "Return to Start!";
                    break;

                case
                PatinteroRunnerState
                    .Completed:

                    objectiveText.text =
                        "Completed!";
                    break;

                case
                PatinteroRunnerState
                    .Tagged:

                    objectiveText.text =
                        "TAGGED!";
                    break;
            }
        }
        else
        {
            roleText.text =
                local.Role ==
                PatinteroRole.CenterGuard
                ? "ROLE: CENTER GUARD"
                : "ROLE: GUARD";

            objectiveText.text =
                "Stop the Runners!";
        }
    }


    private void UpdateCountdown(
        PatinteroMatchManager manager)
    {
        bool show =
            manager.Phase ==
            PatinteroMatchPhase.Countdown;

        countdownPanel
            .SetActive(show);

        if (!show)
            return;

        float remaining =
            manager
                .GetCountdownRemaining();

        if (remaining > 3f)
        {
            countdownText.text =
                "3";
        }
        else if (remaining > 2f)
        {
            countdownText.text =
                "2";
        }
        else if (remaining > 1f)
        {
            countdownText.text =
                "1";
        }
        else
        {
            countdownText.text =
                "GO!";
        }
    }


    private void UpdateRoundResult(
        PatinteroMatchManager manager)
    {
        bool show =
            manager.Phase ==
            PatinteroMatchPhase.RoundResult;

        roundResultPanel
            .SetActive(show);

        if (!show)
            return;

        roundResultText.text =
            manager.LastRoundWinner == 0
            ? "TEAM A WINS THE ROUND!"
            : "TEAM B WINS THE ROUND!";
    }


    private void UpdateMatchResult(
        PatinteroMatchManager manager)
    {
        bool show =
            manager.Phase ==
            PatinteroMatchPhase.MatchOver;

        matchResultPanel
            .SetActive(show);

        if (!show)
            return;

        bool teamAWon =
            manager.TeamAWins >= 2;

        matchResultText.text =
            teamAWon
            ? "TEAM A WINS!"
            : "TEAM B WINS!";

        bool host =
            PatinteroFusionManager
                .Instance
                .IsHost;

        playAgainButton.gameObject
            .SetActive(host);

        returnToLobbyButton.gameObject
            .SetActive(host);
    }


    public void PlayAgain()
    {
        if (!PatinteroFusionManager
            .Instance
            .IsHost)
            return;

        PatinteroMatchManager
            .Instance
            .ServerPlayAgain();
    }


    public void ReturnToLobby()
    {
        if (!PatinteroFusionManager
            .Instance
            .IsHost)
            return;

        PatinteroMatchManager
            .Instance
            .ServerReturnToLobby();
    }


    public void LeaveMatch()
    {
        PatinteroFusionManager
            .Instance
            .LeaveSessionToMain();
    }
}