using System.Collections.Generic;
using System.Linq;

using Fusion;

using UnityEngine;
using UnityEngine.SceneManagement;


public enum PatinteroMatchPhase
{
    Waiting,
    Countdown,
    Playing,
    RoundResult,
    MatchOver
}


public class PatinteroMatchManager :
    NetworkBehaviour
{
    public static PatinteroMatchManager Instance;

    [Header("CHARACTERS")]
    [SerializeField]
    private NetworkObject[] characterPrefabs;

    [Header("RUNNER SPAWNS")]
    [SerializeField]
    private Transform[] runnerSpawns;

    [Header("HORIZONTAL GUARDS")]
    [SerializeField]
    private Transform[] horizontalGuardSpawns;

    [Header("CENTER GUARD")]
    [SerializeField]
    private Transform centerGuardSpawn;

    [Header("ZONES")]
    [SerializeField]
    private BoxCollider startZone;

    [SerializeField]
    private BoxCollider endZone;

    [Header("ROUND")]
    [SerializeField]
    private float countdownSeconds = 4f;

    [SerializeField]
    private float roundDuration = 120f;

    [SerializeField]
    private float resultDuration = 3f;

    [Header("TAGGING")]
    [SerializeField]
    private float tagDistance = 1.25f;

    [Header("RUNNER WIN")]
    [Tooltip(
        "How many runners must complete Start-End-Start to win the round.")]
    [SerializeField]
    private int requiredCompletionsToWinRound = 1;


    [Networked]
    public PatinteroMatchPhase Phase
    {
        get;
        set;
    }

    [Networked]
    public int RoundNumber
    {
        get;
        set;
    }

    [Networked]
    public int TeamAWins
    {
        get;
        set;
    }

    [Networked]
    public int TeamBWins
    {
        get;
        set;
    }

    [Networked]
    public int RunnerTeam
    {
        get;
        set;
    }

    [Networked]
    public int TaggedRunners
    {
        get;
        set;
    }

    [Networked]
    public int CompletedRunners
    {
        get;
        set;
    }

    [Networked]
    public int LastRoundWinner
    {
        get;
        set;
    }

    [Networked]
    private TickTimer CountdownTimer
    {
        get;
        set;
    }

    [Networked]
    private TickTimer RoundTimer
    {
        get;
        set;
    }

    [Networked]
    private TickTimer ResultTimer
    {
        get;
        set;
    }

    private readonly
        Dictionary<
            PlayerRef,
            PatinteroNetworkPlayer>
        avatars = new();


    public bool CanPlayersMove =>
        Phase ==
        PatinteroMatchPhase.Playing;


    private void Awake()
    {
        Instance = this;
    }


    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        RoundNumber = 1;

        TeamAWins = 0;
        TeamBWins = 0;

        RunnerTeam = 0;

        SpawnPlayers();

        SetupRound();
    }


    private void SpawnPlayers()
    {
        List<PlayerRef> players =
            Runner.ActivePlayers
                .ToList();

        int teamSize =
            players.Count / 2;

        foreach (
            PlayerRef player
            in players)
        {
            NetworkObject lobbyObject =
                Runner.GetPlayerObject(
                    player
                );

            if (lobbyObject == null)
                continue;

            PatinteroLobbyPlayer data =
                lobbyObject.GetComponent<
                    PatinteroLobbyPlayer>();

            if (data == null)
                continue;

            int character =
                Mathf.Clamp(
                    data.CharacterIndex,
                    0,
                    characterPrefabs.Length - 1
                );

            Transform spawn =
                GetSpawnFor(
                    data.TeamId,
                    data.TeamIndex,
                    teamSize,
                    out PatinteroRole role,
                    out float lineValue
                );

            NetworkObject avatarObject =
                Runner.Spawn(
                    characterPrefabs[
                        character
                    ],
                    spawn.position,
                    spawn.rotation,
                    player,
                    (r, obj) =>
                    {
                        PatinteroNetworkPlayer p =
                            obj.GetComponent<
                                PatinteroNetworkPlayer>();

                        p.InitializeBeforeSpawn(
                            data.TeamId,
                            data.TeamIndex,
                            role,
                            lineValue
                        );
                    }
                );

            PatinteroNetworkPlayer avatar =
                avatarObject.GetComponent<
                    PatinteroNetworkPlayer>();

            avatars[player] =
                avatar;
        }
    }


    private Transform GetSpawnFor(
        int team,
        int teamIndex,
        int teamSize,
        out PatinteroRole role,
        out float lineValue)
    {
        if (team == RunnerTeam)
        {
            role =
                PatinteroRole.Runner;

            lineValue = 0f;

            return runnerSpawns[
                Mathf.Clamp(
                    teamIndex,
                    0,
                    runnerSpawns.Length - 1
                )
            ];
        }

        bool centerGuard =
            teamIndex ==
            teamSize - 1;

        if (centerGuard)
        {
            role =
                PatinteroRole.CenterGuard;

            lineValue =
                centerGuardSpawn
                    .position.x;

            return centerGuardSpawn;
        }

        role =
            PatinteroRole
                .HorizontalGuard;

        Transform guardSpawn =
            horizontalGuardSpawns[
                Mathf.Clamp(
                    teamIndex,
                    0,
                    horizontalGuardSpawns.Length - 1
                )
            ];

        lineValue =
            guardSpawn.position.z;

        return guardSpawn;
    }


    private void SetupRound()
    {
        TaggedRunners = 0;
        CompletedRunners = 0;

        int teamSize =
            avatars.Count / 2;

        foreach (
            PatinteroNetworkPlayer player
            in avatars.Values)
        {
            Transform spawn =
                GetSpawnFor(
                    player.TeamId,
                    player.TeamIndex,
                    teamSize,
                    out PatinteroRole role,
                    out float lineValue
                );

            player.ServerPrepareRound(
                role,
                lineValue,
                spawn
            );
        }

        Phase =
            PatinteroMatchPhase
                .Countdown;

        CountdownTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                countdownSeconds
            );
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (Phase ==
            PatinteroMatchPhase.Countdown)
        {
            if (CountdownTimer
                .Expired(Runner))
            {
                Phase =
                    PatinteroMatchPhase
                        .Playing;

                RoundTimer =
                    TickTimer
                        .CreateFromSeconds(
                            Runner,
                            roundDuration
                        );
            }

            return;
        }


        if (Phase ==
            PatinteroMatchPhase.Playing)
        {
            CheckRunnerProgress();

            if (Phase !=
                PatinteroMatchPhase.Playing)
                return;

            CheckTags();

            if (Phase !=
                PatinteroMatchPhase.Playing)
                return;

            if (RoundTimer
                .Expired(Runner))
            {
                int guardTeam =
                    RunnerTeam == 0
                    ? 1
                    : 0;

                EndRound(
                    guardTeam
                );
            }

            return;
        }


        if (Phase ==
            PatinteroMatchPhase.RoundResult)
        {
            if (ResultTimer
                .Expired(Runner))
            {
                if (TeamAWins >= 2 ||
                    TeamBWins >= 2)
                {
                    Phase =
                        PatinteroMatchPhase
                            .MatchOver;

                    return;
                }

                RoundNumber++;

                RunnerTeam =
                    RunnerTeam == 0
                    ? 1
                    : 0;

                SetupRound();
            }
        }
    }


    private void CheckRunnerProgress()
    {
        int runners =
            0;

        foreach (
            PatinteroNetworkPlayer player
            in avatars.Values)
        {
            if (player.Role !=
                PatinteroRole.Runner)
                continue;

            runners++;

            if (player.RunnerState ==
                    PatinteroRunnerState
                        .Tagged ||
                player.RunnerState ==
                    PatinteroRunnerState
                        .Completed)
            {
                continue;
            }

            Vector3 position =
                player.transform.position;

            if (player.RunnerState ==
                    PatinteroRunnerState
                        .GoingForward &&
                endZone.bounds.Contains(
                    position))
            {
                player
                    .ServerSetRunnerState(
                        PatinteroRunnerState
                            .Returning
                    );

                continue;
            }

            if (player.RunnerState ==
                    PatinteroRunnerState
                        .Returning &&
                startZone.bounds.Contains(
                    position))
            {
                player
                    .ServerSetRunnerState(
                        PatinteroRunnerState
                            .Completed
                    );

                CompletedRunners++;

                int requirement =
                    Mathf.Clamp(
                        requiredCompletionsToWinRound,
                        1,
                        runners
                    );

                if (CompletedRunners >=
                    requirement)
                {
                    EndRound(
                        RunnerTeam
                    );

                    return;
                }
            }
        }
    }


    private void CheckTags()
    {
        List<PatinteroNetworkPlayer>
            guards =
                avatars.Values
                    .Where(
                        p =>
                            p.Role !=
                            PatinteroRole.Runner
                    )
                    .ToList();

        List<PatinteroNetworkPlayer>
            runners =
                avatars.Values
                    .Where(
                        p =>
                            p.Role ==
                            PatinteroRole.Runner
                    )
                    .ToList();

        foreach (
            PatinteroNetworkPlayer guard
            in guards)
        {
            foreach (
                PatinteroNetworkPlayer runner
                in runners)
            {
                if (runner.RunnerState ==
                        PatinteroRunnerState
                            .Tagged ||
                    runner.RunnerState ==
                        PatinteroRunnerState
                            .Completed)
                {
                    continue;
                }

                Vector3 a =
                    guard.transform.position;

                Vector3 b =
                    runner.transform.position;

                a.y = 0f;
                b.y = 0f;

                float distance =
                    Vector3.Distance(
                        a,
                        b
                    );

                if (distance <=
                    tagDistance)
                {
                    runner
                        .ServerSetRunnerState(
                            PatinteroRunnerState
                                .Tagged
                        );

                    TaggedRunners++;

                    if (
                        TaggedRunners +
                        CompletedRunners
                        >= runners.Count)
                    {
                        int requirement =
                            Mathf.Clamp(
                                requiredCompletionsToWinRound,
                                1,
                                runners.Count
                            );

                        if (
                            CompletedRunners <
                            requirement)
                        {
                            int guardTeam =
                                RunnerTeam == 0
                                ? 1
                                : 0;

                            EndRound(
                                guardTeam
                            );

                            return;
                        }
                    }
                }
            }
        }
    }


    private void EndRound(
        int winnerTeam)
    {
        if (Phase !=
            PatinteroMatchPhase.Playing)
            return;

        LastRoundWinner =
            winnerTeam;

        if (winnerTeam == 0)
        {
            TeamAWins++;
        }
        else
        {
            TeamBWins++;
        }

        Phase =
            PatinteroMatchPhase
                .RoundResult;

        ResultTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                resultDuration
            );
    }


    public float GetCountdownRemaining()
    {
        return
            CountdownTimer
                .RemainingTime(Runner)
                ?? 0f;
    }


    public float GetRoundTimeRemaining()
    {
        return
            RoundTimer
                .RemainingTime(Runner)
                ?? 0f;
    }


    // ======================================
    // MATCH COMPLETE BUTTONS
    // ======================================

    public void ServerPlayAgain()
    {
        if (!HasStateAuthority)
            return;

        TeamAWins = 0;
        TeamBWins = 0;

        RoundNumber = 1;
        RunnerTeam = 0;

        SetupRound();
    }


    public void ServerReturnToLobby()
    {
        if (!HasStateAuthority)
            return;

        foreach (
            PatinteroNetworkPlayer player
            in avatars.Values.ToList())
        {
            if (player != null &&
                player.Object != null &&
                player.Object.IsValid)
            {
                Runner.Despawn(
                    player.Object
                );
            }
        }

        avatars.Clear();

        foreach (
            PlayerRef player
            in Runner.ActivePlayers)
        {
            NetworkObject lobbyObject =
                Runner.GetPlayerObject(
                    player
                );

            if (lobbyObject == null)
                continue;

            PatinteroLobbyPlayer data =
                lobbyObject.GetComponent<
                    PatinteroLobbyPlayer>();

            data?.ServerResetReady();
        }

        Runner.SessionInfo.IsOpen =
            true;

        Runner.SessionInfo.IsVisible =
            true;

        int mainIndex =
            PatinteroFusionManager
                .Instance
                .MainSceneBuildIndex;

        Runner.LoadScene(
            SceneRef.FromIndex(
                mainIndex
            ),
            LoadSceneMode.Single
        );
    }
}