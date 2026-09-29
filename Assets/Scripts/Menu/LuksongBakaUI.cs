using UnityEngine;
using UnityEngine.SceneManagement;

public class LuksongBakaUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Playgame()
    {
        SceneManager.LoadScene("game");
    }
    public void Patintero()
    {
        SceneManager.LoadScene("multiplayer");
    }
    

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
    }

    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
    }
}