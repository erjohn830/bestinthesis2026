using UnityEngine;
using Unity.Services.LevelPlay;

public class RewardedAdsManager : MonoBehaviour
{
    [Header("LEVELPLAY - ANDROID")]
    [SerializeField] private string appKey;
    [SerializeField] private string rewardedAdUnitId;

    [Header("GAME REFERENCES")]
    [Tooltip("Drag the TALO / LosePanel here.")]
    [SerializeField] private GameObject losePanel;

    [Tooltip("Drag BiikManager here.")]
    [SerializeField] private BiikManager biikManager;

    private LevelPlayRewardedAd rewardedAd;

    private bool waitingForReward = false;
    private bool rewardEarned = false;
    private bool adClosed = false;
    private bool rewardAlreadyGiven = false;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (biikManager == null)
        {
            biikManager = FindFirstObjectByType<BiikManager>();
        }

        // Subscribe before initializing LevelPlay.
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

#if UNITY_EDITOR

        Debug.Log(
            "EDITOR MODE - Android rewarded ads are disabled."
        );

#elif UNITY_ANDROID

        // =====================================================
        // CHILD-DIRECTED / COPPA SETTINGS
        // GAME TARGET AUDIENCE: AGES 9-12
        //
        // IMPORTANT:
        // These settings must happen BEFORE LevelPlay.Init().
        // =====================================================

        Debug.Log(
            "Applying child-directed LevelPlay settings..."
        );

        // LevelPlay 9.4+ recommended COPPA API.
        LevelPlayPrivacySettings.SetCOPPA(true);

        // Prevent access to Android Advertising ID (AAID/GAID)
        // for this child-directed application.
        LevelPlay.SetMetaData(
            "is_deviceid_optout",
            "true"
        );

        // =====================================================
        // VALIDATE LEVELPLAY IDS
        // =====================================================

        if (string.IsNullOrWhiteSpace(appKey))
        {
            Debug.LogError(
                "LEVELPLAY ERROR: App Key is empty!"
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogError(
                "LEVELPLAY ERROR: Rewarded Ad Unit ID is empty!"
            );

            return;
        }

        Debug.Log(
            "ANDROID MODE - Initializing LevelPlay..."
        );

        LevelPlay.Init(appKey);

#else

        Debug.LogWarning(
            "RewardedAdsManager is currently configured for Android."
        );

#endif
    }

    // =========================================================
    // LEVELPLAY INITIALIZED
    // =========================================================

    private void OnInitSuccess(
        LevelPlayConfiguration configuration)
    {
        Debug.Log(
            "LEVELPLAY INITIALIZED SUCCESSFULLY"
        );

#if UNITY_ANDROID && !UNITY_EDITOR

        if (string.IsNullOrWhiteSpace(rewardedAdUnitId))
        {
            Debug.LogError(
                "Rewarded Ad Unit ID is missing."
            );

            return;
        }

        // Create LevelPlay rewarded ad object.
        rewardedAd =
            new LevelPlayRewardedAd(
                rewardedAdUnitId
            );

        // Subscribe rewarded ad events.
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
        LoadRewardedAd();

#endif
    }

    // =========================================================
    // LEVELPLAY INITIALIZATION FAILED
    // =========================================================

    private void OnInitFailed(
        LevelPlayInitError error)
    {
        Debug.LogError(
            "LEVELPLAY INIT FAILED: " +
            error
        );
    }

    // =========================================================
    // LOAD REWARDED AD
    // =========================================================

    private void LoadRewardedAd()
    {
        if (rewardedAd == null)
        {
            Debug.LogWarning(
                "Cannot load rewarded ad because rewardedAd is null."
            );

            return;
        }

        Debug.Log(
            "LOADING REWARDED AD..."
        );

        rewardedAd.LoadAd();
    }

    // =========================================================
    // ADS / WATCH AD BUTTON
    // =========================================================

    public void ShowRewardedAd()
    {
#if UNITY_EDITOR

        Debug.LogWarning(
            "Rewarded Android ads must be tested on an Android device."
        );

        return;

#elif UNITY_ANDROID

        Debug.Log(
            "WATCH AD BUTTON PRESSED"
        );

        if (rewardedAd == null)
        {
            Debug.LogWarning(
                "Rewarded ad has not initialized yet."
            );

            return;
        }

        if (!rewardedAd.IsAdReady())
        {
            Debug.LogWarning(
                "Rewarded ad is not ready yet. Trying to load again..."
            );

            LoadRewardedAd();

            return;
        }

        // Reset reward state.
        waitingForReward = true;
        rewardEarned = false;
        adClosed = false;
        rewardAlreadyGiven = false;

        // Freeze gameplay behind the advertisement.
        Time.timeScale = 0f;

        Debug.Log(
            "SHOWING REWARDED AD..."
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
            "REWARDED AD READY"
        );
    }

    // =========================================================
    // AD LOAD FAILED
    // =========================================================

    private void OnAdLoadFailed(
        LevelPlayAdError error)
    {
        Debug.LogError(
            "REWARDED AD LOAD FAILED: " +
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
            "REWARDED AD DISPLAYED"
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
            "REWARDED AD DISPLAY FAILED: " +
            error
        );

        ResetRewardState();

        Time.timeScale = 1f;

        // Try loading another advertisement.
        LoadRewardedAd();
    }

    // =========================================================
    // PLAYER FINISHED AD AND EARNED REWARD
    // =========================================================

    private void OnAdRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
        Debug.Log(
            "PLAYER EARNED REWARD: " +
            reward.Name +
            " x" +
            reward.Amount
        );

        if (!waitingForReward)
        {
            return;
        }

        rewardEarned = true;

        // OnAdRewarded and OnAdClosed can happen
        // in different orders.
        TryGiveReward();
    }

    // =========================================================
    // AD CLOSED
    // =========================================================

    private void OnAdClosed(
        LevelPlayAdInfo adInfo)
    {
        Debug.Log(
            "REWARDED AD CLOSED"
        );

        adClosed = true;

        Time.timeScale = 1f;

        // Give reward only if reward callback
        // was also received.
        TryGiveReward();

        // Prepare next rewarded advertisement.
        LoadRewardedAd();
    }

    // =========================================================
    // CHECK IF PLAYER CAN RECEIVE REVIVE
    // =========================================================

    private void TryGiveReward()
    {
        if (!waitingForReward)
        {
            return;
        }

        if (!rewardEarned)
        {
            return;
        }

        if (!adClosed)
        {
            return;
        }

        if (rewardAlreadyGiven)
        {
            return;
        }

        rewardAlreadyGiven = true;
        waitingForReward = false;

        Time.timeScale = 1f;

        // Hide TALO / LosePanel.
        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        Debug.Log(
            "REWARDED AD COMPLETE - REVIVING PLAYER"
        );

        // Continue current Hulihin Biik level.
        if (biikManager != null)
        {
            biikManager.ReviveFromRewardedAd();
        }
        else
        {
            Debug.LogError(
                "BiikManager is missing from RewardedAdsManager!"
            );
        }
    }

    // =========================================================
    // RESET REWARD STATE
    // =========================================================

    private void ResetRewardState()
    {
        waitingForReward = false;
        rewardEarned = false;
        adClosed = false;
        rewardAlreadyGiven = false;
    }

    // =========================================================
    // CLEAN UP
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

        // Prevent game remaining frozen
        // if this object gets destroyed during an ad.
        Time.timeScale = 1f;
    }
}