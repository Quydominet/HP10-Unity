using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarNPC : MonoBehaviour
{
    private static readonly List<CarNPC> AllNpcs = new List<CarNPC>();

    [Header("Flanking")]
    [SerializeField] private float flankRadius = 12f;      // desired distance from target while flanking
    [SerializeField] private float flankSlotAngleBuffer = 45f; // min angular separation NPCs try to keep from each other around the target
    private float assignedFlankAngle = 0f;

    void OnEnable() => AllNpcs.Add(this);
    void OnDisable() => AllNpcs.Remove(this);

    [Header("Targeting")]
    [SerializeField] private Transform target;
    [SerializeField] private float stopDistance = 6f; // was 10f — closes in tighter before backing off

    [Header("Driving")]
    [SerializeField] private float moveSensitivity = 3f;      // was 2f — throttle saturates to full sooner
    [SerializeField] private float minThrottle = 0.35f;       // NEW — floor so it never fully lifts off, even misaligned
    [SerializeField] private float steerSensitivity = 2.2f;   // was 1.5f — snappier steering response
    [SerializeField] private float reverseAngleThreshold = 150f; // was 110f — reverses only when nearly dead-on backwards; prefers powering through turns

    [Header("Path Following")]
    [SerializeField] private float lookAheadMinDist = 3f;
    [SerializeField] private float lookAheadSpeedScale = 0.5f;

    [Header("Braking")]
    [SerializeField] private float cornerBrakeAngle = 45f;       // was 30f — only reacts to sharper corners
    [SerializeField] private float cornerBrakeLookAhead = 3;
    [SerializeField] private float cornerBrakeStrength = 0.45f;  // was 0.7f — less throttle cut on corners
    [SerializeField] private float hardBrakeThreshold = 0.3f;    // was 0.5f — handbrakes less often, only on very sharp corners
    [SerializeField] private float highSpeedSteerPenalty = 0.15f; // was hardcoded 0.3f — keeps more steering authority at top speed

    [Header("Nitrous")]
    [SerializeField] private float nitrousMinAlignment = 0.75f; // was 0.9f — boosts even when not perfectly lined up
    [SerializeField] private float nitrousMinDistance = 12f;    // was 25f — boosts even at closer range to close gaps fast
    [SerializeField] private float nitrousCornerSafety = 0.6f;  // was 0.85f hardcoded — willing to boost through milder corners

    [Header("Combat")]
    [SerializeField] private float fireAngle = 30f;
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Stuck Detection")]
    [SerializeField] private float stuckCheckInterval = 1f;    // was 1.5f — detects & recovers from stuck faster
    [SerializeField] private float stuckSpeedThreshold = 1f;
    [SerializeField] private float recoveryDuration = 0.6f;    // was 1.5f — recovery is now target-aware, so it doesn't need as long
    [SerializeField] private int stuckStrikesRequired = 2;     // NEW — requires 2 consecutive low-speed checks before triggering recovery, so a one-frame hit doesn't launch a full reverse

    private NavMeshAgent agent;
    private CarController car;
    private Projectile projectile;

    // Stuck state
    private float stuckTimer = 0f;
    private float recoveryTimer = 0f;
    private bool isRecovering = false;
    private Vector3 lastCheckedPosition;
    private int stuckStrikes = 0;

    // Nitrous state
    private bool wasBoostingLastFrame = false;
    private static int nextId = 0;
    private int npcId;

    void Awake()
    {
        npcId = nextId++;
        agent = GetComponent<NavMeshAgent>();
        car = GetComponent<CarController>();
        projectile = GetComponent<Projectile>();

        // Agent setup for pure pathfinding
        agent.speed = 65f; // was 50f — pathing keeps up with a faster, more relentless car
        agent.angularSpeed = 999f;
        agent.acceleration = 999f;
        agent.stoppingDistance = stopDistance;
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.velocity = Vector3.zero;

        lastCheckedPosition = transform.position;
    }
    void AssignFlankSlot()
    {
        if (target == null) return;

        List<CarNPC> siblings = new List<CarNPC>();
        foreach (var npc in AllNpcs)
            if (npc.target == target) siblings.Add(npc);

        siblings.Sort((a, b) => a.npcId.CompareTo(b.npcId));

        int mySlot = siblings.IndexOf(this);
        int totalSlots = siblings.Count;

        float baseAngle = 360f / totalSlots;
        assignedFlankAngle = baseAngle * mySlot;
    }

    Vector3 GetFlankPosition()
    {
        AssignFlankSlot();

        float angleRad = assignedFlankAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Sin(angleRad), 0f, Mathf.Cos(angleRad)) * flankRadius;

        return target.position + offset;
    }

    void Update()
    {

        float distToTarget = Vector3.Distance(transform.position, target.position);

        Vector3 destination = distToTarget > flankRadius * 1.5f
            ? GetFlankPosition()   // still closing in — take an assigned angle
            : target.position;    // close enough — commit directly

        agent.SetDestination(destination);
        agent.nextPosition = transform.position;

        if (!agent.hasPath || !agent.isOnNavMesh) return;
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
        targetTurn *= (1f - speedFactor * highSpeedSteerPenalty);

        // Faster steering adjustment response
        car.TurnInput = Mathf.Lerp(car.TurnInput, targetTurn, Time.deltaTime * 10f); // was 8f

        if (distToTarget > stopDistance)
        {
            if (alignment < -Mathf.Cos(reverseAngleThreshold * Mathf.Deg2Rad))
            {
                // Facing completely wrong way — reverse toward the target, not blindly negating whatever TurnInput was
                car.MoveInput = -0.6f;
                car.TurnInput = turnSign; // turnSign already computed above from Cross(forward, desiredDir)
                car.BrakeInput = false;
            }
            else
            {
                // Throttle never fully bottoms out — keeps committing forward even when misaligned
                float throttle = Mathf.Clamp(alignment * moveSensitivity, minThrottle, 1f);
                float cornerScale = GetCornerThrottleScale();

                if (cornerScale < hardBrakeThreshold)
                {
                    // Sharp corner ahead: cut power and drift/handbrake
                    car.BrakeInput = true;
                    car.MoveInput = throttle * 0.2f; // was 0.1f — keeps a bit more drive torque through the handbrake turn
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
            // Even at stop distance, keep pressure on rather than fully braking — stay on the target's bumper
            car.MoveInput = 0.15f;
            car.BrakeInput = car.GetSpeed() > 3f; // was ">1f" — allows it to coast closer before braking
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
    // Nitrous, Firing & Recovery
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
        bool noSharpTurn = GetCornerThrottleScale() > nitrousCornerSafety;
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

            // Steer the reverse toward the target instead of a fixed blind direction,
            // so it comes out of recovery already facing roughly the right way
            if (target != null)
            {
                Vector3 toTarget = (target.position - transform.position).normalized;
                float turnSign = Mathf.Sign(Vector3.Cross(transform.forward, toTarget).y);
                car.TurnInput = turnSign;
            }

            car.BrakeInput = false;

            if (recoveryTimer <= 0f)
            {
                isRecovering = false;
                stuckTimer = 0f;
                stuckStrikes = 0;
            }
            return true;
        }

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float distMoved = Vector3.Distance(transform.position, lastCheckedPosition);
            bool looksStuck = distMoved < stuckSpeedThreshold * stuckCheckInterval && car.GetSpeed() < stuckSpeedThreshold;

            if (looksStuck)
            {
                // Require consecutive stuck readings before committing to a full recovery maneuver,
                // so a single-frame collision dip (e.g. getting rammed) doesn't trigger a long reverse
                stuckStrikes++;
                if (stuckStrikes >= stuckStrikesRequired)
                {
                    isRecovering = true;
                    recoveryTimer = recoveryDuration;
                    stuckStrikes = 0;
                }
            }
            else
            {
                stuckStrikes = 0;
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