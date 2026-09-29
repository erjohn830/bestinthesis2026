using UnityEngine;

public class StoryHome : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject savePanel;

    void Start()
    {
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }
    }

    // =========================================
    // HOME BUTTON
    // =========================================
    public void AskToGoHome()
    {
        // DO NOT disable SettingsPanel here.
        // SavePanel is inside SettingsPanel,
        // so the parent must stay active.

        if (savePanel != null)
        {
            savePanel.SetActive(true);
            savePanel.transform.SetAsLastSibling();
        }

        Debug.Log("SAVE GAME PANEL OPENED");
    }

    // =========================================
    // YES = SAVE + HOME
    // =========================================
    public void SaveAndHome()
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
            .SaveAndGoHome();
    }

    // =========================================
    // NO = DON'T SAVE + HOME
    // =========================================
    public void DontSaveAndHome()
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
            .DontSaveAndGoHome();
    }

    // =========================================
    // CANCEL
    // =========================================
    public void CancelSave()
    {
        if (savePanel != null)
        {
            savePanel.SetActive(false);
        }
    }
}