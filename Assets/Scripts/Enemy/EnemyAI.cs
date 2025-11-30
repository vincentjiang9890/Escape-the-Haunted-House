using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;
using System;
using JetBrains.Annotations;
using static UnityEngine.UI.Image;

public class EnemyAI : MonoBehaviour
{
    public CapsuleCollider enemyCollider;
    public NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    public LayerMask environmentMask, playerMask, doorMask;
    public EnemySight sight;
    public bool isMoving = false;
    public bool isSearching = false;
    public bool isChasing = false;

    private float walkSpeed = 1.5f;
    private float runSpeed = 3f;

    //roam
    public Vector3 walkPoint;
    public bool walkPointSet;
    public float walkPointRange = 5f;

    //states
    private float attackRange = 1f;
    public float sightRange;
    public bool playerInSightRange, playerInAttackRange;
    private float sightTimer;
    private bool playerSpotted = false;

    public enum EnemyState { Roaming, Processing, Searching, Chasing, Attacking }
    public EnemyState currentState = EnemyState.Roaming;

    [Header("Footstep Params")]
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float sprintStepMultiplier = 0.3f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;

    private float footstepTimer = 0;
    private float GetCurrentOffset => isChasing ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

   
    private void Awake()
    {
        FindPlayer();
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        enemyCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        sightRange = sight.sightDistance;

        agent.stoppingDistance = attackRange * 0.8f;
        
    }

    private void Update()
    {
        //Debug.Log("===============================");
        //Debug.Log("spotted: " + playerSpotted);
        //Debug.Log("sight: " + playerInSightRange);

        if (player == null)
        {
            FindPlayer();
        }

        UpdateStateConditions();

        switch (currentState)
        {
            case EnemyState.Roaming:
                sight.coneAngle = 90f;
                Roaming();
                break;
            case EnemyState.Processing:
                Processing();
                break;
            case EnemyState.Searching:
                Search();
                break;
            case EnemyState.Chasing:
                sight.coneAngle = 120f;
                Chase();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
        }

        if (agent.velocity.magnitude >= .1f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (useFootsteps)
        {
            HandleFootsteps();
        }

        animator.SetBool("isRunning", isMoving && agent.speed == runSpeed);
        animator.SetBool("isWalking", isMoving && agent.speed == walkSpeed);

        
       
    }

    private void UpdateStateConditions()
    {
        playerInSightRange = sight != null && sight.playerFound;
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

        //Debug.Log(playerInSightRange);
        if (playerInSightRange)
        {
            sightTimer += Time.deltaTime;
            playerSpotted = true;
        }
        else
        {
            sightTimer = 0f;
        }
        CheckStateTransitions();
    }

    private void CheckStateTransitions()
    {
        EnemyState previousState = currentState;

        if (playerInSightRange && playerInAttackRange)
        {
            currentState = EnemyState.Attacking;
        }
        else if (playerInSightRange && sightTimer >= 1f)
        {
            currentState = EnemyState.Chasing;
        }
        else if (playerInSightRange && sightTimer < 1f)
        {
            currentState = EnemyState.Processing;
        }
        else if (playerSpotted && !playerInSightRange)
        {
            currentState = EnemyState.Searching;
        }
        else
        {
            currentState = EnemyState.Roaming;
        }

        if (previousState == EnemyState.Searching && currentState == EnemyState.Roaming)
        {
            playerSpotted = false;
        }

        if (currentState == EnemyState.Chasing)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (previousState != currentState)
        {
            OnStateExit(previousState);
            OnStateEnter(currentState);
            Debug.Log("State: " + currentState);
        }
    }

    private void OnStateEnter(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Searching:
                isSearching = false; // Reset search flag
                break;
            case EnemyState.Roaming:
                walkPointSet = false;
                break;
        }
    }

    private void OnStateExit(EnemyState oldState)
    {
        switch (oldState)
        {
            case EnemyState.Searching:
                StopAllCoroutines();
                isSearching = false;
                playerSpotted = false;

                break;
        }
    }

    private void FindPlayer()
    {
        GameObject findPlayer = GameObject.FindGameObjectWithTag("Player");
        if (findPlayer != null)
        {
            player = findPlayer.transform;
        }    
    }

    private void Roaming()
    {
        if (!walkPointSet) SearchResultWalkPoint();

        if (walkPointSet)
        {
            agent.isStopped = false;
            agent.SetDestination(walkPoint);
        }

        agent.speed = walkSpeed;

        if (walkPointSet && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            walkPointSet = false;
            agent.isStopped = true;
        }
    }

    private void SearchResultWalkPoint()
    {
       
        for (int i = 0; i < 30; i++)
        {
            float randomZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
            float randomY = UnityEngine.Random.Range(-3f, 3f);
            float randomX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
            

            Vector3 randomPoint = new Vector3(
                transform.position.x + randomX,
                transform.position.y + randomY, // Start above
                transform.position.z + randomZ
            );

            // Raycast downward to find the ground
            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, 6f, environmentMask))
            {
                // Now check if this point is on NavMesh
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
                {
                    walkPoint = navHit.position;
                    Debug.Log(walkPoint);
                    walkPointSet = true;
                    return;
                
                }
            }
        }

        walkPointSet = false;
    }
    

    private void Processing()
    {
        //agent.SetDestination(transform.position);
        //walkPointSet = false;
        transform.LookAt(player);
    }

    private void Search()
    {
        walkPointSet = false;
        if (!isSearching)
        {
            Vector3 playerLastSeenPosition = player.transform.position;
            agent.SetDestination(playerLastSeenPosition);
            Debug.Log("SEARCH MOVING");
            StartCoroutine(WalkToLastSeenPlayer(playerLastSeenPosition));
            isSearching = true;
        }
    }

    IEnumerator WalkToLastSeenPlayer(Vector3 lastSeenPosition)
    {
        float searchTime = 0f;
        float maxSearchTime = 10f;

        // Wait for path to be calculated
        yield return new WaitForSeconds(0.5f);

        while (searchTime < maxSearchTime && currentState == EnemyState.Searching)
        {
            // Use better destination reached check
            if (HasReachedDestination())
            {
                yield return StartCoroutine(LookAround());
                break;
            }

            searchTime += Time.deltaTime;
            yield return null;
        }

        // Search completed
        isSearching = false;

        // If we're still in searching state after completing, return to roaming
        if (currentState == EnemyState.Searching)
        {
            currentState = EnemyState.Roaming;
            Debug.Log("State: Roaming (Search completed)");
        }
    }

    private bool HasReachedDestination()
    {
        return !agent.pathPending
               && agent.remainingDistance <= agent.stoppingDistance
               && agent.velocity.magnitude < 0.1f;
    }

    IEnumerator LookAround()
    {
        float lookDuration = 3f;
        float timer = 0f;

        float startYRotation = transform.eulerAngles.y;

        while (timer < lookDuration)
        {
            float angle = (timer / lookDuration) * 360f;

            transform.rotation = Quaternion.Euler(0f, startYRotation + angle, 0f);
            

            timer += Time.deltaTime;
            yield return null;
        }

        if (playerInSightRange)
        {
            playerSpotted = true;
        }
        else
        {
            playerSpotted = false;
        }
        //transform.rotation = Quaternion.Euler(0f, startYRotation, 0f);
    }

    private void Chase()
    {
        agent.speed = runSpeed;

        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.speed = 0f;
        agent.acceleration = 0f;
        //agent.SetDestination(transform.position);

        transform.LookAt(player);
        player.transform.LookAt(transform);
        //kill player
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        if (walkPointSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(walkPoint, 0.15f);
            Gizmos.DrawLine(transform.position, walkPoint);
        }
    }

    private void HandleFootsteps()
    {
        if (!isMoving) return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if (Physics.Raycast(enemyCollider.transform.position, Vector3.down, out RaycastHit hit, 2))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/Wood":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                }
            }
            footstepTimer = GetCurrentOffset;
        }
    }
}
