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
                currentRotation = Mathf.Lerp(currentRotation, initialRotation.y + openRot, speed * Time.deltaTime);

                Vector3 newRotation = initialRotation;
                newRotation.y = currentRotation;
                pivot.localEulerAngles = newRotation;
            }
        }
        else
        {
            // Closing the door
            if (Mathf.Abs(currentRotation - initialRotation.y) > 0.1f)
            {
                currentRotation = Mathf.Lerp(currentRotation, initialRotation.y, speed * Time.deltaTime);

                Vector3 newRotation = initialRotation;
                newRotation.y = currentRotation;
                pivot.localEulerAngles = newRotation;
            }
        }

        promptMessage = doorOpen ? "[E] Close Door" : "[E] Open Door";
    }

    protected override void Interact()
    {
        doorOpen = !doorOpen;
    }
}