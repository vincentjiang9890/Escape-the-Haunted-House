using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerInteract : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    private PlayerUI playerUI;
    private InputManager inputManager;

    public TextMeshProUGUI keyText;
    public int keys;

    [SerializeField] private AudioSource interactableAudioSource = default;
    [SerializeField] private AudioClip doorOpenAudio = default;
    [SerializeField] private AudioClip useKeyLockedDoorAudio = default;
    [SerializeField] private AudioClip keyAudio = default;
    [SerializeField] private AudioClip lockedDoorAudio = default;

    void Start()
    {
        //HIDE THE MOUSE
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();

        interactableAudioSource.volume = 0.5f;
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
                        interactableAudioSource.PlayOneShot(keyAudio);
                        interactable.BaseInteract();
                    }
                    else if (interactable.CompareTag("LockedDoor"))
                    {
                        if (interactable.variable == 1 && keys > 0)
                        {
                            keys--;
                            interactable.variable--;

                            interactableAudioSource.PlayOneShot(useKeyLockedDoorAudio);
                            interactableAudioSource.PlayOneShot(doorOpenAudio);
                            interactable.BaseInteract();

                            StartCoroutine(WaitTilEndScreen());

                        }
                        else if (interactable.variable > 1 && keys > 0)
                        {
                            
                            keys--;
                            interactable.variable--;
                            interactableAudioSource.PlayOneShot(useKeyLockedDoorAudio);

                            StartCoroutine(changePromptMessageLockedDoor(interactable));
                        }
                        else
                        {
                            interactableAudioSource.PlayOneShot(lockedDoorAudio);
                            StartCoroutine(changePromptMessageLockedDoor(interactable));
                        }
                    }
                    else if (interactable.CompareTag("UnlockedDoor"))
                    {
                        interactable.BaseInteract();
                    }
                }
            }
        }
    }

    IEnumerator changePromptMessageLockedDoor(Interactable interactable)
    {
        string oldPrompt = interactable.promptMessage;
        interactable.promptMessage = "Door locked. Keys needed to open: " + interactable.variable;
        yield return new WaitForSeconds(1f);
        interactable.promptMessage = oldPrompt;
    }

    IEnumerator WaitTilEndScreen()
    {
        yield return new WaitForSeconds(.75f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);

    }
}
