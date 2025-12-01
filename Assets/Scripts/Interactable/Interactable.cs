using UnityEngine;

//abstract gets subclasses that interihts from here
public abstract class Interactable : MonoBehaviour
{
    public bool useEvents;
    public string promptMessage;
    public int variable;

    public void BaseInteract()
    {
        if (useEvents)
            GetComponent<InteractionEvent>().onInteract.Invoke();
        Interact(); //could set up enum to change order but its fine 
    }
    protected virtual void Interact()
    {
        //no code from here only from inhereited scripts
    }
}
