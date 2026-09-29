using TMPro;
using UnityEngine;

public class MultiplayerGameSelector : MonoBehaviour
{
    // =========================================================
    // PANELS
    // =========================================================

    [Header("PANELS")]
    [SerializeField]
    private GameObject gameSelectionPanel;

    [SerializeField]
    private GameObject sharedLobbyPanel;

    [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField]
    private GameObject createLobbyPanel;

    [SerializeField]
    private GameObject joinLobbyPanel;

    [SerializeField]
    private GameObject waitingRoomPanel;

    [SerializeField]
    private GameObject inviteFriendsPanel;

    [SerializeField]
    private GameObject incomingInvitePanel;

    [SerializeField]
    private GameObject characterSelectionPanel;


    // =========================================================
    // GAME TEXTS
    // =========================================================

    [Header("GAME TEXTS")]

    // Optional:
    // PATINTERO / SEKYU BASE / BAMSAK title.
    [SerializeField]
    private TMP_Text selectedGameText;


    // Join Lobby message:
    // "No Patintero rooms available."
    [SerializeField]
    private TMP_Text noRoomsText;


    // Create Lobby message:
    // "No Patintero servers available."
    [SerializeField]
    private TMP_Text noServersText;


    // =========================================================
    // PATINTERO
    // =========================================================

    public void SelectPatintero()
    {
        OpenGameLobby(
            MultiplayerGameType.Patintero
        );
    }


    // =========================================================
    // SEKYU BASE
    // =========================================================

    public void SelectSekyuBase()
    {
        OpenGameLobby(
            MultiplayerGameType.SekyuBase
        );
    }


    // =========================================================
    // BAMSAK
    // =========================================================

    public void SelectBamsak()
    {
        OpenGameLobby(
            MultiplayerGameType.Bamsak
        );
    }


    // =========================================================
    // OPEN GAME LOBBY
    // =========================================================

    private void OpenGameLobby(
        MultiplayerGameType game)
    {
        MultiplayerGameSelection.SelectedGame =
            game;


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "MULTIPLAYER GAME SELECTED: " +
            MultiplayerGameSelection.GetGameName()
        );

        Debug.Log(
            "SCENE: " +
            MultiplayerGameSelection.GetSceneName()
        );

        Debug.Log(
            "================================"
        );


        // -----------------------------------------
        // UPDATE ALL GAME-SPECIFIC TEXT
        // -----------------------------------------

        UpdateGameTexts();


        // -----------------------------------------
        // HIDE GAME SELECTION
        // -----------------------------------------

        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(false);
        }


        // -----------------------------------------
        // SHOW SHARED LOBBY
        // -----------------------------------------

        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(true);
        }


        // Hide all sub-panels first.
        HideOtherLobbyPanels();


        // Show main lobby page.
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }


        Debug.Log(
            "SHARED LOBBY OPENED"
        );
    }


    // =========================================================
    // UPDATE TEXT
    // =========================================================

    public void UpdateGameTexts()
    {
        string gameName =
            MultiplayerGameSelection
                .GetGameName();


        // Optional game title.
        if (selectedGameText != null)
        {
            selectedGameText.text =
                gameName;
        }


        // JOIN ROOM EMPTY MESSAGE.
        if (noRoomsText != null)
        {
            noRoomsText.text =
                MultiplayerGameSelection
                    .GetNoRoomsMessage();
        }


        // CREATE ROOM EMPTY MESSAGE.
        if (noServersText != null)
        {
            noServersText.text =
                MultiplayerGameSelection
                    .GetNoServersMessage();
        }


        Debug.Log(
            "LOBBY TEXT UPDATED: " +
            gameName
        );
    }


    // =========================================================
    // HIDE OTHER PANELS
    // =========================================================

    private void HideOtherLobbyPanels()
    {
        if (createLobbyPanel != null)
            createLobbyPanel.SetActive(false);

        if (joinLobbyPanel != null)
            joinLobbyPanel.SetActive(false);

        if (waitingRoomPanel != null)
            waitingRoomPanel.SetActive(false);

        if (inviteFriendsPanel != null)
            inviteFriendsPanel.SetActive(false);

        if (incomingInvitePanel != null)
            incomingInvitePanel.SetActive(false);

        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);
    }


    // =========================================================
    // BACK TO GAME SELECTION
    // =========================================================

    public void BackToGameSelection()
    {
        MultiplayerGameSelection.Reset();


        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);


        HideOtherLobbyPanels();


        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(false);
        }


        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(true);
        }


        if (selectedGameText != null)
        {
            selectedGameText.text = "";
        }
    }
}