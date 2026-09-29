using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PatinteroMenuUI : MonoBehaviour
{
    // =========================================================
    // FUSION
    // =========================================================
    [Header("FUSION")]
    [SerializeField]
    private PatinteroFusionManager fusionManager;
    // =========================================================
    // MAIN PANELS
    // =========================================================
    [Header("MAIN PANELS")]
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
    // CREATE ROOM
    // =========================================================
    [Header("CREATE ROOM")]
    [SerializeField]
    private TMP_InputField serverNameInput;
    [SerializeField]
    private TMP_Dropdown minimumPlayersDropdown;
    [SerializeField]
    private TMP_Dropdown maximumPlayersDropdown;
    [SerializeField]
    private TMP_Text createStatusText;
    // =========================================================
    // JOIN ROOM
    // =========================================================
    [Header("JOIN ROOM")]
    [SerializeField]
    private Transform roomListContent;
    [SerializeField]
    private GameObject roomListItemPrefab;
    [SerializeField]
    private TMP_Text noRoomsText;
    [SerializeField]
    private TMP_Text joinStatusText;
    // =========================================================
    // WAITING ROOM INFO
    // =========================================================
    [Header("WAITING ROOM")]
    [SerializeField]
    private TMP_Text roomNameText;
    [SerializeField]
    private TMP_Text playerCountText;
    [SerializeField]
    private TMP_Text minimumPlayersText;
    [SerializeField]
    private TMP_Text maximumPlayersText;
    [SerializeField]
    private TMP_Text lobbyStatusText;
    // =========================================================
    // WAITING ROOM PLAYER ROWS
    // =========================================================
    [Header("WAITING ROOM PLAYER ROWS")]
    [SerializeField]
    private PatinteroPlayerRowUI[] teamARows;
    [SerializeField]
    private PatinteroPlayerRowUI[] teamBRows;
    // =========================================================
    // CHARACTER SPRITES
    // =========================================================
    [Header("WAITING ROOM CHARACTER SPRITES")]
    [SerializeField]
    private Sprite[] characterSprites;
    // =========================================================
    // READY STATUS SPRITES
    // =========================================================
    [Header("WAITING ROOM READY SPRITES")]
    [SerializeField]
    private Sprite readySprite;
    [SerializeField]
    private Sprite notReadySprite;
    // =========================================================
    // HOST / PLAYER CONTROLS
    // =========================================================
    [Header("HOST / PLAYER CONTROLS")]
    [SerializeField]
    private GameObject hostControls;
    [SerializeField]
    private GameObject playerControls;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button readyButton;
    // =========================================================
    // CHARACTER SELECTION
    // =========================================================
    [Header("CHARACTER SELECTION")]
    [SerializeField]
    private TMP_Text selectedCharacterText;
    // =========================================================
    // ROOM LIST
    // =========================================================
    private readonly List<GameObject>
        spawnedRoomItems =
            new List<GameObject>();
    // =========================================================
    // START
    // =========================================================
    private void Start()
    {
        FindFusionManager();
        // Already inside a room?
        if (
            fusionManager != null &&
            fusionManager.IsInSession)
        {
            StartCoroutine(
                RestoreRoomUI()
            );
        }
        else
        {
            HideLobbySubPanels();
        }
        UpdateSelectedCharacterText();
    }
    // =========================================================
    // RESTORE ROOM
    // =========================================================
    private IEnumerator RestoreRoomUI()
    {
        float timeout = 10f;
        while (
            PatinteroLobbyPlayer.Local == null &&
            timeout > 0f)
        {
            timeout -=
                Time.unscaledDeltaTime;
            yield return null;
        }
        if (
            PatinteroLobbyPlayer.Local != null &&
            PatinteroLobbyPlayer
                .Local
                .CharacterIndex < 0)
        {
            ShowCharacterSelection();
        }
        else
        {
            ShowWaitingRoom();
        }
    }
    // =========================================================
    // UPDATE
    // =========================================================
    private void Update()
    {
        if (
            waitingRoomPanel != null &&
            waitingRoomPanel.activeInHierarchy)
        {
            UpdateWaitingRoomInfo();
            RefreshWaitingRoomPlayers();
            UpdateWaitingRoomControls();
        }
        UpdateSelectedCharacterText();
    }
    // =========================================================
    // FIND FUSION MANAGER
    // =========================================================
    private void FindFusionManager()
    {
        if (fusionManager != null)
        {
            return;
        }
        fusionManager =
            PatinteroFusionManager.Instance;
        if (fusionManager == null)
        {
            fusionManager =
                FindFirstObjectByType<
                    PatinteroFusionManager>();
        }
    }
    // =========================================================
    // HIDE LOBBY SUB PANELS
    // =========================================================
    private void HideLobbySubPanels()
    {
        SetPanel(
            lobbyPanel,
            false
        );
        SetPanel(
            createLobbyPanel,
            false
        );
        SetPanel(
            joinLobbyPanel,
            false
        );
        SetPanel(
            waitingRoomPanel,
            false
        );
        SetPanel(
            inviteFriendsPanel,
            false
        );
        SetPanel(
            incomingInvitePanel,
            false
        );
        SetPanel(
            characterSelectionPanel,
            false
        );
    }
    // =========================================================
    // SHOW MAIN PATINTERO LOBBY
    // =========================================================
    public void ShowPatinteroLobby()
    {
        FindFusionManager();
        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(
                false
            );
        }
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                true
            );
        }
        HideLobbySubPanels();
        SetPanel(
            lobbyPanel,
            true
        );
        Debug.Log(
            "PATINTERO LOBBY OPENED"
        );
    }
    // =========================================================
    // SHOW CREATE LOBBY
    // =========================================================
    public void ShowCreateLobby()
    {
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                true
            );
        }
        HideLobbySubPanels();
        SetPanel(
            createLobbyPanel,
            true
        );
        if (createStatusText != null)
        {
            createStatusText.text =
                "";
        }
        Debug.Log(
            "CREATE LOBBY OPENED"
        );
    }
    // =========================================================
    // SHOW JOIN LOBBY
    // =========================================================
    public void ShowJoinLobby()
    {
        FindFusionManager();
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                true
            );
        }
        HideLobbySubPanels();
        SetPanel(
            joinLobbyPanel,
            true
        );
        if (joinStatusText != null)
        {
            joinStatusText.text =
                "Searching for rooms...";
        }
        if (noRoomsText != null)
        {
            noRoomsText
                .gameObject
                .SetActive(false);
        }
        if (fusionManager == null)
        {
            ShowJoinStatus(
                "Fusion Manager was not found."
            );
            return;
        }
        fusionManager
            .OpenRoomBrowser();
    }
    // =========================================================
    // SESSION JOINED
    // Called after CREATE or JOIN succeeds
    // =========================================================
    public void OnSessionJoined()
    {
        Debug.Log(
            "SESSION JOINED - OPENING CHARACTER SELECTION"
        );
        StartCoroutine(
            OpenCharacterSelectionAfterJoiningRoom()
        );
    }
    // =========================================================
    // FIRST CHARACTER SELECTION
    // =========================================================
    private IEnumerator
        OpenCharacterSelectionAfterJoiningRoom()
    {
        float timeout = 10f;
        // Wait until Fusion creates
        // this device's lobby player.
        while (
            PatinteroLobbyPlayer.Local == null &&
            timeout > 0f)
        {
            timeout -=
                Time.unscaledDeltaTime;
            yield return null;
        }
        if (
            PatinteroLobbyPlayer.Local == null)
        {
            Debug.LogError(
                "Could not find local PatinteroLobbyPlayer."
            );
            ShowWaitingRoom();
            yield break;
        }
        // Already selected a character?
        if (
            PatinteroLobbyPlayer
                .Local
                .CharacterIndex >= 0)
        {
            ShowWaitingRoom();
            yield break;
        }
        // First time entering room.
        ShowCharacterSelection();
    }
    // =========================================================
    // INTERNAL CHARACTER PANEL
    // =========================================================
    private void ShowCharacterSelection()
    {
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                true
            );
        }
        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(
                false
            );
        }
        SetPanel(
            lobbyPanel,
            false
        );
        SetPanel(
            createLobbyPanel,
            false
        );
        SetPanel(
            joinLobbyPanel,
            false
        );
        SetPanel(
            waitingRoomPanel,
            false
        );
        SetPanel(
            inviteFriendsPanel,
            false
        );
        SetPanel(
            incomingInvitePanel,
            false
        );
        if (characterSelectionPanel != null)
        {
            characterSelectionPanel
                .SetActive(true);
        }
        else
        {
            Debug.LogError(
                "CharacterSelectionPanel is not assigned."
            );
            return;
        }
        UpdateSelectedCharacterText();
        Debug.Log(
            "CHARACTER SELECTION OPENED"
        );
    }
    // =========================================================
    // CHANGE CHARACTER FROM WAITING ROOM
    // =========================================================
    public void OpenCharacterSelection()
    {
        if (
            PatinteroLobbyPlayer.Local == null)
        {
            SetLobbyStatus(
                "Player is not ready yet."
            );
            return;
        }
        ShowCharacterSelection();
    }
    // =========================================================
    // CANCEL CHARACTER SELECTION
    // =========================================================
    public void CancelCharacterSelection()
    {
        ShowWaitingRoom();
    }
    // =========================================================
    // CONFIRM CHARACTER
    // =========================================================
    public void ConfirmCharacterSelection()
    {
        int index =
            PatinteroLocalSelection
                .CharacterIndex;
        if (
            index < 0 ||
            index > 9)
        {
            if (selectedCharacterText != null)
            {
                selectedCharacterText.text =
                    "Selected: None";
            }
            Debug.LogWarning(
                "SELECT A CHARACTER FIRST."
            );
            return;
        }
        if (
            PatinteroLobbyPlayer.Local == null)
        {
            Debug.LogWarning(
                "Local lobby player is not ready yet."
            );
            return;
        }
        // Send character to Fusion.
        PatinteroLobbyPlayer
            .Local
            .RequestCharacter(
                index
            );
        Debug.Log(
            "CHARACTER CONFIRMED: " +
            GetCharacterName(index) +
            " | INDEX = " +
            index
        );
        ShowWaitingRoom();
    }
    // =========================================================
    // SHOW WAITING ROOM
    // =========================================================
    public void ShowWaitingRoom()
    {
        FindFusionManager();
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                true
            );
        }
        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(
                false
            );
        }
        SetPanel(
            lobbyPanel,
            false
        );
        SetPanel(
            createLobbyPanel,
            false
        );
        SetPanel(
            joinLobbyPanel,
            false
        );
        SetPanel(
            inviteFriendsPanel,
            false
        );
        SetPanel(
            incomingInvitePanel,
            false
        );
        SetPanel(
            characterSelectionPanel,
            false
        );
        if (waitingRoomPanel != null)
        {
            waitingRoomPanel.SetActive(
                true
            );
        }
        else
        {
            Debug.LogError(
                "WaitingRoomPanel is not assigned!"
            );
            return;
        }
        UpdateWaitingRoomInfo();
        RefreshWaitingRoomPlayers();
        UpdateWaitingRoomControls();
        Debug.Log(
            "WAITING ROOM OPENED"
        );
    }
    // =========================================================
    // CREATE SERVER
    // =========================================================
    public void CreateServer()
    {
        FindFusionManager();
        if (fusionManager == null)
        {
            ShowCreateStatus(
                "Fusion Manager was not found."
            );
            return;
        }
        string serverName =
            serverNameInput != null
            ? serverNameInput.text.Trim()
            : "";
        if (
            string.IsNullOrWhiteSpace(
                serverName))
        {
            ShowCreateStatus(
                "Enter a server name."
            );
            return;
        }
        int minimumPlayers =
            GetDropdownNumber(
                minimumPlayersDropdown,
                2
            );
        int maximumPlayers =
            GetDropdownNumber(
                maximumPlayersDropdown,
                2
            );
        if (minimumPlayers < 1)
        {
            minimumPlayers = 1;
        }
        if (
            maximumPlayers <
            minimumPlayers)
        {
            maximumPlayers =
                minimumPlayers;
        }
        ShowCreateStatus(
            "Creating " +
            serverName +
            "..."
        );
        fusionManager.CreateServer(
            serverName,
            minimumPlayers,
            maximumPlayers
        );
    }
    // =========================================================
    // JOIN ROOM
    // =========================================================
    public void JoinRoom(
        string sessionName)
    {
        JoinSelectedRoom(
            sessionName
        );
    }
    // =========================================================
    // JOIN SELECTED ROOM
    // =========================================================
    public void JoinSelectedRoom(
        string sessionName)
    {
        FindFusionManager();
        if (fusionManager == null)
        {
            ShowJoinStatus(
                "Fusion Manager was not found."
            );
            return;
        }
        if (
            string.IsNullOrWhiteSpace(
                sessionName))
        {
            ShowJoinStatus(
                "Invalid room."
            );
            return;
        }
        ShowJoinStatus(
            "Joining " +
            sessionName +
            "..."
        );
        fusionManager.JoinSelectedRoom(
            sessionName
        );
    }
    // =========================================================
    // REFRESH ROOM LIST
    // =========================================================
    public void RefreshRoomList(
        List<SessionInfo> sessions)
    {
        ClearRoomList();
        bool hasRooms =
            sessions != null &&
            sessions.Count > 0;
        if (noRoomsText != null)
        {
            noRoomsText
                .gameObject
                .SetActive(
                    !hasRooms
                );
        }
        if (!hasRooms)
        {
            if (joinStatusText != null)
            {
                joinStatusText.text =
                    "";
            }
            return;
        }
        if (
            roomListContent == null ||
            roomListItemPrefab == null)
        {
            Debug.LogWarning(
                "Room List Content or Room List Item Prefab is not assigned."
            );
            return;
        }
        foreach (
            SessionInfo session
            in sessions)
        {
            GameObject item =
                Instantiate(
                    roomListItemPrefab,
                    roomListContent
                );
            spawnedRoomItems.Add(
                item
            );
            TMP_Text[] texts =
                item
                    .GetComponentsInChildren<
                        TMP_Text>(
                            true
                        );
            if (texts.Length > 0)
            {
                texts[0].text =
                    session.Name;
            }
            if (texts.Length > 1)
            {
                texts[1].text =
                    session.PlayerCount +
                    " / " +
                    session.MaxPlayers;
            }
            Button button =
                item
                    .GetComponentInChildren<
                        Button>(
                            true
                        );
            if (button != null)
            {
                string roomName =
                    session.Name;
                button.onClick
                    .RemoveAllListeners();
                button.onClick
                    .AddListener(
                        () =>
                        {
                            JoinSelectedRoom(
                                roomName
                            );
                        }
                    );
            }
        }
        if (joinStatusText != null)
        {
            joinStatusText.text =
                "";
        }
    }
    // =========================================================
    // CLEAR ROOM LIST
    // =========================================================
    private void ClearRoomList()
    {
        for (
            int i =
                spawnedRoomItems.Count - 1;
            i >= 0;
            i--)
        {
            if (
                spawnedRoomItems[i] != null)
            {
                Destroy(
                    spawnedRoomItems[i]
                );
            }
        }
        spawnedRoomItems.Clear();
    }
    // =========================================================
    // WAITING ROOM INFO
    // =========================================================
    private void UpdateWaitingRoomInfo()
    {
        FindFusionManager();
        if (
            fusionManager == null ||
            fusionManager.Runner == null ||
            !fusionManager.Runner.IsRunning ||
            !fusionManager
                .Runner
                .SessionInfo
                .IsValid)
        {
            return;
        }
        NetworkRunner runner =
            fusionManager.Runner;
        if (roomNameText != null)
        {
            roomNameText.text =
                "ROOM: " +
                runner.SessionInfo.Name;
        }
        if (playerCountText != null)
        {
            playerCountText.text =
                "PLAYERS: " +
                runner
                    .SessionInfo
                    .PlayerCount +
                " / " +
                runner
                    .SessionInfo
                    .MaxPlayers;
        }
        if (minimumPlayersText != null)
        {
            minimumPlayersText.text =
                "MINIMUM: " +
                fusionManager
                    .MinimumPlayers;
        }
        if (maximumPlayersText != null)
        {
            maximumPlayersText.text =
                "MAXIMUM: " +
                fusionManager
                    .MaximumPlayers;
        }
    }
    // =========================================================
    // REFRESH WAITING ROOM PLAYERS
    // =========================================================
    private void RefreshWaitingRoomPlayers()
    {
        List<PatinteroLobbyPlayer> allPlayers =
            PatinteroLobbyPlayer
                .All
                .Where(
                    player =>
                        player != null &&
                        player.Object != null &&
                        player.Object.IsValid
                )
                .OrderBy(
                    player =>
                        player
                            .Object
                            .InputAuthority
                            .PlayerId
                )
                .ToList();
        List<PatinteroLobbyPlayer> teamA =
            allPlayers
                .Where(
                    player =>
                        player.TeamId == 0
                )
                .OrderBy(
                    player =>
                        player.TeamIndex
                )
                .ToList();
        List<PatinteroLobbyPlayer> teamB =
            allPlayers
                .Where(
                    player =>
                        player.TeamId == 1
                )
                .OrderBy(
                    player =>
                        player.TeamIndex
                )
                .ToList();
        FillPlayerRows(
            teamARows,
            teamA
        );
        FillPlayerRows(
            teamBRows,
            teamB
        );
    }
    // =========================================================
    // FILL PLAYER ROWS
    // =========================================================
    private void FillPlayerRows(
        PatinteroPlayerRowUI[] rows,
        List<PatinteroLobbyPlayer> players)
    {
        if (rows == null)
        {
            return;
        }
        for (
            int i = 0;
            i < rows.Length;
            i++)
        {
            PatinteroPlayerRowUI row =
                rows[i];
            if (row == null)
            {
                continue;
            }
            if (i >= players.Count)
            {
                row.ShowEmpty();
                continue;
            }
            PatinteroLobbyPlayer player =
                players[i];
            Sprite characterSprite =
                null;
            int characterIndex =
                player.CharacterIndex;
            if (
                characterSprites != null &&
                characterIndex >= 0 &&
                characterIndex <
                    characterSprites.Length)
            {
                characterSprite =
                    characterSprites[
                        characterIndex
                    ];
            }
            row.ShowPlayer(
                player.PlayerName.ToString(),
                characterSprite,
                player.IsHost,
                player.IsReady,
                readySprite,
                notReadySprite
            );
        }
    }
    // =========================================================
    // HOST / CLIENT CONTROLS
    // =========================================================
    private void UpdateWaitingRoomControls()
    {
        PatinteroLobbyPlayer localPlayer =
            PatinteroLobbyPlayer.Local;
        if (localPlayer == null)
        {
            if (hostControls != null)
            {
                hostControls.SetActive(
                    false
                );
            }
            if (playerControls != null)
            {
                playerControls.SetActive(
                    false
                );
            }
            return;
        }
        bool isHost =
            localPlayer.IsHost;
        // =========================================
        // HOST
        // =========================================
        if (hostControls != null)
        {
            hostControls.SetActive(
                isHost
            );
        }
        // =========================================
        // CLIENT
        // =========================================
        if (playerControls != null)
        {
            playerControls.SetActive(
                !isHost
            );
        }
        // =========================================
        // READY
        // =========================================
        if (readyButton != null)
        {
            readyButton
                .gameObject
                .SetActive(
                    !isHost
                );
            if (!isHost)
            {
                readyButton.interactable =
                    localPlayer
                        .CharacterIndex >= 0;
            }
        }
        // =========================================
        // START
        // =========================================
        if (startButton != null)
        {
            startButton
                .gameObject
                .SetActive(
                    isHost
                );
            if (!isHost)
            {
                startButton.interactable =
                    false;
                return;
            }
            FindFusionManager();
            if (fusionManager == null)
            {
                startButton.interactable =
                    false;
                return;
            }
            string message;
            bool canStart =
                fusionManager
                    .CanStartMatch(
                        out message
                    );
            startButton.interactable =
                canStart;
            if (lobbyStatusText != null)
            {
                if (canStart)
                {
                    lobbyStatusText.text =
                        "All players are ready! Press START.";
                }
                else
                {
                    lobbyStatusText.text =
                        message;
                }
            }
        }
    }
    // =========================================================
    // UPDATE CHARACTER TEXT
    // =========================================================
    private void UpdateSelectedCharacterText()
    {
        if (selectedCharacterText == null)
        {
            return;
        }
        int index =
            PatinteroLocalSelection
                .CharacterIndex;
        if (
            index < 0 ||
            index > 9)
        {
            selectedCharacterText.text =
                "Selected: None";
            return;
        }
        selectedCharacterText.text =
            "Selected: " +
            GetCharacterName(index);
    }
    // =========================================================
    // CHARACTER NAMES
    // =========================================================
    private string GetCharacterName(
        int index)
    {
        switch (index)
        {
            case 0:
                return "Angelo";
            case 1:
                return "Jose";
            case 2:
                return "Juan";
            case 3:
                return "Liam";
            case 4:
                return "Lisa";
            case 5:
                return "Lolo";
            case 6:
                return "Maria";
            case 7:
                return "Miguel";
            case 8:
                return "Pedro";
            case 9:
                return "Robert";
            default:
                return "None";
        }
    }
    // =========================================================
    // READY
    // =========================================================
    public void ToggleReady()
    {
        PatinteroLobbyPlayer localPlayer =
            PatinteroLobbyPlayer.Local;
        if (localPlayer == null)
        {
            SetLobbyStatus(
                "Lobby player is not ready yet."
            );
            return;
        }
        // Host does NOT Ready.
        if (localPlayer.IsHost)
        {
            SetLobbyStatus(
                "Host does not need to press READY."
            );
            return;
        }
        if (
            localPlayer.CharacterIndex < 0)
        {
            SetLobbyStatus(
                "Choose a character first."
            );
            return;
        }
        localPlayer.ToggleReady();
    }
    // =========================================================
    // TEAM A
    // =========================================================
    public void SelectTeamA()
    {
        PatinteroLocalSelection
            .SetTeam(0);
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            PatinteroLobbyPlayer
                .Local
                .RequestTeam(0);
        }
        Debug.Log(
            "TEAM A SELECTED"
        );
    }
    // =========================================================
    // TEAM B
    // =========================================================
    public void SelectTeamB()
    {
        PatinteroLocalSelection
            .SetTeam(1);
        if (
            PatinteroLobbyPlayer.Local != null)
        {
            PatinteroLobbyPlayer
                .Local
                .RequestTeam(1);
        }
        Debug.Log(
            "TEAM B SELECTED"
        );
    }
    // =========================================================
    // START MATCH
    // =========================================================
    public void StartMatch()
    {
        FindFusionManager();
        if (fusionManager == null)
        {
            SetLobbyStatus(
                "Fusion Manager was not found."
            );
            return;
        }
        string message;
        if (
            !fusionManager.CanStartMatch(
                out message))
        {
            SetLobbyStatus(
                message
            );
            Debug.LogWarning(
                "START BLOCKED: " +
                message
            );
            return;
        }
        fusionManager
            .StartPatinteroMatch();
    }
    // =========================================================
    // QUIT ROOM
    // =========================================================
    public void QuitRoom()
    {
        FindFusionManager();
        if (fusionManager == null)
        {
            ShowPatinteroLobby();
            return;
        }
        fusionManager
            .QuitCurrentRoom();
    }
    // =========================================================
    // CLOSE ROOM BROWSER
    // =========================================================
    public void CloseRoomBrowser()
    {
        FindFusionManager();
        if (fusionManager != null)
        {
            fusionManager
                .CloseRoomBrowser();
        }
        else
        {
            ShowPatinteroLobby();
        }
    }
    // =========================================================
    // CLOSE JOIN LOBBY
    // =========================================================
    public void CloseJoinLobby()
    {
        FindFusionManager();
        if (fusionManager != null)
        {
            fusionManager
                .CloseRoomBrowser();
            return;
        }
        ShowPatinteroLobby();
    }
    // =========================================================
    // STATUS
    // =========================================================
    public void SetLobbyStatus(
        string message)
    {
        if (lobbyStatusText != null)
        {
            lobbyStatusText.text =
                message;
        }
        Debug.Log(
            "LOBBY STATUS: " +
            message
        );
    }
    public void ShowCreateStatus(
        string message)
    {
        if (createStatusText != null)
        {
            createStatusText.text =
                message;
        }
        Debug.Log(
            "CREATE STATUS: " +
            message
        );
    }
    public void ShowJoinStatus(
        string message)
    {
        if (joinStatusText != null)
        {
            joinStatusText.text =
                message;
        }
        Debug.Log(
            "JOIN STATUS: " +
            message
        );
    }
    public void ShowError(
        string message)
    {
        if (lobbyStatusText != null)
        {
            lobbyStatusText.text =
                message;
        }
        if (joinStatusText != null)
        {
            joinStatusText.text =
                message;
        }
        if (createStatusText != null)
        {
            createStatusText.text =
                message;
        }
        Debug.LogError(
            message
        );
    }
    // =========================================================
    // DROPDOWN
    // =========================================================
    private int GetDropdownNumber(
        TMP_Dropdown dropdown,
        int fallback)
    {
        if (dropdown == null)
        {
            return fallback;
        }
        if (
            dropdown.options == null ||
            dropdown.options.Count == 0)
        {
            return fallback;
        }
        string text =
            dropdown
                .options[
                    dropdown.value
                ]
                .text;
        if (
            int.TryParse(
                text,
                out int number))
        {
            return number;
        }
        return fallback;
    }
    // =========================================================
    // SAFE PANEL
    // =========================================================
    private void SetPanel(
        GameObject panel,
        bool active)
    {
        if (panel != null)
        {
            panel.SetActive(
                active
            );
        }
    }
    // =========================================================
    // EXIT TO GAME SELECTION
    // =========================================================
    public void ExitToGameSelection()
    {
        HideLobbySubPanels();
        if (sharedLobbyPanel != null)
        {
            sharedLobbyPanel.SetActive(
                false
            );
        }
        if (gameSelectionPanel != null)
        {
            gameSelectionPanel.SetActive(
                true
            );
        }
        Debug.Log(
            "RETURNED TO MULTIPLAYER GAME SELECTION"
        );
    }
}   