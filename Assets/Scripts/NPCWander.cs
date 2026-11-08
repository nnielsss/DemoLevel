
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float waitAtDestination = 2f;
    [SerializeField] private float sampleDistance = 5f;

    private NavMeshAgent agent;

    private Vector3 homePosition;
    private float waitTimer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Controleer of de NPC op het NavMesh staat.
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(
                transform.position,
                out NavMeshHit hit,
                10f,
                agent.areaMask))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogError(
                    gameObject.name + " staat niet op het NavMesh!",
                    this
                );

                enabled = false;
                return;
            }
        }

        // Bewaar de oorspronkelijke positie.
        homePosition = transform.position;

        // Kies direct de eerste bestemming.
        SetRandomDestination();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh || agent.pathPending)
        {
            return;
        }

        // Controleer of de bestemming bereikt is.
        if (!agent.hasPath ||
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;

            waitTimer += Time.deltaTime;

            // Wacht even voordat een nieuwe bestemming wordt gekozen.
            if (waitTimer >= waitAtDestination)
            {
                waitTimer = 0f;

                agent.isStopped = false;

                SetRandomDestination();
            }
        }
    }

    private void SetRandomDestination()
    {
        // Probeer maximaal 20 willekeurige bestemmingen.
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle * wanderRadius;

            Vector3 randomPosition = homePosition +
                new Vector3(randomOffset.x, 0f, randomOffset.y);

            // Zoek het dichtstbijzijnde geldige punt op het NavMesh.
            if (NavMesh.SamplePosition(
                randomPosition,
                out NavMeshHit hit,
                sampleDistance,
                agent.areaMask))
            {
                NavMeshPath path = new NavMeshPath();

                // Controleer of de NPC de bestemming kan bereiken.
                if (agent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        Debug.LogWarning(
            gameObject.name + " kon geen nieuwe bestemming vinden.",
            this
        );
    }
}
