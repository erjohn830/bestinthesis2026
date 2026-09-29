using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendRequestItemUI : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text usernameText;

    [Header("BUTTONS")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    private string senderUid;
    private string senderUsername;
    private int senderAvatarId;

    private ProfileSocialManager manager;


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        string uid,
        string username,
        int avatarId,
        Sprite avatar,
        ProfileSocialManager owner)
    {
        senderUid = uid;
        senderUsername = username;
        senderAvatarId = avatarId;
        manager = owner;


        // Username
        if (usernameText != null)
        {
            usernameText.text = username;
        }


        // Avatar
        if (profileImage != null)
        {
            profileImage.sprite = avatar;
        }


        // ACCEPT BUTTON
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(Accept);
        }
        else
        {
            Debug.LogError(
                "Accept Button is not assigned in FriendRequestItemUI."
            );
        }


        // DECLINE BUTTON
        if (declineButton != null)
        {
            declineButton.onClick.RemoveAllListeners();
            declineButton.onClick.AddListener(Decline);
        }
        else
        {
            Debug.LogError(
                "Decline Button is not assigned in FriendRequestItemUI."
            );
        }


        Debug.Log(
            "Friend request UI created from: " +
            username
        );
    }


    // =========================================================
    // ACCEPT
    // =========================================================

    private void Accept()
    {
        if (manager == null)
        {
            Debug.LogError(
                "ProfileSocialManager is missing."
            );

            return;
        }


        if (string.IsNullOrEmpty(senderUid))
        {
            Debug.LogError(
                "Sender UID is missing."
            );

            return;
        }


        manager.AcceptFriendRequest(
            senderUid,
            senderUsername,
            senderAvatarId
        );
    }


    // =========================================================
    // DECLINE
    // =========================================================

    private void Decline()
    {
        if (manager == null)
        {
            Debug.LogError(
                "ProfileSocialManager is missing."
            );

            return;
        }


        if (string.IsNullOrEmpty(senderUid))
        {
            Debug.LogError(
                "Sender UID is missing."
            );

            return;
        }


        manager.DeclineFriendRequest(
            senderUid
        );
    }
}