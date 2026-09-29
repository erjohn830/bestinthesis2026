using UnityEngine;

public class TutorialWorldArrow : MonoBehaviour
{
    [Header("World Target")]
    public Transform worldTarget;

    [Header("Camera")]
    public Camera targetCamera;

    [Header("Canvas")]
    public RectTransform canvasRect;

    [Header("Arrow")]
    public RectTransform arrowRect;

    [Header("Position")]
    public float targetHeight = 1.5f;

    public Vector2 screenOffset =
        new Vector2(0f, 100f);

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (arrowRect == null)
        {
            arrowRect =
                GetComponent<RectTransform>();
        }
    }

    void LateUpdate()
    {
        if (worldTarget == null ||
            targetCamera == null ||
            canvasRect == null ||
            arrowRect == null)
        {
            return;
        }

        Vector3 worldPosition =
            worldTarget.position +
            Vector3.up * targetHeight;

        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(
                worldPosition
            );

        if (screenPosition.z < 0f)
            return;

        Canvas canvas =
            canvasRect.GetComponent<Canvas>();

        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 localPoint;

        RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                uiCamera,
                out localPoint
            );

        arrowRect.anchoredPosition =
            localPoint +
            screenOffset;
    }
}