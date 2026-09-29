using UnityEngine;
using UnityEngine.EventSystems;


public class PatinteroVirtualJoystick :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("JOYSTICK")]

    [SerializeField]
    private RectTransform background;


    [SerializeField]
    private RectTransform handle;


    private Vector2 input;


    // =========================================================
    // POINTER DOWN
    // =========================================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        OnDrag(eventData);
    }


    // =========================================================
    // DRAG
    // =========================================================

    public void OnDrag(
        PointerEventData eventData)
    {
        if (
            background == null ||
            handle == null)
        {
            return;
        }


        if (
            !RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    background,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPosition))
        {
            return;
        }


        Vector2 size =
            background.rect.size;


        Vector2 normalized =
            new Vector2(
                localPosition.x /
                (size.x * 0.5f),

                localPosition.y /
                (size.y * 0.5f)
            );


        input =
            Vector2.ClampMagnitude(
                normalized,
                1f
            );


        handle.anchoredPosition =
            new Vector2(
                input.x *
                size.x *
                0.35f,

                input.y *
                size.y *
                0.35f
            );


        PatinteroMobileInput
            .SetJoystickMove(
                input
            );
    }


    // =========================================================
    // POINTER UP
    // =========================================================

    public void OnPointerUp(
        PointerEventData eventData)
    {
        ResetJoystick();
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        ResetJoystick();
    }


    private void ResetJoystick()
    {
        input =
            Vector2.zero;


        PatinteroMobileInput
            .SetJoystickMove(
                Vector2.zero
            );


        if (handle != null)
        {
            handle.anchoredPosition =
                Vector2.zero;
        }
    }
}