using Fusion;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    public SimpleKCC KCC;
    public PlayerInput Input;
    public Animator Animator;
    public Transform CameraPivot;
    public Transform CameraHandle;
    public TextMeshPro NameText;

    [Header("Movement Setup")]
    public float WalkSpeed = 2f;
    public float SprintSpeed = 5f;
    public float JumpImpulse = 10f;
    public float UpGravity = 25f;
    public float DownGravity = 40f;
    public float RotationSpeed = 8f;

    [Header("Movement Accelerations")]
    public float GroundAcceleration = 55f;
    public float GroundDeceleration = 25f;
    public float AirAcceleration = 25f;
    public float AirDeceleration = 1.3f;

    [Networked]
    private Vector3 _moveVelocity { get; set; }
    [Networked]
    private NetworkBool _isJumping { get; set; }
    [Networked]
    private NetworkButtons _previousButtons { get; set; }
    [Networked] NetworkString<_16> NickName { get; set; }

    // Animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private void Awake()
    {
        AssignAnimationIDs();
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            string nickname = Runner.SessionInfo.Properties["Nickname"];
            SetNickname(nickname);
            NameText.text = nickname;
        }
    }

    public void SetNickname(string newNickname)
    {
        if (HasInputAuthority)
        {
            Rpc_SetNickname(newNickname);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SetNickname(string nickname)
    {
        NickName = nickname;
    }

    private void LateUpdate()
    {
        if (HasInputAuthority == false)
            return;

        CameraPivot.rotation = Quaternion.Euler(Input.LookRotation);
        Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
        Vector3 direction = Camera.main.transform.position - transform.position;

        direction.y = 0;

        NameText.gameObject.transform.rotation = Quaternion.LookRotation(direction);
        NameText.gameObject.transform.rotation *= Quaternion.Euler(0, 180f, 0);
        //NameText.gameObject.transform.LookAt(Camera.main.transform);
    }

    public override void FixedUpdateNetwork()
    {
        var input = GetInput<GameplayInput>().GetValueOrDefault();
        ProcessInput(input, _previousButtons);

        if (KCC.IsGrounded)
        {
            _isJumping = false;
        }

        _previousButtons = input.Buttons;
    }

    public override void Render()
    {
        Animator.SetFloat(_animIDSpeed, KCC.RealSpeed, 0.15f, Time.deltaTime);
        //Animator.SetFloat(_animIDMotionSpeed, 1f);
        //Animator.SetBool(_animIDJump, _isJumping);
        //Animator.SetBool(_animIDGrounded, KCC.IsGrounded);
        //Animator.SetBool(_animIDFreeFall, KCC.RealVelocity.y < -10f);
    }

    private void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
    {
        float jumpImpulse = 0f;

        if (KCC.IsGrounded && input.Buttons.WasPressed(previousButtons, EInputButton.Jump))
        {
            // Set world space jump vector
            jumpImpulse = JumpImpulse;
            _isJumping = true;
        }

        KCC.SetGravity(KCC.RealVelocity.y >= 0f ? UpGravity : DownGravity);

        float speed = input.Buttons.IsSet(EInputButton.Sprint) ? SprintSpeed : WalkSpeed;

        var lookRotation = Quaternion.Euler(0f, input.LookRotation.y, 0f);
        var moveDirection = lookRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
        var desiredMoveVelocity = moveDirection * speed;

        float acceleration;
        if (desiredMoveVelocity == Vector3.zero)
        {
            acceleration = KCC.IsGrounded ? GroundDeceleration : AirDeceleration;
        }
        else
        {
            var currentRotation = KCC.TransformRotation;
            var targetRotation = Quaternion.LookRotation(moveDirection);
            var nextRotation = Quaternion.Lerp(currentRotation, targetRotation, RotationSpeed * Runner.DeltaTime);

            KCC.SetLookRotation(nextRotation.eulerAngles);

            acceleration = KCC.IsGrounded ? GroundAcceleration : AirAcceleration;
        }

        _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, acceleration * Runner.DeltaTime);

        if (KCC.ProjectOnGround(_moveVelocity, out var projectedVector))
        {
            _moveVelocity = projectedVector;
        }

        KCC.Move(_moveVelocity, jumpImpulse);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        //_animIDGrounded = Animator.StringToHash("Grounded");
        //_animIDJump = Animator.StringToHash("Jump");
        //_animIDFreeFall = Animator.StringToHash("FreeFall");
        //_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    //[Networked] private Vector2 moveInput { get; set; }
    //[Networked] private bool isJumping { get; set; }
    //[Networked] private bool isRunning { get; set; }
    //[Networked] private bool isGrounded { get; set; }


    //public float velocity = 5f;
    //public float moveSpeed = 5f;
    //public float jumpForce = 18f;
    //public float jumpTime = 0.85f;
    //public float gravity = 9.8f;

    //private float jumpElapsedTime = 0;
    //private CharacterController characterController;
    //private Animator animator;
    //private PlayerInputAction playerInputActions;

    //public override void Spawned()
    //{
    //    GetPlayerComponents();
    //    SetupPlayerInputAction();
    //}

    //private void GetPlayerComponents()
    //{
    //    animator = GetComponent<Animator>();
    //    characterController = GetComponent<CharacterController>();
    //}

    //private void SetupPlayerInputAction()
    //{
    //    playerInputActions = new PlayerInputAction();

    //    playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
    //    playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

    //    playerInputActions.Player.Run.performed += ctx => isRunning = true;
    //    playerInputActions.Player.Run.canceled += ctx => isRunning = false;

    //    playerInputActions.Player.Jump.performed += ctx => Jump();
    //}

    //private void OnEnable()
    //{
    //    playerInputActions.Enable();
    //}

    //private void OnDisable()
    //{
    //    playerInputActions.Disable();
    //}

    //public override void FixedUpdateNetwork()
    //{

    //    if (HasStateAuthority)
    //    {
    //        CheckGrounded();
    //        Move();
    //    }


    //    if (animator != null)
    //    {
    //        float minimumSpeed = 0.9f;
    //        animator.SetBool("IsWalking", characterController.velocity.magnitude > minimumSpeed);
    //        animator.SetBool("IsRunning", isRunning);
    //        animator.SetBool("IsJumping", !isGrounded);
    //    }
    //}

    //private void CheckGrounded()
    //{
    //    float raycastDistance = 0.2f;
    //    Vector3 raycastOrigin = transform.position + Vector3.down * 0.1f;
    //    RaycastHit hit;

    //    if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance))
    //    {
    //        isGrounded = true;
    //    }
    //    else
    //    {
    //        isGrounded = false;
    //    }
    //}

    //private void Move()
    //{
    //    float velocityAddition = 0;
    //    float directionX = moveInput.x * (velocity + velocityAddition) * Runner.DeltaTime;
    //    float directionZ = moveInput.y * (velocity + velocityAddition) * Runner.DeltaTime;
    //    float directionY = 0;

    //    if (isJumping)
    //    {
    //        directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Runner.DeltaTime;

    //        jumpElapsedTime += Runner.DeltaTime;
    //        if (jumpElapsedTime >= jumpTime)
    //        {
    //            isJumping = false;
    //            jumpElapsedTime = 0;
    //        }
    //    }

    //    directionY = directionY - gravity * Runner.DeltaTime;

    //    Vector3 forward = Camera.main.transform.forward;
    //    Vector3 right = Camera.main.transform.right;

    //    forward.y = 0;
    //    right.y = 0;

    //    forward.Normalize();
    //    right.Normalize();

    //    forward = forward * directionZ;
    //    right = right * directionX;

    //    if (directionX != 0 || directionZ != 0)
    //    {
    //        float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
    //        Quaternion rotation = Quaternion.Euler(0, angle, 0);
    //        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
    //    }

    //    Vector3 verticalDirection = Vector3.up * directionY;
    //    Vector3 horizontalDirection = forward + right;

    //    Vector3 movement = verticalDirection + horizontalDirection;
    //    characterController.Move(movement);
    //}

    //private void Jump()
    //{
    //    if (isGrounded)
    //    {
    //        isJumping = true;
    //    }
    //}
}
