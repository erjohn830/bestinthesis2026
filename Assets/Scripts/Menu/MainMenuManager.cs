using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject profilePanel;
    public GameObject playGamePanel;
    public GameObject storyModePanel;
    public GameObject multiplayerPanel;
    public GameObject lobbyPanel;

    [Header("Profile")]
    public TMP_Text playerNameText;

    [Header("Lobby")]
    public TMP_Text selectedGameText;

    [Header("Audio Settings")]
    public Slider musicSlider;
    public Slider sfxSlider;

    // =========================================
    // STORY SAVE SLOTS
    // =========================================

    [Header("Story Save Slots")]
    public StorySaveSlotUI storySave1;
    public StorySaveSlotUI storySave2;
    public StorySaveSlotUI storySave3;

    private string selectedGame;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        Time.timeScale = 1f;

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        SetupAudioSliders();

        HideStorySaveSlots();

        OpenMainMenu();
    }

    // =========================================
    // AUDIO SETTINGS
    // =========================================

    void SetupAudioSliders()
    {
        if (AudioSettingsManager.Instance == null)
            return;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;

            musicSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetMusicVolume()
            );
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;

            sfxSlider.SetValueWithoutNotify(
                AudioSettingsManager.Instance
                    .GetSFXVolume()
            );
        }
    }

    public void ChangeMusicVolume(float value)
    {
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance
                .SetMusicVolume(value);
        }
    }

    public void ChangeSFXVolume(float value)
    {
        if (AudioSettingsManager.Instance != null)
        {
            AudioSettingsManager.Instance
                .SetSFXVolume(value);
        }
    }

    // =========================================
    // PANELS
    // =========================================

    void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        profilePanel.SetActive(false);
        playGamePanel.SetActive(false);
        storyModePanel.SetActive(false);
        multiplayerPanel.SetActive(false);
        lobbyPanel.SetActive(false);
    }

    // =========================================
    // MAIN MENU
    // =========================================

    public void OpenMainMenu()
    {
        HideAllPanels();

        mainMenuPanel.SetActive(true);
    }

    // =========================================
    // PROFILE
    // =========================================

    public void OpenProfile()
    {
        HideAllPanels();

        profilePanel.SetActive(true);

        if (playerNameText != null)
        {
            playerNameText.text =
                "Loading...";
        }

        StartCoroutine(
            WaitAndLoadProfile()
        );
    }

    IEnumerator WaitAndLoadProfile()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        float waitTime = 0f;

        while (
            auth.CurrentUser == null &&
            waitTime < 5f)
        {
            waitTime += Time.deltaTime;

            yield return null;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "PROFILE: No Firebase user logged in."
            );

            if (playerNameText != null)
            {
                playerNameText.text =
                    "Name: No User";
            }

            yield break;
        }

        LoadProfile();
    }

    void LoadProfile()
    {
        FirebaseUser user =
            auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning(
                "PROFILE: Firebase user is null."
            );

            return;
        }

        string uid =
            user.UserId;

        db.Collection("students")
          .Document(uid)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted)
              {
                  Debug.LogError(
                      "PROFILE FIRESTORE ERROR: " +
                      task.Exception
                  );

                  if (playerNameText != null)
                  {
                      playerNameText.text =
                          "Name: Error";
                  }

                  return;
              }

              if (!task.IsCompletedSuccessfully)
              {
                  return;
              }

              DocumentSnapshot document =
                  task.Result;

              if (!document.Exists)
              {
                  if (playerNameText != null)
                  {
                      playerNameText.text =
                          "Name: Player";
                  }

                  return;
              }

              if (document.ContainsField(
                  "username"))
              {
                  string username =
                      document.GetValue<string>(
                          "username"
                      );

                  if (playerNameText != null)
                  {
                      playerNameText.text =
                          "Name: " +
                          username;
                  }
              }
              else
              {
                  if (playerNameText != null)
                  {
                      playerNameText.text =
                          "Name: Player";
                  }
              }
          });
    }

    // =========================================
    // PLAY GAME
    // =========================================

    public void OpenPlayGame()
    {
        HideAllPanels();

        playGamePanel.SetActive(true);
    }

    // =========================================
    // STORY MODE
    // =========================================

    public void OpenStoryMode()
    {
        HideAllPanels();

        storyModePanel.SetActive(true);

        RefreshStorySaves();
    }

    // =========================================
    // HIDE ALL SAVE SLOTS
    // =========================================

    void HideStorySaveSlots()
    {
        if (storySave1 != null)
        {
            storySave1.HideSlot();
        }

        if (storySave2 != null)
        {
            storySave2.HideSlot();
        }

        if (storySave3 != null)
        {
            storySave3.HideSlot();
        }
    }

    // =========================================
    // FETCH STORY SAVES FROM FIREBASE
    // =========================================

    public void RefreshStorySaves()
    {
        HideStorySaveSlots();

        if (StoryProgressManager.instance == null)
        {
            Debug.LogWarning(
                "StoryProgressManager not found."
            );

            return;
        }

        Debug.Log(
            "Fetching Story Saves from Firebase..."
        );

        StoryProgressManager.instance
            .FetchTop3Saves(
                DisplayStorySaves
            );
    }

    // =========================================
    // DISPLAY TOP 3 SAVES
    // =========================================

    void DisplayStorySaves(
        List<StoryProgressManager.StorySaveInfo> saves)
    {
        Debug.Log(
            "Story saves loaded: " +
            saves.Count
        );

        // Highest percentage
        if (
            saves.Count > 0 &&
            storySave1 != null)
        {
            storySave1.Setup(
                saves[0]
            );
        }

        // Second highest
        if (
            saves.Count > 1 &&
            storySave2 != null)
        {
            storySave2.Setup(
                saves[1]
            );
        }

        // Third highest
        if (
            saves.Count > 2 &&
            storySave3 != null)
        {
            storySave3.Setup(
                saves[2]
            );
        }
    }

    // =========================================
    // NEW STORY GAME
    // =========================================

    public void StartNewStory()
    {
        if (StoryProgressManager.instance == null)
        {
            Debug.LogWarning(
                "StoryProgressManager not found."
            );

            return;
        }

        Debug.Log(
            "Creating new Story Save..."
        );

        StoryProgressManager.instance
            .NewGame();
    }

    // =========================================
    // MULTIPLAYER
    // =========================================

    public void OpenMultiplayer()
    {
        HideAllPanels();

        multiplayerPanel.SetActive(true);
    }

    // =========================================
    // SELECT GAME
    // =========================================

    public void SelectPatintero()
    {
        selectedGame =
            "Patintero";

        OpenLobby();
    }

    public void SelectSekyuBase()
    {
        selectedGame =
            "Sekyu Base";

        OpenLobby();
    }

    public void SelectBamsak()
    {
        selectedGame =
            "Bamsak";

        OpenLobby();
    }

    // =========================================
    // LOBBY
    // =========================================

    void OpenLobby()
    {
        HideAllPanels();

        lobbyPanel.SetActive(true);

        if (selectedGameText != null)
        {
            selectedGameText.text =
                selectedGame;
        }
    }

    public string GetSelectedGame()
    {
        return selectedGame;
    }

    public void BackToMultiplayer()
    {
        HideAllPanels();

        multiplayerPanel.SetActive(true);
    }
}