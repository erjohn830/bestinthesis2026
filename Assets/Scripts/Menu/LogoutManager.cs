using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    private FirebaseAuth auth;

    [Header("Logout UI")]
    public GameObject logoutConfirmPanel;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        if (logoutConfirmPanel != null)
        {
            logoutConfirmPanel.SetActive(false);
        }
    }

    public void ShowLogoutPanel()
    {
        if (logoutConfirmPanel != null)
        {
            logoutConfirmPanel.SetActive(true);
        }
    }

    public void CancelLogout()
    {
        if (logoutConfirmPanel != null)
        {
            logoutConfirmPanel.SetActive(false);
        }
    }

    public void ConfirmLogout()
    {
        if (auth != null)
        {
            auth.SignOut();
        }

        Debug.Log("User Logged Out");

        SceneManager.LoadScene("Login");
    }
}