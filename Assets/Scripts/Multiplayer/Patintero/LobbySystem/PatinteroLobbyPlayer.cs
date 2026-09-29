using System.Collections.Generic;
using System.Linq;

using Fusion;

using UnityEngine;


public class PatinteroLobbyPlayer : NetworkBehaviour
{
    public static readonly List<PatinteroLobbyPlayer> All =
        new List<PatinteroLobbyPlayer>();


    public static PatinteroLobbyPlayer Local
    {
        get;
        private set;
    }


    // =========================================
    // NETWORK DATA
    // =========================================

    [Networked]
    public NetworkString<_32> PlayerName
    {
        get;
        set;
    }


    [Networked]
    public int CharacterIndex
    {
        get;
        set;
    }


    [Networked]
    public NetworkBool IsReady
    {
        get;
        set;
    }


    [Networked]
    public NetworkBool IsHost
    {
        get;
        set;
    }


    // 0 = Team A
    // 1 = Team B
    [Networked]
    public int TeamId
    {
        get;
        set;
    }


    [Networked]
    public int TeamIndex
    {
        get;
        set;
    }


    // =========================================
    // INITIALIZE
    // =========================================

    public void InitializeBeforeSpawn(
        bool host,
        int initialTeam)
    {
        PlayerName = "Player";

        CharacterIndex = -1;

        IsReady = false;

        IsHost = host;

        TeamId = initialTeam;

        TeamIndex = 0;
    }


    // =========================================
    // SPAWN
    // =========================================

    public override void Spawned()
    {
        if (!All.Contains(this))
        {
            All.Add(this);
        }


        // Keep lobby player alive when
        // changing from Main -> Patintero scene.
        DontDestroyOnLoad(gameObject);


        if (HasInputAuthority)
        {
            Local = this;


            // ---------------------------------
            // PLAYER NAME
            // ---------------------------------

            string localName =
                PatinteroFusionManager.Instance != null
                ? PatinteroFusionManager
                    .Instance
                    .LocalPlayerName
                : "Player";


            RPC_SetPlayerName(
                localName
            );


            // ---------------------------------
            // RESTORE LOCAL TEAM SELECTION
            // ---------------------------------

            int savedTeam =
                PatinteroLocalSelection.TeamId;


            if (
                savedTeam == 0 ||
                savedTeam == 1)
            {
                RequestTeam(
                    savedTeam
                );
            }


            // ---------------------------------
            // RESTORE LOCAL CHARACTER
            // ---------------------------------

            int savedCharacter =
                PatinteroLocalSelection
                    .CharacterIndex;


            if (
                savedCharacter >= 0 &&
                savedCharacter <= 9)
            {
                RequestCharacter(
                    savedCharacter
                );
            }


            Debug.Log(
                "LOCAL PATINTERO LOBBY PLAYER READY | " +
                "Team = " +
                TeamId +
                " | Character = " +
                savedCharacter
            );
        }
    }


    // =========================================
    // DESPAWN
    // =========================================

    public override void Despawned(
        NetworkRunner runner,
        bool hasState)
    {
        All.Remove(this);


        if (Local == this)
        {
            Local = null;
        }
    }


    private void OnDestroy()
    {
        All.Remove(this);


        if (Local == this)
        {
            Local = null;
        }
    }


    // =========================================
    // NAME
    // =========================================

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(
        string newName)
    {
        if (string.IsNullOrWhiteSpace(
            newName))
        {
            newName = "Player";
        }


        newName =
            newName.Trim();


        if (newName.Length > 20)
        {
            newName =
                newName.Substring(
                    0,
                    20
                );
        }


        PlayerName =
            newName;
    }


    // =========================================
    // CHARACTER
    // =========================================

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_SetCharacter(
        int index)
    {
        CharacterIndex =
            Mathf.Clamp(
                index,
                0,
                9
            );


        // Character changed.
        // Player must Ready again.
        IsReady = false;


        Debug.Log(
            "NETWORK CHARACTER SELECTED | " +
            PlayerName.ToString() +
            " = " +
            CharacterIndex
        );
    }


    public void RequestCharacter(
        int index)
    {
        if (!HasInputAuthority)
        {
            return;
        }


        if (
            index < 0 ||
            index > 9)
        {
            Debug.LogWarning(
                "Invalid character index: " +
                index
            );

            return;
        }


        RPC_SetCharacter(
            index
        );
    }


    // =========================================
    // READY
    // =========================================

    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_SetReady(
        bool ready)
    {
        // Cannot Ready without character.
        if (CharacterIndex < 0)
        {
            IsReady = false;

            Debug.LogWarning(
                "READY FAILED: Select a character first."
            );

            return;
        }


        // Host doesn't need Ready.
        if (IsHost)
        {
            IsReady = false;

            return;
        }


        IsReady =
            ready;
    }


    public void ToggleReady()
    {
        if (!HasInputAuthority)
        {
            return;
        }


        RPC_SetReady(
            !IsReady
        );
    }


    public void RequestReady(
        bool ready)
    {
        if (!HasInputAuthority)
        {
            return;
        }


        RPC_SetReady(
            ready
        );
    }


    // =========================================
    // TEAM CHANGE
    // =========================================

    public void RequestTeam(
        int wantedTeam)
    {
        if (!HasInputAuthority)
        {
            return;
        }


        if (
            wantedTeam != 0 &&
            wantedTeam != 1)
        {
            return;
        }


        RPC_RequestTeam(
            wantedTeam
        );
    }


    [Rpc(
        RpcSources.InputAuthority,
        RpcTargets.StateAuthority)]
    private void RPC_RequestTeam(
        int wantedTeam)
    {
        if (
            wantedTeam != 0 &&
            wantedTeam != 1)
        {
            return;
        }


        // Already in this team.
        if (TeamId == wantedTeam)
        {
            return;
        }


        int maxPlayers =
            Runner.SessionInfo.MaxPlayers;


        int maxPerTeam =
            Mathf.Max(
                1,
                maxPlayers / 2
            );


        int currentTeamCount =
            All.Count(
                player =>
                    player != null &&
                    player != this &&
                    player.Object != null &&
                    player.Object.IsValid &&
                    player.TeamId ==
                        wantedTeam
            );


        // Target team already full.
        if (currentTeamCount >= maxPerTeam)
        {
            Debug.LogWarning(
                "TEAM CHANGE FAILED: Team " +
                (wantedTeam == 0
                    ? "A"
                    : "B") +
                " is full."
            );

            return;
        }


        TeamId =
            wantedTeam;


        // Changing team cancels Ready.
        IsReady = false;


        ServerRecalculateTeamIndices();


        Debug.Log(
            PlayerName.ToString() +
            " changed to TEAM " +
            (TeamId == 0
                ? "A"
                : "B")
        );
    }


    // =========================================
    // TEAM INDEX
    // =========================================

    public void ServerRecalculateTeamIndices()
    {
        if (
            Runner == null ||
            !Runner.IsServer)
        {
            return;
        }


        List<PatinteroLobbyPlayer> teamA =
            All
            .Where(
                player =>
                    player != null &&
                    player.Object != null &&
                    player.Object.IsValid &&
                    player.TeamId == 0
            )
            .OrderBy(
                player =>
                    player.Object
                        .InputAuthority
                        .PlayerId
            )
            .ToList();


        List<PatinteroLobbyPlayer> teamB =
            All
            .Where(
                player =>
                    player != null &&
                    player.Object != null &&
                    player.Object.IsValid &&
                    player.TeamId == 1
            )
            .OrderBy(
                player =>
                    player.Object
                        .InputAuthority
                        .PlayerId
            )
            .ToList();


        for (
            int i = 0;
            i < teamA.Count;
            i++)
        {
            teamA[i].TeamIndex =
                i;
        }


        for (
            int i = 0;
            i < teamB.Count;
            i++)
        {
            teamB[i].TeamIndex =
                i;
        }
    }


    // =========================================
    // SERVER
    // =========================================

    public void ServerSetTeam(
        int team,
        int index)
    {
        if (!HasStateAuthority)
        {
            return;
        }


        TeamId =
            Mathf.Clamp(
                team,
                0,
                1
            );


        TeamIndex =
            Mathf.Max(
                0,
                index
            );
    }


    public void ServerResetReady()
    {
        if (!HasStateAuthority)
        {
            return;
        }


        IsReady =
            false;
    }
}