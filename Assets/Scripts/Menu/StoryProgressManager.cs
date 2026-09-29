using UnityEngine;
using UnityEngine.SceneManagement;

using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

public class StoryProgressManager : MonoBehaviour
{
    public static StoryProgressManager instance;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    // Firebase save currently being played
    private string activeSaveId = "";

    // Saved progress
    private int savedStage = 0;

    // Current progress
    private int currentStage = 0;

    // Prevent double New Game clicks
    private bool creatingNewGame = false;


    // =========================================================
    // STORY SAVE INFORMATION
    // =========================================================

    public class StorySaveInfo
    {
        public string id;
        public int completedStage;
        public string currentGame;
        public int progressPercent;
        public DateTime createdAt;
    }


    // =========================================================
    // AUTO CREATE MANAGER
    // =========================================================

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreateManager()
    {
        if (instance != null)
            return;

        StoryProgressManager existing =
            FindFirstObjectByType<StoryProgressManager>();

        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject managerObject =
            new GameObject("StoryProgressManager");

        managerObject.AddComponent<StoryProgressManager>();
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }



    // =========================================
    // NEW GAME
    // =========================================

    public void NewGame()
    {
        if (creatingNewGame)
        {
            Debug.Log(
                "NEW GAME: Already creating a save."
            );

            return;
        }

        creatingNewGame = true;

        StartCoroutine(
            WaitForFirebaseAndCreateNewGame()
        );
    }


    // =========================================
    // WAIT FOR FIREBASE LOGIN
    // =========================================

    private IEnumerator WaitForFirebaseAndCreateNewGame()
    {
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        Debug.Log(
            "NEW GAME: Waiting for Firebase user..."
        );

        float timer = 0f;

        // Wait maximum 5 seconds
        while (
            auth.CurrentUser == null &&
            timer < 5f)
        {
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "NEW GAME FAILED: No Firebase user logged in."
            );

            creatingNewGame = false;

            yield break;
        }

        Debug.Log(
            "NEW GAME: Firebase user found = " +
            auth.CurrentUser.Email
        );

        CreateNewStorySave();
    }


    // =========================================
    // CREATE NEW STORY SAVE
    // =========================================

        private void CreateNewStorySave()
{
    if (auth == null ||
        auth.CurrentUser == null)
    {
        Debug.LogWarning(
            "NEW GAME: No Firebase user."
        );

        creatingNewGame = false;
        return;
    }

    if (db == null)
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    string uid =
        auth.CurrentUser.UserId;

    Dictionary<string, object> data =
        new Dictionary<string, object>();

    data["completedStage"] = 0;

    data["currentGame"] =
        "Hulihin ang Baboy";

    data["progressPercent"] = 25;

    data["createdAt"] =
        FieldValue.ServerTimestamp;

    data["updatedAt"] =
        FieldValue.ServerTimestamp;


    db.Collection("students")
      .Document(uid)
      .Collection("storySaves")
      .AddAsync(data)
      .ContinueWithOnMainThread(task =>
      {
          if (!task.IsCompletedSuccessfully)
          {
              Debug.LogError(
                  "NEW STORY SAVE ERROR: " +
                  task.Exception
              );

              creatingNewGame = false;
              return;
          }

          // Save currently being played
          activeSaveId =
              task.Result.Id;

          savedStage = 0;
          currentStage = 0;

          Debug.Log(
              "NEW STORY SAVE CREATED: " +
              activeSaveId
          );

          Debug.Log(
              "Hulihin ang Baboy - 25%"
          );

          // Sort saves and keep highest 3
          FetchTop3Saves(saves =>
          {
              creatingNewGame = false;

              Time.timeScale = 1f;

              // =================================
              // STORY LOADING SCREEN - 10 SECONDS
              // THEN OPEN HULIHIN STORYBOOK
              // =================================

              if (LoadingScreenManager.Instance != null)
              {
                  LoadingScreenManager.Instance
                      .LoadStoryScene(
                          "StoryBook_HulihinBiik"
                      );
              }
              else
              {
                  Debug.LogWarning(
                      "LoadingScreenManager not found."
                  );

                  SceneManager.LoadScene(
                      "StoryBook_HulihinBiik"
                  );
              }
          });
      });
}


    // =========================================
    // COMPLETE A GAME
    // =========================================

    public void CompleteGame(int stage)
    {
        if (stage < 1 || stage > 4)
        {
            Debug.LogWarning(
                "Invalid Story Stage: " +
                stage
            );

            return;
        }

        // Only update current session.
        // It is not saved until SAVE is pressed.
        if (stage > currentStage)
        {
            currentStage = stage;
        }

        Debug.Log(
            "Current Story Stage: " +
            currentStage
        );

        Debug.Log(
            "Current Game: " +
            GetGameName(currentStage)
        );

        Debug.Log(
            "Current Progress: " +
            GetPercentFromStage(currentStage) +
            "%"
        );
    }


    // =========================================
    // SAVE CURRENT PROGRESS
    // =========================================

    public void SaveProgress(
        Action onFinished = null)
    {
        StartCoroutine(
            WaitForFirebaseAndSave(
                onFinished
            )
        );
    }


    private IEnumerator WaitForFirebaseAndSave(
        Action onFinished)
    {
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        float timer = 0f;

        while (
            auth.CurrentUser == null &&
            timer < 5f)
        {
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "SAVE FAILED: No Firebase user."
            );

            yield break;
        }

        SaveProgressNow(
            onFinished
        );
    }


    private void SaveProgressNow(
        Action onFinished)
    {
        // If current save was removed because
        // it was below Top 3, create it again.
        if (string.IsNullOrEmpty(
            activeSaveId))
        {
            CreateSaveFromCurrentProgress(
                onFinished
            );

            return;
        }

        string uid =
            auth.CurrentUser.UserId;

        Dictionary<string, object> data =
            new Dictionary<string, object>();

        data["completedStage"] =
            currentStage;

        data["currentGame"] =
            GetGameName(currentStage);

        data["progressPercent"] =
            GetPercentFromStage(
                currentStage
            );

        data["updatedAt"] =
            FieldValue.ServerTimestamp;


        db.Collection("students")
          .Document(uid)
          .Collection("storySaves")
          .Document(activeSaveId)
          .SetAsync(
              data,
              SetOptions.MergeAll
          )
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsCompletedSuccessfully)
              {
                  Debug.LogError(
                      "STORY SAVE ERROR: " +
                      task.Exception
                  );

                  return;
              }

              savedStage =
                  currentStage;

              Debug.Log(
                  "STORY SAVED!"
              );

              Debug.Log(
                  GetGameName(savedStage) +
                  " - " +
                  GetPercentFromStage(savedStage) +
                  "%"
              );

              // Sort again after progress changed
              FetchTop3Saves(saves =>
              {
                  if (onFinished != null)
                  {
                      onFinished();
                  }
              });
          });
    }


    // =========================================
    // CREATE SAVE FROM CURRENT PROGRESS
    // =========================================

    private void CreateSaveFromCurrentProgress(
        Action onFinished)
    {
        if (auth == null ||
            auth.CurrentUser == null)
        {
            return;
        }

        string uid =
            auth.CurrentUser.UserId;

        Dictionary<string, object> data =
            new Dictionary<string, object>();

        data["completedStage"] =
            currentStage;

        data["currentGame"] =
            GetGameName(currentStage);

        data["progressPercent"] =
            GetPercentFromStage(
                currentStage
            );

        data["createdAt"] =
            FieldValue.ServerTimestamp;

        data["updatedAt"] =
            FieldValue.ServerTimestamp;


        db.Collection("students")
          .Document(uid)
          .Collection("storySaves")
          .AddAsync(data)
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsCompletedSuccessfully)
              {
                  Debug.LogError(
                      "CREATE SAVE ERROR: " +
                      task.Exception
                  );

                  return;
              }

              activeSaveId =
                  task.Result.Id;

              savedStage =
                  currentStage;

              Debug.Log(
                  "STORY SAVE CREATED: " +
                  activeSaveId
              );

              FetchTop3Saves(saves =>
              {
                  if (onFinished != null)
                  {
                      onFinished();
                  }
              });
          });
    }


    // =========================================
    // FETCH TOP 3 SAVES
    // =========================================

    public void FetchTop3Saves(
        Action<List<StorySaveInfo>> onFinished)
    {
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        // Firebase login may still be restoring
        if (auth.CurrentUser == null)
        {
            StartCoroutine(
                WaitForFirebaseAndFetch(
                    onFinished
                )
            );

            return;
        }

        FetchTop3SavesNow(
            onFinished
        );
    }


    private IEnumerator WaitForFirebaseAndFetch(
        Action<List<StorySaveInfo>> onFinished)
    {
        float timer = 0f;

        while (
            auth.CurrentUser == null &&
            timer < 5f)
        {
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "FETCH FAILED: No Firebase user."
            );

            if (onFinished != null)
            {
                onFinished(
                    new List<StorySaveInfo>()
                );
            }

            yield break;
        }

        FetchTop3SavesNow(
            onFinished
        );
    }


    private void FetchTop3SavesNow(
        Action<List<StorySaveInfo>> onFinished)
    {
        string uid =
            auth.CurrentUser.UserId;

        db.Collection("students")
          .Document(uid)
          .Collection("storySaves")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              List<StorySaveInfo> saves =
                  new List<StorySaveInfo>();

              if (!task.IsCompletedSuccessfully)
              {
                  Debug.LogError(
                      "STORY FETCH ERROR: " +
                      task.Exception
                  );

                  if (onFinished != null)
                  {
                      onFinished(saves);
                  }

                  return;
              }


              // =================================
              // READ ALL SAVES
              // =================================

              foreach (
                  DocumentSnapshot document
                  in task.Result.Documents)
              {
                  StorySaveInfo save =
                      new StorySaveInfo();

                  save.id =
                      document.Id;


                  // STAGE
                  if (document.ContainsField(
                      "completedStage"))
                  {
                      long value =
                          document.GetValue<long>(
                              "completedStage"
                          );

                      save.completedStage =
                          Mathf.Clamp(
                              Convert.ToInt32(value),
                              0,
                              4
                          );
                  }


                  // GAME NAME
                  if (document.ContainsField(
                      "currentGame"))
                  {
                      save.currentGame =
                          document.GetValue<string>(
                              "currentGame"
                          );
                  }
                  else
                  {
                      save.currentGame =
                          GetGameName(
                              save.completedStage
                          );
                  }


                  // PERCENT
                  if (document.ContainsField(
                      "progressPercent"))
                  {
                      long percent =
                          document.GetValue<long>(
                              "progressPercent"
                          );

                      save.progressPercent =
                          Convert.ToInt32(
                              percent
                          );
                  }
                  else
                  {
                      save.progressPercent =
                          GetPercentFromStage(
                              save.completedStage
                          );
                  }


                  // CREATED TIME
                  save.createdAt =
                      DateTime.MinValue;

                  if (document.ContainsField(
                      "createdAt"))
                  {
                      try
                      {
                          Timestamp timestamp =
                              document.GetValue<Timestamp>(
                                  "createdAt"
                              );

                          save.createdAt =
                              timestamp.ToDateTime();
                      }
                      catch
                      {
                          save.createdAt =
                              DateTime.MinValue;
                      }
                  }


                  saves.Add(save);
              }


              // =================================
              // SORT:
              // 100%
              // 75%
              // 50%
              // 25%
              //
              // If same percentage:
              // newest first
              // =================================

              saves.Sort(
                  (a, b) =>
                  {
                      int percentCompare =
                          b.progressPercent
                           .CompareTo(
                               a.progressPercent
                           );

                      if (percentCompare != 0)
                      {
                          return percentCompare;
                      }

                      int dateCompare =
                          b.createdAt
                           .CompareTo(
                               a.createdAt
                           );

                      if (dateCompare != 0)
                      {
                          return dateCompare;
                      }

                      return string.Compare(
                          b.id,
                          a.id,
                          StringComparison.Ordinal
                      );
                  }
              );


              // =================================
              // DELETE LOWEST IF MORE THAN 3
              // =================================

              if (saves.Count > 3)
              {
                  for (
                      int i = 3;
                      i < saves.Count;
                      i++)
                  {
                      string deleteId =
                          saves[i].id;

                      // If currently playing save
                      // gets deleted, clear its ID.
                      if (deleteId ==
                          activeSaveId)
                      {
                          activeSaveId = "";
                      }

                      db.Collection("students")
                        .Document(uid)
                        .Collection("storySaves")
                        .Document(deleteId)
                        .DeleteAsync();

                      Debug.Log(
                          "DELETED LOWEST SAVE: " +
                          saves[i].currentGame +
                          " - " +
                          saves[i].progressPercent +
                          "%"
                      );
                  }

                  saves.RemoveRange(
                      3,
                      saves.Count - 3
                  );
              }


              // =================================
              // DEBUG TOP 3
              // =================================

              Debug.Log(
                  "TOTAL STORY SAVES SHOWN: " +
                  saves.Count
              );

              for (
                  int i = 0;
                  i < saves.Count;
                  i++)
              {
                  Debug.Log(
                      (i + 1) +
                      ". " +
                      saves[i].currentGame +
                      " - " +
                      saves[i].progressPercent +
                      "%"
                  );
              }


              if (onFinished != null)
              {
                  onFinished(saves);
              }
          });
    }


    // =========================================
    // CONTINUE A SAVE SLOT
    // =========================================

    public void ContinueSave(
        string saveId)
    {
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "CONTINUE: No Firebase user."
            );

            return;
        }

        string uid =
            auth.CurrentUser.UserId;

        db.Collection("students")
          .Document(uid)
          .Collection("storySaves")
          .Document(saveId)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsCompletedSuccessfully)
              {
                  Debug.LogError(
                      "CONTINUE SAVE ERROR: " +
                      task.Exception
                  );

                  return;
              }

              DocumentSnapshot document =
                  task.Result;

              if (!document.Exists)
              {
                  Debug.LogWarning(
                      "Story Save no longer exists."
                  );

                  return;
              }

              if (!document.ContainsField(
                  "completedStage"))
              {
                  Debug.LogWarning(
                      "Story Save has no completedStage."
                  );

                  return;
              }


              long value =
                  document.GetValue<long>(
                      "completedStage"
                  );

              savedStage =
                  Mathf.Clamp(
                      Convert.ToInt32(value),
                      0,
                      4
                  );

              currentStage =
                  savedStage;

              activeSaveId =
                  document.Id;


              Debug.Log(
                  "CONTINUE SAVE: " +
                  activeSaveId
              );

              Debug.Log(
                  GetGameName(savedStage) +
                  " - " +
                  GetPercentFromStage(savedStage) +
                  "%"
              );


              LoadCurrentGame();
          });
    }


    // =========================================
    // LOAD CURRENT GAME
    // =========================================

    private void LoadCurrentGame()
{
    Time.timeScale = 1f;

    string sceneToLoad = "";

    if (savedStage == 0)
    {
        sceneToLoad = "StoryBook_HulihinBiik";
    }
    else if (savedStage == 1)
    {
        sceneToLoad = "StoryBook_Sipa";
    }
    else if (savedStage == 2)
    {
        sceneToLoad = "StoryBook_Palosebo";
    }
    else if (savedStage == 3)
    {
        sceneToLoad = "StoryBook_LuksongBaka";
    }
    else
    {
        Debug.Log(
            "Story already completed."
        );

        return;
    }

    if (LoadingScreenManager.Instance != null)
    {
        LoadingScreenManager.Instance
            .LoadStoryScene(
                sceneToLoad
            );
    }
    else
    {
        SceneManager.LoadScene(
            sceneToLoad
        );
    }
}


    // =========================================
    // GAME NAME
    // =========================================

    public string GetGameName(
        int stage)
    {
        if (stage == 0)
            return "Hulihin ang Baboy";

        if (stage == 1)
            return "Sipa";

        if (stage == 2)
            return "Palosebo";

        if (stage == 3)
            return "Luksong Baka";

        return "Story Completed";
    }


    // =========================================
    // PERCENT
    // =========================================

    public int GetPercentFromStage(
        int stage)
    {
        if (stage == 0)
            return 25;

        if (stage == 1)
            return 50;

        if (stage == 2)
            return 75;

        return 100;
    }


    // =========================================
    // SAVE + HOME
    // =========================================

public void SaveAndGoHome()
{
    StartCoroutine(
        SaveAndGoHomeRoutine()
    );
}

private IEnumerator SaveAndGoHomeRoutine()
{
    Time.timeScale = 1f;

    if (LoadingScreenManager.Instance != null)
    {
        LoadingScreenManager.Instance
            .ShowLoading(
                "Saving...",
                "Main"
            );

        LoadingScreenManager.Instance
            .SetProgress(0.15f);
    }
    else
    {
        Debug.LogWarning(
            "LoadingScreenManager not found."
        );
    }

    // Very important:
    // allow one frame for loading UI to appear
    yield return null;

    SaveProgress(() =>
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance
                .SetProgress(0.5f);

            LoadingScreenManager.Instance
                .LoadScene(
                    "Main",
                    "Loading..."
                );
        }
        else
        {
            SceneManager.LoadScene(
                "Main"
            );
        }
    });
}


    // =========================================
    // DON'T SAVE + HOME
    // =========================================

    public void DontSaveAndGoHome()
    {
        // Remove unsaved progress
        currentStage =
            savedStage;

        Time.timeScale = 1f;

        Debug.Log(
            "Story progress NOT saved."
        );

        SceneManager.LoadScene(
            "Main"
        );
    }


    // =========================================
    // GET ACTIVE SAVE
    // =========================================

    public string GetActiveSaveId()
    {
        return activeSaveId;
    }


    // =========================================
    // GET CURRENT STAGE
    // =========================================

    public int GetCurrentStage()
    {
        return currentStage;
    }


    // =========================================
    // GET SAVED STAGE
    // =========================================

    public int GetSavedStage()
    {
        return savedStage;
    }

    // =========================================
// LOAD STORY SCENE WITH LOADING SCREEN
// =========================================

private void LoadStoryScene(string sceneName)
{
    Time.timeScale = 1f;

    if (LoadingScreenManager.Instance != null)
    {
        LoadingScreenManager.Instance.LoadScene(
            sceneName,
            "Loading..."
        );
    }
    else
    {
        SceneManager.LoadScene(sceneName);
    }
}
}