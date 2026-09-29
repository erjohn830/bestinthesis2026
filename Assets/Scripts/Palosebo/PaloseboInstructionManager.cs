using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaloseboInstructionManager : MonoBehaviour
{
    [System.Serializable]
    public class InstructionPage
    {
        [Header("TEXT")]
        public string title;

        [TextArea(2, 5)]
        public string description;

        [Header("ONLY THIS OBJECT SHOWS")]
        public GameObject objectToShow;

        [Header("ONLY THIS ARROW SHOWS")]
        public GameObject arrowToShow;

        [Header("HIGHLIGHT TARGET")]
        [Tooltip("Object/Transform that the glowing highlight follows. " +
                 "Usually the same object as Object To Show.")]
        public Transform focusTarget;

        [Header("HIGHLIGHT POSITION")]
        public Vector2 highlightOffset;

        [Header("HIGHLIGHT SIZE")]
        public Vector2 highlightSize = new Vector2(180f, 180f);
    }

    // =========================================================
    // MANAGER
    // =========================================================

    [Header("MANAGER")]
    [SerializeField] private PaloseboManager paloseboManager;

    // =========================================================
    // UI
    // =========================================================

    [Header("TEXT")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text pageText;

    [Header("BUTTONS")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button startButton;

    // =========================================================
    // PAGE TRANSITION
    // =========================================================

    [Header("PAGE CONTENT")]
    [Tooltip("CanvasGroup used for smooth page fade.")]
    [SerializeField] private CanvasGroup contentCanvasGroup;

    [SerializeField] private float fadeDuration = 0.15f;

    // =========================================================
    // HIGHLIGHT
    // =========================================================

    [Header("HIGHLIGHT")]
    [Tooltip("A glowing ring/image that follows the current object.")]
    [SerializeField] private RectTransform highlightRing;

    [SerializeField] private CanvasGroup highlightCanvasGroup;

    [Tooltip("How much the highlight pulses.")]
    [SerializeField] private float pulseAmount = 0.08f;

    [Tooltip("Pulse animation speed.")]
    [SerializeField] private float pulseSpeed = 3f;

    [Header("CAMERA / CANVAS")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas instructionCanvas;

    [Tooltip("Usually the InstructionPanel RectTransform.")]
    [SerializeField] private RectTransform instructionRoot;

    // =========================================================
    // PAGES
    // =========================================================

    [Header("INSTRUCTION PAGES")]
    [SerializeField] private InstructionPage[] pages;

    // =========================================================
    // PRIVATE
    // =========================================================

    private int currentPage = 0;

    private bool transitioning = false;

    private Coroutine transitionCoroutine;

    private readonly Dictionary<GameObject, bool>
        originalStates =
        new Dictionary<GameObject, bool>();

    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (pages == null || pages.Length == 0)
        {
            Debug.LogWarning(
                "PALOSOBO INSTRUCTION: No instruction pages assigned."
            );

            return;
        }

        CacheOriginalStates();

        currentPage = 0;

        if (contentCanvasGroup != null)
            contentCanvasGroup.alpha = 1f;

        ShowPageImmediately(currentPage);
    }

    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        transitioning = false;

        RestoreOriginalStates();

        if (highlightRing != null)
            highlightRing.gameObject.SetActive(false);
    }

    // =========================================================
    // UPDATE HIGHLIGHT
    // =========================================================

    private void Update()
    {
        if (pages == null ||
            pages.Length == 0 ||
            currentPage < 0 ||
            currentPage >= pages.Length)
        {
            return;
        }

        UpdateHighlight();
    }

    // =========================================================
    // CACHE OBJECT STATES
    // =========================================================

    private void CacheOriginalStates()
    {
        originalStates.Clear();

        if (pages == null)
            return;

        foreach (InstructionPage page in pages)
        {
            if (page == null)
                continue;

            CacheObject(page.objectToShow);
            CacheObject(page.arrowToShow);
        }
    }

    private void CacheObject(GameObject obj)
    {
        if (obj == null)
            return;

        if (!originalStates.ContainsKey(obj))
        {
            originalStates.Add(
                obj,
                obj.activeSelf
            );
        }
    }

    // =========================================================
    // RESTORE OBJECT STATES
    // =========================================================

    private void RestoreOriginalStates()
    {
        foreach (
            KeyValuePair<GameObject, bool> state
            in originalStates)
        {
            if (state.Key != null)
                state.Key.SetActive(state.Value);
        }

        originalStates.Clear();
    }

    // =========================================================
    // NEXT
    // =========================================================

    public void NextPage()
    {
        if (transitioning)
            return;

        if (pages == null ||
            currentPage >= pages.Length - 1)
        {
            return;
        }

        currentPage++;

        StartSmoothTransition();
    }

    // =========================================================
    // BACK
    // =========================================================

    public void PreviousPage()
    {
        if (transitioning)
            return;

        if (currentPage <= 0)
            return;

        currentPage--;

        StartSmoothTransition();
    }

    // =========================================================
    // SMOOTH TRANSITION
    // =========================================================

    private void StartSmoothTransition()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine =
            StartCoroutine(
                ChangePageRoutine()
            );
    }

    private IEnumerator ChangePageRoutine()
    {
        transitioning = true;

        // ------------------------------
        // FADE OUT
        // ------------------------------

        if (contentCanvasGroup != null &&
            fadeDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                contentCanvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        timer / fadeDuration
                    );

                yield return null;
            }

            contentCanvasGroup.alpha = 0f;
        }

        ShowPageImmediately(currentPage);

        // ------------------------------
        // FADE IN
        // ------------------------------

        if (contentCanvasGroup != null &&
            fadeDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                contentCanvasGroup.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        timer / fadeDuration
                    );

                yield return null;
            }

            contentCanvasGroup.alpha = 1f;
        }

        transitioning = false;

        transitionCoroutine = null;
    }

    // =========================================================
    // SHOW PAGE
    // =========================================================

    private void ShowPageImmediately(int index)
    {
        if (pages == null ||
            pages.Length == 0)
        {
            return;
        }

        currentPage =
            Mathf.Clamp(
                index,
                0,
                pages.Length - 1
            );

        // IMPORTANT:
        // Hide every instruction subject first.
        HideAllPageObjects();

        InstructionPage page =
            pages[currentPage];

        if (page == null)
            return;

        // =====================================================
        // SHOW ONLY CURRENT OBJECT
        // =====================================================

        if (page.objectToShow != null)
            page.objectToShow.SetActive(true);

        // =====================================================
        // SHOW ONLY CURRENT ARROW
        // =====================================================

        if (page.arrowToShow != null)
            page.arrowToShow.SetActive(true);

        // =====================================================
        // TEXT
        // =====================================================

        if (titleText != null)
            titleText.text = page.title;

        if (descriptionText != null)
            descriptionText.text = page.description;

        if (pageText != null)
        {
            pageText.text =
                (currentPage + 1) +
                " / " +
                pages.Length;
        }

        // =====================================================
        // BUTTONS
        // =====================================================

        bool firstPage =
            currentPage == 0;

        bool lastPage =
            currentPage ==
            pages.Length - 1;

        if (backButton != null)
            backButton.gameObject.SetActive(!firstPage);

        if (nextButton != null)
            nextButton.gameObject.SetActive(!lastPage);

        if (startButton != null)
            startButton.gameObject.SetActive(lastPage);

        // =====================================================
        // HIGHLIGHT
        // =====================================================

        if (highlightRing != null)
        {
            highlightRing.sizeDelta =
                page.highlightSize;

            highlightRing.gameObject.SetActive(
                page.focusTarget != null
            );
        }
    }

    // =========================================================
    // HIDE EVERYTHING FROM ALL PAGES
    // =========================================================

    private void HideAllPageObjects()
    {
        if (pages == null)
            return;

        foreach (InstructionPage page in pages)
        {
            if (page == null)
                continue;

            if (page.objectToShow != null)
                page.objectToShow.SetActive(false);

            if (page.arrowToShow != null)
                page.arrowToShow.SetActive(false);
        }

        if (highlightRing != null)
            highlightRing.gameObject.SetActive(false);
    }

    // =========================================================
    // HIGHLIGHT CURRENT OBJECT
    // =========================================================

    private void UpdateHighlight()
    {
        if (highlightRing == null ||
            instructionRoot == null)
        {
            return;
        }

        InstructionPage page =
            pages[currentPage];

        if (page == null ||
            page.focusTarget == null)
        {
            highlightRing.gameObject.SetActive(false);
            return;
        }

        highlightRing.gameObject.SetActive(true);

        Vector2 screenPoint;

        // =====================================================
        // UI OBJECT
        // =====================================================

        RectTransform targetRect =
            page.focusTarget as RectTransform;

        if (targetRect != null)
        {
            Camera canvasCamera = null;

            if (instructionCanvas != null &&
                instructionCanvas.renderMode !=
                RenderMode.ScreenSpaceOverlay)
            {
                canvasCamera =
                    instructionCanvas.worldCamera;
            }

            screenPoint =
                RectTransformUtility
                    .WorldToScreenPoint(
                        canvasCamera,
                        targetRect.position
                    );
        }

        // =====================================================
        // WORLD / 3D OBJECT
        // =====================================================

        else
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return;

            screenPoint =
                worldCamera.WorldToScreenPoint(
                    page.focusTarget.position
                );
        }

        Camera rootCamera = null;

        if (instructionCanvas != null &&
            instructionCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            rootCamera =
                instructionCanvas.worldCamera;
        }

        Vector2 localPoint;

        if (
            RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                instructionRoot,
                screenPoint,
                rootCamera,
                out localPoint
            ))
        {
            highlightRing.anchoredPosition =
                localPoint +
                page.highlightOffset;
        }

        // =====================================================
        // PULSE
        // =====================================================

        float pulse =
            1f +
            Mathf.Sin(
                Time.unscaledTime *
                pulseSpeed
            ) *
            pulseAmount;

        highlightRing.localScale =
            Vector3.one * pulse;

        if (highlightCanvasGroup != null)
        {
            highlightCanvasGroup.alpha =
                Mathf.Lerp(
                    0.65f,
                    1f,
                    (
                        Mathf.Sin(
                            Time.unscaledTime *
                            pulseSpeed
                        ) +
                        1f
                    ) * 0.5f
                );
        }
    }

    // =========================================================
    // START GAME
    // =========================================================

    public void StartGame()
    {
        if (transitioning)
            return;

        RestoreOriginalStates();

        if (highlightRing != null)
            highlightRing.gameObject.SetActive(false);

        if (paloseboManager != null)
        {
            paloseboManager.StartFromInstruction();
        }
        else
        {
            Debug.LogError(
                "PALOSOBO INSTRUCTION: " +
                "PaloseboManager is not assigned."
            );
        }
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetInstructions()
    {
        currentPage = 0;

        ShowPageImmediately(
            currentPage
        );
    }
}