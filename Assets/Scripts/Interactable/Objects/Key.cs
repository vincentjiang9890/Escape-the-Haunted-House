using UnityEngine;

public class Key : Interactable
{

    

    void Start()
    {
        promptMessage = "[E] Pick Up Key";
    }

    void Update()
    {
        
    }
    protected override void Interact()
    {
        //Debug.Log("Interacted with " + gameObject.name);
        gameObject.SetActive(false);

    }
}
