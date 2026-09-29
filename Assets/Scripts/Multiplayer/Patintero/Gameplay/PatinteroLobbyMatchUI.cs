using UnityEngine;
using TMPro;

public class PatinteroLobbyMatchUI : MonoBehaviour
{
    // =========================================================
    // PANELS
    // =========================================================

    [Header("LOBBY PANELS")]

    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject createLobbyPanel;

    [SerializeField]
    private GameObject joinLobbyPanel;

    [SerializeField]
    private GameObject waitingRoomPanel;

    [SerializeField]
    private GameObject characterSelectionPanel;


    // =========================================================
    // OPTIONAL TEXT
    // =========================================================

    [Header("OPTIONAL TEXT")]

    [SerializeField]
    private TMP_Text lobbyStatusText;

    [SerializeField]
    private TMP_Text serverNameText;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Do not automatically change panels here.
        // Your existing PatinteroMenuUI can control
        // the initial screen.

        Debug.Log(
            "PatinteroLobbyMatchUI ready."
        );
    }


    // =========================================================
    // SHOW MAIN PATINTERO LOBBY
    // =========================================================

    public void ShowLobbyPanel()
    {
        HideAllPanels();

        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }

        SetStatus("");
    }


    // =========================================================
    // SHOW CREATE LOBBY
    // =========================================================

    public void ShowCreateLobbyPanel()
    {
        HideAllPanels();

        if (createLobbyPanel != null)
        {
            createLobbyPanel.SetActive(true);
        }

        SetStatus(
            "Create a Patintero room."
        );
    }


    // =========================================================
    // SHOW JOIN LOBBY
    // =========================================================

    public void ShowJoinLobbyPanel()
    {
        HideAllPanels();

        if (joinLobbyPanel != null)
        {
            joinLobbyPanel.SetActive(true);
        }

        SetStatus(
            "Choose a room to join."
        );
    }


    // =========================================================
    // SHOW WAITING ROOM
    // =========================================================

    public void ShowWaitingRoomPanel()
    {
        HideAllPanels();

        if (waitingRoomPanel != null)
        {
            waitingRoomPanel.SetActive(true);
        }

        SetStatus(
            "Waiting for players..."
        );
    }


    // =========================================================
    // SHOW CHARACTER SELECTION
    // =========================================================

    public void ShowCharacterSelectionPanel()
    {
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(true);
        }
    }


    // =========================================================
    // CLOSE CHARACTER SELECTION
    // =========================================================

    public void CloseCharacterSelectionPanel()
    {
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }
    }


    // =========================================================
    // BACK TO LOBBY
    // =========================================================

    public void BackToLobby()
    {
        ShowLobbyPanel();
    }


    // =========================================================
    // BACK TO WAITING ROOM
    // =========================================================

    public void BackToWaitingRoom()
    {
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }

        ShowWaitingRoomPanel();
    }


    // =========================================================
    // SERVER NAME
    // =========================================================

    public void SetServerName(
        string serverName)
    {
        if (serverNameText == null)
        {
            return;
        }


        if (string.IsNullOrWhiteSpace(
            serverName))
        {
            serverNameText.text =
                "Server";
        }
        else
        {
            serverNameText.text =
                serverName;
        }
    }


    // =========================================================
    // STATUS
    // =========================================================

    public void SetStatus(
        string message)
    {
        if (lobbyStatusText != null)
        {
            lobbyStatusText.text =
                message;
        }
    }


    // =========================================================
    // HIDE ALL MAIN PANELS
    // =========================================================

    private void HideAllPanels()
    {
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(false);
        }


        if (createLobbyPanel != null)
        {
            createLobbyPanel.SetActive(false);
        }


        if (joinLobbyPanel != null)
        {
            joinLobbyPanel.SetActive(false);
        }


        if (waitingRoomPanel != null)
        {
            waitingRoomPanel.SetActive(false);
        }


        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }
    }
}