using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.PlayerActions playerControls;

    private PlayerMotor motor;
    private PlayerLook look;

    void Awake()
    {
        playerInput = new PlayerInput();
        playerControls = playerInput.Player;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        playerControls.Jump.performed += ctx => motor.Jump(); //callbackcontexxt

        playerControls.Sprint.performed += ctx => motor.Sprint();
        playerControls.Sprint.canceled += ctx => motor.Walk();

        playerControls.Crouch.performed += ctx => motor.Crouch();
        playerControls.Crouch.canceled += ctx => motor.Walk();
    }

    private void LateUpdate()
    {
        motor.ProcessMove(playerControls.Movement.ReadValue<Vector2>());
        look.ProcessLook(playerControls.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerControls.Enable();
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerControls.Disable();
    }
}
