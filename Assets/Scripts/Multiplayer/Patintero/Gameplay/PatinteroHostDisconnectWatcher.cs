using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PatinteroHostDisconnectWatcher : MonoBehaviour
{
    [Header("HOST DISCONNECT")]
    [SerializeField]
    private float returnDelay = 5f;

    [SerializeField]
    private string mainSceneName = "Main";


    private bool clientSessionWasActive;
    private bool returnStarted;


    private void Update()
    {
        if (returnStarted)
        {
            return;
        }


        PatinteroFusionManager manager =
            PatinteroFusionManager.Instance;

        NetworkRunner runner =
            manager != null
                ? manager.Runner
                : null;


        // Remember that this device was a CLIENT in a real room.
        if (
            runner != null &&
            runner.IsRunning &&
            runner.IsInSession)
        {
            if (!runner.IsServer)
            {
                clientSessionWasActive = true;
            }

            return;
        }


        // If this client previously had a live session and that
        // session is gone, Fusion has detected the Host/room loss.
        if (clientSessionWasActive)
        {
            StartCoroutine(
                ReturnAfterHostDisconnect()
            );
        }
    }


    private IEnumerator ReturnAfterHostDisconnect()
    {
        if (returnStarted)
        {
            yield break;
        }

        returnStarted = true;


        Debug.LogWarning(
            "PATINTERO HOST DISCONNECTED. " +
            "Returning to game selection in " +
            returnDelay +
            " seconds."
        );


        yield return
            new WaitForSecondsRealtime(
                returnDelay
            );


        PatinteroFusionManager manager =
            PatinteroFusionManager.Instance;

        NetworkRunner runner =
            manager != null
                ? manager.Runner
                : null;


        // If the client session recovered, cancel the return.
        if (
            runner != null &&
            runner.IsRunning &&
            runner.IsInSession &&
            !runner.IsServer)
        {
            returnStarted = false;
            yield break;
        }


        // Clean up any runner that still remains.
        if (
            runner != null &&
            runner.IsRunning)
        {
            var shutdown =
                runner.Shutdown(
                    destroyGameObject: true
                );

            while (!shutdown.IsCompleted)
            {
                yield return null;
            }
        }


        PatinteroLocalSelection.Reset();
        MultiplayerGameSelection.Reset();


        // Must be a ROOT object before DontDestroyOnLoad.
        if (transform.parent != null)
        {
            transform.SetParent(
                null,
                true
            );
        }

        DontDestroyOnLoad(
            gameObject
        );


        SceneManager.sceneLoaded +=
            OnSceneLoaded;


        SceneManager.LoadScene(
            mainSceneName
        );
    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (
            !string.Equals(
                scene.name,
                mainSceneName,
                System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }


        SceneManager.sceneLoaded -=
            OnSceneLoaded;


        MultiplayerGameSelector selector =
            FindFirstObjectByType<
                MultiplayerGameSelector>(
                FindObjectsInactive.Include
            );


        if (selector != null)
        {
            selector.BackToGameSelection();
        }
        else
        {
            PatinteroMenuUI menu =
                FindFirstObjectByType<
                    PatinteroMenuUI>(
                    FindObjectsInactive.Include
                );

            if (menu != null)
            {
                menu.ExitToGameSelection();
            }
        }


        Debug.Log(
            "RETURNED TO GAME SELECTION AFTER HOST DISCONNECT"
        );


        Destroy(
            gameObject
        );
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }
}
