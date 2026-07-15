using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

[System.Serializable]
public class Gear
{
    [Tooltip("Top speed for this gear in m/s, x3.6 for km/h")]
    public float maxSpeed;
    [Tooltip("Force multiplier while in this gear")]
    public float torqueMultiplier;
}

[System.Serializable]
public class CamConfig
{
    [Tooltip("The part where the camera is placed")]
    public Transform CamPositionObject;
    [Tooltip("Field of view")]
    public float FOV;
}

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float SpeedForce = 1f;   // Acceleration rate only — tune between 0.5 and 3.0
    public float TurnAngle = 20f;
    public float BrakeForce = 50f;

    [Header("Steering Settings")]
    [SerializeField] private float steerSpeed = 5f;          // how fast the wheel turns
    [SerializeField] private float steerReturnSpeed = 8f;    // how fast it re-centers
    [SerializeField] private float highSpeedSteerScale = 0.4f; // turn reduction at top speed
    [SerializeField] private float gripStrength = 6f;        // lateral friction / anti-drift

    private float currentSteerAngle = 0f;

    [Header("Nitrous Settings")]
    public float NitrousForce = 10f;
    public float NitrousCapacity = 5f;
    public float CurrentNitrous { get; private set; }
    public int CurrentNitrousStage { get; private set; } = 0;
    private float CalculatedNitroForce;
    private bool NitrousActive = false;

    [Tooltip("Maximum number of stacked nitro presses")]
    public int MaxNitrousStage = 4;

    [Tooltip("Additional force per extra stage (e.g. 0.5 = +50% per stage)")]
    public float NitrousStageMultiplier = 0.25f;

    [Header("Wheel Settings")]
    public float wheelFrictionStiffness = 2f;
    private GameObject BrakeEffect;

    [Header("Balance Settings")]
    [SerializeField] private float flipTorqueStrength = 5f;
    [SerializeField] private float flipDamping = 2f;

    [Header("Downforce Settings")]
    [SerializeField] private float downforceStrength = 2f;  // tune this
    [SerializeField] private float downforceMaxSpeed = 44f; // usually matches top speed

    [Header("Gear Settings")]
    [SerializeField]
    private Gear[] _gears = new Gear[]
    {
        new Gear { maxSpeed = 8f,  torqueMultiplier = 1.6f  },  // 1st
        new Gear { maxSpeed = 14f, torqueMultiplier = 1.34f },  // 2nd
        new Gear { maxSpeed = 29f, torqueMultiplier = 1.2f  },  // 3rd
        new Gear { maxSpeed = 37f, torqueMultiplier = 1.0f  },  // 4th
        new Gear { maxSpeed = 44f, torqueMultiplier = 0.87f },  // 5th
    };
    [SerializeField] private Gear _reverseGear = new Gear { maxSpeed = 8f, torqueMultiplier = 0.6f };

    public Gear[] gears => _gears;
    public Gear reverseGear => _reverseGear;

    [SerializeField] private float shiftUpBuffer = 0.95f;   // shift at 95% of gear's max
    [SerializeField] private float shiftDownBuffer = 0.6f;  // drop back at 60%
    public int CurrentGear { get; private set; } = 0;

    [Header("Camera Settings")]
    [SerializeField]
    private CamConfig[] CamConfigs = new CamConfig[]
    {
        new CamConfig { CamPositionObject = null, FOV = 60f }, // default
    };
    [SerializeField] private CinemachineCamera CineCamera;

    private int currentCamIndex = 0;

    [Header("Sound Settings")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip idle;
    [SerializeField] private AudioClip decel;
    [SerializeField] private AudioClip accel;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [HideInInspector] public float MoveInput = 0;
    [HideInInspector] public float TurnInput = 0;
    [HideInInspector] public bool BrakeInput = false;
    [HideInInspector] public bool NitrousPressed = false;
    [SerializeField] private bool controlled = false;
    private bool CamChangePressed = false;

    WheelCollider[] wheels;

    private Rigidbody CarBody;
    public bool isGrounded = false;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    Vector3 RemoveY(Vector3 target) => new Vector3(target.x, 0, target.z);

    public float GetSpeed()
    {
        if (CarBody == null) return 0f;
        return RemoveY(CarBody.linearVelocity).magnitude;
    }
    public float GetTopSpeed() => gears[gears.Length - 1].maxSpeed;
    public int GetGearCount() => gears.Length;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        CarBody = GetComponent<Rigidbody>();
        CarBody.maxAngularVelocity = 20f;
        controlled = transform.tag == "Player";

        Transform Wheels = transform.Find("Wheels");

        wheels = new WheelCollider[4];
        wheels[0] = Wheels.Find("RBwheel").GetComponent<WheelCollider>();
        wheels[1] = Wheels.Find("LBwheel").GetComponent<WheelCollider>();
        wheels[2] = Wheels.Find("RFwheel").GetComponent<WheelCollider>();
        wheels[3] = Wheels.Find("LFwheel").GetComponent<WheelCollider>();

        Collider bodyCollider = CarBody.transform.Find("body").GetComponent<Collider>();
        int carBodyLayer = LayerMask.NameToLayer("Mesh");

        foreach (var wheel in wheels)
        {
            wheel.excludeLayers = 1 << carBodyLayer;
            Physics.IgnoreCollision(wheel, bodyCollider);
        }

        CurrentNitrous = NitrousCapacity;
        //SetWheelFriction();

        ApplyCamera();
    }

    void Update()
    {
        SetWheel();

        if (!controlled) return;

        MoveInput = Input.GetAxis("Vertical");
        TurnInput = Input.GetAxis("Horizontal");
        BrakeInput = Input.GetKey(KeyCode.Space);
        NitrousPressed = Input.GetKeyDown(KeyCode.LeftShift);
        CamChangePressed = Input.GetKeyDown(KeyCode.C);

        if (CamChangePressed)
        {
            CamChangePressed = false; // Debounce

            currentCamIndex = (currentCamIndex + 1) % CamConfigs.Length;
            ApplyCamera();
        }

        if (NitrousPressed && CurrentNitrous > 0f)
        {
            NitrousPressed = false; // Debounce

            CurrentNitrousStage = Mathf.Clamp(CurrentNitrousStage + 1, 1, MaxNitrousStage);
            NitrousActive = true;
        }

        if (source != null)
        {
            if (MoveInput > 0f)
            {
                if (source.clip != accel)
                {
                    source.clip = accel;
                    source.Play();
                }
            }
            else if (MoveInput < 0f)
            {
                if (source.clip != decel)
                {
                    source.clip = decel;
                    source.Play();
                }
            }
            else
            {
                if (source.clip != idle)
                {
                    source.clip = idle;
                    source.Play();
                }
            }
        }

        ApplyNitrous();
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            Move();
            if (BrakeInput) Brake();
        }

        Turn();
        BalanceGyro();
        ApplyDownforce();
    }

    // -------------------------------------------------------------------------
    // Movement
    // -------------------------------------------------------------------------
    void ApplyDownforce()
    {
        float speed = GetSpeed();
        float speedFraction = Mathf.Clamp01(speed / downforceMaxSpeed);

        // Quadratic - downforce grows with the square of speed, like real aerodynamics
        float downforce = downforceStrength * speedFraction * speedFraction;

        CarBody.AddForce(-transform.up * downforce, ForceMode.Acceleration);
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
            if (CurrentGear < 0) CurrentGear = 0;

            if (CurrentGear < gears.Length - 1 && speed >= gears[CurrentGear].maxSpeed * shiftUpBuffer)
                CurrentGear++;
            else if (CurrentGear > 0 && speed < gears[CurrentGear - 1].maxSpeed * shiftDownBuffer)
                CurrentGear--;

            gear = gears[CurrentGear];
        }

        float speedRatio = Mathf.Clamp01(speed / gear.maxSpeed);
        float forceFade = 1f - Mathf.Pow(speedRatio, 8);

        float baseForce = gear.maxSpeed * gear.torqueMultiplier;
        float driveForce = baseForce * SpeedForce * forceFade;

        // Nitrous is additive - bypasses forceFade so it pushes past the gear cap
        float totalForce = driveForce + (MoveInput > 0 ? CalculatedNitroForce : 0f);

        float torque = MoveInput * totalForce;
        Vector3 MoveForce = Vector3.forward * MoveInput * totalForce;

        if (BrakeInput) return;

        wheels[0].motorTorque = torque;
        wheels[1].motorTorque = torque;
        wheels[2].motorTorque = torque;
        wheels[3].motorTorque = torque;

        CarBody.AddRelativeForce(MoveForce);
    }
    public void Turn()
    {
        float speed = RemoveY(CarBody.linearVelocity).magnitude;

        // Reduce steering angle at high speed — more realistic, prevents spinouts
        float speedFraction = Mathf.Clamp01(speed / GetTopSpeed());
        float steerLimit = Mathf.Lerp(1f, highSpeedSteerScale, speedFraction);
        float dot = Vector3.Dot(transform.forward, CarBody.linearVelocity);
        float targetSteer = TurnInput * TurnAngle * steerLimit * (dot < 0 ? -1f : 1f);

        // Smooth the steering input instead of snapping
        float blendSpeed = TurnInput != 0 ? steerSpeed : steerReturnSpeed;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, blendSpeed * Time.fixedDeltaTime);

        // Apply as yaw torque — scaled by actual speed so slow turns feel sluggish
        float torqueStrength = Mathf.Lerp(0f, 1f, speed / 3f); // fades in from standstill
        CarBody.AddRelativeTorque(Vector3.up * currentSteerAngle * torqueStrength * (isGrounded ? 1f : 0.45f));

        // Cancel lateral (sideways) velocity — simulates tire grip
        Vector3 localVelocity = transform.InverseTransformDirection(CarBody.linearVelocity);
        localVelocity.x = Mathf.Lerp(localVelocity.x, 0f, gripStrength * Time.fixedDeltaTime);
        CarBody.linearVelocity = transform.TransformDirection(localVelocity);
    }
    public void Brake()
    {
        // Check velocity in the car's own local space, not world space
        float forwardSpeed = transform.InverseTransformDirection(CarBody.linearVelocity).z;

        if (Mathf.Abs(forwardSpeed) < 0.01f) return; // already stopped, nothing to do

        // Don't apply more force than needed to bring speed to exactly zero this frame
        float maxBrakeForce = Mathf.Abs(forwardSpeed) * CarBody.mass / Time.fixedDeltaTime;
        float appliedForce = Mathf.Min(BrakeForce, maxBrakeForce);

        float brakeDir = Mathf.Sign(forwardSpeed); // brake opposes current direction of travel
        CarBody.AddRelativeForce(-Vector3.forward * brakeDir * appliedForce);

        //BrakeEffect.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Nitrous
    // -------------------------------------------------------------------------

    public void ApplyNitrous()
    {
        if (!NitrousActive)
        {
            CalculatedNitroForce = 0f;

            // Recharge
            if (CurrentNitrous < NitrousCapacity)
                CurrentNitrous = Mathf.Min(CurrentNitrous + Time.deltaTime, NitrousCapacity);

            return;
        }

        float stageMultiplier = 1f + (CurrentNitrousStage - 1) * NitrousStageMultiplier;

        // Additive force boost — scaled by stage, independent of speed
        CalculatedNitroForce = NitrousForce * stageMultiplier;

        float consumptionThisFrame = (1f + stageMultiplier) * Time.deltaTime;
        CurrentNitrous = Mathf.Max(0f, CurrentNitrous - consumptionThisFrame);

        if (CurrentNitrous <= 0f)
        {
            NitrousActive = false;
            CurrentNitrousStage = 0;
            CalculatedNitroForce = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Misc
    // -------------------------------------------------------------------------
    
    void BalanceGyro()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Quaternion delta = targetRotation * Quaternion.Inverse(transform.rotation);

        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        axis.y = 0f;

        Vector3 correctiveTorque = axis.normalized * angle * flipTorqueStrength * (isGrounded ? 0.5f : 1f);
        Vector3 dampingTorque = -CarBody.angularVelocity * flipDamping;

        CarBody.AddTorque(correctiveTorque + dampingTorque);
    }
    
    void SetWheel()
    {
        float dot = Vector3.Dot(transform.forward, CarBody.linearVelocity.normalized);
        float speed = RemoveY(CarBody.linearVelocity).magnitude;

        // Reduce steering angle at high speed — more realistic, prevents spinouts
        float speedFraction = Mathf.Clamp01(speed / GetTopSpeed());
        float steerLimit = Mathf.Lerp(1f, highSpeedSteerScale, speedFraction);
        float targetSteer = TurnInput * TurnAngle * steerLimit;

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out Vector3 pos, out Quaternion rot);

            GameObject wheelMesh = wheels[i].transform.GetChild(0).gameObject;
            wheelMesh.transform.position = pos;
            wheelMesh.transform.position -= transform.right * 0.15f * (i % 2 == 0 ? 1f : -1f);

            Quaternion wheelRotation = rot;

            if (i > 1) wheelRotation = Quaternion.Euler(0, targetSteer, 0) * wheelRotation;

            if (i % 2 != 0) wheelRotation *= Quaternion.Euler(0, 180, 0);

            wheelMesh.transform.rotation = wheelRotation;
        }
    }

    void ApplyCamera()
    {
        if (CineCamera == null || controlled == false || CamConfigs.Length == 0 || CamConfigs[currentCamIndex].CamPositionObject == null) return;
        CineCamera.Target.TrackingTarget = CamConfigs[currentCamIndex].CamPositionObject;
        CineCamera.Lens.FieldOfView = CamConfigs[currentCamIndex].FOV;
    }
}