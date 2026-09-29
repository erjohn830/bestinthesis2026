using UnityEngine;
using UnityEngine.EventSystems;

public class PaloseboBalanceButton :
    MonoBehaviour,
    IPointerDownHandler
{
    public enum ButtonSide
    {
        Blue,
        Red
    }

    [Header("BUTTON")]
    [SerializeField]
    private ButtonSide buttonSide;

    [Header("BALANCE SYSTEM")]
    [SerializeField]
    private PaloseboBalance balanceSystem;

    public void OnPointerDown(
        PointerEventData eventData
    )
    {
        if (balanceSystem == null)
        {
            Debug.LogError(
                name +
                ": BalanceSystem is NOT assigned!"
            );

            return;
        }

        if (buttonSide == ButtonSide.Blue)
        {
            balanceSystem.PressBlue();
        }
        else
        {
            balanceSystem.PressRed();
        }
    }
}