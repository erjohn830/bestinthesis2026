using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatinteroInviteFriendItem : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [SerializeField]
    private Image profileImage;

    [SerializeField]
    private TMP_Text friendNameText;

    [SerializeField]
    private Button inviteButton;

    [SerializeField]
    private TMP_Text sentText;


    // =========================================================
    // FRIEND DATA
    // =========================================================

    private string friendUid = "";

    private string friendName = "";

    private PatinteroInviteManager inviteManager;

    private bool inviteBeingSent = false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // Try to find missing references automatically.
        // You should STILL assign them in Inspector.

        if (profileImage == null)
        {
            Transform child =
                transform.Find("ProfileImage");

            if (child != null)
            {
                profileImage =
                    child.GetComponent<Image>();
            }
        }


        if (friendNameText == null)
        {
            Transform child =
                transform.Find("FriendNameText");

            if (child != null)
            {
                friendNameText =
                    child.GetComponent<TMP_Text>();
            }
        }


        if (inviteButton == null)
        {
            Transform child =
                transform.Find("InviteButton");

            if (child != null)
            {
                inviteButton =
                    child.GetComponent<Button>();
            }
        }


        if (sentText == null)
        {
            Transform child =
                transform.Find("SentText");

            if (child != null)
            {
                sentText =
                    child.GetComponent<TMP_Text>();
            }
        }


        ResetVisuals();
    }


    // =========================================================
    // SETUP
    // Called by PatinteroInviteManager for each Firebase friend.
    // =========================================================

    public void Setup(
        string uid,
        string displayName,
        Sprite profileSprite,
        PatinteroInviteManager manager)
    {
        friendUid =
            uid != null
            ? uid.Trim()
            : "";


        friendName =
            !string.IsNullOrWhiteSpace(displayName)
            ? displayName.Trim()
            : "Player";


        inviteManager =
            manager;


        inviteBeingSent =
            false;


        // -----------------------------------------------------
        // FRIEND NAME
        // -----------------------------------------------------

        if (friendNameText != null)
        {
            friendNameText.text =
                friendName;

            friendNameText.gameObject
                .SetActive(true);
        }
        else
        {
            Debug.LogError(
                "InviteFriendItem: FriendNameText is not assigned!",
                gameObject
            );
        }


        // -----------------------------------------------------
        // PROFILE IMAGE
        // -----------------------------------------------------

        if (profileImage != null)
        {
            if (profileSprite != null)
            {
                profileImage.sprite =
                    profileSprite;

                profileImage.gameObject
                    .SetActive(true);

                profileImage.enabled =
                    true;
            }
            else
            {
                // No avatar Sprite was found.
                // Hide the empty white square.
                profileImage.sprite =
                    null;

                profileImage.gameObject
                    .SetActive(false);
            }
        }


        // -----------------------------------------------------
        // SENT TEXT
        // -----------------------------------------------------

        if (sentText != null)
        {
            sentText.text =
                "INVITED";

            sentText.gameObject
                .SetActive(false);
        }


        // -----------------------------------------------------
        // INVITE BUTTON
        // -----------------------------------------------------

        if (inviteButton != null)
        {
            inviteButton.gameObject
                .SetActive(true);

            inviteButton.interactable =
                true;


            // Prevent duplicate listeners whenever the
            // prefab is reused/reconfigured.
            inviteButton.onClick
                .RemoveAllListeners();


            inviteButton.onClick
                .AddListener(
                    SendInvite
                );
        }
        else
        {
            Debug.LogError(
                "InviteFriendItem: InviteButton is not assigned!",
                gameObject
            );
        }


        Debug.Log(
            "Friend item created: " +
            friendName +
            " | UID: " +
            friendUid
        );
    }


    // =========================================================
    // SEND INVITE
    // =========================================================

    private void SendInvite()
    {
        // Prevent double clicks.
        if (inviteBeingSent)
        {
            return;
        }


        if (inviteManager == null)
        {
            Debug.LogError(
                "InviteFriendItem: PatinteroInviteManager is missing!",
                gameObject
            );

            ShowInviteFailed();

            return;
        }


        if (string.IsNullOrWhiteSpace(
            friendUid))
        {
            Debug.LogError(
                "InviteFriendItem: Friend UID is empty!",
                gameObject
            );

            ShowInviteFailed();

            return;
        }


        inviteBeingSent =
            true;


        if (inviteButton != null)
        {
            inviteButton.interactable =
                false;
        }


        Debug.Log(
            "Sending Patintero invite to: " +
            friendName +
            " | UID: " +
            friendUid
        );


        inviteManager.SendInviteToFriend(
            friendUid,
            friendName,
            this
        );
    }


    // =========================================================
    // INVITE SUCCESS
    // =========================================================

    public void ShowInviteSent()
    {
        inviteBeingSent =
            false;


        if (inviteButton != null)
        {
            inviteButton.interactable =
                false;

            inviteButton.gameObject
                .SetActive(false);
        }


        if (sentText != null)
        {
            sentText.text =
                "INVITED";

            sentText.gameObject
                .SetActive(true);
        }


        Debug.Log(
            "Invite successfully sent to: " +
            friendName
        );
    }


    // =========================================================
    // INVITE FAILED
    // =========================================================

    public void ShowInviteFailed()
    {
        inviteBeingSent =
            false;


        if (inviteButton != null)
        {
            inviteButton.gameObject
                .SetActive(true);

            inviteButton.interactable =
                true;
        }


        if (sentText != null)
        {
            sentText.gameObject
                .SetActive(false);
        }


        Debug.LogWarning(
            "Invite failed for: " +
            friendName
        );
    }


    // =========================================================
    // RESET VISUALS
    // =========================================================

    private void ResetVisuals()
    {
        inviteBeingSent =
            false;


        if (inviteButton != null)
        {
            inviteButton.gameObject
                .SetActive(true);

            inviteButton.interactable =
                true;

            inviteButton.onClick
                .RemoveAllListeners();
        }


        if (sentText != null)
        {
            sentText.gameObject
                .SetActive(false);
        }
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (inviteButton != null)
        {
            inviteButton.onClick
                .RemoveAllListeners();
        }
    }
}