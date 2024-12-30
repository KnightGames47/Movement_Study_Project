using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPerson_Movement : MonoBehaviour, FPS_Input.IPlayerActions
{
    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float gravity = -9.8f;
    public float groundedGravity = -2f;
    public float jumpHeight = 3f;
    [Range(0f, 1f)]
    public float crouchSpeed = 0.5f;

    public Camera mainCam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float minClampX = -80f;
    public float maxClampX = 80f;

    private FPS_Input playerInput;
    private CharacterController characterController;
    private Vector3 playerMoveDirection;
    private Vector3 playerVerticalVelocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private float camMovementX;
    private float camMovementY;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private float crouchTimer = 0f;
    private bool lerpCrouch = false;


    #region Unity Callbacks ---------------------------------------------------------------------------------------
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    //This initialization of the movement maps can be done with the 'PlayerInput' component in editor.
    public void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
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

        ProcessLook();
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

        characterController.Move(transform.TransformDirection(playerMoveDirection) * movementSpeed * Time.deltaTime);
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

    private void ProcessLook()
    {
        xRotation -= (camMovementY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, minClampX, maxClampX);
        mainCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);//rotates the camera up and down

        transform.Rotate(Vector3.up * (camMovementX * Time.deltaTime) * xSensitivity);//rotate the player to look left and right
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
                crouchTimer = 0;
            }
        }
    }
    #endregion

    #region Movement Callbacks ---------------------------------------------------------------------------------------
    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector3 moveDir = Vector3.zero;
        moveDir.x = context.ReadValue<Vector2>().x;
        moveDir.z = context.ReadValue<Vector2>().y;

        playerMoveDirection = moveDir;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded)//Only jump if grounded
        {
            playerVerticalVelocity.y = Mathf.Sqrt(jumpHeight * -gravity);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        camMovementX = context.ReadValue<Vector2>().x;
        camMovementY = context.ReadValue<Vector2>().y;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        //This is a toggling method, which feels a bit wrong
        isSprinting = !isSprinting;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void OnChangeCamera(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}

