using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogTypewriter : MonoBehaviour
{
    public TextMeshProUGUI dialogText;

    public GameObject nextButton;   
    public GameObject playButton;   

    public string[] sentences;
    public float typingSpeed = 0.05f;

    public string sceneToLoad;   // ← ONLY ONE

    private int index = 0;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (dialogText == null)
        {
            Debug.LogError("DialogText is not assigned!");
            return;
        }

        if (sentences == null || sentences.Length == 0)
        {
            Debug.LogWarning("No sentences assigned!");
            return;
        }

        playButton.SetActive(false);
        nextButton.SetActive(true);

        typingCoroutine = StartCoroutine(TypeSentence());
    }

    IEnumerator TypeSentence()
    {
        dialogText.text = "";

        foreach (char letter in sentences[index])
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        if (index == sentences.Length - 1)
        {
            nextButton.SetActive(false);
            playButton.SetActive(true);
        }
    }

    public void NextSentence()
    {
        if (index < sentences.Length - 1)
        {
            index++;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeSentence());
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}