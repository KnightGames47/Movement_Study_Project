using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

[RequireComponent(typeof(FirstPerson_PlayerMover), typeof(FirstPerson_PlayerLook))]
public class FirstPerson_InputManager : MonoBehaviour
{
    //The input manager has to have refernces to the different action response types
    //This way we can have the inputs be handled here, while the application to the character controller are elsewhere
    private FPS_Input playerInput;
    private FPS_Input.PlayerActions playerActionsMap;
    private FirstPerson_PlayerMover playerMover;
    private FirstPerson_PlayerLook playerLook;

    private void Awake()
    {
        playerInput = new FPS_Input();
        playerActionsMap = playerInput.Player;
        playerMover = GetComponent<FirstPerson_PlayerMover>();
        playerLook = GetComponent<FirstPerson_PlayerLook>();

        //This is how we subscribe to actions 
        playerActionsMap.Jump.performed += ctx => playerMover.OnJump();
        playerActionsMap.Sprint.performed += ctx => playerMover.OnSprint();
        playerActionsMap.Crouch.performed += ctx => playerMover.OnCrouch();
    }

    private void OnEnable()
    {
        playerActionsMap.Enable();
    }

    private void OnDisable()
    {
        playerActionsMap.Disable();
    }

    private void FixedUpdate()
    {
        //tell player mover to move from value from our action
        playerMover.ProcessMove(playerActionsMap.Movement.ReadValue<Vector2>());

        //dealing with player look
        playerLook.ProcessLook(playerActionsMap.Look.ReadValue<Vector2>());
    }
}
