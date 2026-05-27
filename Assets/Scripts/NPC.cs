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
    [SerializeField] private float lookAheadMinDist = 3f;       // Minimum distance to look ahead
    [SerializeField] private float lookAheadSpeedScale = 0.5f;  // Extra look-ahead distance per m/s of speed

    [Header("Braking")]
    [SerializeField] private float cornerBrakeAngle = 30f;      // Lowered to detect softer corners earlier
    [SerializeField] private float cornerBrakeLookAhead = 3;    // Checked 3 corners ahead
    [SerializeField] private float cornerBrakeStrength = 0.7f;  // Increased throttle reduction
    [SerializeField] private float hardBrakeThreshold = 0.5f;   // Aggressive handbraking for sharp turns

    [Header("Nitrous")]
    [SerializeField] private float nitrousMinAlignment = 0.9f;
    [SerializeField] private float nitrousMinDistance = 25f;

    [Header("Combat")]
    [SerializeField] private float fireAngle = 30f;
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckCheckInterval = 1.5f;
    [SerializeField] private float stuckSpeedThreshold = 1f;
    [SerializeField] private float recoveryDuration = 1.5f;

    private NavMeshAgent agent;
    private CarController car;
    private Projectile projectile;

    // Stuck state
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;
    private bool isRecovering = false;
    private Vector3 lastCheckedPosition;

    // Nitrous state
    private bool wasBoostingLastFrame = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        car = GetComponent<CarController>();
        projectile = GetComponent<Projectile>();

        // Agent setup for pure pathfinding
        agent.speed = 50f;
        agent.angularSpeed = 999f;
        agent.acceleration = 999f;
        agent.stoppingDistance = stopDistance;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.velocity = Vector3.zero;

        lastCheckedPosition = transform.position;
    }

    void Update()
    {
        if (target == null) return;

        agent.SetDestination(target.position);
        agent.nextPosition = transform.position;

        if (!agent.hasPath || !agent.isOnNavMesh) return;

        float distToTarget = Vector3.Distance(transform.position, target.position);

        if (HandleStuckRecovery()) return;

        Vector3 waypoint = GetDynamicWaypoint();
        Vector3 desiredDir = (waypoint - transform.position).normalized;

        DriveToward(desiredDir, distToTarget);
        HandleNitrous(desiredDir, distToTarget);
        HandleFiring(distToTarget);
    }

    // -------------------------------------------------------------------------
    // Waypoint Selection (Look-Ahead)
    // -------------------------------------------------------------------------

    Vector3 GetDynamicWaypoint()
    {
        var path = agent.path;
        if (path == null || path.corners.Length < 2)
            return transform.position;

        // Dynamic look-ahead: The faster we go, the further ahead we look to start turning early
        float currentSpeed = car.GetSpeed();
        float dynamicLookAhead = lookAheadMinDist + (currentSpeed * lookAheadSpeedScale);

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 toCorner = path.corners[i] - transform.position;
            float dist = toCorner.magnitude;
            float dot = Vector3.Dot(transform.forward, toCorner.normalized);

            // Accept waypoint if it's far enough ahead and roughly in front of us
            if (dist > dynamicLookAhead && dot > -0.1f)
                return path.corners[i];
        }

        return path.corners[path.corners.Length - 1];
    }

    // -------------------------------------------------------------------------
    // Driving & Steering Logic
    // -------------------------------------------------------------------------

    void DriveToward(Vector3 desiredDir, float distToTarget)
    {
        Vector3 forward = transform.forward;
        float alignment = Vector3.Dot(forward, desiredDir);

        // Determine turning direction and strength
        float turnSign = Mathf.Sign(Vector3.Cross(forward, desiredDir).y);
        float angleToTarget = Vector3.Angle(forward, desiredDir);

        // Map steering input proportionally to angle error
        float targetTurn = Mathf.Clamp(angleToTarget / 45f, 0f, 1f) * turnSign * steerSensitivity;

        // Reduce steering slightly at top speed to avoid spinouts, but keep it responsive
        float speedFactor = Mathf.Clamp01(car.GetSpeed() / car.GetTopSpeed());
        targetTurn *= (1f - speedFactor * 0.3f);

        // Faster steering adjustment response (Lerp factor raised from 4f to 8f)
        car.TurnInput = Mathf.Lerp(car.TurnInput, targetTurn, Time.deltaTime * 8f);

        if (distToTarget > stopDistance)
        {
            if (alignment < -Mathf.Cos(reverseAngleThreshold * Mathf.Deg2Rad))
            {
                // Facing completely wrong way — reverse
                car.MoveInput = -0.6f;
                car.TurnInput = -car.TurnInput; // Counter-steer while reversing
                car.BrakeInput = false;
            }
            else
            {
                float throttle = Mathf.Clamp(alignment * moveSensitivity, 0f, 1f);
                float cornerScale = GetCornerThrottleScale();

                if (cornerScale < hardBrakeThreshold)
                {
                    // Sharp corner ahead: cut power and drift/handbrake
                    car.BrakeInput = true;
                    car.MoveInput = throttle * 0.1f; // Keep a tiny bit of throttle to assist torque turning
                }
                else
                {
                    car.BrakeInput = false;
                    car.MoveInput = throttle * cornerScale;
                }
            }
        }
        else
        {
            car.MoveInput = 0f;
            car.BrakeInput = car.GetSpeed() > 1f;
        }
    }

    // -------------------------------------------------------------------------
    // Predictive Corner Braking
    // -------------------------------------------------------------------------

    float GetCornerThrottleScale()
    {
        var corners = agent.path.corners;
        if (corners.Length < 3) return 1f;

        int checkUpTo = Mathf.Min(1 + (int)cornerBrakeLookAhead, corners.Length - 1);
        float accumulatedDistance = 0f;

        for (int i = 1; i < checkUpTo; i++)
        {
            Vector3 segment1 = corners[i] - corners[i - 1];
            accumulatedDistance += segment1.magnitude;

            Vector3 a = segment1.normalized;
            Vector3 b = (corners[i + 1] - corners[i]).normalized;
            float angle = Vector3.Angle(a, b);

            if (angle > cornerBrakeAngle)
            {
                // Calculate if our current speed requires us to slow down *now* before reaching the corner
                float speed = car.GetSpeed();
                float brakingDistanceRequired = (speed * speed) / (2f * 9.81f * cornerBrakeStrength);

                if (accumulatedDistance <= brakingDistanceRequired)
                {
                    return Mathf.Clamp01(1f - (cornerBrakeStrength * (angle / 90f)));
                }
            }
        }

        return 1f;
    }

    // -------------------------------------------------------------------------
    // Nitrous, Firing & Recovery (Maintained from your original logic)
    // -------------------------------------------------------------------------

    void HandleNitrous(Vector3 desiredDir, float distToTarget)
    {
        if (car.CurrentNitrous <= 0f)
        {
            car.NitrousPressed = false;
            wasBoostingLastFrame = false;
            return;
        }

        float alignment = Vector3.Dot(transform.forward, desiredDir);
        bool wellAligned = alignment >= nitrousMinAlignment;
        bool farEnough = distToTarget >= nitrousMinDistance;
        bool noSharpTurn = GetCornerThrottleScale() > 0.85f; // Safer threshold for boosting
        bool shouldBoost = wellAligned && farEnough && noSharpTurn;

        car.NitrousPressed = shouldBoost && !wasBoostingLastFrame;
        wasBoostingLastFrame = shouldBoost;
    }

    bool HandleStuckRecovery()
    {
        if (isRecovering)
        {
            recoveryTimer -= Time.deltaTime;
            car.MoveInput = -1f;
            car.TurnInput = 1f;
            car.BrakeInput = false;

            if (recoveryTimer <= 0f)
            {
                isRecovering = false;
                stuckTimer = 0f;
            }
            return true;
        }

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float distMoved = Vector3.Distance(transform.position, lastCheckedPosition);
            if (distMoved < stuckSpeedThreshold * stuckCheckInterval && car.GetSpeed() < stuckSpeedThreshold)
            {
                isRecovering = true;
                recoveryTimer = recoveryDuration;
            }
            lastCheckedPosition = transform.position;
            stuckTimer = 0f;
        }
        return false;
    }

    void HandleFiring(float distToTarget)
    {
        if (projectile == null) return;

        Vector3 aimPoint = PredictTargetPosition(distToTarget);
        Vector3 toAim = (aimPoint - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toAim);

        bool inRange = distToTarget <= stopDistance;
        bool aimed = angle <= fireAngle;

        projectile.firing = inRange && aimed;
    }

    Vector3 PredictTargetPosition(float distToTarget)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null || projectileSpeed <= 0f)
            return target.position;

        float timeToHit = distToTarget / projectileSpeed;
        Vector3 predictedPos = target.position + targetRb.linearVelocity * timeToHit;

        return predictedPos;
    }
}