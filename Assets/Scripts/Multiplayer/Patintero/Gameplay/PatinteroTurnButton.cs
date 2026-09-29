using UnityEngine;


public class PatinteroTurnButton :
    MonoBehaviour
{
    public void Turn()
    {
        PatinteroMobileInput
            .QueueTurn();
    }
}