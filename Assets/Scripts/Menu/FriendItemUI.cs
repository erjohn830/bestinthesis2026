using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendItemUI : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [SerializeField]
    private Image profileImage;

    [SerializeField]
    private TMP_Text usernameText;

    [SerializeField]
    private TMP_Text statusText;


    // =========================================================
    // FRIEND DATA
    // =========================================================

    private string friendUid = "";

    private string friendUsername = "";


    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        string uid,
        string username,
        Sprite avatar)
    {
        friendUid =
            string.IsNullOrWhiteSpace(uid)
            ? ""
            : uid.Trim();


        friendUsername =
            string.IsNullOrWhiteSpace(username)
            ? "Player"
            : username.Trim();


        // -----------------------------------------------------
        // USERNAME
        // -----------------------------------------------------

        if (usernameText != null)
        {
            usernameText.text =
                friendUsername;
        }


        // -----------------------------------------------------
        // PROFILE IMAGE
        // -----------------------------------------------------

        if (profileImage != null)
        {
            if (avatar != null)
            {
                profileImage.sprite =
                    avatar;

                profileImage.gameObject
                    .SetActive(true);
            }
            else
            {
                profileImage.sprite =
                    null;

                profileImage.gameObject
                    .SetActive(false);
            }
        }


        // -----------------------------------------------------
        // STATUS
        // -----------------------------------------------------

        if (statusText != null)
        {
            statusText.text =
                "FRIEND";
        }


        Debug.Log(
            "Friend loaded: " +
            friendUsername +
            " | UID: " +
            friendUid
        );
    }


    // =========================================================
    // VIEW FRIEND
    // =========================================================

    public void ViewFriend()
    {
        if (string.IsNullOrWhiteSpace(
            friendUid))
        {
            Debug.LogWarning(
                "Friend UID is empty."
            );

            return;
        }


        Debug.Log(
            "VIEW FRIEND: " +
            friendUsername +
            " | UID: " +
            friendUid
        );


        // Later you can open a FriendProfilePanel here.
    }
}