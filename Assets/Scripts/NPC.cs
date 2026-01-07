using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarNPC : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float stopDistance = 10f;   // how far to stay away
    [SerializeField] private float turnSpeed = 5f;       // rotation snap speed
    [SerializeField] private float moveSensitivity = 1f; // acceleration strength
    [SerializeField] private float fireAngle = 30f;      // cone angle in degrees

    private NavMeshAgent agent;
    private CarController car;
    private Projectile projectile;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        car = GetComponent<CarController>();
        projectile = GetComponent<Projectile>();

        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Update()
    {
        if (target != null && agent.isOnNavMesh)
            agent.SetDestination(target.position);

        if (agent.hasPath)
        {
            // World direction toward steering target
            Vector3 desiredDir = (agent.steeringTarget - transform.position).normalized;

            // Distance to player
            float distToPlayer = Vector3.Distance(transform.position, target.position);

            // Forward input: only move if farther than stopDistance
            float move = distToPlayer > stopDistance
                ? Mathf.Clamp(Vector3.Dot(transform.forward, desiredDir) * moveSensitivity, 0f, 1f)
                : 0f;
            car.MoveInput = move;


            Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed * 0.5f);

            // --- Cone check for firing ---
            Vector3 toTarget = (target.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toTarget);

            bool inCone = angle <= fireAngle;
            bool inRange = distToPlayer <= stopDistance;

            projectile.firing = (inRange || inCone);
        }

        agent.nextPosition = transform.position;
    }
}