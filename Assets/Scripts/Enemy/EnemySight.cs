using System.Collections.Generic;
using UnityEngine;

public class EnemySight : MonoBehaviour
{
    public float coneAngle = 90f;
    public float sightDistance = 10f;
    private LayerMask obstacleMask;
    public List<Transform> objectsInSight = new List<Transform>();
    public bool playerFound = false;

    public bool doorToOpen = false;

    private float lastInteractionTimeDoor = 0f;
    private float interactionCooldownDoor = 1f;

    private void Start()
    {
        obstacleMask = LayerMask.GetMask("Default", "Walls", "Environment", "Player", "Interactable");
    }

    private void Update()
    {
        CastLineOfSight();
        DrawSightCone();
    }

    private void CastLineOfSight()
    {
        objectsInSight.Clear();
        playerFound = false;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        Collider[] hitColliders = Physics.OverlapSphere(origin, sightDistance, obstacleMask);

        foreach (Collider collider in hitColliders) //everything in circle
        {
            Vector3 targetDirection = (collider.transform.position - origin).normalized;
            float angle = Vector3.Angle(direction, targetDirection);

            if (angle <= coneAngle / 2f) //everything in angle in front
            {
                float distance = Vector3.Distance(origin, collider.transform.position);

                if (!objectsInSight.Contains(collider.transform))
                {
                    objectsInSight.Add(collider.transform);
                }

                //perform a raycast
                RaycastHit hit;
                if (Physics.Raycast(origin, targetDirection, out hit, sightDistance, obstacleMask)) //performs raycast on collider and returns true if hits collider
                {
                    if (hit.collider == collider)
                    {
                        if (!objectsInSight.Contains(collider.transform))
                        {
                            objectsInSight.Add(collider.transform);
                        }

                        // Check if it's the player
                        if (collider.CompareTag("Player"))
                        {
                            playerFound = true;
                            //Debug.Log($"Player detected: {collider.name} at angle {angle} degrees");
                        }

                        if (collider.CompareTag("UnlockedDoor"))
                        {
                            Vector3 closestPoint = collider.ClosestPoint(origin);
                            float distanceToDoor = Vector3.Distance(origin, closestPoint);

                            if (distanceToDoor < 1.5f) // Increased slightly for better UX
                            {
                                if (Time.time - lastInteractionTimeDoor >= interactionCooldownDoor)
                                {
                                    Interactable interactable = collider.GetComponent<Interactable>();
                                    UnlockedDoor door = interactable.GetComponent<UnlockedDoor>();
                                    if (interactable != null && !door.doorOpen)
                                    {
                                        interactable.BaseInteract();
                                        lastInteractionTimeDoor = Time.time; // Reset the timer
                                    }
                                }
                            }
                        }

                        Debug.DrawRay(origin, targetDirection * distance, Color.green, 0.1f);
                    }
                    else
                    {
                        // Something is blocking the view
                        //Debug.DrawRay(origin, targetDirection * distance, Color.red, 0.1f);
                    }
                }
            }
        }
        //Debug.Log(playerFound);
    }

    private void DrawSightCone()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        Debug.DrawRay(origin, forward * sightDistance, Color.white, 0.1f);

        Vector3 leftBoundary = Quaternion.Euler(0, -coneAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, coneAngle / 2, 0) * forward;
        Debug.DrawRay(origin, leftBoundary * sightDistance, Color.white, 0.1f);
        Debug.DrawRay(origin, rightBoundary * sightDistance, Color.white, 0.1f);
    }

}
