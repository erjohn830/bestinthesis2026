using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance;

    [Header("Loading UI")]
    [Header("Story Loading")]
    public float storyLoadingSeconds = 10f;
    public GameObject loadingPanel;
    public Slider loadingSlider;
    public TMP_Text loadingText;
    public TMP_Text triviaText;

    [Header("Trivia")]
    public float triviaChangeSeconds = 5f;

    private Coroutine triviaCoroutine;
    private Canvas loadingCanvas;

    // =========================================
    // AWAKE
    // =========================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            // Loading UI appears above other UI
            loadingCanvas =
                GetComponentInChildren<Canvas>(true);

            if (loadingCanvas != null)
            {
                loadingCanvas.overrideSorting = true;
                loadingCanvas.sortingOrder = 999;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // =========================================
    // START
    // =========================================

    void Start()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;
        }
    }

    // =========================================
    // SHOW LOADING
    // =========================================

    public void ShowLoading(
        string message,
        string sceneName)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);

            loadingPanel.transform
                .SetAsLastSibling();
        }

        if (loadingText != null)
        {
            loadingText.text = message;
        }

        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
        }

        if (triviaCoroutine != null)
        {
            StopCoroutine(
                triviaCoroutine
            );
        }

        triviaCoroutine =
            StartCoroutine(
                TriviaLoop(sceneName)
            );

        Canvas.ForceUpdateCanvases();

        Debug.Log(
            "LOADING SCREEN SHOWN: " +
            message
        );
    }

    // =========================================
    // RANDOM TRIVIA
    // =========================================

    IEnumerator TriviaLoop(
        string sceneName)
    {
        string[] trivia =
            GetTrivia(sceneName);

        if (trivia.Length == 0)
        {
            yield break;
        }

        int lastIndex = -1;

        while (true)
        {
            int randomIndex;

            do
            {
                randomIndex =
                    Random.Range(
                        0,
                        trivia.Length
                    );
            }
            while (
                trivia.Length > 1 &&
                randomIndex == lastIndex
            );

            lastIndex = randomIndex;

            if (triviaText != null)
            {
                triviaText.text =
                    trivia[randomIndex];
            }

            yield return
                new WaitForSecondsRealtime(
                    triviaChangeSeconds
                );
        }
    }

    // =========================================
    // TRIVIA
    // =========================================

    string[] GetTrivia(
        string sceneName)
    {
        // HULIHIN ANG BABOY
        if (sceneName == "HulihinBiik")
        {
            return new string[]
            {
                "TRIVIA: Hulihin ang Baboy tests speed, timing, and quick reactions.",

                "TRIVIA: Players must move quickly to catch the escaping pig.",

                "TRIVIA: Chase games can develop agility and coordination."
            };
        }

        // SIPA
        if (sceneName == "Sipa")
        {
            return new string[]
            {
                "TRIVIA: Sipa tests balance, timing, and coordination.",

                "TRIVIA: The goal is to keep the sipa from touching the ground.",

                "TRIVIA: Good timing is important when repeatedly kicking the sipa."
            };
        }

        // PALOSEBO
        if (sceneName == "Palosebo")
        {
            return new string[]
            {
                "TRIVIA: Palo-sebo is a traditional Filipino climbing game.",

                "TRIVIA: Players climb a slippery pole to reach the prize.",

                "TRIVIA: Palo-sebo requires strength, balance, and determination."
            };
        }

        // LUKSONG BAKA
        if (sceneName == "LuksongBakaGame")
        {
            return new string[]
            {
                "TRIVIA: Luksong Baka is a traditional Filipino jumping game.",

                "TRIVIA: Players jump over another player who acts as the baka.",

                "TRIVIA: The challenge becomes harder as the height increases."
            };
        }

        // PATINTERO
        if (sceneName == "PatinteroGame")
        {
            return new string[]
            {
                "TRIVIA: Patintero is a traditional Filipino team game.",

                "TRIVIA: Players cross marked lines while avoiding defenders.",

                "TRIVIA: Patintero develops teamwork, strategy, speed, and awareness."
            };
        }

        // SEKYU BASE
        if (sceneName == "SekyuBase")
        {
            return new string[]
            {
                "TRIVIA: Sekyu Base focuses on teamwork, defense, and strategy.",

                "TRIVIA: Players protect their base while watching their opponents.",

                "TRIVIA: Timing and communication are important in Sekyu Base."
            };
        }

        // BAMSAK
        if (sceneName == "Bamsak")
        {
            return new string[]
            {
                "TRIVIA: Bamsak challenges players to react quickly.",

                "TRIVIA: Awareness of other players is important in Bamsak.",

                "TRIVIA: Quick movement and timing can give players an advantage."
            };
        }

        // DEFAULT
        return new string[]
    {
        "<b>TRIVIA:</b>\nHulihin ang Baboy tests speed, timing, and quick reactions.",

        "<b>TRIVIA:</b>\nPlayers must move quickly to catch the escaping pig.",

        "<b>TRIVIA:</b>\nChase games can develop agility and coordination."
    };
    }

    // =========================================
    // SET PROGRESS
    // =========================================

    public void SetProgress(
        float value)
    {
        if (loadingSlider != null)
        {
            loadingSlider.value =
                Mathf.Clamp01(value);
        }
    }

    // =========================================
    // HIDE LOADING
    // =========================================

    public void HideLoading()
    {
        if (triviaCoroutine != null)
        {
            StopCoroutine(
                triviaCoroutine
            );

            triviaCoroutine = null;
        }

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    // =========================================
    // NORMAL SCENE LOADING
    // =========================================

    public void LoadScene(
        string sceneName,
        string message = "Loading...")
    {
        StartCoroutine(
            LoadSceneRoutine(
                sceneName,
                message
            )
        );
    }

    IEnumerator LoadSceneRoutine(
        string sceneName,
        string message)
    {
        ShowLoading(
            message,
            sceneName
        );

        // Allow UI to appear first
        yield return null;

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName
            );

        operation.allowSceneActivation = false;

        while (
            operation.progress < 0.9f)
        {
            float progress =
                operation.progress / 0.9f;

            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        yield return
            new WaitForSecondsRealtime(
                0.5f
            );

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        HideLoading();
    }

    // =========================================
    // STORY MODE - NEW GAME
    // =========================================

    public void LoadNewStoryGame()
{
    Debug.Log(
        "LOADING NEW STORY GAME"
    );

    LoadStoryScene(
        "HulihinBiik"
    );
}
    // =========================================
// STORY MODE - CONTINUE
// =========================================

public void LoadStoryStage(int savedStage)
{
    Debug.Log(
        "LOADING STORY STAGE: " +
        savedStage
    );

    if (savedStage == 0)
    {
        LoadStoryScene(
            "HulihinBiik"
        );
    }
    else if (savedStage == 1)
    {
        LoadStoryScene(
            "Sipa"
        );
    }
    else if (savedStage == 2)
    {
        LoadStoryScene(
            "Palosebo"
        );
    }
    else if (savedStage == 3)
    {
        LoadStoryScene(
            "LuksongBakaGame"
        );
    }
    else
    {
        Debug.Log(
            "Story Mode already completed!"
        );
    }
}

    // =========================================
    // STORY MODE NEXT GAME
    // =========================================

    public void LoadNextStoryGame(
        int completedStage)
    {
        if (completedStage == 1)
        {
            LoadScene(
                "Sipa",
                "Loading..."
            );
        }

        else if (completedStage == 2)
        {
            LoadScene(
                "Palosebo",
                "Loading..."
            );
        }

        else if (completedStage == 3)
        {
            LoadScene(
                "LuksongBakaGame",
                "Loading..."
            );
        }

        else if (completedStage == 4)
        {
            LoadScene(
                "Main",
                "Loading..."
            );
        }
    }
    // =========================================
// STORY MODE LOADING - MINIMUM 10 SECONDS
// =========================================
public void LoadStoryScene(string sceneName)
{
    StartCoroutine(
        LoadStorySceneRoutine(
            sceneName,
            "Loading...",
            storyLoadingSeconds
        )
    );
}

IEnumerator LoadStorySceneRoutine(
    string sceneName,
    string message,
    float minimumSeconds)
{
    Time.timeScale = 1f;

    ShowLoading(
        message,
        sceneName
    );

    // Give Unity one frame to show UI
    yield return null;

    AsyncOperation operation =
        SceneManager.LoadSceneAsync(
            sceneName
        );

    operation.allowSceneActivation = false;

    float timer = 0f;

    while (true)
    {
        timer += Time.unscaledDeltaTime;

        // 0 → 1 during the 10 seconds
        float timeProgress =
            Mathf.Clamp01(
                timer / minimumSeconds
            );

        // Actual Unity loading progress
        float sceneProgress =
            Mathf.Clamp01(
                operation.progress / 0.9f
            );

        // Slider follows the slower progress
        float finalProgress =
            Mathf.Min(
                timeProgress,
                sceneProgress
            );

        SetProgress(finalProgress);

        bool timeFinished =
            timer >= minimumSeconds;

        bool sceneFinished =
            operation.progress >= 0.9f;

        if (timeFinished &&
            sceneFinished)
        {
            break;
        }

        yield return null;
    }

    SetProgress(1f);

    yield return new WaitForSecondsRealtime(
        0.2f
    );

    operation.allowSceneActivation = true;

    while (!operation.isDone)
    {
        yield return null;
    }

    HideLoading();
}
}