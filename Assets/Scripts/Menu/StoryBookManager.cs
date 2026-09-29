using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class StoryBookManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;

        [TextArea(2, 5)]
        public string dialogue;
    }

    [System.Serializable]
    public class StoryPage
    {
        public GameObject page;
        public DialogueLine[] dialogues;
    }


    [Header("STORY PAGES")]
    public StoryPage[] pages;


    [Header("DIALOGUE UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;


    [Header("TYPEWRITER")]
    public float typingSpeed = 0.04f;


    [Header("INTRO BLACK PANELS")]
    public RectTransform topBlackPanel;
    public RectTransform bottomBlackPanel;

    public float blackPanelMoveDistance = 500f;
    public float blackPanelMoveDuration = 1f;


    [Header("END")]
    public GameObject playButton;

    public string gameSceneName =
        "HulihinBiik";


    private int currentPage = 0;
    private int currentDialogue = 0;

    private Coroutine typingCoroutine;

    private bool isTyping = false;

    private string fullText = "";


    // =========================================
    // START
    // =========================================

    void Start()
    {
        Time.timeScale = 1f;

        if (playButton != null)
        {
            playButton.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        StartCoroutine(
            StartStoryRoutine()
        );
    }


    // =========================================
    // STORY INTRO
    // =========================================

    IEnumerator StartStoryRoutine()
    {
        yield return StartCoroutine(
            AnimateBlackPanels()
        );

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowPage(0);
    }


    // =========================================
    // BLACK PANEL ANIMATION
    // =========================================

    IEnumerator AnimateBlackPanels()
{
    if (topBlackPanel == null ||
        bottomBlackPanel == null)
    {
        yield break;
    }

    Vector2 topFinal =
        topBlackPanel.anchoredPosition;

    Vector2 bottomFinal =
        bottomBlackPanel.anchoredPosition;


    Vector2 topStart =
        topFinal +
        new Vector2(
            0f,
            blackPanelMoveDistance
        );

    Vector2 bottomStart =
        bottomFinal -
        new Vector2(
            0f,
            blackPanelMoveDistance
        );


    topBlackPanel.anchoredPosition =
        topStart;

    bottomBlackPanel.anchoredPosition =
        bottomStart;


    float time = 0f;

    while (time <
           blackPanelMoveDuration)
    {
        time +=
            Time.unscaledDeltaTime;

        float t =
            Mathf.Clamp01(
                time /
                blackPanelMoveDuration
            );

        // Smooth Ease In + Ease Out
        t =
            t * t *
            (3f - 2f * t);


        topBlackPanel.anchoredPosition =
            Vector2.Lerp(
                topStart,
                topFinal,
                t
            );

        bottomBlackPanel.anchoredPosition =
            Vector2.Lerp(
                bottomStart,
                bottomFinal,
                t
            );


        yield return null;
    }


    topBlackPanel.anchoredPosition =
        topFinal;

    bottomBlackPanel.anchoredPosition =
        bottomFinal;
}

    // =========================================
    // SHOW PAGE
    // =========================================

    void ShowPage(int pageIndex)
    {
        for (int i = 0;
             i < pages.Length;
             i++)
        {
            if (pages[i].page != null)
            {
                pages[i].page.SetActive(
                    i == pageIndex
                );
            }
        }

        currentPage =
            pageIndex;

        currentDialogue =
            0;

        ShowCurrentDialogue();
    }


    // =========================================
    // SHOW DIALOGUE
    // =========================================

    void ShowCurrentDialogue()
    {
        if (currentPage >=
            pages.Length)
        {
            FinishStory();
            return;
        }


        DialogueLine[] dialogueList =
            pages[currentPage]
            .dialogues;


        if (dialogueList == null ||
            dialogueList.Length == 0)
        {
            GoToNextPage();
            return;
        }


        if (currentDialogue >=
            dialogueList.Length)
        {
            GoToNextPage();
            return;
        }


        DialogueLine line =
            dialogueList[
                currentDialogue
            ];


        if (speakerNameText != null)
        {
            speakerNameText.text =
                line.speaker;
        }


        fullText =
            line.dialogue;


        if (typingCoroutine != null)
        {
            StopCoroutine(
                typingCoroutine
            );
        }


        typingCoroutine =
            StartCoroutine(
                TypeDialogue()
            );
    }


    // =========================================
    // TYPEWRITER
    // =========================================

    IEnumerator TypeDialogue()
    {
        isTyping = true;

        dialogueText.text = "";


        foreach (char letter
                 in fullText)
        {
            dialogueText.text +=
                letter;

            yield return
                new WaitForSecondsRealtime(
                    typingSpeed
                );
        }


        dialogueText.text =
            fullText;

        isTyping = false;

        typingCoroutine = null;
    }


    // =========================================
    // CLICK DIALOGUE
    // =========================================

    public void DialogueClicked()
    {
        // If still typing:
        // show complete sentence.
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(
                    typingCoroutine
                );

                typingCoroutine = null;
            }

            dialogueText.text =
                fullText;

            isTyping = false;

            return;
        }


        // If sentence is already complete:
        // next dialogue.
        currentDialogue++;

        ShowCurrentDialogue();
    }


    // =========================================
    // NEXT PAGE
    // =========================================

    void GoToNextPage()
    {
        currentPage++;


        if (currentPage >=
            pages.Length)
        {
            FinishStory();
            return;
        }


        ShowPage(
            currentPage
        );
    }


    // =========================================
    // FINISH STORY
    // =========================================

    void FinishStory()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(
                typingCoroutine
            );
        }


        isTyping = false;


        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }


        if (playButton != null)
        {
            playButton.SetActive(true);
        }
    }


    // =========================================
    // PLAY GAME
    // =========================================

    public void PlayGame()
    {
        Time.timeScale = 1f;


        if (LoadingScreenManager.Instance
            != null)
        {
            LoadingScreenManager.Instance
                .LoadStoryScene(
                    gameSceneName
                );
        }
        else
        {
            SceneManager.LoadScene(
                gameSceneName
            );
        }
    }
}