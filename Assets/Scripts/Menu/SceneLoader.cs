using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadStory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LuksongBakaStory");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    public void LoadLuskongBakaGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LuksongBakaGame");
    }

    public void LoadMultiplayer()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("multiplayer");
    }

    public void LoadPantintero()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("PatinteroGame");
    }

    public void LoadSipa()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Sipa");
    }

    public void LoadSipaStory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SipaStory");
    }

    public void LoadPalosebo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Palosebo");
    }

    public void LoadPaloseboStory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("PaloseboStory");
    }

    public void LoadHulihinBiik()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HulihinBiik");
    }

    public void LoadHulihinBiikStory()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HuliinBiikStory");
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Quit Game");
        Application.Quit();
    }
}