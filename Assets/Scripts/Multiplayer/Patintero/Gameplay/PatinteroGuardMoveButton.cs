using UnityEngine;
using UnityEngine.EventSystems;


public class PatinteroGuardMoveButton :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("MOVEMENT")]

    [SerializeField]
    private PatinteroGuardMoveDirection direction;


    public void OnPointerDown(
        PointerEventData eventData)
    {
        PatinteroMobileInput
            .SetGuardButton(
                direction,
                true
            );
    }


    public void OnPointerUp(
        PointerEventData eventData)
    {
        Release();
    }


    public void OnPointerExit(
        PointerEventData eventData)
    {
        Release();
    }


    private void OnDisable()
    {
        Release();
    }


    private void Release()
    {
        PatinteroMobileInput
            .SetGuardButton(
                direction,
                false
            );
    }
}