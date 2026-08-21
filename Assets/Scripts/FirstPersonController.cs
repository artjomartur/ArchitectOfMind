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

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Lock cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Ground Check
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- LOOK ---
        if (Mouse.current != null)
        {
            Vector2 lookInput = Mouse.current.delta.ReadValue() * mouseSensitivity;
            
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
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
        }

        // Normalise move input to prevent fast diagonal movement
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        float speed = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ? runSpeed : walkSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * speed * Time.deltaTime);

        // --- JUMP & GRAVITY ---
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
