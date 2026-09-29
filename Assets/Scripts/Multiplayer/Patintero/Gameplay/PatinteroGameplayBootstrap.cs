using System.Collections;
using Fusion;
using UnityEngine;

public class PatinteroGameplayBootstrap : MonoBehaviour
{
    [Header("NETWORK PREFAB")]
    [SerializeField]
    private NetworkPrefabRef gameplayPlayerPrefab;

    [Header("TEMP SPAWN")]
    [SerializeField]
    private Transform hiddenSpawn;


    private IEnumerator Start()
    {
        // The prefab asset may keep Hidden Spawn = None.
        if (hiddenSpawn == null)
        {
            GameObject hiddenSpawnObject =
                GameObject.Find("HiddenSpawn");

            if (hiddenSpawnObject != null)
            {
                hiddenSpawn =
                    hiddenSpawnObject.transform;
            }
            else
            {
                Debug.LogWarning(
                    "PATINTERO: HiddenSpawn was not found."
                );
            }
        }


        while (
            PatinteroFusionManager.Instance == null ||
            PatinteroFusionManager.Instance.Runner == null ||
            !PatinteroFusionManager.Instance.Runner.IsRunning)
        {
            yield return null;
        }


        NetworkRunner runner =
            PatinteroFusionManager
                .Instance
                .Runner;


        // Only Host/Server creates gameplay players.
        if (!runner.IsServer)
        {
            yield break;
        }


        // Give the gameplay scene one frame to finish loading.
        yield return null;


        foreach (
            PlayerRef player
            in runner.ActivePlayers)
        {
            SpawnGameplayPlayer(
                runner,
                player
            );
        }


        PatinteroMatchSetupCache.Clear();


        Debug.Log(
            "PATINTERO GAMEPLAY BOOTSTRAP COMPLETE"
        );
    }


    private void SpawnGameplayPlayer(
        NetworkRunner runner,
        PlayerRef player)
    {
        string playerName =
            "Player";

        int teamId =
            GetFallbackTeam(player);

        int characterIndex =
            -1;


        // =====================================================
        // FIRST: exact values captured before scene change.
        // =====================================================

        if (
            PatinteroMatchSetupCache.TryGet(
                player,
                out PatinteroMatchSetupCache.PlayerSetup setup))
        {
            playerName =
                setup.PlayerName;

            teamId =
                setup.TeamId;

            characterIndex =
                setup.CharacterIndex;
        }
        else
        {
            // Backup path if the lobby object survived scene load.
            PatinteroLobbyPlayer lobbyPlayer =
                FindLobbyPlayer(player);

            if (lobbyPlayer != null)
            {
                playerName =
                    lobbyPlayer.PlayerName.ToString();

                teamId =
                    lobbyPlayer.TeamId;

                characterIndex =
                    lobbyPlayer.CharacterIndex;
            }
        }


        if (
            teamId != 0 &&
            teamId != 1)
        {
            teamId =
                GetFallbackTeam(player);
        }


        if (
            characterIndex < 0 ||
            characterIndex > 9)
        {
            Debug.LogError(
                "PATINTERO PLAYER " +
                player.PlayerId +
                " HAS NO VALID CHARACTER. " +
                "Emergency fallback = Angelo (0)."
            );

            characterIndex = 0;
        }


        // Exactly one gameplay root per PlayerRef.
        PatinteroGameplayPlayer existing =
            FindGameplayPlayer(player);

        if (
            existing != null &&
            existing.Object != null &&
            existing.Object.IsValid)
        {
            existing.ServerInitializeFromLobby(
                playerName,
                teamId,
                characterIndex
            );

            runner.SetPlayerObject(
                player,
                existing.Object
            );

            return;
        }


        NetworkObject oldPlayerObject =
            null;

        runner.TryGetPlayerObject(
            player,
            out oldPlayerObject
        );


        Vector3 spawnPosition =
            hiddenSpawn != null
                ? hiddenSpawn.position
                : Vector3.zero;


        // =====================================================
        // CRITICAL FIX:
        //
        // Networked identity is written BEFORE Fusion invokes
        // PatinteroGameplayPlayer.Spawned().
        // =====================================================

        NetworkObject newObject =
            runner.Spawn(
                gameplayPlayerPrefab,
                spawnPosition,
                Quaternion.identity,
                player,
                (r, obj) =>
                {
                    PatinteroGameplayPlayer gameplayPlayer =
                        obj.GetComponent<
                            PatinteroGameplayPlayer>();

                    if (gameplayPlayer == null)
                    {
                        Debug.LogError(
                            "PatinteroGameplayPlayer component " +
                            "is missing from the gameplay prefab."
                        );

                        return;
                    }

                    gameplayPlayer.InitializeBeforeSpawn(
                        playerName,
                        teamId,
                        characterIndex
                    );
                }
            );


        if (newObject == null)
        {
            Debug.LogError(
                "FAILED TO SPAWN PATINTERO PLAYER " +
                player.PlayerId
            );

            return;
        }


        runner.SetPlayerObject(
            player,
            newObject
        );


        Debug.Log(
            "GAMEPLAY PLAYER SPAWNED | PLAYER " +
            player.PlayerId +
            " | TEAM = " +
            teamId +
            " | CHARACTER = " +
            characterIndex
        );


        // The old lobby player is no longer needed after the
        // new gameplay object already contains its exact setup.
        if (
            oldPlayerObject != null &&
            oldPlayerObject.IsValid &&
            oldPlayerObject != newObject)
        {
            runner.Despawn(
                oldPlayerObject
            );
        }
    }


    private PatinteroLobbyPlayer FindLobbyPlayer(
        PlayerRef player)
    {
        for (
            int i = 0;
            i < PatinteroLobbyPlayer.All.Count;
            i++)
        {
            PatinteroLobbyPlayer lobbyPlayer =
                PatinteroLobbyPlayer.All[i];

            if (
                lobbyPlayer == null ||
                lobbyPlayer.Object == null ||
                !lobbyPlayer.Object.IsValid)
            {
                continue;
            }

            if (
                lobbyPlayer.Object.InputAuthority ==
                player)
            {
                return lobbyPlayer;
            }
        }

        return null;
    }


    private PatinteroGameplayPlayer FindGameplayPlayer(
        PlayerRef player)
    {
        PatinteroGameplayPlayer[] players =
            FindObjectsByType<
                PatinteroGameplayPlayer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            PatinteroGameplayPlayer gameplayPlayer
            in players)
        {
            if (
                gameplayPlayer == null ||
                gameplayPlayer.Object == null ||
                !gameplayPlayer.Object.IsValid)
            {
                continue;
            }

            if (
                gameplayPlayer.Object.InputAuthority ==
                player)
            {
                return gameplayPlayer;
            }
        }

        return null;
    }


    private int GetFallbackTeam(
        PlayerRef player)
    {
        // 1v1 testing fallback:
        // Player 1 -> Team A
        // Player 2 -> Team B
        return
            Mathf.Abs(
                player.PlayerId - 1
            ) % 2;
    }
}
