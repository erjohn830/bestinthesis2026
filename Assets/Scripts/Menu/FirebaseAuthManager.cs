using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Login")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Create Account")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Status")]
    public TMP_Text statusText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        ShowLogin();

        if (statusText != null)
            statusText.text = "";

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    db = FirebaseFirestore.DefaultInstance;

                    Debug.Log("Firebase Ready");

                    CheckSavedLogin();
                }
                else
                {
                    Debug.LogError(
                        "Firebase Error: " + task.Result
                    );

                    if (statusText != null)
                        statusText.text = "Firebase connection failed.";

                    ShowLogin();
                }
            });
    }

    // =========================
    // SAVED LOGIN
    // =========================
    void CheckSavedLogin()
    {
        if (auth != null && auth.CurrentUser != null)
        {
            Debug.Log(
                "Already logged in: " +
                auth.CurrentUser.Email
            );

            SceneManager.LoadScene("Main");
        }
        else
        {
            ShowLogin();
        }
    }

    // =========================
    // LOGIN
    // =========================
    public async void Login()
    {
        if (auth == null)
        {
            Debug.LogWarning("Firebase is not ready.");

            if (statusText != null)
                statusText.text = "Please wait. Firebase is loading.";

            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password))
        {
            if (statusText != null)
                statusText.text = "Enter email and password.";

            return;
        }

        try
        {
            if (statusText != null)
                statusText.text = "Logging in...";

            AuthResult result =
                await auth.SignInWithEmailAndPasswordAsync(
                    email,
                    password
                );

            if (result.User != null)
            {
                Debug.Log("Login Success");

                if (statusText != null)
                    statusText.text = "Login successful!";

                SceneManager.LoadScene("Main");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "Login Failed: " + e
            );

            if (statusText != null)
                statusText.text =
                    "Login failed:\n" + GetSimpleError(e);
        }
    }

    // =========================
    // SHOW CREATE ACCOUNT
    // =========================
    public void ShowRegister()
    {
        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (registerPanel != null)
            registerPanel.SetActive(true);

        if (statusText != null)
            statusText.text = "";

        Debug.Log("Create Account Panel Opened");
    }

    // =========================
    // SHOW LOGIN
    // =========================
    public void ShowLogin()
    {
        if (registerPanel != null)
            registerPanel.SetActive(false);

        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (statusText != null)
            statusText.text = "";
    }

    // =========================
    // CREATE ACCOUNT
    // =========================
    public async void CreateAccount()
    {
        Debug.Log("CREATE ACCOUNT BUTTON PRESSED");

        if (statusText != null)
            statusText.text = "Checking...";

        if (auth == null || db == null)
        {
            Debug.LogWarning("Firebase is not ready.");

            if (statusText != null)
                statusText.text =
                    "Firebase is still loading. Try again.";

            return;
        }

        if (registerUsernameInput == null ||
            registerEmailInput == null ||
            registerPasswordInput == null)
        {
            Debug.LogError(
                "Create Account input field is missing!"
            );

            if (statusText != null)
                statusText.text =
                    "Input fields are not assigned.";

            return;
        }

        string username =
            registerUsernameInput.text.Trim();

        string email =
            registerEmailInput.text.Trim();

        string password =
            registerPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            statusText.text =
                "Please enter a username.";

            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            statusText.text =
                "Please enter an email.";

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            statusText.text =
                "Please enter a password.";

            return;
        }

        if (password.Length < 6)
        {
            statusText.text =
                "Password must be at least 6 characters.";

            return;
        }

        try
        {
            statusText.text =
                "Creating account...";

            // STEP 1: Firebase Authentication
            AuthResult result =
                await auth.CreateUserWithEmailAndPasswordAsync(
                    email,
                    password
                );

            FirebaseUser user =
                result.User;

            if (user == null)
            {
                statusText.text =
                    "Account could not be created.";

                return;
            }

            Debug.Log(
                "AUTH ACCOUNT CREATED: " +
                user.UserId
            );

            statusText.text =
                "Saving profile...";

            // STEP 2: Firestore profile
            Dictionary<string, object> userData =
                new Dictionary<string, object>();

            userData["uid"] =
                user.UserId;

            userData["username"] =
                username;

            userData["email"] =
                email;

            userData["createdAt"] =
                FieldValue.ServerTimestamp;

            await db.Collection("students")
                    .Document(user.UserId)
                    .SetAsync(userData);

            Debug.Log(
                "FIRESTORE PROFILE SAVED"
            );

            statusText.text =
                "Account created successfully!";

            // Go to Main
            SceneManager.LoadScene("Main");
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "CREATE ACCOUNT ERROR: " +
                e
            );

            if (statusText != null)
            {
                statusText.text =
                    "Create account failed:\n" +
                    GetSimpleError(e);
            }
        }
    }

    // =========================
    // SIMPLE ERROR MESSAGE
    // =========================
    string GetSimpleError(System.Exception e)
    {
        string message = e.Message;

        if (e.InnerException != null)
            message = e.InnerException.Message;

        if (message.Length > 120)
            message =
                message.Substring(0, 120);

        return message;
    }
}