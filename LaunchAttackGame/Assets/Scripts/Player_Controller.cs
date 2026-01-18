using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private PlayerInputActions input;
    private Vector2 moveInput;
    private bool jumpPressed;
    private Vector3 velocity;

    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;
    private bool grounded;



    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Jump.performed += ctx => jumpPressed = true;
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        // --- MOVEMENT INPUT ---
        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // Animator: running state
        bool isRunning = inputDir.magnitude > 0.1f;
        animator.SetBool("isRunning", isRunning);

        // --- ROTATION + MOVEMENT ---
        if (isRunning)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;

            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle,
                                          rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // --- GROUND CHECK ---
        GroundCheck(); 
        animator.SetBool("isGrounded", grounded); 
        animator.SetBool("inAir", !grounded);

        // --- JUMP ---
        if (grounded && jumpPressed) 
        { 
            velocity.y = jumpForce; 
            animator.SetTrigger("Jump"); 
            jumpPressed = false; 
        }

        // Reset jumpPressed if not grounded
        if (!grounded)
            jumpPressed = false;

        // --- GRAVITY ---
        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void GroundCheck()
    { // Start from the bottom of the CharacterController
        Vector3 start = transform.position + controller.center; 
        float rayLength = (controller.height / 2f) + 0.1f; 

        grounded = Physics.Raycast(start, Vector3.down, rayLength, groundMask); }
    }
