using UnityEngine;
using UnityEngine.InputSystem;
using static TP_PlayerMovement;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class CombinationController_RB : MonoBehaviour, FPS_Input.IPlayerActions
{
    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float jumpForce = 3f;
    public float jumpCooldown;
    [Range(0f, 1f)]
    public float crouchSpeed = 0.5f;
    public float rotationSpeed;

    [Header("First Person Camera")]
    public Camera firstPersonCam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float minClampX = -80f;
    public float maxClampX = 80f;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    [Header("References")]
    public Camera basicTPCam;
    public Transform player;
    public Transform playerObject;
    public Transform orientation;
    public Transform combatLookAt;

    //mechanical
    private bool readyToJump = true;
    private bool isGrounded = true;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool lerpCrouch = false;
    private float crouchTimer = 0f;

    //for first person
    private float xRotation = 0f;
    private float camMovementX;
    private float camMovementY;

    //Third person
    private Vector3 moveDirection;

    //General
    private Vector3 playerMoveDirection;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private FPS_Input playerInput;

    [SerializeField]
    private CameraStyle currentStyle;

    public enum CameraStyle
    {
        FirstPerson,
        Basic_TP,
        Combat_TP,
        Topdown_TP
    }

    #region Unity Callbacks ---------------------------------------------------------------------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        currentStyle = CameraStyle.Basic_TP;
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
        if (currentStyle == CameraStyle.FirstPerson)
            ProcessLook();

        ProcessMovement();
    }

    private void Update()
    {
        CheckGrounded();//we are checking if we are grouned each frame

        ProcessCrouch();
    }
    #endregion

    #region Helper Functions ---------------------------------------------------------------------------------------
    private void ProcessMovement()
    {
        float movementSpeed = speed;
        if (isSprinting)
            movementSpeed = sprintSpeed;

        //we want the movement direction to be based around our orientation.forward, not the player forward
        Vector3 moveVectorDir = new Vector3();
        //This is for the player look movement direction
        if (playerMoveDirection != Vector3.zero)
        {
            moveVectorDir = (orientation.forward * playerMoveDirection.z) + (orientation.right * playerMoveDirection.x);

            if (currentStyle == CameraStyle.Basic_TP)
                playerObject.forward = Vector3.Slerp(playerObject.forward, moveVectorDir, Time.deltaTime * rotationSpeed);

            moveVectorDir *= movementSpeed;
        }

        if (currentStyle == CameraStyle.Combat_TP)
            playerObject.forward = orientation.forward;

        rb.linearVelocity = new Vector3(moveVectorDir.x, rb.linearVelocity.y, moveVectorDir.z);
    }

    private void ProcessLook()
    {
        xRotation -= (camMovementY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, minClampX, maxClampX);
        basicTPCam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);//rotates the camera up and down

        transform.Rotate(Vector3.up * (camMovementX * Time.deltaTime) * xSensitivity);//rotate the player to look left and right

    }

    
    private void ProcessCrouch()
    {
        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= crouchSpeed;
            if (isCrouching)
                capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, 1, p);
            else
                capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, 2, p);

            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0;
            }
        }
    }

    private void CheckGrounded()
    {
        //For the rigid body version of this, we need to check to see if we are grounded manually
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    #endregion

    #region Movement Callbacks ---------------------------------------------------------------------------------------

    public void OnChangeCamera(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            currentStyle++;
            if ((int)currentStyle == System.Enum.GetValues(typeof(CameraStyle)).Length) currentStyle = 0;

            if(currentStyle == CameraStyle.FirstPerson)
            {
                firstPersonCam.enabled = true;
                basicTPCam.enabled = false;
                //we will need to turn off all of the other cams as well...
            }
        }
    }

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
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (currentStyle == CameraStyle.Basic_TP)
        {
            Vector3 viewDir = player.position - new Vector3(basicTPCam.transform.position.x, player.position.y, basicTPCam.transform.position.z);
            orientation.forward = viewDir.normalized;
        }
        else if (currentStyle == CameraStyle.Combat_TP)
        {
            Vector3 viewDir = combatLookAt.position - new Vector3(basicTPCam.transform.position.x, combatLookAt.position.y, basicTPCam.transform.position.z);
            orientation.forward = viewDir.normalized;
        }
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
