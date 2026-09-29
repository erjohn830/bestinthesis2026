
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundSlideshow : MonoBehaviour
{
    [Header("UI Image")]
    [SerializeField] private Image backgroundImage;

    [Header("Backgrounds")]
    [SerializeField] private Sprite[] backgrounds;

    [Header("Settings")]
    [SerializeField] private float changeTime = 6f;
    [SerializeField] private float zoomAmount = 1.15f;

    [Header("Pan Settings")]
    [SerializeField] private float leftOffset = 120f;

    private int currentIndex;
    private RectTransform rect;

    private void Start()
    {
        rect = backgroundImage.GetComponent<RectTransform>();

        if (backgrounds.Length > 0)
        {
            backgroundImage.sprite = backgrounds[0];
        }

        StartCoroutine(Slideshow());
    }

    private IEnumerator Slideshow()
    {
        while (true)
        {
            yield return StartCoroutine(KenBurns());

            currentIndex++;

            if (currentIndex >= backgrounds.Length)
                currentIndex = 0;

            backgroundImage.sprite = backgrounds[currentIndex];
        }
    }

    private IEnumerator KenBurns()
    {
        float timer = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * zoomAmount;

        // Start centered
        Vector2 startPos = Vector2.zero;

        // Slowly move left
        Vector2 endPos = new Vector2(-leftOffset, 0f);

        rect.localScale = startScale;
        rect.anchoredPosition = startPos;

        while (timer < changeTime)
        {
            timer += Time.deltaTime;

            float t = timer / changeTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            yield return null;
        }

        rect.localScale = endScale;
        rect.anchoredPosition = endPos;
    }
}