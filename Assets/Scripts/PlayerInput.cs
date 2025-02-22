using Fusion;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    public Vector2 LookRotation => _input.LookRotation;

    private GameplayInput _input;

    private PlayerInputAction playerInputActions;

    private Vector2 moveInput { get; set; }
    private Vector2 lookInput { get; set; }
    private bool isJumping { get; set; }
    private bool isRunning { get; set; }

    private void Awake()
    {
        playerInputActions = new PlayerInputAction();

        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInputActions.Player.Run.performed += ctx => isRunning = true;
        playerInputActions.Player.Run.canceled += ctx => isRunning = false;

        playerInputActions.Player.Jump.performed += ctx => isJumping = true;
        playerInputActions.Player.Jump.canceled += ctx => isJumping = false;

        playerInputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }


    public override void Spawned()
    {
        if (HasInputAuthority == false)
            return;

        var networkEvents = Runner.GetComponent<NetworkEvents>();
        networkEvents.OnInput.AddListener(OnInput);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner == null)
            return;

        var networkEvents = runner.GetComponent<NetworkEvents>();
        if (networkEvents != null)
        {
            networkEvents.OnInput.RemoveListener(OnInput);
        }
    }

    private void Update()
    {

        if (HasInputAuthority == false)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            _input.MoveDirection = default;
            return;
        }

        var lookRotationDelta = new Vector2(-lookInput.y, lookInput.x);
        _input.LookRotation = ClampLookRotation(_input.LookRotation + lookRotationDelta);

        var moveDirection = new Vector2(moveInput.x, moveInput.y);
        _input.MoveDirection = moveDirection.normalized;

        _input.Buttons.Set(EInputButton.Jump, isJumping);
        _input.Buttons.Set(EInputButton.Sprint, isRunning);
    }

    private void OnInput(NetworkRunner runner, NetworkInput networkInput)
    {
        networkInput.Set(_input);
    }

    private Vector2 ClampLookRotation(Vector2 lookRotation)
    {
        lookRotation.x = Mathf.Clamp(lookRotation.x, -30f, 70f);
        return lookRotation;
    }
}

