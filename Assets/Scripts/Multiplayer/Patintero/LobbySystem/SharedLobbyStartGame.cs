using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharedLobbyStartGame : MonoBehaviour
{
    [Header("FUSION")]
    [SerializeField]
    private PatinteroFusionManager fusionManager;

    private bool startRequested = false;


    // =========================================================
    // START SELECTED GAME
    // =========================================================

    public void StartSelectedGame()
    {
        if (startRequested)
        {
            return;
        }

        if (fusionManager == null)
        {
            fusionManager =
                PatinteroFusionManager.Instance;
        }

        if (fusionManager == null)
        {
            Debug.LogError(
                "START GAME FAILED: PatinteroFusionManager was not found."
            );

            return;
        }

        NetworkRunner runner =
            fusionManager.Runner;

        if (runner == null)
        {
            Debug.LogError(
                "START GAME FAILED: NetworkRunner is null."
            );

            return;
        }

        if (!runner.IsRunning ||
            !runner.IsInSession)
        {
            Debug.LogError(
                "START GAME FAILED: Fusion session is not running."
            );

            return;
        }

        if (!runner.IsSceneAuthority)
        {
            Debug.LogWarning(
                "START GAME IGNORED: Only the host can start the game."
            );

            return;
        }

        // Validates 1v1 testing / normal player counts,
        // equal teams, character selection, Ready status,
        // recalculates team indices, and closes the room.
        if (!fusionManager.PrepareSelectedMatchStart(
            out string startMessage))
        {
            Debug.LogWarning(
                "START GAME BLOCKED: " +
                startMessage
            );

            return;
        }

        // Preserve every player's exact lobby choice BEFORE
        // the Host changes from Main -> Patintero.
        PatinteroMatchSetupCache.Capture(
            fusionManager.GetLobbyPlayers()
        );


        string sceneName =
            MultiplayerGameSelection
                .GetSceneName();

        if (string.IsNullOrWhiteSpace(
            sceneName))
        {
            Debug.LogError(
                "START GAME FAILED: No multiplayer game was selected."
            );

            return;
        }

        int buildIndex =
            FindSceneBuildIndex(
                sceneName
            );

        if (buildIndex < 0)
        {
            Debug.LogError(
                "START GAME FAILED: Scene '" +
                sceneName +
                "' is not in the Build Scene List."
            );

            // Re-open the session because scene loading did not start.
            if (runner.SessionInfo.IsValid)
            {
                runner.SessionInfo.IsOpen = true;
                runner.SessionInfo.IsVisible = true;
            }

            return;
        }

        startRequested = true;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "STARTING MULTIPLAYER GAME"
        );

        Debug.Log(
            "GAME = " +
            MultiplayerGameSelection
                .GetGameName()
        );

        Debug.Log(
            "SCENE = " +
            sceneName
        );

        Debug.Log(
            "BUILD INDEX = " +
            buildIndex
        );

        Debug.Log(
            "PLAYERS = " +
            fusionManager
                .GetLobbyPlayers()
                .Count
        );

        Debug.Log(
            "================================"
        );

        // IMPORTANT:
        // Do not call SceneManager.LoadScene here.
        // The Host loads through Fusion so all clients follow.
        runner.LoadScene(
            SceneRef.FromIndex(buildIndex),
            LoadSceneMode.Single
        );
    }


    // =========================================================
    // FIND SCENE BY NAME
    // =========================================================

    private int FindSceneBuildIndex(
        string sceneName)
    {
        int sceneCount =
            SceneManager
                .sceneCountInBuildSettings;

        for (int i = 0;
             i < sceneCount;
             i++)
        {
            string scenePath =
                SceneUtility
                    .GetScenePathByBuildIndex(i);

            string name =
                System.IO.Path
                    .GetFileNameWithoutExtension(
                        scenePath
                    );

            if (string.Equals(
                name,
                sceneName,
                System.StringComparison
                    .OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
