using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firebase.Auth;
using Firebase.Firestore;

using TMPro;

using UnityEngine;


public class PatinteroInviteManager : MonoBehaviour
{
    public static PatinteroInviteManager Instance;


    // =========================================================
    // FIREBASE FRIEND STRUCTURE
    //
    // students/{uid}/friends/{friendUid}
    // =========================================================

    [Header("FIREBASE FRIEND STRUCTURE")]

    [SerializeField]
    private string usersCollection = "students";

    [SerializeField]
    private string friendsCollection = "friends";

    [SerializeField]
    private string friendNameField = "username";

    [SerializeField]
    private string profileImageIdField = "profileImageId";


    // =========================================================
    // PROFILE AVATARS
    // =========================================================

    [Header("PROFILE AVATARS")]

    [Tooltip(
        "Use exactly the same avatar Sprite order " +
        "as ProfileSocialManager."
    )]
    [SerializeField]
    private Sprite[] avatars;


    // =========================================================
    // INVITE FRIEND PANEL
    // =========================================================

    [Header("INVITE FRIEND PANEL")]

    [SerializeField]
    private GameObject inviteFriendsPanel;

    [SerializeField]
    private Transform friendListContent;

    [SerializeField]
    private PatinteroInviteFriendItem friendItemPrefab;

    [SerializeField]
    private GameObject emptyFriendsText;

    [SerializeField]
    private TMP_Text inviteStatusText;


    // =========================================================
    // INCOMING INVITE
    // =========================================================

    [Header("INCOMING INVITE")]

    [SerializeField]
    private GameObject incomingInvitePanel;

    [SerializeField]
    private TMP_Text incomingInviteText;


    // =========================================================
    // FIREBASE
    // =========================================================

    private FirebaseAuth auth;

    private FirebaseFirestore db;

    private ListenerRegistration inviteListener;


    // =========================================================
    // CURRENT INCOMING INVITE
    // =========================================================

    private DocumentReference currentInviteDocument;

    private string currentInviteSessionName = "";

    private string currentInviteSenderName = "";


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


        auth =
            FirebaseAuth.DefaultInstance;


        db =
            FirebaseFirestore.DefaultInstance;
    }


    private void Start()
    {
        if (inviteFriendsPanel != null)
        {
            inviteFriendsPanel.SetActive(false);
        }


        if (incomingInvitePanel != null)
        {
            incomingInvitePanel.SetActive(false);
        }


        StartCoroutine(
            WaitForFirebaseLogin()
        );
    }


    private void OnDestroy()
    {
        if (inviteListener != null)
        {
            inviteListener.Stop();

            inviteListener = null;
        }


        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // WAIT FOR FIREBASE LOGIN
    // =========================================================

    private IEnumerator WaitForFirebaseLogin()
    {
        while (
            auth == null ||
            auth.CurrentUser == null)
        {
            yield return new WaitForSeconds(
                0.5f
            );
        }


        Debug.Log(
            "PATINTERO INVITES READY FOR UID: " +
            auth.CurrentUser.UserId
        );


        StartIncomingInviteListener();
    }


    // =========================================================
    // OPEN FRIEND INVITE PANEL
    // =========================================================

    public void OpenInvitePanel()
    {
        Debug.Log(
            "PATINTERO INVITE BUTTON PRESSED"
        );


        if (inviteFriendsPanel == null)
        {
            Debug.LogError(
                "Invite Friends Panel is not assigned!"
            );

            return;
        }


        inviteFriendsPanel.SetActive(true);


        SetStatus(
            "Loading friends..."
        );


        LoadFriends();
    }


    // =========================================================
    // CLOSE FRIEND INVITE PANEL
    // =========================================================

    public void CloseInvitePanel()
    {
        if (inviteFriendsPanel != null)
        {
            inviteFriendsPanel.SetActive(false);
        }
    }


    // =========================================================
    // LOAD FRIENDS
    //
    // students/MY_UID/friends/FRIEND_UID
    //
    // Also reads students/FRIEND_UID to get the
    // newest username and profileImageId.
    // =========================================================

    private async void LoadFriends()
    {
        ClearFriendList();

        ShowEmptyFriends(false);

        SetStatus("Loading friends...");


        // =========================================
        // FIREBASE AUTH
        // =========================================

        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }


        float waitTime = 0f;

        while (
            auth.CurrentUser == null &&
            waitTime < 5f)
        {
            await Task.Delay(250);

            waitTime += 0.25f;
        }


        if (auth.CurrentUser == null)
        {
            Debug.LogError(
                "PATINTERO: Firebase user is not logged in."
            );

            SetStatus(
                "Please login again."
            );

            ShowEmptyFriends(true);

            return;
        }


        string myUid =
            auth.CurrentUser.UserId;


        // =========================================
        // DEBUG
        // =========================================

        Debug.Log(
            "========= PATINTERO FRIEND LIST ========="
        );

        Debug.Log(
            "PROJECT ID = " +
            Firebase.FirebaseApp
                .DefaultInstance
                .Options
                .ProjectId
        );

        Debug.Log(
            "USER UID = " +
            myUid
        );

        Debug.Log(
            "ACTUAL PATH = " +
            usersCollection +
            "/" +
            myUid +
            "/" +
            friendsCollection
        );

        Debug.Log(
            "=========================================="
        );


        try
        {
            // =========================================
            // GET THIS USER'S FRIENDS
            //
            // students/{myUid}/friends
            // =========================================

            QuerySnapshot friendSnapshot =
                await db
                .Collection("students")
                .Document(myUid)
                .Collection("friends")
                .GetSnapshotAsync();


            Debug.Log(
                "FRIEND DOCUMENT COUNT = " +
                friendSnapshot.Count
            );


            int loadedFriends = 0;


            foreach (
                DocumentSnapshot friendDocument
                in friendSnapshot.Documents)
            {
                // Friend document ID = friend's UID
                string friendUid =
                    friendDocument.Id;


                Debug.Log(
                    "Found friend UID: " +
                    friendUid
                );


                // =====================================
                // GET FRIEND PROFILE
                //
                // students/{friendUid}
                // =====================================

                DocumentSnapshot profileSnapshot =
                    await db
                    .Collection("students")
                    .Document(friendUid)
                    .GetSnapshotAsync();


                if (!profileSnapshot.Exists)
                {
                    Debug.LogWarning(
                        "Friend profile not found: " +
                        friendUid
                    );

                    continue;
                }


                Dictionary<string, object> profileData =
                    profileSnapshot.ToDictionary();


                // =====================================
                // USERNAME
                // =====================================

                string username =
                    "Friend";


                if (profileData.TryGetValue(
                    "username",
                    out object usernameValue))
                {
                    if (usernameValue != null)
                    {
                        username =
                            usernameValue.ToString();
                    }
                }


                // =====================================
                // PROFILE IMAGE ID
                // =====================================

                int profileImageId = 0;


                if (profileData.TryGetValue(
                    "profileImageId",
                    out object imageValue))
                {
                    try
                    {
                        profileImageId =
                            Convert.ToInt32(
                                imageValue
                            );
                    }
                    catch
                    {
                        profileImageId = 0;
                    }
                }


                // =====================================
                // GET AVATAR SPRITE
                // =====================================

                Sprite avatar =
                    GetAvatar(
                        profileImageId
                    );


                // =====================================
                // CREATE UI ITEM
                // =====================================

                if (friendItemPrefab == null)
                {
                    Debug.LogError(
                        "Friend Item Prefab is not assigned!"
                    );

                    continue;
                }


                if (friendListContent == null)
                {
                    Debug.LogError(
                        "Friend List Content is not assigned!"
                    );

                    continue;
                }


                PatinteroInviteFriendItem newItem =
                    Instantiate(
                        friendItemPrefab,
                        friendListContent
                    );


                newItem.Setup(
                    friendUid,
                    username,
                    avatar,
                    this
                );


                loadedFriends++;


                Debug.Log(
                    "Friend added to Invite list: " +
                    username
                );
            }


            // =========================================
            // FINISH
            // =========================================

            if (loadedFriends == 0)
            {
                ShowEmptyFriends(true);

                SetStatus(
                    "No friends found."
                );
            }
            else
            {
                ShowEmptyFriends(false);

                SetStatus(
                    "Choose a friend to invite."
                );
            }


            Debug.Log(
                "TOTAL FRIENDS SHOWN = " +
                loadedFriends
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "FAILED TO LOAD PATINTERO FRIENDS:\n" +
                exception
            );


            ShowEmptyFriends(true);

            SetStatus(
                "Failed to load friends."
            );
        }
    }


    // =========================================================
    // GET AVATAR
    // =========================================================

    private Sprite GetAvatar(
        int id)
    {
        if (avatars == null ||
            avatars.Length == 0)
        {
            return null;
        }


        if (id < 0 ||
            id >= avatars.Length)
        {
            id = 0;
        }


        return avatars[id];
    }


    // =========================================================
    // CLEAR FRIEND LIST
    // =========================================================

    private void ClearFriendList()
    {
        if (friendListContent == null)
        {
            Debug.LogError(
                "Friend List Content is not assigned!"
            );

            return;
        }


        for (
            int i =
                friendListContent.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                friendListContent
                .GetChild(i)
                .gameObject
            );
        }
    }


    // =========================================================
    // SHOW / HIDE EMPTY FRIEND MESSAGE
    // =========================================================

    private void ShowEmptyFriends(
        bool show)
    {
        if (emptyFriendsText != null)
        {
            emptyFriendsText.SetActive(
                show
            );
        }
    }


    // =========================================================
    // SEND PATINTERO INVITE
    // =========================================================

    public void SendInviteToFriend(
        string friendUid,
        string friendName,
        PatinteroInviteFriendItem item)
    {
        // -----------------------------------------------------
        // FIREBASE LOGIN CHECK
        // -----------------------------------------------------

        if (auth == null ||
            auth.CurrentUser == null)
        {
            SetStatus(
                "You are not logged in."
            );


            item?.ShowInviteFailed();

            return;
        }


        // -----------------------------------------------------
        // FRIEND UID CHECK
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(
            friendUid))
        {
            SetStatus(
                "Invalid friend."
            );


            item?.ShowInviteFailed();

            return;
        }


        // -----------------------------------------------------
        // FUSION ROOM CHECK
        // -----------------------------------------------------

        PatinteroFusionManager fusion =
            PatinteroFusionManager.Instance;


        if (fusion == null ||
            !fusion.IsInSession)
        {
            SetStatus(
                "You are not inside a Patintero room."
            );


            item?.ShowInviteFailed();

            return;
        }


        if (fusion.Runner == null ||
            !fusion.Runner.SessionInfo.IsOpen)
        {
            SetStatus(
                "The room is no longer open."
            );


            item?.ShowInviteFailed();

            return;
        }


        // -----------------------------------------------------
        // ROOM NAME
        // -----------------------------------------------------

        string roomName =
            fusion.Runner
            .SessionInfo
            .Name;


        // -----------------------------------------------------
        // SENDER NAME
        // -----------------------------------------------------

        string senderName =
            fusion.LocalPlayerName;


        if (PatinteroLobbyPlayer.Local != null)
        {
            string networkName =
                PatinteroLobbyPlayer.Local
                .PlayerName
                .ToString();


            if (!string.IsNullOrWhiteSpace(
                networkName))
            {
                senderName =
                    networkName;
            }
        }


        // -----------------------------------------------------
        // CREATE FIRESTORE INVITE DATA
        // -----------------------------------------------------

        Dictionary<string, object> invite =
            new Dictionary<string, object>()
            {
                {
                    "fromUid",
                    auth.CurrentUser.UserId
                },

                {
                    "fromName",
                    senderName
                },

                {
                    "toUid",
                    friendUid
                },

                {
                    "toName",
                    friendName
                },

                {
                    "game",
                    "Patintero"
                },

                {
                    "sessionName",
                    roomName
                },

                {
                    "status",
                    "pending"
                },

                {
                    "createdAt",
                    FieldValue.ServerTimestamp
                }
            };


        // IMPORTANT:
        // Only ONE Firestore write happens here.
        SendInviteAndUpdateUI(
            invite,
            friendName,
            item
        );
    }


    // =========================================================
    // WRITE INVITE TO FIRESTORE
    // =========================================================

    private async void SendInviteAndUpdateUI(
        Dictionary<string, object> invite,
        string friendName,
        PatinteroInviteFriendItem item)
    {
        try
        {
            await db
            .Collection("gameInvites")
            .AddAsync(invite);


            SetStatus(
                "Invite sent to " +
                friendName +
                "."
            );


            item?.ShowInviteSent();


            Debug.Log(
                "PATINTERO INVITE SENT TO: " +
                friendName
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Game invite failed:\n" +
                exception
            );


            SetStatus(
                "Failed to invite " +
                friendName +
                "."
            );


            item?.ShowInviteFailed();
        }
    }


    // =========================================================
    // LISTEN FOR INCOMING INVITES
    // =========================================================

    private void StartIncomingInviteListener()
    {
        if (auth == null ||
            auth.CurrentUser == null)
        {
            return;
        }


        string myUid =
            auth.CurrentUser.UserId;


        Query query =
            db
            .Collection("gameInvites")
            .WhereEqualTo(
                "toUid",
                myUid
            );


        inviteListener =
            query.Listen(
                snapshot =>
                {
                    // Already showing an invite.
                    if (currentInviteDocument != null)
                    {
                        return;
                    }


                    foreach (
                        DocumentSnapshot document
                        in snapshot.Documents)
                    {
                        Dictionary<string, object> data =
                            document.ToDictionary();


                        string status =
                            ReadString(
                                data,
                                "status"
                            );


                        string game =
                            ReadString(
                                data,
                                "game"
                            );


                        if (status != "pending")
                        {
                            continue;
                        }


                        if (game != "Patintero")
                        {
                            continue;
                        }


                        string sessionName =
                            ReadString(
                                data,
                                "sessionName"
                            );


                        if (string.IsNullOrWhiteSpace(
                            sessionName))
                        {
                            continue;
                        }


                        string senderName =
                            ReadString(
                                data,
                                "fromName"
                            );


                        currentInviteDocument =
                            document.Reference;


                        currentInviteSessionName =
                            sessionName;


                        currentInviteSenderName =
                            string.IsNullOrWhiteSpace(
                                senderName)
                            ? "A friend"
                            : senderName;


                        ShowIncomingInvite();

                        break;
                    }
                }
            );
    }


    // =========================================================
    // SHOW INCOMING INVITE
    // =========================================================

    private void ShowIncomingInvite()
    {
        if (incomingInvitePanel == null)
        {
            Debug.LogError(
                "Incoming Invite Panel is not assigned!"
            );

            return;
        }


        incomingInvitePanel.SetActive(true);


        if (incomingInviteText != null)
        {
            incomingInviteText.text =
                currentInviteSenderName +
                " invited you to play Patintero.\n\n" +
                "Server: " +
                currentInviteSessionName;
        }
    }


    // =========================================================
    // ACCEPT INVITE
    // =========================================================

    public async void AcceptInvite()
    {
        if (currentInviteDocument == null)
        {
            return;
        }


        string roomToJoin =
            currentInviteSessionName;


        try
        {
            Dictionary<string, object> update =
                new Dictionary<string, object>()
                {
                    {
                        "status",
                        "accepted"
                    }
                };


            await currentInviteDocument
                .UpdateAsync(
                    update
                );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Could not update invite status:\n" +
                exception.Message
            );
        }


        if (incomingInvitePanel != null)
        {
            incomingInvitePanel.SetActive(false);
        }


        ClearCurrentInvite();


        if (PatinteroFusionManager.Instance != null)
        {
            PatinteroFusionManager
                .Instance
                .JoinInvitedRoom(
                    roomToJoin
                );
        }
    }


    // =========================================================
    // DECLINE INVITE
    // =========================================================

    public async void DeclineInvite()
    {
        if (currentInviteDocument == null)
        {
            return;
        }


        try
        {
            Dictionary<string, object> update =
                new Dictionary<string, object>()
                {
                    {
                        "status",
                        "declined"
                    }
                };


            await currentInviteDocument
                .UpdateAsync(
                    update
                );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "Could not decline invite:\n" +
                exception.Message
            );
        }


        if (incomingInvitePanel != null)
        {
            incomingInvitePanel.SetActive(false);
        }


        ClearCurrentInvite();
    }


    // =========================================================
    // CLEAR CURRENT INVITE
    // =========================================================

    private void ClearCurrentInvite()
    {
        currentInviteDocument = null;

        currentInviteSessionName = "";

        currentInviteSenderName = "";
    }


    // =========================================================
    // READ STRING
    // =========================================================

    private string ReadString(
        Dictionary<string, object> data,
        string key)
    {
        if (data.TryGetValue(
            key,
            out object value))
        {
            if (value != null)
            {
                return value.ToString();
            }
        }


        return "";
    }


    // =========================================================
    // READ INT
    // =========================================================

    private int ReadInt(
        Dictionary<string, object> data,
        string key,
        int fallback = 0)
    {
        if (!data.TryGetValue(
            key,
            out object value))
        {
            return fallback;
        }


        if (value == null)
        {
            return fallback;
        }


        try
        {
            return Convert.ToInt32(
                value
            );
        }
        catch
        {
            return fallback;
        }
    }


    // =========================================================
    // STATUS
    // =========================================================

    private void SetStatus(
        string message)
    {
        if (inviteStatusText != null)
        {
            inviteStatusText.text =
                message;
        }
    }
}