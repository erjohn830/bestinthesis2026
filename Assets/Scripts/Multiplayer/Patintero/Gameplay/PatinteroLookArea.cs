using UnityEngine;
using UnityEngine.EventSystems;

public class PatinteroLookArea :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    // =========================================================
    // CAMERA LOOK
    // =========================================================

    [Header("CAMERA LOOK")]
    [SerializeField] private float sensitivity = 0.08f;

    [SerializeField] private float maxPixelDelta = 50f;

    [SerializeField] private bool invertX = false;


    private bool dragging;


    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        dragging =
            true;
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!dragging)
        {
            return;
        }


        float deltaX =
            Mathf.Clamp(
                eventData.delta.x,
                -maxPixelDelta,
                maxPixelDelta
            );


        if (invertX)
        {
            deltaX =
                -deltaX;
        }


        PatinteroMobileInput.CameraYaw =
            Mathf.Repeat(
                PatinteroMobileInput.CameraYaw +
                deltaX *
                sensitivity,
                360f
            );
    }


    // =========================================================
    // POINTER UP
    // =========================================================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        dragging =
            false;
    }


    // =========================================================
    // POINTER EXIT
    // =========================================================

    public void OnPointerExit(
        PointerEventData eventData)
    {
        dragging =
            false;
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        dragging =
            false;
    }
}