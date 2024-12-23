using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FirstPerson_Rigidbody_Movement : MonoBehaviour, FPS_Input.IPlayerActions
{
    public float speed = 5f;
    public float sprintSpeed = 7f;
    public float jumpForce = 3f;
    [Range(0f, 1f)]
    public float crouchSpeed = 0.5f;

    public Camera mainCam;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    public float minClampX = -80f;
    public float maxClampX = 80f;

    public Transform feetTransform;
    public LayerMask floorMask;//we need to be able to see if the player is on the floor, by using a floor mask

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private FPS_Input playerInput;

    private Vector3 playerMoveDirection;
    private bool isGrounded;

    private float xRotation = 0f;
    private float camMovementX;
    private float camMovementY;

    private bool isSprinting = false;

    private bool isCrouching = false;
    private float crouchTimer = 0f;
    private bool lerpCrouch = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
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
        ProcessLook();
    }

    private void Update()
    {
        CheckGrounded();//we are checking if we are grouned each frame

        ProcessCrouch();
    }

    private void ProcessMovement()
    {
        float movementSpeed = speed;
        if (isSprinting)
            movementSpeed = sprintSpeed;

        Vector3 moveVector = transform.TransformDirection(playerMoveDirection) * movementSpeed;

        rb.linearVelocity = new Vector3(moveVector.x, rb.linearVelocity.y, moveVector.z);
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
        isGrounded = Physics.CheckSphere(feetTransform.position, 0.1f, floorMask);
    }


    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = !isCrouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        camMovementX = context.ReadValue<Vector2>().x;
        camMovementY = context.ReadValue<Vector2>().y;
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

    public void OnChangeCamera(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }
}
