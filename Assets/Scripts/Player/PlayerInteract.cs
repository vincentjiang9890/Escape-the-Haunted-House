using System.Collections;
using TMPro;
using UnityEngine;
public class PlayerInteract : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    private PlayerUI playerUI;
    private InputManager inputManager;

    public TextMeshProUGUI keyText;
    public int keys;
    
    void Start()
    {
        //HIDE THE MOUSE
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }
    void Update()
    {
        playerUI.UpdateText(string.Empty);
        keyText.text = string.Format("Keys: {0}", keys);

        Ray ray = new Ray(cam.transform.position, cam.transform.forward); //ray at cam center
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, distance, mask)) //raycast to center
        {
            //check if game has interactable compent on
            if (hitInfo.collider.GetComponent<Interactable>() != null) // check if seeing interatbale
            {
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>(); //set inteaactle to varaible

                Debug.Log(hitInfo.collider.GetComponent<Interactable>().promptMessage);
                playerUI.UpdateText(interactable.promptMessage);
                if (inputManager.playerControls.Interact.triggered)
                {
                    if (interactable.CompareTag("Key"))
                    {
                        keys++;
                        interactable.BaseInteract();
                    }
                    else if (interactable.CompareTag("Door"))
                    {
                        if (keys > 0)
                        {
                            keys--;
                            interactable.BaseInteract();
                        }
                        else
                        {
                            StartCoroutine(changePromptMessage(interactable));
                        }
                    }
                }
            }
        }
    }

    IEnumerator changePromptMessage(Interactable interactable)
    {
        string oldPrompt = interactable.promptMessage;
        interactable.promptMessage = "Door Locked";
        yield return new WaitForSeconds(1f);
        interactable.promptMessage = oldPrompt;
    }
}
