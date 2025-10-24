using UnityEngine;

public class Door : Interactable
{
    private bool doorOpen = false;
    public float openRot, speed;

    private Transform pivot;
    private float currentRotation;
    private Vector3 initialRotation;

    void Start()
    {
        promptMessage = "Open Door";
        pivot = transform.parent.transform;
        initialRotation = pivot.localEulerAngles;
        currentRotation = initialRotation.y;
    }

    void Update()
    {
        if (doorOpen)
        {
            if (currentRotation < openRot)
            {
                currentRotation = Mathf.Lerp(currentRotation, openRot, speed * Time.deltaTime);

                Vector3 newRotation = initialRotation;
                newRotation.y = currentRotation;
                pivot.localEulerAngles = newRotation;
            }
            promptMessage = "";
        }
    }

    protected override void Interact()
    {
        doorOpen = true;
        
        //gameObject.GetComponent<Animator>().SetBool("isOpen", doorOpen);
    }
}
