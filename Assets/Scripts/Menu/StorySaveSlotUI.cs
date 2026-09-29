using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySaveSlotUI : MonoBehaviour
{
    public TMP_Text gameNameText;
    public TMP_Text percentText;
    public Button continueButton;

    private string saveId;

    public void Setup(StoryProgressManager.StorySaveInfo save)
    {
        saveId = save.id;

        gameObject.SetActive(true);

        if (gameNameText != null)
            gameNameText.text = save.currentGame.ToUpper();

        if (percentText != null)
            percentText.text = save.progressPercent + "%";

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ContinueGame);
        }
    }

    public void HideSlot()
    {
        saveId = "";
        gameObject.SetActive(false);
    }

    private void ContinueGame()
    {
        if (string.IsNullOrEmpty(saveId))
            return;

        if (StoryProgressManager.instance == null)
            return;

        StoryProgressManager.instance.ContinueSave(saveId);
    }
}