using System.Collections.Generic;
using Fusion;
using UnityEngine;

public static class PatinteroMatchSetupCache
{
    public struct PlayerSetup
    {
        public string PlayerName;
        public int TeamId;
        public int CharacterIndex;

        public PlayerSetup(
            string playerName,
            int teamId,
            int characterIndex)
        {
            PlayerName = playerName;
            TeamId = teamId;
            CharacterIndex = characterIndex;
        }
    }


    private static readonly Dictionary<int, PlayerSetup>
        setups =
            new Dictionary<int, PlayerSetup>();


    // Host calls this while still in the waiting room.
    public static void Capture(
        List<PatinteroLobbyPlayer> lobbyPlayers)
    {
        setups.Clear();

        if (lobbyPlayers == null)
        {
            return;
        }

        foreach (
            PatinteroLobbyPlayer lobbyPlayer
            in lobbyPlayers)
        {
            if (
                lobbyPlayer == null ||
                lobbyPlayer.Object == null ||
                !lobbyPlayer.Object.IsValid)
            {
                continue;
            }

            int playerId =
                lobbyPlayer
                    .Object
                    .InputAuthority
                    .PlayerId;

            PlayerSetup setup =
                new PlayerSetup(
                    lobbyPlayer.PlayerName.ToString(),
                    lobbyPlayer.TeamId,
                    lobbyPlayer.CharacterIndex
                );

            setups[playerId] =
                setup;

            Debug.Log(
                "PATINTERO SETUP CACHED | PLAYER " +
                playerId +
                " | TEAM = " +
                setup.TeamId +
                " | CHARACTER = " +
                setup.CharacterIndex
            );
        }
    }


    public static bool TryGet(
        PlayerRef player,
        out PlayerSetup setup)
    {
        return
            setups.TryGetValue(
                player.PlayerId,
                out setup
            );
    }


    public static void Clear()
    {
        setups.Clear();
    }
}
