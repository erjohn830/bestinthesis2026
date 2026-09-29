using Fusion;
using UnityEngine;

public class PatinteroGameplayPlayer : NetworkBehaviour
{
    public static PatinteroGameplayPlayer Local;


    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("MOVEMENT")]
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private float attackerSpeed = 4.5f;

    [SerializeField]
    private float defenderSpeed = 4f;

    [SerializeField]
    private float rotationSpeed = 12f;


    // =========================================================
    // TOUCH / TAG
    // =========================================================

    [Header("TOUCH / TAG")]
    [SerializeField]
    private float touchAnimationDuration = 0.65f;

    [SerializeField]
    private float touchCooldown = 0.9f;


    // =========================================================
    // CAMERA
    // =========================================================

    [Header("CAMERA")]
    [SerializeField]
    private Transform cameraTarget;


    // =========================================================
    // CHARACTERS
    // =========================================================

    [Header("10 CHARACTER MODELS")]
    [SerializeField]
    private GameObject[] characterModels =
        new GameObject[10];

    [SerializeField]
    private Animator[] characterAnimators =
        new Animator[10];


    // =========================================================
    // NETWORK DATA
    // =========================================================

    [Networked]
    public NetworkString<_32> PlayerName
    {
        get;
        set;
    }

    [Networked]
    public int TeamId
    {
        get;
        set;
    }

    [Networked]
    public int CharacterIndex
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool Configured
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool IsDefender
    {
        get;
        set;
    }

    [Networked]
    public int GuardLineIndex
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool ReachedEnd
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool ReturnedHome
    {
        get;
        set;
    }

    [Networked]
    public NetworkBool IsMoving
    {
        get;
        set;
    }

    [Networked]
    public int FacingSign
    {
        get;
        set;
    }


    // 0  = Normal
    // 1  = Win
    // -1 = Lose

    [Networked]
    public int ResultState
    {
        get;
        set;
    }


    // =========================================================
    // TOUCH TIMERS
    // =========================================================

    [Networked]
    private TickTimer TouchAnimationTimer
    {
        get;
        set;
    }


    [Networked]
    private TickTimer TouchCooldownTimer
    {
        get;
        set;
    }


    [Networked]
    private NetworkButtons PreviousButtons
    {
        get;
        set;
    }


    // =========================================================
    // LOCAL VISUAL STATE
    // =========================================================

    private int lastVisualCharacter = -1;

    private string lastAnimation = "";

    // Visuals are controlled locally on every client.
    // This avoids relying only on Fusion Render() timing.
    private bool visualReady = false;

    private readonly string[] characterNames =
    {
        "Angelo",
        "Jose",
        "Juan",
        "Liam",
        "Lisa",
        "Lolo",
        "Maria",
        "Miguel",
        "Pedro",
        "Robert"
    };


    // =========================================================
    // CAMERA TARGET
    // =========================================================

    public Transform CameraTarget
    {
        get
        {
            return cameraTarget != null
                ? cameraTarget
                : transform;
        }
    }


    // =========================================================
    // SPAWNED
    // =========================================================

    public override void Spawned()
    {
        if (controller == null)
        {
            controller =
                GetComponent<CharacterController>();
        }

        // Make sure the arrays still point to the 10 prefab children.
        CacheCharacterReferences();

        // Start clean.
        DisableAllCharacterModels();

        // Bootstrap should already have written the network data.
        // If the Host somehow received the player with Configured=false,
        // make the object usable instead of leaving the whole player hidden.
        if (
            Object.HasStateAuthority &&
            !Configured)
        {
            Configured = true;

            Debug.LogWarning(
                "PATINTERO PLAYER CONFIGURED SAFETY FALLBACK | " +
                Object.InputAuthority +
                " | TEAM = " +
                TeamId +
                " | CHARACTER = " +
                CharacterIndex
            );
        }

        visualReady = true;

        if (Object.HasInputAuthority)
        {
            Local = this;

            Debug.Log(
                "LOCAL PATINTERO GAMEPLAY PLAYER READY | " +
                Object.InputAuthority +
                " | TEAM = " +
                TeamId +
                " | CHARACTER = " +
                CharacterIndex +
                " | CONFIGURED = " +
                Configured
            );
        }

        // Show the selected character immediately.
        ApplyCharacterVisual();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Local == this)
        {
            Local = null;
        }
    }


    // =========================================================
    // SERVER INITIALIZE FROM LOBBY
    // =========================================================
    //
    // Called by PatinteroGameplayBootstrap.
    //
    // This makes the gameplay player receive the player's
    // lobby name, team and selected character.
    // =========================================================

    public void InitializeBeforeSpawn(
        string playerName,
        int teamId,
        int characterIndex)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }

        PlayerName =
            playerName.Trim();

        TeamId =
            Mathf.Clamp(
                teamId,
                0,
                1
            );

        CharacterIndex =
            Mathf.Clamp(
                characterIndex,
                0,
                9
            );

        FacingSign = 1;
        IsDefender = false;
        GuardLineIndex = -1;
        ReachedEnd = false;
        ReturnedHome = false;
        IsMoving = false;
        ResultState = 0;
        TouchAnimationTimer = TickTimer.None;
        TouchCooldownTimer = TickTimer.None;

        // ApplyCharacterVisual() depends on this.
        Configured = true;
    }


    // Kept for compatibility with any existing caller.
    public void ServerInitializeFromLobby(
        string playerName,
        int teamId,
        int characterIndex)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        InitializeBeforeSpawn(
            playerName,
            teamId,
            characterIndex
        );

        Debug.Log(
            "SERVER INITIALIZED PATINTERO PLAYER | " +
            PlayerName.ToString() +
            " | TEAM " +
            TeamId +
            " | CHARACTER " +
            CharacterIndex
        );
    }


    // =========================================================
    // PLAYER SETUP RPC
    // =========================================================

    


    // =========================================================
    // NETWORK UPDATE
    // =========================================================

    public override void FixedUpdateNetwork()
    {
        // Host / State Authority moves players.
        if (!Object.HasStateAuthority)
        {
            return;
        }


        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        if (
            match == null ||
            !Configured)
        {
            return;
        }


        if (!match.IsRoundPlaying)
        {
            IsMoving = false;

            return;
        }


        if (
            !GetInput(
                out PatinteroInputData input))
        {
            return;
        }


        // =====================================================
        // BUTTON PRESSES
        // =====================================================

        bool turnPressed =
            input.Buttons.WasPressed(
                PreviousButtons,
                PatinteroButton.Turn
            );


        bool touchPressed =
            input.Buttons.WasPressed(
                PreviousButtons,
                PatinteroButton.Touch
            );


        PreviousButtons =
            input.Buttons;


        // =====================================================
        // DEFENDER ROTATE
        // =====================================================

        if (
            IsDefender &&
            turnPressed)
        {
            FacingSign *= -1;


            if (FacingSign == 0)
            {
                FacingSign = 1;
            }
        }


        // =====================================================
        // DEFENDER TOUCH / TAG
        // =====================================================

        if (
            IsDefender &&
            touchPressed)
        {
            TryGuardTouch();
        }


        // =====================================================
        // STOP MOVEMENT DURING TOUCH ANIMATION
        // =====================================================

        if (
            IsDefender &&
            IsTouchAnimationPlaying())
        {
            IsMoving = false;

            ApplyDefenderFacing();

            return;
        }


        // =====================================================
        // MOVEMENT
        // =====================================================

        if (IsDefender)
        {
            MoveDefender(input);
        }
        else
        {
            MoveAttacker(input);
        }
    }


    // =========================================================
    // TOUCH / TAG
    // =========================================================

    private void TryGuardTouch()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }


        // Cooldown still active.
        if (
            TouchCooldownTimer.IsRunning &&
            !TouchCooldownTimer.Expired(Runner))
        {
            return;
        }


        TouchAnimationTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                touchAnimationDuration
            );


        TouchCooldownTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                touchCooldown
            );


        IsMoving = false;


        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        if (match != null)
        {
            match.TryGuardTouch(this);
        }
    }


    // =========================================================
    // TOUCH ANIMATION CHECK
    // =========================================================

    public bool IsTouchAnimationPlaying()
    {
        if (Runner == null)
        {
            return false;
        }


        return
            TouchAnimationTimer.IsRunning &&
            !TouchAnimationTimer.Expired(Runner);
    }


    // =========================================================
    // ATTACKER MOVEMENT
    // =========================================================

    private void MoveAttacker(
        PatinteroInputData input)
    {
        if (controller == null)
        {
            return;
        }


        Vector2 move =
            Vector2.ClampMagnitude(
                input.Move,
                1f
            );


        Quaternion cameraRotation =
            Quaternion.Euler(
                0f,
                input.CameraYaw,
                0f
            );


        Vector3 direction =
            cameraRotation *
            new Vector3(
                move.x,
                0f,
                move.y
            );


        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();


            Vector3 velocity =
                direction *
                attackerSpeed;


            velocity.y = -2f;


            controller.Move(
                velocity *
                Runner.DeltaTime
            );


            float targetY =
                Quaternion.LookRotation(
                    direction,
                    Vector3.up
                )
                .eulerAngles
                .y;


            float newY =
                Mathf.LerpAngle(
                    transform.eulerAngles.y,
                    targetY,
                    rotationSpeed *
                    Runner.DeltaTime
                );


            // Keep body vertical.
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    newY,
                    0f
                );


            IsMoving = true;
        }
        else
        {
            controller.Move(
                Vector3.down *
                2f *
                Runner.DeltaTime
            );


            ForceUpright();

            IsMoving = false;
        }
    }


    // =========================================================
    // DEFENDER MOVEMENT
    // =========================================================

    private void MoveDefender(
        PatinteroInputData input)
    {
        if (controller == null)
        {
            return;
        }


        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        if (match == null)
        {
            return;
        }


        PatinteroGuardLine line =
            match.GetGuardLine(
                GuardLineIndex
            );


        if (line == null)
        {
            return;
        }


        // GuardLine01-04:
        // X = Left / Right
        //
        // GuardLine05:
        // Y = Up / Down

        float movement =
            line.GetMovementInput(
                input.Move
            );


        Vector3 direction =
            line.Axis *
            movement;


        Vector3 velocity =
            direction *
            defenderSpeed;


        velocity.y = -2f;


        controller.Move(
            velocity *
            Runner.DeltaTime
        );


        // =====================================================
        // CLAMP DEFENDER TO YELLOW LINE
        // =====================================================

        Vector3 correctedPosition =
            line.ClampToLine(
                transform.position
            );


        if (
            Vector3.Distance(
                transform.position,
                correctedPosition
            ) > 0.001f)
        {
            controller.enabled = false;


            transform.position =
                correctedPosition;


            controller.enabled = true;
        }


        // =====================================================
        // FACE FRONT / BACK
        // =====================================================

        ApplyDefenderFacing();


        IsMoving =
            Mathf.Abs(movement) >
            0.05f;
    }


    // =========================================================
    // DEFENDER FACING
    // =========================================================

    private void ApplyDefenderFacing()
    {
        PatinteroGameplayRoundManager match =
            PatinteroGameplayRoundManager.Instance;


        if (match == null)
        {
            return;
        }


        PatinteroGuardLine line =
            match.GetGuardLine(
                GuardLineIndex
            );


        if (line == null)
        {
            return;
        }


        Vector3 facing =
            line.GetFacingDirection(
                FacingSign
            );


        if (facing.sqrMagnitude <= 0.001f)
        {
            return;
        }


        float targetY =
            Quaternion.LookRotation(
                facing,
                Vector3.up
            )
            .eulerAngles
            .y;


        float newY =
            Mathf.LerpAngle(
                transform.eulerAngles.y,
                targetY,
                rotationSpeed *
                Runner.DeltaTime
            );


        // Body always stays vertical.
        transform.rotation =
            Quaternion.Euler(
                0f,
                newY,
                0f
            );
    }


    // =========================================================
    // FORCE UPRIGHT
    // =========================================================

    private void ForceUpright()
    {
        transform.rotation =
            Quaternion.Euler(
                0f,
                transform.eulerAngles.y,
                0f
            );
    }


    // =========================================================
    // ROUND SETUP
    // =========================================================

    public void SetupForRound(
        bool defender,
        int guardLine,
        Vector3 position,
        Quaternion rotation)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }


        IsDefender =
            defender;


        GuardLineIndex =
            guardLine;


        ReachedEnd =
            false;


        ReturnedHome =
            false;


        ResultState =
            0;


        FacingSign =
            1;


        IsMoving =
            false;


        TouchAnimationTimer =
            TickTimer.None;


        TouchCooldownTimer =
            TickTimer.None;


        rotation =
            Quaternion.Euler(
                0f,
                rotation.eulerAngles.y,
                0f
            );


        Teleport(
            position,
            rotation
        );
    }


    // =========================================================
    // TELEPORT
    // =========================================================

    public void Teleport(
        Vector3 position,
        Quaternion rotation)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }


        rotation =
            Quaternion.Euler(
                0f,
                rotation.eulerAngles.y,
                0f
            );


        if (controller != null)
        {
            controller.enabled = false;
        }


        transform.SetPositionAndRotation(
            position,
            rotation
        );


        if (controller != null)
        {
            controller.enabled = true;
        }
    }


    // =========================================================
    // ATTACKER PROGRESS
    // =========================================================

    public void MarkReachedEnd()
    {
        if (
            !Object.HasStateAuthority ||
            IsDefender)
        {
            return;
        }


        ReachedEnd = true;
    }


    public void MarkReturnedHome()
    {
        if (
            !Object.HasStateAuthority ||
            IsDefender ||
            !ReachedEnd)
        {
            return;
        }


        ReturnedHome = true;
    }


    // =========================================================
    // ROUND RESULT
    // =========================================================

    public void SetRoundResult(
        bool winner)
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }


        ResultState =
            winner
                ? 1
                : -1;


        IsMoving =
            false;
    }


    // =========================================================
    // LOCAL VISUAL UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (!visualReady)
        {
            return;
        }

        // Enforce exactly one visible character every frame,
        // even if another script/prefab state changes a child.
        ApplyCharacterVisual();
    }


    // =========================================================
    // RENDER
    // =========================================================

    public override void Render()
    {
        ApplyCharacterVisual();

        UpdateAnimation();
    }


    // =========================================================
    // DISABLE ALL CHARACTER MODELS
    // =========================================================

    private void CacheCharacterReferences()
    {
        Transform characterRoot =
            transform.Find("CharacterModels");

        if (characterRoot == null)
        {
            Debug.LogError(
                "CharacterModels child was not found under " +
                gameObject.name
            );

            return;
        }

        // Parent must stay active.
        if (!characterRoot.gameObject.activeSelf)
        {
            characterRoot.gameObject.SetActive(true);
        }

        if (
            characterModels == null ||
            characterModels.Length != 10)
        {
            characterModels =
                new GameObject[10];
        }

        if (
            characterAnimators == null ||
            characterAnimators.Length != 10)
        {
            characterAnimators =
                new Animator[10];
        }

        for (
            int i = 0;
            i < characterNames.Length;
            i++)
        {
            Transform child =
                characterRoot.Find(
                    characterNames[i]
                );

            if (child == null)
            {
                Debug.LogError(
                    "Missing character child: " +
                    characterNames[i]
                );

                continue;
            }

            // Always use the actual runtime child.
            characterModels[i] =
                child.gameObject;

            if (characterAnimators[i] == null)
            {
                characterAnimators[i] =
                    child.GetComponentInChildren<Animator>(
                        true
                    );
            }
        }
    }


    private void DisableAllCharacterModels()
    {
        CacheCharacterReferences();

        if (characterModels == null)
        {
            return;
        }

        for (
            int i = 0;
            i < characterModels.Length;
            i++)
        {
            if (characterModels[i] != null)
            {
                characterModels[i]
                    .SetActive(false);
            }
        }
    }


    // =========================================================
    // CHARACTER VISUAL
    // =========================================================

    private void ApplyCharacterVisual()
    {
        if (!visualReady)
        {
            return;
        }

        CacheCharacterReferences();

        if (
            characterModels == null ||
            characterModels.Length != 10)
        {
            return;
        }

        int index =
            CharacterIndex;

        if (
            index < 0 ||
            index >= characterModels.Length)
        {
            DisableAllCharacterModels();

            return;
        }

        bool characterChanged =
            lastVisualCharacter != index;

        lastVisualCharacter =
            index;

        for (
            int i = 0;
            i < characterModels.Length;
            i++)
        {
            GameObject model =
                characterModels[i];

            if (model == null)
            {
                continue;
            }

            bool shouldBeActive =
                i == index;

            if (
                model.activeSelf !=
                shouldBeActive)
            {
                model.SetActive(
                    shouldBeActive
                );
            }
        }

        if (characterChanged)
        {
            lastAnimation = "";

            Debug.Log(
                "CHARACTER VISUAL ACTIVE | " +
                Object.InputAuthority +
                " | INDEX = " +
                index +
                " | NAME = " +
                characterNames[index]
            );
        }
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    private void UpdateAnimation()
    {
        if (
            !visualReady ||
            characterAnimators == null ||
            characterAnimators.Length == 0)
        {
            return;
        }


        int index =
            Mathf.Clamp(
                CharacterIndex,
                0,
                characterAnimators.Length - 1
            );


        Animator animator =
            characterAnimators[index];


        if (animator == null)
        {
            return;
        }


        // Prevent:
        // "Game object with animator is inactive"
        if (
            !animator.enabled ||
            !animator.gameObject.activeInHierarchy)
        {
            return;
        }


        string animationName;


        // =====================================================
        // WIN
        // =====================================================

        if (ResultState > 0)
        {
            animationName =
                "Win";
        }


        // =====================================================
        // LOSE
        // =====================================================

        else if (ResultState < 0)
        {
            animationName =
                "Lose";
        }


        // =====================================================
        // TAYA HAND / TOUCH
        // =====================================================

        else if (
            IsDefender &&
            IsTouchAnimationPlaying())
        {
            animationName =
                "TayaHand";
        }


        // =====================================================
        // DEFENDER
        // =====================================================

        else if (IsDefender)
        {
            animationName =
                IsMoving
                    ? "Taya"
                    : "TayaIdle";
        }


        // =====================================================
        // ATTACKER
        // =====================================================

        else
        {
            animationName =
                IsMoving
                    ? "Running"
                    : "Idle";
        }


        if (
            animationName ==
            lastAnimation)
        {
            return;
        }


        lastAnimation =
            animationName;


        animator.CrossFade(
            animationName,
            0.10f
        );
    }
}
