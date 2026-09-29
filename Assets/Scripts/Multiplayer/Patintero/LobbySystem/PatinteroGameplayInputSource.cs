using UnityEngine;
using UnityEngine.InputSystem;

public class PatinteroGameplayInputSource : MonoBehaviour
{
    public static Vector2 CurrentMove;

    private void Update()
    {
        Vector2 move = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
            {
                move.x -= 1f;
            }

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
            {
                move.x += 1f;
            }

            if (keyboard.sKey.isPressed ||
                keyboard.downArrowKey.isPressed)
            {
                move.y -= 1f;
            }

            if (keyboard.wKey.isPressed ||
                keyboard.upArrowKey.isPressed)
            {
                move.y += 1f;
            }
        }

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        CurrentMove = move;
    }

    private void OnDisable()
    {
        CurrentMove = Vector2.zero;
    }
}