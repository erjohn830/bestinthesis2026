using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatinteroRoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text serverNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text settingsText;
    [SerializeField] private Button joinButton;

    private string sessionName;
    private PatinteroMenuUI menuUI;

    public void Setup(
        SessionInfo session,
        PatinteroMenuUI ui)
    {
        menuUI = ui;

        // We use SessionInfo.Name as the Server Name.
        sessionName = session.Name;

        serverNameText.text =
            session.Name;

        playerCountText.text =
            "Players: " +
            session.PlayerCount +
            " / " +
            session.MaxPlayers;

        int minimum = 6;

        if (session.Properties.TryGetValue(
            "min",
            out SessionProperty minProperty))
        {
            if (minProperty.IsInt)
            {
                minimum =
                    (int)minProperty.PropertyValue;
            }
        }

        settingsText.text =
            "Min: " +
            minimum +
            "   Max: " +
            session.MaxPlayers;

        joinButton.interactable =
            session.IsOpen &&
            session.PlayerCount <
            session.MaxPlayers;

        joinButton.onClick.RemoveAllListeners();

        joinButton.onClick.AddListener(
            JoinThisRoom
        );
    }

    private void JoinThisRoom()
    {
        if (menuUI == null)
            return;

        menuUI.JoinSelectedRoom(
            sessionName
        );
    }
}