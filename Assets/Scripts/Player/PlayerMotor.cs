using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Windows;
using static EnemyAI;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private CapsuleCollider playerCollider;
    private Vector3 playerVelocity;
    private Vector2 currentInput;

    private InputManager inputManager;

    public float speed = 3f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 5f;
    public float crouchSpeed = 2f;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isMoving = false;

    private float maxStamina = 100f;
    public float stamina;
    [SerializeField] private float drainMultiplier = 20f;
    private float staminaGainTimer = 1f;
    private float staminaGainTimerEmpty = 3f;
    [SerializeField] private float staminaGainMultiplier = 10f;
    private bool isGainingStamina = false;
    private Coroutine staminaGainCoroutine;
    private Coroutine fadeOutCoroutine;

    public Image frontStamina;
    public Image backStamina;
    private float lerpTimer;
    public float chipSpeed = 1f;
    public float fadeDuration = 1.5f;

    //[SerializeField] private float movementThreshold = 0.1f;

    public bool isGrounded;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;

    [Header("Footstep Params")]
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.3f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] tileClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCollider = GetComponent<CapsuleCollider>();
        inputManager = GetComponent<InputManager>();
        stamina = maxStamina;

        frontStamina.color = new Color(frontStamina.color.r, frontStamina.color.g, frontStamina.color.b, 0f);
        backStamina.color = new Color(backStamina.color.r, backStamina.color.g, backStamina.color.b, 0f);
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        UpdateStaminaUI();

        if (isSprinting && isGrounded && isMoving)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
                Debug.Log("Stopped fade out coroutine");
            }
            
            frontStamina.color = new Color(frontStamina.color.r, frontStamina.color.g, frontStamina.color.b, 1);
            backStamina.color = new Color(backStamina.color.r, backStamina.color.g, backStamina.color.b, 1);

            stamina -= Time.deltaTime * drainMultiplier;
            stamina = Mathf.Max(stamina, 0);

            if (isGainingStamina && staminaGainCoroutine != null)
            {
                StopCoroutine(staminaGainCoroutine);
                isGainingStamina = false;
            }
        }

        if (stamina <= 0)
        {
            stamina = Mathf.Max(stamina, 0);
            inputManager.playerControls.Sprint.Disable();
            inputManager.playerControls.Jump.Disable();
        }


        if (!isGainingStamina && stamina < maxStamina && !isSprinting)
        {
            float delay = stamina <= 0 ? staminaGainTimerEmpty : staminaGainTimer;
            staminaGainCoroutine = StartCoroutine(GainStamina(delay));
        }

        if (stamina > 0 && !inputManager.playerControls.Sprint.enabled)
        {
            inputManager.playerControls.Sprint.Enable();
            inputManager.playerControls.Jump.Enable();
        }

        if (useFootsteps)
        {
            HandleFootsteps();
        }
    }

    IEnumerator GainStamina(float timeTillGain)
    {
        isGainingStamina = true;
        yield return new WaitForSeconds(timeTillGain);

        while (stamina < maxStamina && !isSprinting)
        {
            stamina += Time.deltaTime * staminaGainMultiplier;
            stamina = Mathf.Min(stamina, maxStamina);
            yield return null;
        }

        isGainingStamina = false;
        if (stamina >= maxStamina && !isSprinting)
        {
            fadeOutCoroutine = StartCoroutine(FadeOut(frontStamina, backStamina, 0f, fadeDuration));
        }
    }

    IEnumerator FadeOut(Image imageFront, Image imageBack, float targetAlpha, float duration)
    {
        //Debug.Log("FADE");
        float startAlphaFront = frontStamina.color.a;
        float startAlphaBack = backStamina.color.a;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float currentAlphaFront = Mathf.Lerp(startAlphaFront, targetAlpha, timeElapsed / duration);
            float currentAlphaBack = Mathf.Lerp(startAlphaBack, targetAlpha, timeElapsed / duration);

            Color currentColorFront = imageFront.color;
            Color currentColorBack = imageBack.color;

            currentColorFront.a = currentAlphaFront;
            currentColorBack.a = currentAlphaBack;

            imageFront.color = currentColorFront;
            imageBack.color = currentColorBack;

            yield return null;
        }
        fadeOutCoroutine = null;
        Debug.Log("FADE COMPLETED");
    }

    public void ProcessMove(Vector2 input)
    {
        currentInput = input;
        isMoving = currentInput.magnitude > 0.1f;

        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
                Debug.Log("Stopped fade out coroutine");
            }

            frontStamina.color = new Color(frontStamina.color.r, frontStamina.color.g, frontStamina.color.b, 1);
            backStamina.color = new Color(backStamina.color.r, backStamina.color.g, backStamina.color.b, 1);

            if (isGainingStamina && staminaGainCoroutine != null)
            {
                StopCoroutine(staminaGainCoroutine);
                isGainingStamina = false;
            }

            playerVelocity.y = Mathf.Sqrt(jumpHeight * gravity * -1f);
            stamina -= (maxStamina / 5);
        }
    }
    public void Sprint()
    {
        isSprinting = true;
        isCrouching = false;
        speed = sprintSpeed;
        footstepAudioSource.volume = .75f;
    }
    public void Walk()
    {
        isSprinting = false;
        isCrouching = false;
        speed = walkSpeed;
        footstepAudioSource.volume = .5f;

        playerCollider.height = 2;
        Vector3 crouchHeight = Vector3.zero;
        playerCollider.center = crouchHeight;
    }
    public void Crouch()
    {
        if (isGrounded)
        {
            isSprinting = false;
            isCrouching = true;
            speed = crouchSpeed;
            footstepAudioSource.volume = .25f;
        }
        playerCollider.height = 1;
        Vector3 crouchHeight = Vector3.zero;
        crouchHeight.y = -0.5f;
        playerCollider.center = crouchHeight;
    }

    private void HandleFootsteps()
    {
        if (!isGrounded || !isMoving) return;
        
        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(playerCollider.transform.position, Vector3.down, out RaycastHit hit, 2))
            {
                switch(hit.collider.tag)
                {
                    case "Footsteps/Wood":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/Tile":
                        footstepAudioSource.PlayOneShot(tileClips[UnityEngine.Random.Range(0, tileClips.Length - 1)]);
                        break;
                    case "Footsteps/Grass":
                        footstepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, tileClips.Length - 1)]);
                        break;
                }
            }
            footstepTimer = GetCurrentOffset;
        }
    }

    public void UpdateStaminaUI()
    {
        float fillFront = frontStamina.fillAmount;
        float staminaFraction = stamina / maxStamina;
        if (fillFront != staminaFraction)
        {
            if (fillFront > staminaFraction)
            {
                frontStamina.fillAmount = staminaFraction;
                lerpTimer = 0f; 
            }
            else if (fillFront < staminaFraction)
            {
                lerpTimer += Time.deltaTime;
                float percentComplete = lerpTimer / chipSpeed;
                percentComplete = percentComplete * percentComplete;
                frontStamina.fillAmount = Mathf.Lerp(fillFront, staminaFraction, percentComplete);
            }
        }
        else
        {
            lerpTimer = 0f;
        }
    }
}
