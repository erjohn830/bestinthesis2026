using UnityEngine;

public class StoryNextButton : MonoBehaviour
{
    [Header("Story Stage Completed")]
    public int completedStage;

    public void NextGame()
    {
        if (StoryProgressManager.instance == null)
        {
            Debug.LogWarning(
                "StoryProgressManager not found."
            );

            return;
        }

        Time.timeScale = 1f;

        StoryProgressManager.instance
            .CompleteGame(completedStage);

        if (completedStage == 1)
        {
            LoadGame("StoryBook_Sipa");
        }
        else if (completedStage == 2)
        {
            LoadGame("StoryBook_Palosebo");
        }
        else if (completedStage == 3)
        {
            LoadGame("StoryBook_LuksongBaka");
        }
        else if (completedStage == 4)
        {
            StoryProgressManager.instance
                .SaveProgress(() =>
                {
                    LoadGame("Main");
                });
        }
    }

    void LoadGame(string sceneName)
    {
        Time.timeScale = 1f;

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance
                .LoadStoryScene(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager
                .LoadScene(sceneName);
        }
    }
}