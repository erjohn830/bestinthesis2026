using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SearchPlayerItemUI : MonoBehaviour
{
    [Header("PLAYER")]
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text usernameText;

    [Header("FRIEND BUTTON")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Image actionButtonImage;

    [Header("BUTTON SPRITES")]
    [SerializeField] private Sprite addFriendSprite;
    [SerializeField] private Sprite cancelRequestSprite;
    [SerializeField] private Sprite friendSprite;

    private string targetUid;
    private ProfileSocialManager manager;

    private FriendButtonState currentState =
        FriendButtonState.AddFriend;


    private enum FriendButtonState
    {
        AddFriend,
        CancelRequest,
        Friend
    }


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
        targetUid = uid;
        manager = owner;

        if (usernameText != null)
        {
            usernameText.text = username;
        }

        if (profileImage != null)
        {
            profileImage.sprite = avatar;
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        SetAddFriend();

        // Check if:
        // already friend
        // request already sent
        if (manager != null)
        {
            manager.CheckRelationshipState(
                targetUid,
                this
            );
        }
    }


    // =========================================================
    // BUTTON CLICK
    // =========================================================

    private void OnActionButtonClicked()
    {
        if (manager == null)
        {
            Debug.LogError(
                "ProfileSocialManager is missing."
            );

            return;
        }

        if (string.IsNullOrEmpty(targetUid))
        {
            Debug.LogError(
                "Target UID is missing."
            );

            return;
        }


        // ADD FRIEND
        if (currentState ==
            FriendButtonState.AddFriend)
        {
            manager.SendFriendRequest(
                targetUid,
                this
            );

            return;
        }


        // CANCEL REQUEST
        if (currentState ==
            FriendButtonState.CancelRequest)
        {
            manager.CancelFriendRequest(
                targetUid,
                this
            );

            return;
        }


        // FRIEND
        if (currentState ==
            FriendButtonState.Friend)
        {
            Debug.Log(
                "This player is already your friend."
            );
        }
    }


    // =========================================================
    // ADD FRIEND STATE
    // =========================================================

    public void SetAddFriend()
    {
        currentState =
            FriendButtonState.AddFriend;

        if (actionButtonImage != null &&
            addFriendSprite != null)
        {
            actionButtonImage.sprite =
                addFriendSprite;
        }

        if (actionButton != null)
        {
            actionButton.interactable = true;
        }
    }


    // =========================================================
    // REQUEST SENT / CANCEL STATE
    // =========================================================

    public void SetSent()
    {
        currentState =
            FriendButtonState.CancelRequest;

        if (actionButtonImage != null &&
            cancelRequestSprite != null)
        {
            actionButtonImage.sprite =
                cancelRequestSprite;
        }

        if (actionButton != null)
        {
            actionButton.interactable = true;
        }
    }


    // =========================================================
    // FRIEND STATE
    // =========================================================

    public void SetFriend()
    {
        currentState =
            FriendButtonState.Friend;

        if (actionButtonImage != null &&
            friendSprite != null)
        {
            actionButtonImage.sprite =
                friendSprite;
        }

        if (actionButton != null)
        {
            actionButton.interactable = false;
        }
    }
}