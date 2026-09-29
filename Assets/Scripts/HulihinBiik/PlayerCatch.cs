using UnityEngine;
using UnityEngine.UI;

public class PlayerCatch : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] private BiikManager biikManager;
    [SerializeField] private PigAI pig;
    [SerializeField] private Button catchButton;

    [Header("CATCH DISTANCE")]
    [SerializeField] private float showCatchDistance = 2.3f;
    [SerializeField] private float hideCatchDistance = 2.8f;

    [Header("NEXT LEVEL")]
    [SerializeField] private float nextLevelCatchDelay = 1.5f;

    private bool catchAvailable = false;
    private bool detectionEnabled = true;
    private bool catchingStarted = false;

    // Used to detect:
    // Caught -> Released/Fleeing
    private bool pigWasCaught = false;

    // Prevent button appearing immediately
    // when Level 2 / Level 3 begins.
    private float catchEnableTime = 0f;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (biikManager == null)
        {
            biikManager =
                FindFirstObjectByType<BiikManager>();
        }

        if (pig == null)
        {
            pig =
                FindFirstObjectByType<PigAI>();
        }

        if (catchButton != null)
        {
            catchButton.gameObject.SetActive(false);

            catchButton.onClick.RemoveListener(
                PressCatchButton
            );

            catchButton.onClick.AddListener(
                PressCatchButton
            );
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        catchAvailable = false;
        catchingStarted = false;
        detectionEnabled = true;
        pigWasCaught = false;

        catchEnableTime = 0f;

        HideCatchButton();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // If Pig reference disappeared,
        // try finding it again.
        if (pig == null)
        {
            pig =
                FindFirstObjectByType<PigAI>();

            HideCatchButton();
            return;
        }


        // =====================================================
        // DETECT WHEN PIG BECOMES CAUGHT
        // =====================================================

        if (pig.IsCaught)
        {
            pigWasCaught = true;

            catchAvailable = false;
            HideCatchButton();

            return;
        }


        // =====================================================
        // IMPORTANT FIX
        //
        // Pig WAS caught before,
        // but PigAI.IsCaught is now FALSE.
        //
        // This means:
        //
        // Level 1 -> Level 2
        // OR
        // Level 2 -> Level 3
        // OR
        // failed catch escape.
        // =====================================================

        if (pigWasCaught && !pig.IsCaught)
        {
            pigWasCaught = false;

            catchingStarted = false;
            catchAvailable = false;
            detectionEnabled = true;

            // Small protection delay.
            catchEnableTime =
                Time.time + nextLevelCatchDelay;

            HideCatchButton();

            Debug.Log(
                "PIG RELEASED - Catch system automatically reset!"
            );
        }


        // =====================================================
        // DETECTION DISABLED
        // =====================================================

        if (!detectionEnabled)
        {
            HideCatchButton();
            return;
        }


        // =====================================================
        // CURRENTLY CATCHING
        // =====================================================

        if (catchingStarted)
        {
            HideCatchButton();
            return;
        }


        // =====================================================
        // CATCH PROTECTION DELAY
        // =====================================================

        if (Time.time < catchEnableTime)
        {
            HideCatchButton();
            return;
        }


        // =====================================================
        // CHECK DISTANCE
        // =====================================================

        CheckPigDistance();
    }


    // =========================================================
    // CHECK PLAYER -> PIG DISTANCE
    // =========================================================

    private void CheckPigDistance()
    {
        if (pig == null)
        {
            HideCatchButton();
            return;
        }


        Vector3 playerPosition =
            transform.position;

        Vector3 pigPosition =
            pig.transform.position;


        // Ignore vertical/Y distance.
        playerPosition.y = 0f;
        pigPosition.y = 0f;


        float distance =
            Vector3.Distance(
                playerPosition,
                pigPosition
            );


        // =====================================================
        // ENTER CATCH RADIUS
        // =====================================================

        if (!catchAvailable &&
            distance <= showCatchDistance)
        {
            catchAvailable = true;

            ShowCatchButton();

            Debug.Log(
                "HULIHIN AVAILABLE | Distance = " +
                distance.ToString("F2")
            );
        }


        // =====================================================
        // EXIT CATCH RADIUS
        // =====================================================

        else if (catchAvailable &&
                 distance > hideCatchDistance)
        {
            catchAvailable = false;

            HideCatchButton();

            Debug.Log(
                "HULIHIN HIDDEN | Distance = " +
                distance.ToString("F2")
            );
        }
    }


    // =========================================================
    // PRESS HULIHIN
    // =========================================================

    public void PressCatchButton()
    {
        if (!detectionEnabled)
        {
            return;
        }

        if (catchingStarted)
        {
            return;
        }

        if (!catchAvailable)
        {
            return;
        }

        if (pig == null)
        {
            return;
        }

        if (pig.IsCaught)
        {
            return;
        }

        if (Time.time < catchEnableTime)
        {
            return;
        }

        if (biikManager == null)
        {
            Debug.LogError(
                "BiikManager is missing from PlayerCatch!"
            );

            return;
        }


        catchingStarted = true;
        catchAvailable = false;

        HideCatchButton();


        Debug.Log(
            "HULIHIN BUTTON PRESSED!"
        );


        biikManager.TryCatchPig(pig);
    }


    // =========================================================
    // SHOW BUTTON
    // =========================================================

    private void ShowCatchButton()
    {
        if (catchButton != null)
        {
            catchButton.gameObject.SetActive(true);
        }
    }


    // =========================================================
    // HIDE BUTTON
    // =========================================================

    private void HideCatchButton()
    {
        if (catchButton != null)
        {
            catchButton.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // ENABLE DETECTION
    // =========================================================

    public void EnableDetection()
    {
        detectionEnabled = true;
        catchingStarted = false;
        catchAvailable = false;

        catchEnableTime = Time.time;

        HideCatchButton();

        Debug.Log(
            "Catch Detection ENABLED"
        );
    }


    // =========================================================
    // DISABLE DETECTION
    // =========================================================

    public void DisableDetection()
    {
        detectionEnabled = false;
        catchingStarted = false;
        catchAvailable = false;

        // IMPORTANT:
        // Remember that this catch happened.
        pigWasCaught = true;

        HideCatchButton();

        Debug.Log(
            "Catch Detection DISABLED"
        );
    }


    // =========================================================
    // RESET AFTER FAILED CATCH
    // =========================================================

    public void ResetCatch()
    {
        catchingStarted = false;
        catchAvailable = false;
        detectionEnabled = true;
        pigWasCaught = false;

        catchEnableTime = Time.time;

        if (pig == null)
        {
            pig =
                FindFirstObjectByType<PigAI>();
        }

        HideCatchButton();

        Debug.Log(
            "Catch system RESET"
        );
    }


    // =========================================================
    // RESET LEVEL 2 / LEVEL 3
    // =========================================================

    public void ResetForNextLevel()
    {
        catchingStarted = false;
        catchAvailable = false;
        detectionEnabled = true;
        pigWasCaught = false;

        catchEnableTime = Time.time;

        PigAI foundPig =
            FindFirstObjectByType<PigAI>();

        if (foundPig != null)
        {
            pig = foundPig;
        }

        HideCatchButton();

        Debug.Log(
            "PlayerCatch READY FOR NEXT LEVEL"
        );
    }


    // =========================================================
    // CHANGE PIG
    // =========================================================

    public void SetPig(PigAI newPig)
    {
        pig = newPig;

        catchingStarted = false;
        catchAvailable = false;
        pigWasCaught = false;

        catchEnableTime = Time.time;

        HideCatchButton();
    }


    // =========================================================
    // OLD COLLIDER CATCH SYSTEM
    // =========================================================

    public void TryDetectPig(Collider other)
    {
        // Intentionally empty.
        //
        // Touching the pig does NOT catch it anymore.
    }
}