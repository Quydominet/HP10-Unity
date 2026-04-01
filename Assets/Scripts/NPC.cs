using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarNPC : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private float stopDistance = 10f;

    [Header("Driving")]
    [SerializeField] private float moveSensitivity = 2f;
    [SerializeField] private float steerSensitivity = 1.5f;
    [SerializeField] private float reverseAngleThreshold = 110f;

    [Header("Path Following")]
    [SerializeField] private float lookAheadSpeedFactor = 10f; // higher = look further ahead

    [Header("Combat")]
    [SerializeField] private float fireAngle = 30f;

    private NavMeshAgent agent;
    private CarController car;
    private Projectile projectile;
    private Rigidbody rb;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        car = GetComponent<CarController>();
        projectile = GetComponent<Projectile>();
        rb = GetComponent<Rigidbody>();

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.velocity = Vector3.zero;
    }

    void Update()
    {
        if (target == null) return;

        agent.SetDestination(target.position);
        agent.nextPosition = transform.position;

        if (!agent.hasPath || !agent.isOnNavMesh) return;

        Vector3 waypoint = GetDynamicWaypoint();
        Vector3 desiredDir = (waypoint - transform.position).normalized;

        float distToTarget = Vector3.Distance(transform.position, target.position);

        DriveToward(desiredDir, distToTarget);
        HandleFiring(distToTarget);
    }

    // 🔥 NEW: Dynamic waypoint selection
    Vector3 GetDynamicWaypoint()
    {
        var path = agent.path;

        if (path == null || path.corners.Length < 2)
            return transform.position;

        float speed = rb.linearVelocity.magnitude;

        // Determine how far ahead to look
        int index = Mathf.Clamp(
            1 + Mathf.FloorToInt(speed / lookAheadSpeedFactor),
            1,
            path.corners.Length - 1
        );

        return path.corners[index];
    }

    void DriveToward(Vector3 desiredDir, float distToTarget)
    {
        Vector3 forward = transform.forward;

        float alignment = Vector3.Dot(forward, desiredDir);
        float turnSign = Mathf.Sign(Vector3.Cross(forward, desiredDir).y);

        float misalignment = (1f - alignment) * 0.5f;

        // Smooth curve for your torque-based steering
        float turnStrength = Mathf.Pow(misalignment, 0.8f);

        float targetTurn = turnSign * turnStrength * steerSensitivity;

        // Optional: reduce turning at high speed (stability)
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / 20f);
        targetTurn *= (1f - speedFactor * 0.5f);

        // Smooth input (VERY important for your controller)
        car.TurnInput = Mathf.Lerp(car.TurnInput, targetTurn, Time.deltaTime * 4f);

        if (distToTarget > stopDistance)
        {
            if (alignment < -Mathf.Cos(reverseAngleThreshold * Mathf.Deg2Rad))
            {
                car.MoveInput = -0.5f;
                car.TurnInput = -car.TurnInput;
            }
            else
            {
                car.MoveInput = Mathf.Clamp(alignment * moveSensitivity, 0f, 1f);
            }
        }
        else
        {
            car.MoveInput = 0f;
        }
    }

    void HandleFiring(float distToTarget)
    {
        Vector3 toTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toTarget);

        bool inRange = distToTarget <= stopDistance;
        bool aimed = angle <= fireAngle;

        projectile.firing = inRange && aimed;
    }
}