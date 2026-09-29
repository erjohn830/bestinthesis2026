using Fusion;
using UnityEngine;


// =============================================================
// PATINTERO NETWORK BUTTONS
// =============================================================

public enum PatinteroButton
{
    Turn = 0,
    Touch = 1
}


// =============================================================
// SHARED PATINTERO INPUT DATA
// =============================================================

public struct PatinteroInputData : INetworkInput
{
    // X = left/right
    // Y = forward/backward
    public Vector2 Move;

    // Third-person camera direction
    public float CameraYaw;

    // Turn + Touch
    public NetworkButtons Buttons;
}