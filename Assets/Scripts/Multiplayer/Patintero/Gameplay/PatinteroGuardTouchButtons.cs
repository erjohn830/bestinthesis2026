using UnityEngine;
using UnityEngine.EventSystems;

public class PatinteroGuardTouchButton :
    MonoBehaviour,
    IPointerDownHandler
{
    // =========================================================
    // TOUCH BUTTON
    // =========================================================

    public void OnPointerDown(
        PointerEventData eventData)
    {
        PatinteroMobileInput
            .QueueTouch();


        Debug.Log(
            "DEFENDER TOUCH BUTTON PRESSED"
        );
    }
}