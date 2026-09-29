using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProfileSocialManager : MonoBehaviour
{
    [Header("TOP PROFILE")]
    public Image profileImage;
    public TMP_Text profileNameText;
    public TMP_Text bioText;

    [Header("PANELS")]
    public GameObject friendsPanel;
    public GameObject friendRequestsPanel;
    public GameObject searchPlayersPanel;
    public GameObject editProfilePanel;

    [Header("EDIT PROFILE")]
    public Image currentProfileImage;
    public TMP_InputField bioInput;

    [Header("AVATARS")]
    public Sprite[] avatars;

    [Header("FRIENDS")]
    public Transform friendsContent;
    public GameObject friendItemPrefab;

    [Header("FRIEND REQUESTS")]
    public Transform requestsContent;
    public GameObject friendRequestItemPrefab;

    [Header("SEARCH PLAYERS")]
    public TMP_InputField searchInput;
    public Transform searchContent;
    public GameObject searchPlayerItemPrefab;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private string currentUsername = "";
    private string currentBio = "";

    private int currentAvatarId = 0;
    private int selectedAvatarId = 0;


    void OnEnable()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        OpenMyProfile();
    }


    // =========================================
    // TAB BUTTONS
    // =========================================

    public void OpenMyProfile()
    {
        HideAllPanels();
        LoadMyProfile();
    }

    public void OpenFriends()
    {
        HideAllPanels();

        if (friendsPanel != null)
            friendsPanel.SetActive(true);

        LoadFriends();
    }

    public void OpenRequests()
    {
        HideAllPanels();

        if (friendRequestsPanel != null)
            friendRequestsPanel.SetActive(true);

        LoadFriendRequests();
    }

    public void OpenSearchPlayers()
    {
        HideAllPanels();

        if (searchPlayersPanel != null)
            searchPlayersPanel.SetActive(true);
    }


    // =========================================
    // MY PROFILE
    // =========================================

    void LoadMyProfile()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogWarning("No Firebase user.");
            return;
        }

        string uid = auth.CurrentUser.UserId;

        db.Collection("students")
            .Document(uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("PROFILE LOAD ERROR: " + task.Exception);
                    return;
                }

                DocumentSnapshot doc = task.Result;

                if (!doc.Exists)
                    return;

                if (doc.ContainsField("username"))
                    currentUsername = doc.GetValue<string>("username");

                if (doc.ContainsField("bio"))
                    currentBio = doc.GetValue<string>("bio");
                else
                    currentBio = "";

                if (doc.ContainsField("profileImageId"))
                    currentAvatarId = Convert.ToInt32(
                        doc.GetValue<long>("profileImageId")
                    );
                else
                    currentAvatarId = 0;

                UpdateProfileUI();
            });
    }


    void UpdateProfileUI()
    {
        if (profileNameText != null)
            profileNameText.text = currentUsername;

        if (bioText != null)
        {
            if (string.IsNullOrWhiteSpace(currentBio))
                bioText.text = "No bio yet.";
            else
                bioText.text = currentBio;
        }

        if (profileImage != null)
            profileImage.sprite = GetAvatar(currentAvatarId);
    }


    // =========================================
    // EDIT PROFILE
    // =========================================

    public void OpenEditProfile()
    {
        if (editProfilePanel == null)
            return;

        HideAllPanels();
        editProfilePanel.SetActive(true);

        selectedAvatarId = currentAvatarId;

        if (currentProfileImage != null)
            currentProfileImage.sprite = GetAvatar(selectedAvatarId);

        if (bioInput != null)
            bioInput.text = currentBio;
    }


    public void SelectAvatar(int avatarId)
    {
        if (avatars == null ||
            avatarId < 0 ||
            avatarId >= avatars.Length)
        {
            return;
        }

        selectedAvatarId = avatarId;

        if (currentProfileImage != null)
            currentProfileImage.sprite = GetAvatar(avatarId);
    }


    public void SaveProfile()
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        string uid = auth.CurrentUser.UserId;
        string newBio = "";

        if (bioInput != null)
            newBio = bioInput.text.Trim();

        if (newBio.Length > 100)
            newBio = newBio.Substring(0, 100);

        Dictionary<string, object> data =
            new Dictionary<string, object>();

        data["bio"] = newBio;
        data["profileImageId"] = selectedAvatarId;
        data["updatedAt"] = FieldValue.ServerTimestamp;

        db.Collection("students")
            .Document(uid)
            .SetAsync(data, SetOptions.MergeAll)
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("SAVE PROFILE ERROR: " + task.Exception);
                    return;
                }

                currentBio = newBio;
                currentAvatarId = selectedAvatarId;

                UpdateProfileUI();
                OpenMyProfile();

                Debug.Log("PROFILE SAVED");
            });
    }


    public void CancelEditProfile()
    {
        selectedAvatarId = currentAvatarId;
        OpenMyProfile();
    }


    // =========================================
    // FRIENDS
    // =========================================

    void LoadFriends()
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        ClearContent(friendsContent);

        string uid = auth.CurrentUser.UserId;

        db.Collection("students")
            .Document(uid)
            .Collection("friends")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("LOAD FRIENDS ERROR: " + task.Exception);
                    return;
                }

                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    string friendUid = doc.Id;
                    string username = "Player";
                    int avatarId = 0;

                    if (doc.ContainsField("username"))
                        username = doc.GetValue<string>("username");

                    if (doc.ContainsField("profileImageId"))
                    {
                        avatarId = Convert.ToInt32(
                            doc.GetValue<long>("profileImageId")
                        );
                    }

                    GameObject item = Instantiate(
                        friendItemPrefab,
                        friendsContent
                    );

                    FriendItemUI ui = item.GetComponent<FriendItemUI>();

                    if (ui != null)
                    {
                        ui.Setup(
                            friendUid,
                            username,
                            GetAvatar(avatarId)
                        );
                    }
                }
            });
    }


    // =========================================
    // FRIEND REQUESTS
    // =========================================

    void LoadFriendRequests()
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        ClearContent(requestsContent);

        string uid = auth.CurrentUser.UserId;

        db.Collection("students")
            .Document(uid)
            .Collection("friendRequests")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("REQUEST LOAD ERROR: " + task.Exception);
                    return;
                }

                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    string senderUid = doc.Id;
                    string username = "Player";
                    int avatarId = 0;

                    if (doc.ContainsField("fromUsername"))
                        username = doc.GetValue<string>("fromUsername");

                    if (doc.ContainsField("profileImageId"))
                    {
                        avatarId = Convert.ToInt32(
                            doc.GetValue<long>("profileImageId")
                        );
                    }

                    GameObject item = Instantiate(
                        friendRequestItemPrefab,
                        requestsContent
                    );

                    FriendRequestItemUI ui =
                        item.GetComponent<FriendRequestItemUI>();

                    if (ui != null)
                    {
                        ui.Setup(
                            senderUid,
                            username,
                            avatarId,
                            GetAvatar(avatarId),
                            this
                        );
                    }
                }
            });
    }


    public void AcceptFriendRequest(
        string senderUid,
        string senderUsername,
        int senderAvatarId)
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        string myUid = auth.CurrentUser.UserId;

        Dictionary<string, object> senderData =
            new Dictionary<string, object>();

        senderData["uid"] = senderUid;
        senderData["username"] = senderUsername;
        senderData["profileImageId"] = senderAvatarId;
        senderData["addedAt"] = FieldValue.ServerTimestamp;

        Dictionary<string, object> myData =
            new Dictionary<string, object>();

        myData["uid"] = myUid;
        myData["username"] = currentUsername;
        myData["profileImageId"] = currentAvatarId;
        myData["addedAt"] = FieldValue.ServerTimestamp;

        DocumentReference myFriend =
            db.Collection("students")
            .Document(myUid)
            .Collection("friends")
            .Document(senderUid);

        DocumentReference theirFriend =
            db.Collection("students")
            .Document(senderUid)
            .Collection("friends")
            .Document(myUid);

        DocumentReference request =
            db.Collection("students")
            .Document(myUid)
            .Collection("friendRequests")
            .Document(senderUid);

        Task task1 = myFriend.SetAsync(senderData);
        Task task2 = theirFriend.SetAsync(myData);
        Task task3 = request.DeleteAsync();

        Task.WhenAll(task1, task2, task3)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("FRIEND REQUEST ACCEPTED");
                    LoadFriendRequests();
                }
                else
                {
                    Debug.LogError("ACCEPT ERROR: " + task.Exception);
                }
            });
    }


    public void DeclineFriendRequest(string senderUid)
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        string uid = auth.CurrentUser.UserId;

        db.Collection("students")
            .Document(uid)
            .Collection("friendRequests")
            .Document(senderUid)
            .DeleteAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("REQUEST DECLINED");
                    LoadFriendRequests();
                }
                else
                {
                    Debug.LogError("DECLINE ERROR: " + task.Exception);
                }
            });
    }

    // =========================================================
// CHECK RELATIONSHIP STATE
// =========================================================

public void CheckRelationshipState(
    string targetUid,
    SearchPlayerItemUI itemUI)
{
    if (auth == null || auth.CurrentUser == null)
        return;

    string myUid = auth.CurrentUser.UserId;

    DocumentReference friendDoc =
        db.Collection("students")
        .Document(myUid)
        .Collection("friends")
        .Document(targetUid);

    DocumentReference requestDoc =
        db.Collection("students")
        .Document(targetUid)
        .Collection("friendRequests")
        .Document(myUid);

    Task<DocumentSnapshot> friendTask =
        friendDoc.GetSnapshotAsync();

    Task<DocumentSnapshot> requestTask =
        requestDoc.GetSnapshotAsync();

    Task.WhenAll(friendTask, requestTask)
        .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError(
                    "RELATIONSHIP CHECK ERROR: " +
                    task.Exception
                );

                return;
            }

            if (friendTask.Result.Exists)
            {
                if (itemUI != null)
                    itemUI.SetFriend();

                return;
            }

            if (requestTask.Result.Exists)
            {
                if (itemUI != null)
                    itemUI.SetSent();

                return;
            }

            if (itemUI != null)
                itemUI.SetAddFriend();
        });
}


// =========================================================
// CANCEL FRIEND REQUEST
// =========================================================

public void CancelFriendRequest(
    string targetUid,
    SearchPlayerItemUI itemUI)
{
    if (auth == null || auth.CurrentUser == null)
        return;

    string myUid = auth.CurrentUser.UserId;

    DocumentReference requestDoc =
        db.Collection("students")
        .Document(targetUid)
        .Collection("friendRequests")
        .Document(myUid);

    requestDoc.DeleteAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError(
                    "CANCEL REQUEST ERROR: " +
                    task.Exception
                );

                return;
            }

            Debug.Log(
                "FRIEND REQUEST CANCELLED"
            );

            if (itemUI != null)
            {
                itemUI.SetAddFriend();
            }
        });
}


    // =========================================
    // SEARCH PLAYER
    // =========================================

    public void SearchPlayer()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            Debug.LogWarning("No Firebase user is logged in.");
            return;
        }

        if (searchInput == null)
        {
            Debug.LogError("Search Input is not assigned in ProfileSocialManager.");
            return;
        }

        if (searchContent == null)
        {
            Debug.LogError("Search Content is not assigned in ProfileSocialManager.");
            return;
        }

        if (searchPlayerItemPrefab == null)
        {
            Debug.LogError("Search Player Item Prefab is not assigned.");
            return;
        }

        string searchName = searchInput.text.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(searchName))
        {
            Debug.LogWarning("Enter a player name.");
            ClearContent(searchContent);
            return;
        }

        ClearContent(searchContent);

        Debug.Log("SEARCHING USERNAME ONLY: " + searchName);

        // Search username/name only.
        // Bio is NOT used anywhere in this search.
        db.Collection("students")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("SEARCH ERROR: " + task.Exception);
                    return;
                }

                int resultCount = 0;

                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    // Do not show the currently logged-in player.
                    if (doc.Id == auth.CurrentUser.UserId)
                        continue;

                    if (!doc.ContainsField("username"))
                        continue;

                    string username = doc.GetValue<string>("username");

                    if (string.IsNullOrWhiteSpace(username))
                        continue;

                    // Username only, case-insensitive, partial match.
                    // Example: "nic" can find "Nica".
                    if (username.IndexOf(
                            searchName,
                            StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    int avatarId = 0;

                    if (doc.ContainsField("profileImageId"))
                    {
                        try
                        {
                            avatarId = Convert.ToInt32(
                                doc.GetValue<long>("profileImageId")
                            );
                        }
                        catch
                        {
                            avatarId = 0;
                        }
                    }

                    GameObject item = Instantiate(
                        searchPlayerItemPrefab,
                        searchContent
                    );

                    SearchPlayerItemUI ui =
                        item.GetComponent<SearchPlayerItemUI>();

                    if (ui != null)
                    {
                        ui.Setup(
                            doc.Id,
                            username,
                            avatarId,
                            GetAvatar(avatarId),
                            this
                        );
                    }
                    else
                    {
                        Debug.LogError(
                            "SearchPlayerItemUI is missing from SearchPlayerItem prefab."
                        );

                        Destroy(item);
                        continue;
                    }

                    resultCount++;
                }

                if (resultCount == 0)
                {
                    Debug.Log("PLAYER NOT FOUND: " + searchName);
                }
                else
                {
                    Debug.Log(
                        "FOUND " + resultCount +
                        " PLAYER(S) FOR: " + searchName
                    );
                }
            });
    }


    public void SendFriendRequest(
        string targetUid,
        SearchPlayerItemUI itemUI)
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        string myUid = auth.CurrentUser.UserId;

        DocumentReference friendDoc =
            db.Collection("students")
            .Document(myUid)
            .Collection("friends")
            .Document(targetUid);

        DocumentReference requestDoc =
            db.Collection("students")
            .Document(targetUid)
            .Collection("friendRequests")
            .Document(myUid);

        Task<DocumentSnapshot> friendTask =
            friendDoc.GetSnapshotAsync();

        Task<DocumentSnapshot> requestTask =
            requestDoc.GetSnapshotAsync();

        Task.WhenAll(friendTask, requestTask)
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    Debug.LogError("REQUEST CHECK ERROR: " + task.Exception);
                    return;
                }

                if (friendTask.Result.Exists)
                {
                    if (itemUI != null)
                        itemUI.SetFriend();

                    return;
                }

                if (requestTask.Result.Exists)
                {
                    if (itemUI != null)
                        itemUI.SetSent();

                    return;
                }

                Dictionary<string, object> requestData =
                    new Dictionary<string, object>();

                requestData["fromUid"] = myUid;
                requestData["fromUsername"] = currentUsername;
                requestData["profileImageId"] = currentAvatarId;
                requestData["createdAt"] = FieldValue.ServerTimestamp;

                requestDoc.SetAsync(requestData)
                    .ContinueWithOnMainThread(requestSaveTask =>
                    {
                        if (requestSaveTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("FRIEND REQUEST SENT");

                            if (itemUI != null)
                                itemUI.SetSent();
                        }
                        else
                        {
                            Debug.LogError(
                                "SEND REQUEST ERROR: " +
                                requestSaveTask.Exception
                            );
                        }
                    });
            });
    }


    // =========================================
    // HELPERS
    // =========================================

    Sprite GetAvatar(int id)
    {
        if (avatars == null || avatars.Length == 0)
            return null;

        if (id < 0 || id >= avatars.Length)
            id = 0;

        return avatars[id];
    }


    void HideAllPanels()
    {
        if (friendsPanel != null)
            friendsPanel.SetActive(false);

        if (friendRequestsPanel != null)
            friendRequestsPanel.SetActive(false);

        if (searchPlayersPanel != null)
            searchPlayersPanel.SetActive(false);

        if (editProfilePanel != null)
            editProfilePanel.SetActive(false);
    }


    void ClearContent(Transform content)
    {
        if (content == null)
            return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}