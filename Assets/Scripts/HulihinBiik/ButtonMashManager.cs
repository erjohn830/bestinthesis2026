using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMashManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject mashPanel;
    [SerializeField] private GameObject smashButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text mashText;
    [SerializeField] private TMP_Text mashTimerText;

    [Header("Mash Settings")]
    [SerializeField] private int minimumPresses = 15;
    [SerializeField] private int maximumPresses = 20;
    [SerializeField] private float mashTimeLimit = 7f;

    [Tooltip("Prevents one touch from accidentally counting twice.")]
    [SerializeField] private float minimumPressInterval = 0.06f;

    private int requiredPresses;
    private int currentPresses;

    private float remainingTime;
    private float lastPressTime;

    private bool mashActive;

    private Action successAction;
    private Action failureAction;

    public bool IsMashActive => mashActive;

    private void Start()
    {
        HideMashUI();
    }

    private void Update()
    {
        if (!mashActive)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        if (mashTimerText != null)
        {
            mashTimerText.text =
                remainingTime.ToString("0.0") + "s";
        }

        if (remainingTime <= 0f)
        {
            FinishFailure();
        }
    }

    public void StartMash(
        Action onSuccess,
        Action onFailure)
    {
        requiredPresses = UnityEngine.Random.Range(
            minimumPresses,
            maximumPresses + 1
        );

        currentPresses = 0;
        remainingTime = mashTimeLimit;
        lastPressTime = -100f;

        successAction = onSuccess;
        failureAction = onFailure;

        mashActive = true;

        if (mashPanel != null)
        {
            mashPanel.SetActive(true);
        }

        if (smashButton != null)
        {
            smashButton.SetActive(true);
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = requiredPresses;
            progressBar.value = 0f;
        }

        UpdateMashText();
    }

    // Connect this method to SmashButton On Click.
    public void PressMashButton()
    {
        if (!mashActive)
        {
            return;
        }

        if (Time.unscaledTime - lastPressTime <
            minimumPressInterval)
        {
            return;
        }

        lastPressTime = Time.unscaledTime;
        currentPresses++;

        if (progressBar != null)
        {
            progressBar.value = currentPresses;
        }

        UpdateMashText();

        if (currentPresses >= requiredPresses)
        {
            FinishSuccess();
        }
    }

    private void UpdateMashText()
    {
        if (mashText != null)
        {
            mashText.text =
                "PINDUTIN!\n" +
                currentPresses +
                " / " +
                requiredPresses;
        }
    }

    private void FinishSuccess()
    {
        if (!mashActive)
        {
            return;
        }

        mashActive = false;
        HideMashUI();

        Action callback = successAction;

        ClearCallbacks();
        callback?.Invoke();
    }

    private void FinishFailure()
    {
        if (!mashActive)
        {
            return;
        }

        mashActive = false;
        HideMashUI();

        Action callback = failureAction;

        ClearCallbacks();
        callback?.Invoke();
    }

    public void StopMash()
    {
        mashActive = false;
        HideMashUI();
        ClearCallbacks();
    }

    private void HideMashUI()
    {
        if (mashPanel != null)
        {
            mashPanel.SetActive(false);
        }

        if (smashButton != null)
        {
            smashButton.SetActive(false);
        }
    }

    private void ClearCallbacks()
    {
        successAction = null;
        failureAction = null;
    }
}