using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SipaInstructionManager : MonoBehaviour
{
    [Header("MAIN")]
    [SerializeField] private GameObject instructionPanel;

    [Header("TEXT")]
    [SerializeField] private TMP_Text instructionTitle;
    [SerializeField] private TMP_Text instructionDescription;
    [SerializeField] private TMP_Text pageText;

    [Header("NAVIGATION BUTTONS")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button startButton;

    [Header("WORLD OBJECTS")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject pato;
    [SerializeField] private GameObject ground;

    [Header("GAMEPLAY UI")]
    [SerializeField] private GameObject controllerUI;
    [SerializeField] private GameObject movementButton;
    [SerializeField] private GameObject kickButton;
    [SerializeField] private GameObject scoreUI;

    [Header("INSTRUCTION ARROWS")]
    [SerializeField] private GameObject playerArrow;
    [SerializeField] private GameObject patoArrow;
    [SerializeField] private GameObject movementArrow;
    [SerializeField] private GameObject kickArrow;
    [SerializeField] private GameObject scoreArrow;
    [SerializeField] private GameObject groundArrow;

    private int currentPage = 0;

    private const int TOTAL_PAGES = 6;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        HideAllArrows();

        if (instructionPanel != null)
            instructionPanel.SetActive(false);
    }


    // =====================================================
    // OPEN INSTRUCTIONS
    // =====================================================

    public void OpenInstructions()
    {
        currentPage = 0;

        if (instructionPanel != null)
            instructionPanel.SetActive(true);

        ShowCurrentPage();
    }


    // Compatibility with older SipaManager
    public void BeginInstructions()
    {
        OpenInstructions();
    }


    // =====================================================
    // SHOW PAGE
    // =====================================================

    private void ShowCurrentPage()
    {
        // Reset tutorial objects first.
        HideTutorialObjects();
        HideAllArrows();


        // =================================================
        // PAGE 1 - PLAYER
        // =================================================

        if (currentPage == 0)
        {
            SetInstructionText(
                "MANLALARO",
                "Ito ang iyong manlalaro. " +
                "Kontrolin siya upang sundan ang Pato. " +
                "Kailangan mong pumuwesto nang maayos upang " +
                "masipa ang Pato at mapanatili ito sa ere."
            );

            Show(player);
            Show(playerArrow);
        }


        // =================================================
        // PAGE 2 - PATO
        // =================================================

        else if (currentPage == 1)
        {
            SetInstructionText(
                "PATO",
                "Ito ang Pato. Ito ang pangunahing bagay na " +
                "kailangan mong sipain. Huwag hayaang " +
                "bumagsak ang Pato sa lupa."
            );

            Show(pato);
            Show(patoArrow);
        }


        // =================================================
        // PAGE 3 - MOVEMENT
        // =================================================

        else if (currentPage == 2)
        {
            SetInstructionText(
                "PAGGALAW",
                "Gamitin ang kaliwa at kanang button upang " +
                "igalaw ang manlalaro. Sundan ang direksyon " +
                "ng Pato at pumuwesto sa ilalim nito bago sumipa."
            );

            Show(player);

            // Parent MUST be enabled first.
            Show(controllerUI);

            Show(movementButton);

            // Kick should not appear on movement page.
            Hide(kickButton);

            Show(movementArrow);
        }


        // =================================================
        // PAGE 4 - KICK
        // =================================================

        else if (currentPage == 3)
        {
            SetInstructionText(
                "PAGSIPA",
                "Pindutin ang Kick Button upang sumipa. " +
                "Hintayin na lumapit ang Pato sa paa ng " +
                "manlalaro. Kapag nagtama ang paa at Pato, " +
                "tatalbog muli ang Pato pataas."
            );

            Show(player);
            Show(pato);

            // Parent MUST be active.
            Show(controllerUI);

            // Hide movement controls.
            Hide(movementButton);

            // Show only kick.
            Show(kickButton);

            Show(kickArrow);
        }


        // =================================================
        // PAGE 5 - SCORE
        // =================================================

        else if (currentPage == 4)
        {
            SetInstructionText(
                "ISKOR",
                "Bawat matagumpay na pagsipa sa Pato ay " +
                "magbibigay ng isang puntos. Abutin ang " +
                "target na 10 puntos upang manalo."
            );

            Show(scoreUI);
            Show(scoreArrow);
        }


        // =================================================
        // PAGE 6 - GROUND
        // =================================================

        else if (currentPage == 5)
        {
            SetInstructionText(
                "HUWAG HAYAANG BUMAGSAK",
                "Kapag tumama ang Pato sa lupa bago mo " +
                "maabot ang target na puntos, matatalo ka. " +
                "Panatilihin itong nasa ere hanggang " +
                "makumpleto ang target score."
            );

            Show(pato);
            Show(ground);
            Show(groundArrow);
        }


        UpdatePageUI();
    }


    // =====================================================
    // TEXT
    // =====================================================

    private void SetInstructionText(
        string title,
        string description)
    {
        if (instructionTitle != null)
        {
            instructionTitle.gameObject.SetActive(true);
            instructionTitle.text = title;
        }

        if (instructionDescription != null)
        {
            instructionDescription.gameObject.SetActive(true);
            instructionDescription.text = description;

            // Make absolutely sure text is visible.
            instructionDescription.enabled = true;
        }
    }


    // =====================================================
    // PAGE UI
    // =====================================================

    private void UpdatePageUI()
    {
        if (pageText != null)
        {
            pageText.gameObject.SetActive(true);

            pageText.text =
                "Text: " +
                (currentPage + 1) +
                " / " +
                TOTAL_PAGES;
        }


        if (backButton != null)
        {
            backButton.gameObject.SetActive(
                currentPage > 0
            );
        }


        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(
                currentPage < TOTAL_PAGES - 1
            );
        }


        if (startButton != null)
        {
            startButton.gameObject.SetActive(
                currentPage == TOTAL_PAGES - 1
            );
        }
    }


    // =====================================================
    // NEXT
    // =====================================================

    public void NextPage()
    {
        if (currentPage >= TOTAL_PAGES - 1)
            return;

        currentPage++;

        ShowCurrentPage();
    }


    // =====================================================
    // BACK
    // =====================================================

    public void PreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;

        ShowCurrentPage();
    }


    public void BackPage()
    {
        PreviousPage();
    }


    // =====================================================
    // START GAME
    // =====================================================

    public void StartGame()
    {
        HideAllArrows();

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        // Restore the gameplay objects first.
        RestoreAllTargets();

        // IMPORTANT:
        // Do NOT start gameplay directly.
        // SipaManager must run its countdown first.
        if (SipaManager.Instance != null)
        {
            SipaManager.Instance.StartGameFromInstructions();
        }
        else
        {
            Debug.LogError(
                "SipaInstructionManager: SipaManager.Instance was not found!"
            );
        }
    }


    // =====================================================
    // CLOSE
    // =====================================================

    public void CloseInstructions()
    {
        HideAllArrows();

        if (instructionPanel != null)
            instructionPanel.SetActive(false);

        RestoreAllTargets();
    }


    // =====================================================
    // HIDE TUTORIAL OBJECTS
    // =====================================================

    private void HideTutorialObjects()
    {
        Hide(player);
        Hide(pato);
        Hide(ground);

        Hide(scoreUI);

        // IMPORTANT:
        // Disable the entire controls parent.
        Hide(controllerUI);
    }


    // =====================================================
    // ARROWS
    // =====================================================

    private void HideAllArrows()
    {
        Hide(playerArrow);
        Hide(patoArrow);
        Hide(movementArrow);
        Hide(kickArrow);
        Hide(scoreArrow);
        Hide(groundArrow);
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private void Show(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }


    private void Hide(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(false);
    }


    // =====================================================
    // RESTORE GAME OBJECTS
    // =====================================================

    public void RestoreAllTargets()
    {
        Show(player);
        Show(pato);
        Show(ground);

        Show(scoreUI);

        Show(controllerUI);
        Show(movementButton);
        Show(kickButton);

        HideAllArrows();
    }
}