using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;



using Firebase.Auth;

using Firebase.Firestore;



using Fusion;

using Fusion.Sockets;



using UnityEngine;

using UnityEngine.SceneManagement;





public class PatinteroFusionManager :

    MonoBehaviour,

    INetworkRunnerCallbacks

{

    public static PatinteroFusionManager Instance;





    // =========================================================

    // NETWORK PREFAB

    // =========================================================



    [Header("NETWORK PREFAB")]



    [SerializeField]

    private NetworkObject lobbyPlayerPrefab;





    // =========================================================

    // SCENES

    // =========================================================



    [Header("SCENE BUILD INDEX")]



    [SerializeField]

    private int mainSceneBuildIndex = 0;



    [SerializeField]

    private int patinteroSceneBuildIndex = 1;





    // =========================================================

    // TESTING

    // =========================================================



    [Header("TESTING")]



    [SerializeField]

    private bool allowTwoPlayerTesting = true;





    // =========================================================

    // FUSION

    // =========================================================



    private NetworkRunner runner;



    private NetworkSceneManagerDefault sceneManager;



    private bool manualShutdown = false;



    private bool browsingRooms = false;





    private List<SessionInfo> cachedSessions =

        new List<SessionInfo>();





    // =========================================================

    // FIREBASE

    // =========================================================



    private FirebaseAuth firebaseAuth;



    private FirebaseFirestore firestore;





    // =========================================================

    // PUBLIC

    // =========================================================



    public NetworkRunner Runner

    {

        get

        {

            return runner;

        }

    }





    public bool IsHost

    {

        get

        {

            return runner != null &&

                   runner.IsRunning &&

                   runner.IsServer;

        }

    }





    public bool IsInSession

    {

        get

        {

            return runner != null &&

                   runner.IsRunning &&

                   runner.IsInSession;

        }

    }





    public string LocalPlayerName

    {

        get;

        private set;

    } = "Player";





    public int MinimumPlayers

    {

        get;

        private set;

    } = 6;





    public int MaximumPlayers

    {

        get;

        private set;

    } = 10;





    public int MainSceneBuildIndex

    {

        get

        {

            return mainSceneBuildIndex;

        }

    }





    public int PatinteroSceneBuildIndex

    {

        get

        {

            return patinteroSceneBuildIndex;

        }

    }





    // =========================================================

    // UNITY

    // =========================================================



    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // This manager must be a ROOT object so it can survive scene loads.
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        DontDestroyOnLoad(gameObject);

        firebaseAuth =
            FirebaseAuth.DefaultInstance;

        firestore =
            FirebaseFirestore.DefaultInstance;

        LocalPlayerName =
            "Player";
    }



    // =========================================================

    // LOAD EXACT FIREBASE PROFILE NAME

    //

    // students/{uid}

    //     username

    // =========================================================



    private async Task LoadExactFirebaseUsername()

    {

        LocalPlayerName =

            "Player";





        if (firebaseAuth == null)

        {

            firebaseAuth =

                FirebaseAuth.DefaultInstance;

        }





        if (firestore == null)

        {

            firestore =

                FirebaseFirestore.DefaultInstance;

        }





        FirebaseUser user =

            firebaseAuth.CurrentUser;





        if (user == null)

        {

            Debug.LogWarning(

                "PATINTERO: No Firebase user is logged in."

            );



            return;

        }





        try

        {

            DocumentSnapshot snapshot =

                await firestore

                .Collection("students")

                .Document(user.UserId)

                .GetSnapshotAsync();





            if (snapshot.Exists)

            {

                Dictionary<string, object> data =

                    snapshot.ToDictionary();





                if (data.TryGetValue(

                    "username",

                    out object usernameValue))

                {

                    if (usernameValue != null)

                    {

                        string username =

                            usernameValue.ToString().Trim();





                        if (!string.IsNullOrWhiteSpace(

                            username))

                        {

                            LocalPlayerName =

                                username;





                            Debug.Log(

                                "PATINTERO PROFILE NAME: " +

                                LocalPlayerName

                            );





                            return;

                        }

                    }

                }

            }





            // Firebase Auth display name is only

            // a backup.

            if (!string.IsNullOrWhiteSpace(

                user.DisplayName))

            {

                LocalPlayerName =

                    user.DisplayName.Trim();

            }





            Debug.Log(

                "PATINTERO FALLBACK NAME: " +

                LocalPlayerName

            );

        }

        catch (Exception exception)

        {

            Debug.LogError(

                "Failed to load Firebase username:\n" +

                exception

            );

        }

    }





    // =========================================================

    // CREATE SERVER

    // =========================================================



    public async void CreateServer(

        string serverName,

        int minimumPlayers,

        int maximumPlayers)

    {

        // Get the exact Profile name first.

        await LoadExactFirebaseUsername();





        if (runner != null)

        {

            Debug.LogWarning(

                "Fusion Runner already exists."

            );



            return;

        }





        if (string.IsNullOrWhiteSpace(

            serverName))

        {

            FindUI()?.ShowCreateStatus(

                "Enter a Server Name."

            );



            return;

        }





        serverName =

            serverName.Trim();





        if (minimumPlayers >

            maximumPlayers)

        {

            FindUI()?.ShowCreateStatus(

                "Minimum cannot be higher than Maximum."

            );



            return;

        }





        MinimumPlayers =

            minimumPlayers;





        MaximumPlayers =

            maximumPlayers;





        CreateNewRunner();





        Dictionary<string, SessionProperty>

            properties =

                new Dictionary<

                    string,

                    SessionProperty>();





        // Selected multiplayer game:
        // 1 = Patintero, 2 = Sekyu Base, 3 = Bamsak
        properties["g"] = GetSelectedGameId();



        properties["min"] =

            minimumPlayers;



        properties["max"] =

            maximumPlayers;





        NetworkSceneInfo sceneInfo =

            CurrentSceneInfo();





        StartGameArgs args =

            new StartGameArgs()

            {

                GameMode =

                    GameMode.Host,



                SessionName =

                    serverName,



                PlayerCount =

                    maximumPlayers,



                IsOpen =

                    true,



                IsVisible =

                    true,



                SessionProperties =

                    properties,



                Scene =

                    sceneInfo,



                SceneManager =

                    sceneManager

            };





        FindUI()?.ShowCreateStatus(

            "Creating Server..."

        );





        Debug.Log(

            "Creating server as: " +

            LocalPlayerName

        );





        StartGameResult result =

            await runner.StartGame(

                args

            );





        if (!result.Ok)

        {

            string error =

                result.ErrorMessage;





            Debug.LogError(

                "Create Server Failed: " +

                error

            );





            DestroyFailedRunner();





            FindUI()?.ShowCreateStatus(

                "Cannot create server:\n" +

                error

            );





            return;

        }





        ReadSessionSettings();





        FindUI()?.OnSessionJoined();

    }





    // =========================================================

    // OPEN ROOM BROWSER

    // =========================================================



    public async void OpenRoomBrowser()

    {

        if (runner != null)

        {

            if (browsingRooms)

            {

                return;

            }





            return;

        }





        CreateNewRunner();





        browsingRooms =

            true;





        StartGameResult result =

            await runner.JoinSessionLobby(

                SessionLobby.ClientServer

            );





        if (!result.Ok)

        {

            string error =

                result.ErrorMessage;





            browsingRooms =

                false;





            DestroyFailedRunner();





            FindUI()?.ShowJoinStatus(

                "Cannot load rooms:\n" +

                error

            );





            return;

        }





        FindUI()?.ShowJoinStatus(

            "Select a server."

        );

    }





    // =========================================================

    // JOIN SELECTED ROOM

    // =========================================================



    public async void JoinSelectedRoom(

        string sessionName)

    {

        // Load exact Firebase Profile username.

        await LoadExactFirebaseUsername();





        if (runner == null)

        {

            FindUI()?.ShowJoinStatus(

                "Room browser is not connected."

            );



            return;

        }





        if (string.IsNullOrWhiteSpace(

            sessionName))

        {

            return;

        }





        browsingRooms =

            false;





        FindUI()?.ShowJoinStatus(

            "Joining " +

            sessionName +

            "..."

        );





        Debug.Log(

            "Joining server as: " +

            LocalPlayerName

        );





        NetworkSceneInfo sceneInfo =

            CurrentSceneInfo();





        StartGameArgs args =

            new StartGameArgs()

            {

                GameMode =

                    GameMode.Client,



                SessionName =

                    sessionName,



                EnableClientSessionCreation =

                    false,



                Scene =

                    sceneInfo,



                SceneManager =

                    sceneManager

            };





        StartGameResult result =

            await runner.StartGame(

                args

            );





        if (!result.Ok)

        {

            string error =

                result.ErrorMessage;





            DestroyFailedRunner();





            FindUI()?.ShowJoinStatus(

                "Cannot join server:\n" +

                error

            );





            return;

        }





        ReadSessionSettings();





        FindUI()?.OnSessionJoined();

    }





    // =========================================================

    // JOIN ROOM FROM FRIEND INVITE

    // =========================================================



    public async void JoinInvitedRoom(

        string sessionName)

    {

        await LoadExactFirebaseUsername();





        if (string.IsNullOrWhiteSpace(

            sessionName))

        {

            FindUI()?.ShowError(

                "Invalid multiplayer invite."

            );



            return;

        }





        sessionName =

            sessionName.Trim();





        // Already inside a Fusion session.

        if (runner != null &&

            runner.IsRunning &&

            runner.IsInSession)

        {

            // Already inside the invited room.

            if (runner.SessionInfo.Name ==

                sessionName)

            {

                FindUI()?.ShowWaitingRoom();



                return;

            }





            FindUI()?.ShowError(

                "Leave your current room before accepting another invite."

            );



            return;

        }





        if (runner == null)

        {

            CreateNewRunner();

        }





        NetworkSceneInfo sceneInfo =

            CurrentSceneInfo();





        StartGameArgs args =

            new StartGameArgs()

            {

                GameMode =

                    GameMode.Client,



                SessionName =

                    sessionName,



                EnableClientSessionCreation =

                    false,



                Scene =

                    sceneInfo,



                SceneManager =

                    sceneManager

            };





        Debug.Log(

            "Joining invited multiplayer room: " +

            sessionName +

            " as " +

            LocalPlayerName

        );





        StartGameResult result =

            await runner.StartGame(

                args

            );





        if (!result.Ok)

        {

            string error =

                result.ErrorMessage;





            DestroyFailedRunner();





            FindUI()?.ShowError(

                "Unable to join invited room:\n" +

                error

            );





            return;

        }





        ReadSessionSettings();





        FindUI()?.OnSessionJoined();

    }





    // =========================================================

    // CLOSE ROOM BROWSER

    // =========================================================



    public async void CloseRoomBrowser()
    {
        if (runner == null)
        {
            browsingRooms = false;
            FindUI()?.ShowPatinteroLobby();
            return;
        }

        // CRITICAL:
        // If we already joined/created a real game session, closing or hiding
        // the room-browser UI must NOT shut down the NetworkRunner.
        if (runner.IsRunning &&
            runner.IsInSession)
        {
            browsingRooms = false;

            Debug.Log(
                "CloseRoomBrowser ignored because the player is already in an active Fusion session."
            );

            FindUI()?.ShowWaitingRoom();
            return;
        }

        // Only a lobby-browser-only Runner may be shut down here.
        if (!browsingRooms)
        {
            FindUI()?.ShowPatinteroLobby();
            return;
        }

        manualShutdown = true;

        NetworkRunner oldRunner =
            runner;

        runner = null;
        sceneManager = null;
        browsingRooms = false;

        try
        {
            await oldRunner.Shutdown(
                destroyGameObject: true
            );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Error while closing room browser:\n" +
                exception.Message
            );
        }

        manualShutdown = false;

        FindUI()?.ShowPatinteroLobby();
    }



    // =========================================================

    // CREATE RUNNER

    // =========================================================



    private void CreateNewRunner()
    {
        if (runner != null)
        {
            Debug.LogWarning(
                "CreateNewRunner ignored because a NetworkRunner already exists."
            );
            return;
        }

        GameObject runnerObject =
            new GameObject(
                "PatinteroNetworkRunner"
            );

        // IMPORTANT:
        // Keep the runner as a ROOT object.
        // Do not parent it under Canvas, SharedLobby, or PatinteroNetworkRoot.
        runnerObject.transform.SetParent(null);

        DontDestroyOnLoad(
            runnerObject
        );

        runner =
            runnerObject
            .AddComponent<NetworkRunner>();

        sceneManager =
            runnerObject
            .AddComponent<
                NetworkSceneManagerDefault>();

        // Needed so this Runner sends PatinteroInputData for its local player.
        runner.ProvideInput = true;

        runner.AddCallbacks(
            this
        );

        Debug.Log(
            "Patintero NetworkRunner created as ROOT DontDestroyOnLoad object."
        );
    }



    // =========================================================

    // CURRENT SCENE

    // =========================================================



    private NetworkSceneInfo CurrentSceneInfo()

    {

        NetworkSceneInfo info =

            new NetworkSceneInfo();





        int index =

            SceneManager

            .GetActiveScene()

            .buildIndex;





        if (index >= 0)

        {

            info.AddSceneRef(

                SceneRef.FromIndex(

                    index

                ),

                LoadSceneMode.Single

            );

        }





        return info;

    }





    // =========================================================

    // FAILED RUNNER

    // =========================================================



    private void DestroyFailedRunner()

    {

        if (runner == null)

        {

            return;

        }





        GameObject runnerObject =

            runner.gameObject;





        runner =

            null;





        sceneManager =

            null;





        browsingRooms =

            false;





        if (runnerObject != null)

        {

            Destroy(

                runnerObject

            );

        }

    }





    // =========================================================

    // SESSION SETTINGS

    // =========================================================



    private int GetSelectedGameId()
    {
        switch (MultiplayerGameSelection.SelectedGame)
        {
            case MultiplayerGameType.SekyuBase:
                return 2;

            case MultiplayerGameType.Bamsak:
                return 3;

            case MultiplayerGameType.Patintero:
            default:
                return 1;
        }
    }


    private void ApplyGameIdToSelection(
        int gameId)
    {
        switch (gameId)
        {
            case 2:
                MultiplayerGameSelection.SelectedGame =
                    MultiplayerGameType.SekyuBase;
                break;

            case 3:
                MultiplayerGameSelection.SelectedGame =
                    MultiplayerGameType.Bamsak;
                break;

            case 1:
            default:
                MultiplayerGameSelection.SelectedGame =
                    MultiplayerGameType.Patintero;
                break;
        }
    }


    private void ReadSessionSettings()
    {
        if (runner == null ||
            !runner.SessionInfo.IsValid)
        {
            return;
        }

        if (runner
            .SessionInfo
            .Properties
            .TryGetValue(
                "g",
                out SessionProperty game)
            &&
            game.IsInt)
        {
            int gameId =
                (int)game.PropertyValue;

            ApplyGameIdToSelection(
                gameId
            );

            Debug.Log(
                "ROOM GAME = " +
                MultiplayerGameSelection.GetGameName()
            );
        }

        if (runner
            .SessionInfo
            .Properties
            .TryGetValue(
                "min",
                out SessionProperty min)
            &&
            min.IsInt)
        {
            MinimumPlayers =
                (int)min.PropertyValue;
        }

        if (runner
            .SessionInfo
            .Properties
            .TryGetValue(
                "max",
                out SessionProperty max)
            &&
            max.IsInt)
        {
            MaximumPlayers =
                (int)max.PropertyValue;
        }
    }



    // =========================================================

    // GET PLAYERS

    // =========================================================



    public List<PatinteroLobbyPlayer>

        GetLobbyPlayers()

    {

        return

            PatinteroLobbyPlayer.All



            .Where(

                player =>

                    player != null &&

                    player.Object != null &&

                    player.Object.IsValid

            )



            .OrderBy(

                player =>

                    player.Object

                    .InputAuthority

                    .PlayerId

            )



            .ToList();

    }





    // =========================================================

    // RECOMMENDED TEAM

    // =========================================================



    private int GetRecommendedTeam()

    {

        List<PatinteroLobbyPlayer> players =

            GetLobbyPlayers();





        int teamACount =

            players.Count(

                player =>

                    player.TeamId == 0

            );





        int teamBCount =

            players.Count(

                player =>

                    player.TeamId == 1

            );





        int maxPerTeam =

            Mathf.Max(

                1,

                MaximumPlayers / 2

            );





        if (teamACount >= maxPerTeam)

        {

            return 1;

        }





        if (teamBCount >= maxPerTeam)

        {

            return 0;

        }





        return

            teamACount <= teamBCount

            ? 0

            : 1;

    }





    // =========================================================

    // CAN START MATCH

    // =========================================================



    public bool CanStartMatch(

        out string message)

    {

        message = "";





        if (!IsHost)

        {

            message =

                "Only the Host can start.";



            return false;

        }





        List<PatinteroLobbyPlayer> players =

            GetLobbyPlayers();





        int count =

            players.Count;





        bool testing =

            allowTwoPlayerTesting &&

            count == 2;





        if (!testing &&

            count < MinimumPlayers)

        {

            message =

                "Waiting for more players.";



            return false;

        }





        bool validCount =

            count == 6 ||

            count == 8 ||

            count == 10;





        if (!testing &&

            !validCount)

        {

            message =

                "Need 6, 8, or 10 players.";



            return false;

        }





        int teamA =

            players.Count(

                player =>

                    player.TeamId == 0

            );





        int teamB =

            players.Count(

                player =>

                    player.TeamId == 1

            );





        if (teamA != teamB)

        {

            message =

                "Teams must have equal players.";



            return false;

        }





        foreach (

            PatinteroLobbyPlayer player

            in players)

        {

            if (player.CharacterIndex < 0)

            {

                message =

                    "Waiting for character selection.";



                return false;

            }





            // Host doesn't need to Ready.

            if (!player.IsHost &&

                !player.IsReady)

            {

                message =

                    "Waiting for players to Ready.";



                return false;

            }

        }





        message =

            "All players are ready!";





        return true;

    }





    // =========================================================

    // START MATCH

    // =========================================================



    public bool PrepareSelectedMatchStart(
        out string message)
    {
        if (!CanStartMatch(
            out message))
        {
            return false;
        }

        List<PatinteroLobbyPlayer> players =
            GetLobbyPlayers();

        if (players.Count > 0)
        {
            players[0]
                .ServerRecalculateTeamIndices();
        }

        if (runner == null ||
            !runner.IsRunning ||
            !runner.IsInSession)
        {
            message =
                "Fusion session is not running.";

            return false;
        }

        // Prevent more players from joining while gameplay is starting.
        runner.SessionInfo.IsOpen =
            false;

        runner.SessionInfo.IsVisible =
            false;

        message =
            "Starting " +
            MultiplayerGameSelection.GetGameName() +
            "...";

        return true;
    }


    // Kept for old Inspector button references.
    // It now routes to the shared multi-game START script.
    public void StartPatinteroMatch()
    {
        SharedLobbyStartGame sharedStarter =
            FindFirstObjectByType<
                SharedLobbyStartGame>();

        if (sharedStarter == null)
        {
            Debug.LogError(
                "SharedLobbyStartGame was not found. Use the shared START button."
            );

            return;
        }

        sharedStarter.StartSelectedGame();
    }



    // =========================================================

    // COPY ROOM NAME

    // Old/simple invite fallback

    // =========================================================



    public void CopyInvite()

    {

        if (!IsInSession)

        {

            return;

        }





        string serverName =

            runner.SessionInfo.Name;





        GUIUtility.systemCopyBuffer =

            serverName;





        FindUI()?.SetLobbyStatus(

            "Server name copied: " +

            serverName

        );

    }

    // =========================================================

    // LEAVE GAME AND RETURN TO MAIN SCENE

    // =========================================================



    public async void LeaveSessionToMain()

    {

        Debug.Log(

            "Leaving Patintero session and returning to Main..."

        );





        // Prevent OnShutdown from treating this

        // as an unexpected disconnect.

        manualShutdown = true;





        if (runner != null)

        {

            NetworkRunner oldRunner =

                runner;





            // Clear references first.

            runner = null;



            sceneManager = null;



            browsingRooms = false;





            try

            {

                await oldRunner.Shutdown(

                    destroyGameObject: true

                );

            }

            catch (Exception exception)

            {

                Debug.LogWarning(

                    "Error while shutting down Fusion:\n" +

                    exception.Message

                );

            }

        }





        manualShutdown = false;





        // Return to your Main scene.

        SceneManager.LoadScene(

            mainSceneBuildIndex

        );

    }





    // =========================================================

    // QUIT ROOM

    // =========================================================



    public async void QuitCurrentRoom()

    {

        if (runner == null)

        {

            FindUI()?.ShowPatinteroLobby();



            return;

        }





        manualShutdown =

            true;





        NetworkRunner oldRunner =

            runner;





        runner =

            null;





        sceneManager =

            null;





        browsingRooms =

            false;





        await oldRunner.Shutdown(

            destroyGameObject: true

        );





        manualShutdown =

            false;





        FindUI()?.ShowPatinteroLobby();

    }





    // =========================================================

    // PLAYER JOINED

    // =========================================================



    public void OnPlayerJoined(

        NetworkRunner networkRunner,

        PlayerRef player)

    {

        if (!networkRunner.IsServer)

        {

            return;

        }





        int initialTeam =

            GetRecommendedTeam();





        NetworkObject lobbyObject =

            networkRunner.Spawn(

                lobbyPlayerPrefab,



                Vector3.zero,



                Quaternion.identity,



                player,



                (r, obj) =>

                {

                    PatinteroLobbyPlayer data =

                        obj.GetComponent<

                            PatinteroLobbyPlayer>();





                    bool host =

                        player ==

                        r.LocalPlayer;





                    data.InitializeBeforeSpawn(

                        host,

                        initialTeam

                    );

                }

            );





        networkRunner.SetPlayerObject(

            player,

            lobbyObject

        );





        PatinteroLobbyPlayer lobbyPlayer =

            lobbyObject.GetComponent<

                PatinteroLobbyPlayer>();





        if (lobbyPlayer != null)

        {

            lobbyPlayer

                .ServerRecalculateTeamIndices();

        }

    }





    // =========================================================

    // PLAYER LEFT

    // =========================================================



    public void OnPlayerLeft(

        NetworkRunner networkRunner,

        PlayerRef player)

    {

        if (!networkRunner.IsServer)

        {

            return;

        }





        if (networkRunner

            .TryGetPlayerObject(

                player,

                out NetworkObject obj))

        {

            networkRunner.Despawn(

                obj

            );

        }





        List<PatinteroLobbyPlayer> players =

            GetLobbyPlayers();





        if (players.Count > 0)

        {

            players[0]

                .ServerRecalculateTeamIndices();

        }

    }





    // =========================================================

    // ROOM LIST

    // =========================================================



    public void OnSessionListUpdated(
        NetworkRunner networkRunner,
        List<SessionInfo> sessionList)
    {
        int selectedGameId =
            GetSelectedGameId();

        cachedSessions =
            sessionList

            .Where(
                session =>
                {
                    if (!session.IsVisible)
                    {
                        return false;
                    }

                    if (!session
                        .Properties
                        .TryGetValue(
                            "g",
                            out SessionProperty game))
                    {
                        return false;
                    }

                    if (!game.IsInt)
                    {
                        return false;
                    }

                    return
                        (int)game.PropertyValue
                        == selectedGameId;
                }
            )

            .ToList();

        FindUI()?.RefreshRoomList(
            cachedSessions
        );
    }



    // =========================================================

    // NETWORK INPUT

    // =========================================================



    public void OnInput(

        NetworkRunner networkRunner,
        NetworkInput input)

    {

        

    }





    // =========================================================

    // SHUTDOWN

    // =========================================================



    public void OnShutdown(

        NetworkRunner networkRunner,

        ShutdownReason shutdownReason)

    {

        if (runner ==

            networkRunner)

        {

            runner =

                null;





            sceneManager =

                null;





            browsingRooms =

                false;

        }





        if (manualShutdown)

        {

            return;

        }





        Debug.LogWarning(

            "Fusion Shutdown: " +

            shutdownReason

        );





        FindUI()?.ShowError(

            "Room closed: " +

            shutdownReason

        );

    }





    // =========================================================

    // FIND UI

    // =========================================================



    private PatinteroMenuUI FindUI()

    {

        return

            FindFirstObjectByType<

                PatinteroMenuUI>();

    }





    // =========================================================

    // REQUIRED FUSION CALLBACKS

    // =========================================================



    public void OnConnectedToServer(

        NetworkRunner networkRunner)

    {

    }





    public void OnDisconnectedFromServer(

        NetworkRunner networkRunner,

        NetDisconnectReason reason)

    {

    }





    public void OnConnectFailed(

        NetworkRunner networkRunner,

        NetAddress remoteAddress,

        NetConnectFailedReason reason)

    {

    }





    public void OnConnectRequest(

        NetworkRunner networkRunner,

        NetworkRunnerCallbackArgs

            .ConnectRequest request,

        byte[] token)

    {

    }





    public void OnCustomAuthenticationResponse(

        NetworkRunner networkRunner,

        Dictionary<string, object> data)

    {

    }





    public void OnHostMigration(

        NetworkRunner networkRunner,

        HostMigrationToken hostMigrationToken)

    {

    }





    public void OnInputMissing(

        NetworkRunner networkRunner,

        PlayerRef player,

        NetworkInput input)

    {

    }





    public void OnObjectEnterAOI(

        NetworkRunner networkRunner,

        NetworkObject obj,

        PlayerRef player)

    {

    }





    public void OnObjectExitAOI(

        NetworkRunner networkRunner,

        NetworkObject obj,

        PlayerRef player)

    {

    }





    public void OnReliableDataProgress(

        NetworkRunner networkRunner,

        PlayerRef player,

        ReliableKey key,

        float progress)

    {

    }





    // Correct callback for your installed

    // Fusion version.

    public void OnReliableDataReceived(

        NetworkRunner networkRunner,

        PlayerRef player,

        ReliableKey key,

        ArraySegment<byte> data)

    {

    }





    public void OnSceneLoadStart(

        NetworkRunner networkRunner)

    {

    }





    public void OnSceneLoadDone(

        NetworkRunner networkRunner)

    {

    }





    public void OnUserSimulationMessage(

        NetworkRunner networkRunner,

        SimulationMessagePtr message)

    {

    }

}