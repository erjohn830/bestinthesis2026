using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UIStoryManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> scenes;

    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject playButton;

    private int currentIndex = 0;

    void Start()
    {
        {
            if (nextButton == null)
                Debug.LogError("Next Button is NOT assigned!");

            if (playButton == null)
                Debug.LogError("Play Button is NOT assigned!");

            UpdateScene();
        }
    }

    public void Next()
    {
        if (currentIndex < scenes.Count - 1)
        {
            currentIndex++;
            UpdateScene();
        }
    }

    void UpdateScene()
    {
        // Disable all scenes first
        for (int i = 0; i < scenes.Count; i++)
        {
            scenes[i].SetActive(i == currentIndex);
        }

        // If last page
        if (currentIndex == scenes.Count - 1)
        {
            nextButton.SetActive(false);
            playButton.SetActive(true);
        }
        else
        {
            nextButton.SetActive(true);
            playButton.SetActive(false);
        }
    }

    public void Previous()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateScene();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Luksong-Baka");
    }

    public void QuitStory()
    {
        SceneManager.LoadScene("Mainmenu");
    }
}