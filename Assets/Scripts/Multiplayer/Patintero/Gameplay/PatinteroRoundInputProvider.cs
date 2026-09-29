using System;
using System.Collections;
using System.Collections.Generic;

using Fusion;
using Fusion.Sockets;

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class PatinteroRoundInputProvider :
    MonoBehaviour,
    INetworkRunnerCallbacks
{
    private NetworkRunner runner;

    private bool callbacksAdded;


    // =========================================================
    // START
    // =========================================================

    private IEnumerator Start()
    {
        while (
            PatinteroFusionManager.Instance == null ||
            PatinteroFusionManager.Instance.Runner == null ||
            !PatinteroFusionManager.Instance.Runner.IsRunning)
        {
            yield return null;
        }


        runner =
            PatinteroFusionManager
                .Instance
                .Runner;


        if (runner == null)
        {
            Debug.LogError(
                "PATINTERO: NetworkRunner not found."
            );

            yield break;
        }


        runner.ProvideInput = true;


        if (!callbacksAdded)
        {
            runner.AddCallbacks(this);

            callbacksAdded = true;
        }


        Debug.Log(
            "PATINTERO ROUND INPUT READY"
        );
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (
            callbacksAdded &&
            runner != null)
        {
            runner.RemoveCallbacks(this);

            callbacksAdded = false;
        }
    }


    // =========================================================
    // INPUT
    // =========================================================

    public void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        Vector2 movement =
            PatinteroMobileInput
                .GetMove();


#if ENABLE_INPUT_SYSTEM

        // =====================================================
        // PC TESTING
        //
        // WASD = movement
        // Q    = rotate
        // E    = touch/tag
        // =====================================================

        if (Keyboard.current != null)
        {
            Vector2 keyboard =
                Vector2.zero;


            if (Keyboard.current.wKey.isPressed)
            {
                keyboard.y += 1f;
            }


            if (Keyboard.current.sKey.isPressed)
            {
                keyboard.y -= 1f;
            }


            if (Keyboard.current.aKey.isPressed)
            {
                keyboard.x -= 1f;
            }


            if (Keyboard.current.dKey.isPressed)
            {
                keyboard.x += 1f;
            }


            if (keyboard.sqrMagnitude > 0f)
            {
                movement =
                    keyboard.normalized;
            }


            // Defender rotate
            if (
                Keyboard.current.qKey
                    .wasPressedThisFrame)
            {
                PatinteroMobileInput
                    .QueueTurn();
            }


            // Defender hand/tag
            if (
                Keyboard.current.eKey
                    .wasPressedThisFrame)
            {
                PatinteroMobileInput
                    .QueueTouch();
            }
        }

#endif


        PatinteroInputData data =
            new PatinteroInputData();


        data.Move =
            Vector2.ClampMagnitude(
                movement,
                1f
            );


        data.CameraYaw =
            PatinteroMobileInput
                .CameraYaw;


        // =====================================================
        // TURN BUTTON
        // =====================================================

        if (
            PatinteroMobileInput
                .ConsumeTurn())
        {
            data.Buttons.Set(
                PatinteroButton.Turn,
                true
            );
        }


        // =====================================================
        // TOUCH BUTTON
        // =====================================================

        if (
            PatinteroMobileInput
                .ConsumeTouch())
        {
            data.Buttons.Set(
                PatinteroButton.Touch,
                true
            );
        }


        input.Set(data);
    }


    // =========================================================
    // FUSION CALLBACKS
    // =========================================================

    public void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
    }


    public void OnPlayerLeft(
        NetworkRunner runner,
        PlayerRef player)
    {
    }


    public void OnInputMissing(
        NetworkRunner runner,
        PlayerRef player,
        NetworkInput input)
    {
    }


    public void OnShutdown(
        NetworkRunner runner,
        ShutdownReason shutdownReason)
    {
    }


    public void OnConnectedToServer(
        NetworkRunner runner)
    {
    }


    public void OnDisconnectedFromServer(
        NetworkRunner runner,
        NetDisconnectReason reason)
    {
    }


    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }


    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason)
    {
    }


    public void OnUserSimulationMessage(
        NetworkRunner runner,
        SimulationMessagePtr message)
    {
    }


    public void OnSessionListUpdated(
        NetworkRunner runner,
        List<SessionInfo> sessionList)
    {
    }


    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        Dictionary<string, object> data)
    {
    }


    public void OnHostMigration(
        NetworkRunner runner,
        HostMigrationToken hostMigrationToken)
    {
    }


    public void OnSceneLoadDone(
        NetworkRunner runner)
    {
    }


    public void OnSceneLoadStart(
        NetworkRunner runner)
    {
    }


    public void OnObjectExitAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }


    public void OnObjectEnterAOI(
        NetworkRunner runner,
        NetworkObject obj,
        PlayerRef player)
    {
    }


    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        ArraySegment<byte> data)
    {
    }


    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress)
    {
    }
}