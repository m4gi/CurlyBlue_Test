using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isJumping;
    private float verticalVelocity;
    private bool isRunning;
    private bool isGrounded;

    private Animator animator;
    private PlayerInputAction playerInputActions;

    private void Awake()
    {
        GetPlayerComponents();
        SetupPlayerInputAction();
    }

    private void GetPlayerComponents()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void SetupPlayerInputAction()
    {
        playerInputActions = new PlayerInputAction();

        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        playerInputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        playerInputActions.Player.Run.performed += ctx => isRunning = true;
        playerInputActions.Player.Run.canceled += ctx => isRunning = false;


        playerInputActions.Player.Jump.performed += ctx => Jump();
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void Update()
    {      
        Move();
        Rotate();      
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyGravity();
    }

    private void CheckGrounded()
    {
        float raycastDistance = 0.2f; 
        Vector3 raycastOrigin = transform.position + Vector3.down * 0.1f;
        RaycastHit hit;

        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        moveDirection = transform.TransformDirection(moveDirection);

        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 moveVelocity = moveDirection * currentSpeed;

        if (isGrounded)
        {
            verticalVelocity = -0.5f;
            if (isJumping)
            {
                verticalVelocity = jumpForce;
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        moveVelocity.y = verticalVelocity;
        characterController.Move(moveVelocity * Time.deltaTime);
    }

    private void Rotate()
    {
        if (lookInput != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            isJumping = true;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void UpdateAnimations()
    {
        float speed = moveInput.magnitude;
        if (isRunning)
        {
            speed *= 2; 
        }
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsGrounded", isGrounded);
    }
}
