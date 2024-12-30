using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPerson_PlayerMover : MonoBehaviour
{
    //With the way that these are set up, we can have a bunch of different scripts that individually take care of 
    //Things like jumping and gravity, but it makes sense to put some of the things in here
    //We are putting movement, sprinting, gravity, crouching, and jumping in here.

    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float gravity = -9.8f;
    public float groundedGravity = -2f;
    public float jumpHeight = 3f;
    [Range(0f, 1f)]
    public float crouchSpeed = 0.5f;

    private CharacterController characterController;
    private Vector3 playerVelocity;
    private bool isGrounded;

    private bool isSprinting = false;

    private bool isCrouching = false;
    private float crouchTimer = 0f;
    private bool lerpCrouch = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();    
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        ProcessCrouch();
    }

    /// <summary>
    /// Receives input from input manager and apply to character controller
    /// </summary>
    /// <param name="input"></param>
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;

        moveDirection.x = input.x;
        moveDirection.z = input.y;

        float moveSpeed = speed;
        if (isSprinting)
            moveSpeed = sprintSpeed;

        characterController.Move(transform.TransformDirection(moveDirection) * moveSpeed * Time.deltaTime);

        //applying gravity, and caping it when grounded
        playerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && playerVelocity.y < 0)
            playerVelocity.y = groundedGravity;
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    private void ProcessCrouch()
    {
        if(lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= crouchSpeed;

            if (isCrouching)
                characterController.height = Mathf.Lerp(characterController.height, 1, p);
            else 
                characterController.height = Mathf.Lerp(characterController.height, 2, p);

            if(p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -gravity);
        }

    }

    public void OnSprint()
    {
        //same toggle method as before
        isSprinting = !isSprinting;
    }

    public void OnCrouch()
    {
        isCrouching = !isCrouching;
        crouchTimer = 0f;
        lerpCrouch = true;
    }
}
