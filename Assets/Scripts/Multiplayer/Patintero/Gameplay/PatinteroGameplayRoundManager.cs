using System.Collections.Generic;
using Fusion;
using UnityEngine;


// =============================================================
// GAMEPLAY PHASE
// =============================================================

public enum PatinteroGameplayPhase
{
    Waiting = 0,
    Intro = 1,
    Instructions = 2,
    Countdown = 3,
    Playing = 4,
    RoundResult = 5,
    SwitchingSides = 6,
    MatchOver = 7
}


// =============================================================
// PATINTERO GAMEPLAY ROUND MANAGER
// =============================================================

public class PatinteroGameplayRoundManager : NetworkBehaviour
{
    public static PatinteroGameplayRoundManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // NETWORK READY
    // =========================================================

    private bool networkReady = false;

    public bool NetworkReady
    {
        get
        {
            return networkReady;
        }
    }


    // =========================================================
    // MATCH SETTINGS
    // =========================================================

    [Header("MATCH SETTINGS")]

    [SerializeField]
    private float roundTime = 180f;

    [SerializeField]
    private float introDuration = 5f;

    [SerializeField]
    private float instructionDuration = 10f;

    [SerializeField]
    private float countdownDuration = 4f;

    [SerializeField]
    private float roundResultDuration = 4f;

    [SerializeField]
    private float sideSwitchDuration = 3f;


    // =========================================================
    // TOUCH / TAG
    // =========================================================

    [Header("TOUCH / TAG SETTINGS")]

    [SerializeField]
    private float tagDistance = 1.5f;

    [SerializeField]
    [Range(10f, 180f)]
    private float tagForwardAngle = 100f;


    // =========================================================
    // COURT
    // =========================================================

    [Header("COURT ZONES")]

    [SerializeField]
    private BoxCollider startZone;

    [SerializeField]
    private BoxCollider endZone;


    // =========================================================
    // GUARD LINES
    // =========================================================

    [Header("GUARD LINES")]

    [SerializeField]
    private PatinteroGuardLine[] guardLines =
        new PatinteroGuardLine[5];


    // =========================================================
    // ATTACKER SPAWNS
    // =========================================================

    [Header("ATTACKER SPAWNS")]

    [SerializeField]
    private Transform[] attackerSpawns =
        new Transform[5];


    // =========================================================
    // TEAM INTRO SPAWNS
    // =========================================================

    [Header("TEAM A INTRO SPAWNS")]

    [SerializeField]
    private Transform[] teamAIntroSpawns =
        new Transform[5];


    [Header("TEAM B INTRO SPAWNS")]

    [SerializeField]
    private Transform[] teamBIntroSpawns =
        new Transform[5];


    // =========================================================
    // NETWORK MATCH DATA
    // =========================================================

    [Networked]
    public int PhaseValue { get; set; }

    [Networked]
    public int RoundNumber { get; set; }

    [Networked]
    public int AttackingTeam { get; set; }

    [Networked]
    public int TeamAScore { get; set; }

    [Networked]
    public int TeamBScore { get; set; }

    [Networked]
    public int LastRoundWinnerTeam { get; set; }

    // 1 = attackers returned safely
    // 2 = attacker tagged
    // 3 = time expired
    [Networked]
    public int LastRoundReason { get; set; }

    [Networked]
    public int MatchWinnerTeam { get; set; }

    [Networked]
    public TickTimer PhaseTimer { get; set; }

    [Networked]
    public TickTimer RoundTimer { get; set; }


    // =========================================================
    // PLAYER LIST
    // =========================================================

    private readonly List<PatinteroGameplayPlayer>
        players =
            new List<PatinteroGameplayPlayer>();


    // =========================================================
    // CURRENT PHASE
    // =========================================================

    public PatinteroGameplayPhase CurrentPhase
    {
        get
        {
            // IMPORTANT:
            // Do not access PhaseValue until Fusion Spawned().
            if (!networkReady)
            {
                return PatinteroGameplayPhase.Waiting;
            }

            return
                (PatinteroGameplayPhase)
                PhaseValue;
        }
    }


    public bool IsRoundPlaying
    {
        get
        {
            if (!networkReady)
            {
                return false;
            }

            return
                CurrentPhase ==
                PatinteroGameplayPhase.Playing;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // IMPORTANT:
        //
        // Do NOT set Instance here.
        //
        // Networked properties cannot be safely accessed
        // until Fusion calls Spawned().
    }


    // =========================================================
    // FUSION SPAWNED
    // =========================================================

    public override void Spawned()
    {
        // The manager only becomes globally available
        // AFTER Fusion has spawned this NetworkBehaviour.
        Instance = this;

        networkReady = true;


        Debug.Log(
            "========================================"
        );

        Debug.Log(
            "PATINTERO ROUND MANAGER SPAWNED BY FUSION"
        );

        Debug.Log(
            "STATE AUTHORITY = " +
            Object.HasStateAuthority
        );

        Debug.Log(
            "========================================"
        );


        // Clients receive the networked values from host.
        // Only State Authority initializes them.
        if (!Object.HasStateAuthority)
        {
            return;
        }


        PhaseValue =
            (int)
            PatinteroGameplayPhase.Waiting;

        RoundNumber = 0;

        AttackingTeam = 0;

        TeamAScore = 0;

        TeamBScore = 0;

        LastRoundWinnerTeam = -1;

        LastRoundReason = 0;

        MatchWinnerTeam = -1;
    }


    // =========================================================
    // DESPAWNED
    // =========================================================

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        networkReady = false;

        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        networkReady = false;

        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // NETWORK UPDATE
    // =========================================================

    public override void FixedUpdateNetwork()
    {
        if (!networkReady)
        {
            return;
        }


        if (!Object.HasStateAuthority)
        {
            return;
        }


        RefreshPlayers();


        switch (CurrentPhase)
        {
            // =================================================
            // WAITING
            // =================================================

            case PatinteroGameplayPhase.Waiting:

                TryStartMatch();

                break;


            // =================================================
            // INTRO
            // =================================================

            case PatinteroGameplayPhase.Intro:

                if (PhaseTimer.Expired(Runner))
                {
                    PositionPlayersForRound();

                    StartInstructions();
                }

                break;


            // =================================================
            // INSTRUCTIONS
            // =================================================

            case PatinteroGameplayPhase.Instructions:

                if (PhaseTimer.Expired(Runner))
                {
                    StartCountdown();
                }

                break;


            // =================================================
            // COUNTDOWN
            // =================================================

            case PatinteroGameplayPhase.Countdown:

                if (PhaseTimer.Expired(Runner))
                {
                    StartRound();
                }

                break;


            // =================================================
            // PLAYING
            // =================================================

            case PatinteroGameplayPhase.Playing:

                ProcessRound();

                break;


            // =================================================
            // ROUND RESULT
            // =================================================

            case PatinteroGameplayPhase.RoundResult:

                if (PhaseTimer.Expired(Runner))
                {
                    ContinueAfterRound();
                }

                break;


            // =================================================
            // SWITCHING SIDES
            // =================================================

            case PatinteroGameplayPhase.SwitchingSides:

                if (PhaseTimer.Expired(Runner))
                {
                    StartCountdown();
                }

                break;


            // =================================================
            // MATCH OVER
            // =================================================

            case PatinteroGameplayPhase.MatchOver:

                break;
        }
    }


    // =========================================================
    // REFRESH PLAYERS
    // =========================================================

    private void RefreshPlayers()
    {
        players.Clear();


        PatinteroGameplayPlayer[] found =
            FindObjectsByType<PatinteroGameplayPlayer>(
                FindObjectsSortMode.None
            );


        for (int i = 0; i < found.Length; i++)
        {
            if (
                found[i] != null &&
                found[i].Object != null)
            {
                players.Add(
                    found[i]
                );
            }
        }


        players.Sort(
            (a, b) =>
                a.Object.InputAuthority.PlayerId
                    .CompareTo(
                        b.Object.InputAuthority.PlayerId
                    )
        );
    }


    // =========================================================
    // ACTIVE PLAYER COUNT
    // =========================================================

    private int GetActivePlayerCount()
    {
        int count = 0;


        foreach (
            PlayerRef player
            in Runner.ActivePlayers)
        {
            count++;
        }


        return count;
    }


    // =========================================================
    // TRY START MATCH
    // =========================================================

    private void TryStartMatch()
    {
        int expectedPlayers =
            GetActivePlayerCount();


        if (expectedPlayers < 2)
        {
            return;
        }


        if (players.Count != expectedPlayers)
        {
            return;
        }


        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].Configured)
            {
                return;
            }
        }


        int teamA =
            CountTeam(0);

        int teamB =
            CountTeam(1);


        if (teamA != teamB)
        {
            return;
        }


        if (
            teamA < 1 ||
            teamA > 5)
        {
            return;
        }


        TeamAScore = 0;

        TeamBScore = 0;

        RoundNumber = 1;

        // Team A attacks first.
        AttackingTeam = 0;

        MatchWinnerTeam = -1;


        PositionPlayersForIntro();


        PhaseValue =
            (int)
            PatinteroGameplayPhase.Intro;


        PhaseTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                introDuration
            );


        Debug.Log(
            "PATINTERO MATCH STARTED"
        );
    }


    // =========================================================
    // COUNT TEAM
    // =========================================================

    private int CountTeam(
        int teamId)
    {
        int count = 0;


        for (int i = 0; i < players.Count; i++)
        {
            if (
                players[i].Configured &&
                players[i].TeamId == teamId)
            {
                count++;
            }
        }


        return count;
    }


    // =========================================================
    // INTRO POSITIONS
    // =========================================================

    private void PositionPlayersForIntro()
    {
        List<PatinteroGameplayPlayer> teamA =
            GetTeamPlayers(0);

        List<PatinteroGameplayPlayer> teamB =
            GetTeamPlayers(1);


        for (
            int i = 0;
            i < teamA.Count &&
            i < teamAIntroSpawns.Length;
            i++)
        {
            Transform spawn =
                teamAIntroSpawns[i];


            if (spawn == null)
            {
                continue;
            }


            teamA[i].Teleport(
                spawn.position,
                spawn.rotation
            );
        }


        for (
            int i = 0;
            i < teamB.Count &&
            i < teamBIntroSpawns.Length;
            i++)
        {
            Transform spawn =
                teamBIntroSpawns[i];


            if (spawn == null)
            {
                continue;
            }


            teamB[i].Teleport(
                spawn.position,
                spawn.rotation
            );
        }
    }


    // =========================================================
    // ROUND POSITIONS
    // =========================================================

    private void PositionPlayersForRound()
    {
        int defendingTeam =
            1 -
            AttackingTeam;


        List<PatinteroGameplayPlayer> attackers =
            GetTeamPlayers(
                AttackingTeam
            );


        List<PatinteroGameplayPlayer> defenders =
            GetTeamPlayers(
                defendingTeam
            );


        // =====================================================
        // ATTACKERS
        // =====================================================

        for (int i = 0; i < attackers.Count; i++)
        {
            if (
                attackerSpawns == null ||
                attackerSpawns.Length == 0)
            {
                break;
            }


            int spawnIndex =
                Mathf.Clamp(
                    i,
                    0,
                    attackerSpawns.Length - 1
                );


            Transform spawn =
                attackerSpawns[
                    spawnIndex
                ];


            if (spawn == null)
            {
                continue;
            }


            attackers[i].SetupForRound(
                false,
                -1,
                spawn.position,
                spawn.rotation
            );
        }


        // =====================================================
        // DEFENDERS
        // =====================================================

        for (int i = 0; i < defenders.Count; i++)
        {
            int lineIndex =
                GetGuardLineAssignment(
                    defenders.Count,
                    i
                );


            PatinteroGuardLine line =
                GetGuardLine(
                    lineIndex
                );


            if (line == null)
            {
                continue;
            }


            defenders[i].SetupForRound(
                true,
                lineIndex,
                line.MidPoint,
                line.GetFacingRotation(1)
            );
        }
    }


    // =========================================================
    // GUARD LINE ASSIGNMENT
    // =========================================================

    private int GetGuardLineAssignment(
        int defenderCount,
        int defenderIndex)
    {
        if (defenderCount <= 1)
        {
            return 0;
        }


        if (defenderCount == 2)
        {
            return
                Mathf.Clamp(
                    defenderIndex,
                    0,
                    1
                );
        }


        if (defenderCount == 3)
        {
            return
                Mathf.Clamp(
                    defenderIndex,
                    0,
                    2
                );
        }


        if (defenderCount == 4)
        {
            return
                Mathf.Clamp(
                    defenderIndex,
                    0,
                    3
                );
        }


        // 5v5:
        // index 4 = middle guard
        return
            Mathf.Clamp(
                defenderIndex,
                0,
                4
            );
    }


    // =========================================================
    // GET GUARD LINE
    // =========================================================

    public PatinteroGuardLine GetGuardLine(
        int index)
    {
        if (
            guardLines == null ||
            index < 0 ||
            index >= guardLines.Length)
        {
            return null;
        }


        return guardLines[index];
    }


    // =========================================================
    // INSTRUCTIONS
    // =========================================================

    private void StartInstructions()
    {
        PhaseValue =
            (int)
            PatinteroGameplayPhase.Instructions;


        PhaseTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                instructionDuration
            );


        Debug.Log(
            "PATINTERO INSTRUCTIONS STARTED"
        );
    }


    // =========================================================
    // COUNTDOWN
    // =========================================================

    private void StartCountdown()
    {
        PhaseValue =
            (int)
            PatinteroGameplayPhase.Countdown;


        PhaseTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                countdownDuration
            );


        Debug.Log(
            "PATINTERO COUNTDOWN STARTED"
        );
    }


    // =========================================================
    // START ROUND
    // =========================================================

    private void StartRound()
    {
        PhaseValue =
            (int)
            PatinteroGameplayPhase.Playing;


        RoundTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                roundTime
            );


        Debug.Log(
            "ROUND " +
            RoundNumber +
            " STARTED"
        );
    }


    // =========================================================
    // PROCESS ROUND
    // =========================================================

    private void ProcessRound()
    {
        List<PatinteroGameplayPlayer> attackers =
            GetTeamPlayers(
                AttackingTeam
            );


        bool everyoneReturned =
            attackers.Count > 0;


        for (int i = 0; i < attackers.Count; i++)
        {
            PatinteroGameplayPlayer attacker =
                attackers[i];


            Vector3 position =
                attacker.transform.position;


            // =================================================
            // REACH END
            // =================================================

            if (
                !attacker.ReachedEnd &&
                IsInside(
                    endZone,
                    position))
            {
                attacker.MarkReachedEnd();
            }


            // =================================================
            // RETURN TO START
            // =================================================

            if (
                attacker.ReachedEnd &&
                !attacker.ReturnedHome &&
                IsInside(
                    startZone,
                    position))
            {
                attacker.MarkReturnedHome();
            }


            if (!attacker.ReturnedHome)
            {
                everyoneReturned =
                    false;
            }
        }


        // =====================================================
        // ATTACKERS FINISHED
        // =====================================================

        if (everyoneReturned)
        {
            EndRound(
                AttackingTeam,
                1
            );

            return;
        }


        // =====================================================
        // TIMER EXPIRED
        // =====================================================

        if (RoundTimer.Expired(Runner))
        {
            EndRound(
                1 - AttackingTeam,
                3
            );
        }
    }


    // =========================================================
    // GUARD TOUCH
    // =========================================================

    public void TryGuardTouch(
        PatinteroGameplayPlayer defender)
    {
        if (!networkReady)
        {
            return;
        }


        if (!Object.HasStateAuthority)
        {
            return;
        }


        if (!IsRoundPlaying)
        {
            return;
        }


        if (
            defender == null ||
            !defender.IsDefender)
        {
            return;
        }


        int defendingTeam =
            1 -
            AttackingTeam;


        if (defender.TeamId != defendingTeam)
        {
            return;
        }


        List<PatinteroGameplayPlayer> attackers =
            GetTeamPlayers(
                AttackingTeam
            );


        for (int i = 0; i < attackers.Count; i++)
        {
            PatinteroGameplayPlayer attacker =
                attackers[i];


            if (attacker == null)
            {
                continue;
            }


            Vector3 defenderPosition =
                defender.transform.position;

            Vector3 attackerPosition =
                attacker.transform.position;


            defenderPosition.y = 0f;

            attackerPosition.y = 0f;


            Vector3 directionToAttacker =
                attackerPosition -
                defenderPosition;


            float distance =
                directionToAttacker.magnitude;


            if (distance > tagDistance)
            {
                continue;
            }


            if (
                directionToAttacker.sqrMagnitude <=
                0.0001f)
            {
                continue;
            }


            directionToAttacker.Normalize();


            Vector3 defenderForward =
                defender.transform.forward;


            defenderForward.y = 0f;


            if (
                defenderForward.sqrMagnitude <=
                0.0001f)
            {
                defenderForward =
                    Vector3.forward;
            }


            defenderForward.Normalize();


            float angle =
                Vector3.Angle(
                    defenderForward,
                    directionToAttacker
                );


            if (
                angle >
                tagForwardAngle * 0.5f)
            {
                continue;
            }


            Debug.Log(
                "TOUCH SUCCESS: " +
                defender.PlayerName.ToString() +
                " TAGGED " +
                attacker.PlayerName.ToString()
            );


            EndRound(
                defendingTeam,
                2
            );


            return;
        }


        Debug.Log(
            "TOUCH MISSED"
        );
    }


    // =========================================================
    // ZONE CHECK
    // =========================================================

    private bool IsInside(
        BoxCollider zone,
        Vector3 position)
    {
        if (zone == null)
        {
            return false;
        }


        return
            zone.bounds.Contains(
                position
            );
    }


    // =========================================================
    // END ROUND
    // =========================================================

    private void EndRound(
        int winningTeam,
        int reason)
    {
        if (
            CurrentPhase !=
            PatinteroGameplayPhase.Playing)
        {
            return;
        }


        LastRoundWinnerTeam =
            winningTeam;

        LastRoundReason =
            reason;


        if (winningTeam == 0)
        {
            TeamAScore++;
        }
        else
        {
            TeamBScore++;
        }


        for (int i = 0; i < players.Count; i++)
        {
            bool won =
                players[i].TeamId ==
                winningTeam;


            players[i].SetRoundResult(
                won
            );
        }


        PhaseValue =
            (int)
            PatinteroGameplayPhase.RoundResult;


        PhaseTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                roundResultDuration
            );
    }


    // =========================================================
    // CONTINUE AFTER ROUND
    // =========================================================

    private void ContinueAfterRound()
    {
        // First team to 2 round wins.
        if (
            TeamAScore >= 2 ||
            TeamBScore >= 2)
        {
            MatchWinnerTeam =
                TeamAScore >= 2
                    ? 0
                    : 1;


            PhaseValue =
                (int)
                PatinteroGameplayPhase.MatchOver;


            return;
        }


        RoundNumber++;


        // Switch attacker / defender.
        AttackingTeam =
            1 -
            AttackingTeam;


        PositionPlayersForRound();


        PhaseValue =
            (int)
            PatinteroGameplayPhase.SwitchingSides;


        PhaseTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                sideSwitchDuration
            );
    }


    // =========================================================
    // GET TEAM PLAYERS
    // =========================================================

    private List<PatinteroGameplayPlayer>
        GetTeamPlayers(
            int teamId)
    {
        List<PatinteroGameplayPlayer> result =
            new List<PatinteroGameplayPlayer>();


        for (int i = 0; i < players.Count; i++)
        {
            if (
                players[i].Configured &&
                players[i].TeamId ==
                teamId)
            {
                result.Add(
                    players[i]
                );
            }
        }


        return result;
    }


    // =========================================================
    // ROUND TIMER
    // =========================================================

    public float GetRoundTimeRemaining()
    {
        if (!networkReady)
        {
            return 0f;
        }


        if (Runner == null)
        {
            return 0f;
        }


        float? remaining =
            RoundTimer.RemainingTime(
                Runner
            );


        return
            remaining.HasValue
                ? Mathf.Max(
                    0f,
                    remaining.Value
                )
                : 0f;
    }


    // =========================================================
    // PHASE TIMER
    // =========================================================

    public float GetPhaseTimeRemaining()
    {
        if (!networkReady)
        {
            return 0f;
        }

        if (Runner == null)
        {
            return 0f;
        }


        float? remaining =
            PhaseTimer.RemainingTime(
                Runner
            );


        return
            remaining.HasValue
                ? Mathf.Max(
                    0f,
                    remaining.Value
                )
                : 0f;
    }
}