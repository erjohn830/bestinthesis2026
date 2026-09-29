using UnityEngine;


// =============================================================
// DEFENDER BUTTON DIRECTIONS
// =============================================================

public enum PatinteroGuardMoveDirection
{
    Left,
    Right,
    Up,
    Down
}


// =============================================================
// PATINTERO MOBILE INPUT
// =============================================================

public static class PatinteroMobileInput
{
    // =========================================================
    // MOVEMENT
    // =========================================================

    private static Vector2 joystickMove =
        Vector2.zero;


    private static bool leftHeld;
    private static bool rightHeld;
    private static bool upHeld;
    private static bool downHeld;


    // =========================================================
    // ONE-PRESS BUTTONS
    // =========================================================

    private static bool turnQueued;
    private static bool touchQueued;


    // =========================================================
    // CAMERA
    // =========================================================

    public static float CameraYaw = 0f;


    // =========================================================
    // ATTACKER JOYSTICK
    // =========================================================

    public static void SetJoystickMove(
        Vector2 value)
    {
        joystickMove =
            Vector2.ClampMagnitude(
                value,
                1f
            );
    }


    // =========================================================
    // DEFENDER MOVE BUTTON
    // =========================================================

    public static void SetGuardButton(
        PatinteroGuardMoveDirection direction,
        bool pressed)
    {
        switch (direction)
        {
            case PatinteroGuardMoveDirection.Left:

                leftHeld = pressed;

                break;


            case PatinteroGuardMoveDirection.Right:

                rightHeld = pressed;

                break;


            case PatinteroGuardMoveDirection.Up:

                upHeld = pressed;

                break;


            case PatinteroGuardMoveDirection.Down:

                downHeld = pressed;

                break;
        }
    }


    // =========================================================
    // CURRENT MOVEMENT
    // =========================================================

    public static Vector2 GetMove()
    {
        Vector2 buttonInput =
            Vector2.zero;


        if (leftHeld)
        {
            buttonInput.x -= 1f;
        }


        if (rightHeld)
        {
            buttonInput.x += 1f;
        }


        if (upHeld)
        {
            buttonInput.y += 1f;
        }


        if (downHeld)
        {
            buttonInput.y -= 1f;
        }


        // Defender buttons get priority.
        if (buttonInput.sqrMagnitude > 0.001f)
        {
            return buttonInput.normalized;
        }


        return joystickMove;
    }


    // =========================================================
    // TURN
    // =========================================================

    public static void QueueTurn()
    {
        turnQueued = true;
    }


    public static bool ConsumeTurn()
    {
        bool value =
            turnQueued;


        turnQueued = false;


        return value;
    }


    // =========================================================
    // TOUCH / TAG
    // =========================================================

    public static void QueueTouch()
    {
        touchQueued = true;
    }


    public static bool ConsumeTouch()
    {
        bool value =
            touchQueued;


        touchQueued = false;


        return value;
    }


    // =========================================================
    // RESET GUARD MOVEMENT
    // =========================================================

    public static void ResetGuardButtons()
    {
        leftHeld = false;
        rightHeld = false;
        upHeld = false;
        downHeld = false;
    }


    // =========================================================
    // RESET EVERYTHING
    // =========================================================

    public static void Reset()
    {
        joystickMove =
            Vector2.zero;


        CameraYaw = 0f;


        ResetGuardButtons();


        turnQueued = false;
        touchQueued = false;
    }
}