using UnityEngine;

[System.Serializable]
public class Gear
{
    [Tooltip("Top speed for this gear in m/s, x3.6 for k/m")]
    public float maxSpeed;
    [Tooltip("Force multiplier while in this gear")]
    public float torqueMultiplier;
}

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float SpeedForce = 100f;
    public float TurnAngle = 20f;
    public float BrakeForce = 50f;

    [Header("Nitrous Settings")]
    public float NitrousForce = 10f;
    public float NitrousCapacity  = 5f;

    [Tooltip("Maximum number of stacked nitro presses")]
    public int MaxNitrousStage = 4;
    [Tooltip("Additional force per extra stage (e.g. 0.5 = +50% per stage)")]
    public float NitrousStageMultiplier = 0.25f;

    [Header("Wheel Settings")]
    public float wheelFrictionStiffness = 2f;
    private GameObject BrakeEffect;

    public float CurrentNitrous { get; private set; }

    public int CurrentNitrousStage { get; private set; } = 0;
    private float CalculatedNitroForce;
    private bool NitrousActive = false;

    [Header("Balance Settings")]
    [SerializeField] private float flipTorqueStrength = 5f;
    [SerializeField] private float flipDamping = 2f;


    [Header("Gear Settings")]
    [SerializeField]
    public Gear[] gears { get; private set; } = new Gear[]
{
    new Gear { maxSpeed = 8f,  torqueMultiplier = 1.6f  },  // 1st
    new Gear { maxSpeed = 14f, torqueMultiplier = 1.3f  },  // 2nd
    new Gear { maxSpeed = 20f, torqueMultiplier = 1.0f  },  // 3rd
    new Gear { maxSpeed = 27f, torqueMultiplier = 0.75f },  // 4th
    new Gear { maxSpeed = 33f, torqueMultiplier = 0.55f },  // 5th
};
    [SerializeField] public Gear reverseGear { get; private set; } = new Gear { maxSpeed = 8f, torqueMultiplier = 0.6f };
    [SerializeField] private float shiftUpBuffer = 0.95f;   // shift at 95% of gear's max
    [SerializeField] private float shiftDownBuffer = 0.6f;  // drop back at 60%

    public int CurrentGear { get; private set; } = 0;
    public float GetTopSpeed() => gears[gears.Length - 1].maxSpeed;
    public int GetGearCount() => gears.Length;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint; // empty GameObject under car
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [HideInInspector] public float MoveInput = 0;
    [HideInInspector] public float TurnInput = 0;
    [HideInInspector] public bool BrakeInput = false;
    [HideInInspector] public bool NitrousPressed = false;
    [SerializeField] private bool controlled = false;

    WheelCollider[] wheels;

    private Rigidbody CarBody;
    public bool isGrounded = false;

    Vector3 RemoveY(Vector3 target)
    {
        return new Vector3(target.x, 0, target.z);
    }
    public float GetSpeed()
    {
        return RemoveY(CarBody.linearVelocity).magnitude;
    }
    void Awake()
    {
        CarBody = GetComponent<Rigidbody>();
        CarBody.maxAngularVelocity = 20f;
        controlled = (transform.tag == "Player");

        wheels = new WheelCollider[4];
        wheels[0] = transform.Find("RBwheel").GetComponent<WheelCollider>();
        wheels[1] = transform.Find("LBwheel").GetComponent<WheelCollider>();
        wheels[2] = transform.Find("RFwheel").GetComponent<WheelCollider>();
        wheels[3] = transform.Find("LFwheel").GetComponent<WheelCollider>();

        CurrentNitrous = NitrousCapacity;
        //SetWheelFriction();
    }
    void SetWheelFriction()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve fwd = wheel.forwardFriction;
            fwd.stiffness = wheelFrictionStiffness;
            wheel.forwardFriction = fwd;

            WheelFrictionCurve side = wheel.sidewaysFriction;
            side.stiffness = wheelFrictionStiffness;
            wheel.sidewaysFriction = side;
        }
    }
    void Update()
    {
        SetWheel();

        if (!controlled) return;

        MoveInput = Input.GetAxis("Vertical");
        TurnInput = Input.GetAxis("Horizontal");
        BrakeInput = Input.GetKey(KeyCode.Space);
        NitrousPressed = Input.GetKeyDown(KeyCode.LeftShift);
    }
    void FixedUpdate()
    {
        if (!enabled) return;

        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            Move();
            Turn();
            if ((MoveInput > 0) && BrakeInput) Brake();
        }

        if (NitrousPressed && CurrentNitrous > 0f)
        {
            CurrentNitrousStage = Mathf.Clamp(CurrentNitrousStage + 1, 1, MaxNitrousStage);
            NitrousActive = true;
            NitrousPressed = false; // Consume it
        }

        ApplyNitrous();
        BalanceGyro();
    }
    void BalanceGyro()
    {
        //if (isGrounded) return;
        Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Quaternion delta = targetRotation * Quaternion.Inverse(transform.rotation);

        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        axis.y = 0f;

        Vector3 correctiveTorque = axis.normalized * angle * flipTorqueStrength;
        Vector3 dampingTorque = -CarBody.angularVelocity * flipDamping;

        CarBody.AddTorque(correctiveTorque + dampingTorque);
    }
    void SetWheel()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;

            // Visual wheel position/rotation update
            wheels[i].GetWorldPose(out pos, out rot);

            GameObject wheelMesh = wheels[i].transform.GetChild(0).gameObject;
            wheelMesh.transform.position = pos;

            Quaternion wheelRotation = rot;
            
            if (i > 1)
            {
                Quaternion steerRotation = Quaternion.Euler(0, 20 * TurnInput, 0);
                wheelRotation = steerRotation * wheelRotation;
            }

            if (i % 2 != 0)
            {
                wheelRotation *= Quaternion.Euler(0, 180, 0);
            }

            wheelMesh.transform.rotation = wheelRotation;
        }
    }
    //Public for NPC use
    public void ApplyNitrous()
    {
        if (!NitrousActive)
        {
            if (CurrentNitrous < NitrousCapacity)
            {
                CurrentNitrous += Time.deltaTime;
                CurrentNitrous = Mathf.Min(CurrentNitrous, NitrousCapacity);
            }

            return;
        }

        float speed = CarBody.linearVelocity.normalized.magnitude;
        float stageMultiplier = 1f + (CurrentNitrousStage - 1) * NitrousStageMultiplier;

        CalculatedNitroForce = speed * NitrousForce * stageMultiplier * 0.5f;

        float consumptionThisFrame = (1 + stageMultiplier) * Time.deltaTime;
        CurrentNitrous = Mathf.Max(0, CurrentNitrous - consumptionThisFrame);

        if (CurrentNitrous <= 0f)
        {
            NitrousActive = false;
            CurrentNitrousStage = 0;
        }
    }
    public void Move()
    {
        float speed = RemoveY(CarBody.linearVelocity).magnitude;
        bool isReversing = MoveInput < 0;

        Gear gear;

        if (isReversing)
        {
            CurrentGear = -1;
            gear = reverseGear;
        }
        else
        {
            // Reset to 0 if coming out of reverse
            if (CurrentGear < 0) CurrentGear = 0;

            // Auto shift up
            if (CurrentGear < gears.Length - 1 && speed >= gears[CurrentGear].maxSpeed * shiftUpBuffer)
                CurrentGear++;
            // Auto shift down
            else if (CurrentGear > 0 && speed < gears[CurrentGear - 1].maxSpeed * shiftDownBuffer)
                CurrentGear--;

            gear = gears[CurrentGear];
        }

        float speedRatio = Mathf.Clamp01(speed / gear.maxSpeed);
        float forceFade = 1f - Mathf.Pow(speedRatio, 8);

        float torque = MoveInput * SpeedForce * gear.torqueMultiplier * forceFade;
        Vector3 MoveForce = Vector3.forward * MoveInput * SpeedForce * gear.torqueMultiplier * forceFade;

        wheels[0].motorTorque = torque;
        wheels[1].motorTorque = torque;
        wheels[2].motorTorque = torque;
        wheels[3].motorTorque = torque;

        CarBody.AddRelativeForce(MoveForce);
    }
    public void Turn()
    {
        float speed = RemoveY(CarBody.linearVelocity).normalized.magnitude;
        Vector3 TurnTorque = Vector3.up * TurnInput * TurnAngle;

        //if (speed < 0.01f) return; // No turning when nearly stopped

        CarBody.AddRelativeTorque(TurnTorque);

        Quaternion re = Quaternion.Euler(
            Vector3.up * TurnInput * speed * TurnAngle * Time.fixedDeltaTime
        );

        CarBody.MoveRotation(CarBody.rotation * re);
    }

    public void Brake()
    {
        
        if (CarBody.linearVelocity.z != 0)
        {
            CarBody.AddRelativeForce(-Vector3.forward);
        }

        //BrakeEffect.SetActive(true);
    }
}