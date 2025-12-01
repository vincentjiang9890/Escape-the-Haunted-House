using System.Collections;
using UnityEngine;

public class UnlockedDoor : Interactable
{
    public bool doorOpen = false;
    public float openRot = 90f;
    public float speed = 2f;

    private Transform pivot;
    private Vector3 initialRotation;
    private float currentRotation;
    private float targetRotation;

    [SerializeField] private AudioSource interactableAudioSource = default;
    [SerializeField] private AudioClip doorOpenAudio = default;
    [SerializeField] private AudioClip doorCloseAudio = default;
    private bool isDoorMoving = false;
    private bool audioPlayed = false;


    void Start()
    {
        promptMessage = "[E] Open Door";
        pivot = transform;
        initialRotation = pivot.localEulerAngles;
        currentRotation = initialRotation.y;
        targetRotation = initialRotation.y; // Start closed
    }

    void Update()
    {
        if (doorOpen)
        {
            // Opening the door
            if (Mathf.Abs(currentRotation - (initialRotation.y + openRot)) > 0.1f)
            {
                isDoorMoving = true;
                if (!audioPlayed)
                {
                    interactableAudioSource.PlayOneShot(doorOpenAudio);
                    audioPlayed = true;
                }

                currentRotation = Mathf.Lerp(currentRotation, initialRotation.y + openRot, speed * Time.deltaTime);

                Vector3 newRotation = initialRotation;
                newRotation.y = currentRotation;
                pivot.localEulerAngles = newRotation;
            }
            else
            {
                if (isDoorMoving)
                {
                    isDoorMoving = false;
                    audioPlayed = false;
                }
            }
        }
        else
        {
            // Closing the door
            if (Mathf.Abs(currentRotation - initialRotation.y) > 0.1f)
            {
                isDoorMoving = true;
                if (!audioPlayed)
                {
                    interactableAudioSource.PlayOneShot(doorCloseAudio);
                    audioPlayed = true;
                }

                currentRotation = Mathf.Lerp(currentRotation, initialRotation.y, speed * Time.deltaTime);

                Vector3 newRotation = initialRotation;
                newRotation.y = currentRotation;
                pivot.localEulerAngles = newRotation;
            }
            else
            {
                if (isDoorMoving)
                {
                    isDoorMoving = false;
                    audioPlayed = false;
                }
            }
        }

        promptMessage = doorOpen ? "[E] Close Door" : "[E] Open Door";
    }

    protected override void Interact()
    {
        doorOpen = !doorOpen;
    }
}