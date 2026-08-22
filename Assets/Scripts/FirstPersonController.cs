using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 8.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look")]
    public float mouseSensitivity = 0.1f;
    public float upLimit = -90.0f;
    public float downLimit = 90.0f;

    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 velocity;
    private float rotationX = 0f;
    private bool isGrounded;

    [Header("Intro Cutscene")]
    public bool isAutoWalkingToIntro = true;

    // Mobile Input States
    private Vector2 mobileMoveInput = Vector2.zero;
    private Vector2 mobileLookInput = Vector2.zero;
    private bool mobileJumpPressed = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Unlock cursor initially during onboarding so the player can interact with the questionnaire
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Ground Check
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Auto-run cutscene at start: move Fox automatically to the first platform starting position (Z = -1.5f)
        if (isAutoWalkingToIntro)
        {
            Vector3 target = new Vector3(0f, transform.position.y, -1.5f);
            Vector3 dir = (target - transform.position);
            dir.y = 0f; // horizontal only
            
            if (dir.magnitude > 0.1f)
            {
                dir.Normalize();
                characterController.Move(dir * walkSpeed * Time.deltaTime);
                
                // Keep Fox facing forward along the path
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward), 8f * Time.deltaTime);
            }
            else
            {
                isAutoWalkingToIntro = false; // give control to the player!
                
                // Hide and lock cursor ONLY if not on mobile/touch screen devices
#if !UNITY_ANDROID && !UNITY_IOS
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
#endif
            }

            // Apply gravity so the Fox stays grounded during auto-walk
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
            return;
        }

        // --- LOOK ---
        Vector2 lookInput = Vector2.zero;
        if (Mouse.current != null)
        {
            lookInput += Mouse.current.delta.ReadValue() * mouseSensitivity;
        }
        
        // Add mobile look (swipe delta)
        lookInput += mobileLookInput;
        // Consume mobile look delta
        mobileLookInput = Vector2.zero;

        if (lookInput.sqrMagnitude > 0.0001f)
        {
            // Horizontal rotation (yaw)
            transform.Rotate(Vector3.up * lookInput.x);

            // Vertical rotation (pitch)
            rotationX -= lookInput.y;
            rotationX = Mathf.Clamp(rotationX, upLimit, downLimit);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
            }
        }

        // --- MOVE ---
        Vector2 keyboardMoveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) keyboardMoveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) keyboardMoveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) keyboardMoveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) keyboardMoveInput.x += 1f;
        }

        // Combine inputs
        Vector2 moveInput = keyboardMoveInput + mobileMoveInput;

        // Normalise move input to prevent fast diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        bool isSprinting = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
        float speed = isSprinting ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * speed * Time.deltaTime);

        // --- JUMP & GRAVITY ---
        bool jumpTriggered = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) || mobileJumpPressed;
        if (jumpTriggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }
        // Consume jump input
        mobileJumpPressed = false;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // --- MOBILE API HOOKS ---
    public void SetMobileMove(Vector2 move)
    {
        mobileMoveInput = move;
    }

    public void SetMobileLook(Vector2 lookDelta)
    {
        mobileLookInput = lookDelta;
    }

    public void TriggerJump()
    {
        mobileJumpPressed = true;
    }
}
