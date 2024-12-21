using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TP_CC_PlayerMovement : MonoBehaviour, FPS_Input.IPlayerActions
{
    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float gravity = -9.8f;
    public float groundedGravity = -2f;
    public float jumpHeight = 3f;
    public float jumpCooldown;
    [Range(0f, 1f)]
    public float crouchSpeed = 0.5f;
    public float rotationSpeed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    [Header("References")]
    public Transform player;
    public Transform playerObject;
    public Transform orientation;
    public Camera mainCam;

    private bool readyToJump = true;
    private bool isGrounded = true;
    private bool isSprinting = false;

    private bool isCrouching = false;
    private bool lerpCrouch = false;
    private float crouchTimer = 0f;

    private FPS_Input playerInput;
    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector3 playerMoveDirection;
    private Vector3 playerVerticalVelocity;

    #region Unity Callbacks ---------------------------------------------------------------------------------------
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    //This initialization of the movement maps can be done with the 'PlayerInput' component in editor.
    public void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerInput == null)
        {
            playerInput = new FPS_Input();
            playerInput.Player.SetCallbacks(this);//We are hooking up the callbacks from the input to the ones here.
        }

        playerInput.Player.Enable();
    }

    public void OnDisable()
    {
        playerInput.Player.Disable();
    }

    private void FixedUpdate()
    {
        ProcessMovement();
        ProcessGravity();
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;

        ProcessCrouch();
    }
    #endregion

    #region Helper Functions ---------------------------------------------------------------------------------------
    private void ProcessMovement()
    {
        float movementSpeed = speed;
        if (isSprinting)
            movementSpeed = sprintSpeed;

        Vector3 moveVectorDir = new Vector3();
        //This is for the player look movement direction
        if (playerMoveDirection != Vector3.zero)
        {
            moveVectorDir = (orientation.forward * playerMoveDirection.z) + (orientation.right * playerMoveDirection.x);
            
            playerObject.forward = Vector3.Slerp(playerObject.forward, moveVectorDir, Time.deltaTime * rotationSpeed);

            moveVectorDir *= movementSpeed;
        }

        characterController.Move(transform.TransformDirection(moveVectorDir) * Time.deltaTime);
    }

    private void ProcessGravity()
    {
        playerVerticalVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVerticalVelocity.y < 0)
        {
            //done so that we don't have a constant growing gravity when we are grounded
            playerVerticalVelocity.y = groundedGravity;
        }
        characterController.Move(playerVerticalVelocity * Time.deltaTime);
    }

    private void ProcessCrouch()
    {
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= crouchSpeed;
            if (isCrouching)
                characterController.height = Mathf.Lerp(characterController.height, 1, p);
            else
                characterController.height = Mathf.Lerp(characterController.height, 2, p);

            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0;
            }
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    #endregion

    #region Movement Callbacks ---------------------------------------------------------------------------------------
    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && readyToJump)
        {
            readyToJump = false;
            playerVerticalVelocity.y = Mathf.Sqrt(jumpHeight * -gravity);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector3 viewDir = player.position - new Vector3(mainCam.transform.position.x, player.position.y, mainCam.transform.position.z);
        orientation.forward = viewDir.normalized;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector3 moveDir = Vector3.zero;
        moveDir.x = context.ReadValue<Vector2>().x;
        moveDir.z = context.ReadValue<Vector2>().y;

        playerMoveDirection = moveDir;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = !isSprinting;
    }
    #endregion
}
