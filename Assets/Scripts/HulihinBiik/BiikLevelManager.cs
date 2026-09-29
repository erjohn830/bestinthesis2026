using TMPro;
using UnityEngine;

public class BiikLevelManager : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maximumLevel = 3;

    [Header("References")]
    [SerializeField] private PigAI pigAI;
    [SerializeField] private PlayerBiikMovement playerMovement;
    [SerializeField] private TMP_Text levelText;

    public int CurrentLevel
    {
        get { return currentLevel; }
    }

    public bool IsLastLevel
    {
        get { return currentLevel >= maximumLevel; }
    }

    private void Start()
    {
        ApplyCurrentLevel();
    }

    public void ApplyCurrentLevel()
    {
        if (pigAI != null)
        {
            pigAI.SetDifficulty(currentLevel);
        }

        if (playerMovement != null)
        {
            if (currentLevel == 1)
            {
                playerMovement.SetMaximumSpeed(8f);
            }
            else if (currentLevel == 2)
            {
                playerMovement.SetMaximumSpeed(9f);
            }
            else
            {
                playerMovement.SetMaximumSpeed(10f);
            }
        }

        UpdateLevelText();

        Debug.Log(
            "Hulihin Biik - LEVEL " +
            currentLevel
        );
    }

    public bool GoToNextLevel()
    {
        if (currentLevel >= maximumLevel)
        {
            return false;
        }

        currentLevel++;

        ApplyCurrentLevel();

        return true;
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text =
                "LEVEL " + currentLevel;
        }
    }
}