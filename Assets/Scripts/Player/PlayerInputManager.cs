using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.PlayerActions playerControls;
    public PlayerInput.UIActions playerUIControls;

    private PlayerMotor motor;
    private PlayerLook look;

    [SerializeField] GameObject settingsPanel;
    private bool isPaused = false;

    public bool holdingCrouch = false;

    void Awake()
    {
        playerInput = new PlayerInput();
        playerControls = playerInput.Player;
        playerUIControls = playerInput.UI;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();

        playerUIControls.Menu.started += ctx => TogglePause();

        playerControls.Jump.performed += ctx => motor.Jump(); //callbackcontexxt

        playerControls.Sprint.performed += ctx => motor.Sprint();
        playerControls.Sprint.canceled += ctx => motor.Walk();

        playerControls.Crouch.performed += ctx =>
        {
            motor.Crouch();
            holdingCrouch = true;
        };
        playerControls.Crouch.canceled += ctx =>
        {
            motor.Walk();
            holdingCrouch = false;
        };
    }

    private void LateUpdate()
    {
        if (!isPaused)
        {
            motor.ProcessMove(playerControls.Movement.ReadValue<Vector2>());
            look.ProcessLook(playerControls.Look.ReadValue<Vector2>());
        }
    }
    private void TogglePause()
    {
        isPaused = !isPaused;
        print(isPaused);
        settingsPanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        playerControls.Disable();
    }

    private void ResumeGame()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        playerControls.Enable();
    }

    public void OnEnable()
    {
        ResumeGame();
    }
    private void OnDisable()
    {
        if (!isPaused)
        {
            PauseGame();
        }
    }
}
