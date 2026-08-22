using UnityEngine;

public class FoxAnimationController : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private string currentAnimationState = "";

    void Start()
    {
        animator = GetComponent<Animator>();
        // Find CharacterController on parent (Player)
        characterController = GetComponentInParent<CharacterController>();
    }

    void Update()
    {
        if (animator == null || characterController == null) return;

        // Determine speed and grounded state
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        float speed = horizontalVelocity.magnitude;
        bool isGrounded = characterController.isGrounded;

        string targetState = "Fox_Idle";

        if (!isGrounded)
        {
            targetState = "Fox_Falling"; // Play falling animation when in air
        }
        else if (speed > 5.5f)
        {
            targetState = "Fox_Run_InPlace"; // Sprinting
        }
        else if (speed > 0.1f)
        {
            targetState = "Fox_Walk_InPlace"; // Walking
        }

        // Apply state transition using CrossFade to ensure smooth blending
        ChangeAnimationState(targetState);
    }

    private void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;

        animator.CrossFade(newState, 0.15f);
        currentAnimationState = newState;
    }
}

