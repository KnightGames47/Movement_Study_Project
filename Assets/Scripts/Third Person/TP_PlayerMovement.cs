using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class TP_PlayerMovement : MonoBehaviour, FPS_Input.IPlayerActions
{
    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float jumpForce = 3f;
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

    private Vector3 moveDirection;
    private Vector3 playerMoveDirection;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private FPS_Input playerInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
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
    }

    private void Update()
    {
        CheckGrounded();
        ProcessCrouch();
    }

    private void ProcessMovement()
    {
        float movementSpeed = speed;
        if (isSprinting)
            movementSpeed = sprintSpeed;

        //we want the movement direction to be based around our orientation.forward, not the player forward

        //This is for the player look movement direction
        if (playerMoveDirection != Vector3.zero)
        {
            playerObject.forward = Vector3.Slerp(playerObject.forward, playerMoveDirection, Time.deltaTime * rotationSpeed);
        }
        

        Vector3 moveVector = transform.TransformDirection(playerMoveDirection) * movementSpeed;

        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);
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
        //isGrounded = Physics.CheckSphere(feetTransform.position, 0.1f, floorMask);
        //Using the raycast instead of sphere cast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        playerMoveDirection = (orientation.forward * context.ReadValue<Vector2>().y) +
            (orientation.right * context.ReadValue<Vector2>().x);

        playerMoveDirection.Normalize();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded && readyToJump)
        {
            readyToJump = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector3 viewDir = player.position - new Vector3(mainCam.transform.position.x, player.position.y, mainCam.transform.position.z);
        orientation.forward = viewDir.normalized;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = !isSprinting;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }
}
