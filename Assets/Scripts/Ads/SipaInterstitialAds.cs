using UnityEngine;
using Unity.Services.LevelPlay;

public class SipaRewardedAds : MonoBehaviour
{
    [Header("LEVELPLAY - ANDROID")]
    [SerializeField] private string appKey;
    [SerializeField] private string rewardedAdUnitId;

    [Header("SIPA GAME")]
    [SerializeField] private SipaManager sipaManager;

    [Header("UI")]
    [SerializeField] private GameObject losePanel;

    private LevelPlayRewardedAd rewardedAd;

    private bool waitingToContinue = false;
    private bool rewardEarned = false;
    private bool adClosed = false;
    private bool rewardAlreadyGiven = false;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (sipaManager == null)
        {
            sipaManager = SipaManager.Instance;
        }

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;


#if UNITY_EDITOR

        Debug.Log(
            "EDITOR MODE - Initializing LevelPlay mock ads..."
        );

        LevelPlay.Init("editor");


#elif UNITY_ANDROID

        // =====================================================
        // CHILD-DIRECTED SETTINGS
        // TARGET AUDIENCE: 9-12 YEARS OLD
        //
        // MUST RUN BEFORE LevelPlay.Init()
        // =====================================================

        Debug.Log(
            "Applying Sipa child-directed ad settings..."
        );

        // Tell LevelPlay this user/app is child-directed.
        LevelPlayPrivacySettings.SetCOPPA(true);

        // Prevent access to Android Advertising ID.
        LevelPlay.SetMetaData(
            "is_deviceid_optout",
            "true"
        );

        // Tell Unity Ads to serve non-behavioral ads.
        LevelPlay.SetMetaData(
            "UnityAds_coppa",
            "true"
        );


        // =====================================================
        // CHECK LEVELPLAY IDS
        // =====================================================

        if (string.IsNullOrWhiteSpace(appKey))
        {
            Debug.LogError(
                "SIPA ADS ERROR: LevelPlay App Key is missing!"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogError(
                "SIPA ADS ERROR: Rewarded Ad Unit ID is missing!"
            );

            return;
        }


        Debug.Log(
            "ANDROID MODE - Initializing Sipa LevelPlay..."
        );

        LevelPlay.Init(appKey);


#else

        Debug.LogWarning(
            "SipaRewardedAds is currently configured for Android."
        );

#endif
    }


    // =========================================================
    // LEVELPLAY INITIALIZATION SUCCESS
    // =========================================================

    private void OnInitSuccess(
        LevelPlayConfiguration configuration)
    {
        Debug.Log(
            "SIPA LEVELPLAY INITIALIZED SUCCESSFULLY!"
        );


#if UNITY_EDITOR

        rewardedAd =
            new LevelPlayRewardedAd(
                "editor_rewarded"
            );


#elif UNITY_ANDROID

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogError(
                "SIPA Rewarded Ad Unit ID is missing!"
            );

            return;
        }

        rewardedAd =
            new LevelPlayRewardedAd(
                rewardedAdUnitId
            );

#else

        return;

#endif


        // Subscribe to rewarded ad events.
        rewardedAd.OnAdLoaded +=
            OnAdLoaded;

        rewardedAd.OnAdLoadFailed +=
            OnAdLoadFailed;

        rewardedAd.OnAdDisplayed +=
            OnAdDisplayed;

        rewardedAd.OnAdDisplayFailed +=
            OnAdDisplayFailed;

        rewardedAd.OnAdRewarded +=
            OnAdRewarded;

        rewardedAd.OnAdClosed +=
            OnAdClosed;


        // Load first rewarded ad.
        LoadAd();
    }


    // =========================================================
    // LEVELPLAY INITIALIZATION FAILED
    // =========================================================

    private void OnInitFailed(
        LevelPlayInitError error)
    {
        Debug.LogError(
            "SIPA LEVELPLAY INIT FAILED: " +
            error
        );
    }


    // =========================================================
    // LOAD REWARDED AD
    // =========================================================

    private void LoadAd()
    {
        if (rewardedAd == null)
        {
            Debug.LogWarning(
                "Cannot load Sipa rewarded ad. rewardedAd is null."
            );

            return;
        }

        Debug.Log(
            "LOADING SIPA REWARDED AD..."
        );

        rewardedAd.LoadAd();
    }


    // =========================================================
    // ADS BUTTON
    //
    // Lose
    // ↓
    // Player presses ADS
    // ↓
    // Watch rewarded advertisement
    // ↓
    // Continue current level
    // =========================================================

    public void ShowAdThenContinue()
    {
#if UNITY_EDITOR

        Debug.LogWarning(
            "Sipa Android rewarded ads should be tested " +
            "on an Android device."
        );

        return;


#elif UNITY_ANDROID

        Debug.Log(
            "SIPA REWARDED ADS BUTTON PRESSED"
        );


        if (rewardedAd == null)
        {
            Debug.LogWarning(
                "Sipa rewarded ad has not initialized yet."
            );

            return;
        }


        if (!rewardedAd.IsAdReady())
        {
            Debug.LogWarning(
                "Sipa rewarded ad is not ready yet. " +
                "Trying to load again..."
            );

            LoadAd();

            // Do NOT continue for free.
            return;
        }


        // Reset reward state.
        waitingToContinue = true;
        rewardEarned = false;
        adClosed = false;
        rewardAlreadyGiven = false;


        // Freeze game while advertisement is open.
        Time.timeScale = 0f;


        Debug.Log(
            "SIPA AD READY - SHOWING REWARDED AD..."
        );


        rewardedAd.ShowAd();

#endif
    }


    // =========================================================
    // AD LOADED
    // =========================================================

    private void OnAdLoaded(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "SIPA REWARDED AD READY!"
        );
    }


    // =========================================================
    // AD LOAD FAILED
    // =========================================================

    private void OnAdLoadFailed(
        LevelPlayAdError error)
    {
        Debug.LogError(
            "SIPA REWARDED AD LOAD FAILED: " +
            error
        );
    }


    // =========================================================
    // AD DISPLAYED
    // =========================================================

    private void OnAdDisplayed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "SIPA REWARDED AD DISPLAYED!"
        );
    }


    // =========================================================
    // AD DISPLAY FAILED
    // =========================================================

    private void OnAdDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogError(
            "SIPA REWARDED DISPLAY FAILED: " +
            error
        );


        ResetRewardState();

        Time.timeScale = 1f;


        // Prepare another ad.
        LoadAd();
    }


    // =========================================================
    // PLAYER COMPLETED AD
    // =========================================================

    private void OnAdRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
        Debug.Log(
            "SIPA PLAYER EARNED REWARD: " +
            reward.Name +
            " x" +
            reward.Amount
        );


        if (!waitingToContinue)
        {
            return;
        }


        rewardEarned = true;


        // Reward callback and close callback
        // can happen in different order.
        TryContinueGame();
    }


    // =========================================================
    // AD CLOSED
    // =========================================================

    private void OnAdClosed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "SIPA REWARDED AD CLOSED!"
        );


        adClosed = true;

        Time.timeScale = 1f;


        TryContinueGame();


        // Prepare next rewarded ad.
        LoadAd();
    }


    // =========================================================
    // CHECK IF PLAYER CAN CONTINUE
    // =========================================================

    private void TryContinueGame()
    {
        // ADS button must have been pressed.
        if (!waitingToContinue)
        {
            return;
        }


        // Player must actually earn the reward.
        if (!rewardEarned)
        {
            return;
        }


        // Ad must be fully closed.
        if (!adClosed)
        {
            return;
        }


        // Prevent double reward.
        if (rewardAlreadyGiven)
        {
            return;
        }


        rewardAlreadyGiven = true;
        waitingToContinue = false;


        ContinueNow();
    }


    // =========================================================
    // CONTINUE CURRENT SIPA LEVEL
    // =========================================================

    private void ContinueNow()
    {
        Debug.Log(
            "REWARDED AD COMPLETE - CONTINUING SIPA LEVEL..."
        );


        Time.timeScale = 1f;


        // Hide TALO panel.
        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }


        // Find SipaManager if Inspector reference is missing.
        if (sipaManager == null)
        {
            sipaManager = SipaManager.Instance;
        }


        if (sipaManager != null)
        {
            // Continue current level/progress.
            sipaManager.ContinueAfterAd();
        }
        else
        {
            Debug.LogError(
                "SipaManager was not found!"
            );
        }


        rewardEarned = false;
        adClosed = false;
    }


    // =========================================================
    // RESET REWARD STATE
    // =========================================================

    private void ResetRewardState()
    {
        waitingToContinue = false;
        rewardEarned = false;
        adClosed = false;
        rewardAlreadyGiven = false;
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -=
            OnInitSuccess;

        LevelPlay.OnInitFailed -=
            OnInitFailed;


        if (rewardedAd != null)
        {
            rewardedAd.OnAdLoaded -=
                OnAdLoaded;

            rewardedAd.OnAdLoadFailed -=
                OnAdLoadFailed;

            rewardedAd.OnAdDisplayed -=
                OnAdDisplayed;

            rewardedAd.OnAdDisplayFailed -=
                OnAdDisplayFailed;

            rewardedAd.OnAdRewarded -=
                OnAdRewarded;

            rewardedAd.OnAdClosed -=
                OnAdClosed;
        }


        Time.timeScale = 1f;
    }
}