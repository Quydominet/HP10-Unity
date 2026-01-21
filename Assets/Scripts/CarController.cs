using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float speedForce = 1500f;
    public float turnForce = 350f;
    public float brakeForce = 2000f;
    public float baseMaxSpeed = 20f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;

    [Header("Control")]
    public bool isPlayer = true;

    [HideInInspector] public float MoveInput;
    [HideInInspector] public float TurnInput;

    // 🔥 NITRO / EFFECT
    [HideInInspector] public float SpeedMultiplier = 1f;
    [HideInInspector] public float MaxSpeedMultiplier = 1f;

    // Wheel system (code cũ)
    private WheelCollider[] wheels;

    private Rigidbody rb;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Lấy wheel collider (code cũ)
        wheels = new WheelCollider[4];
        wheels[0] = transform.Find("RBwheel").GetComponent<WheelCollider>();
        wheels[1] = transform.Find("LBwheel").GetComponent<WheelCollider>();
        wheels[2] = transform.Find("RFwheel").GetComponent<WheelCollider>();
        wheels[3] = transform.Find("LFwheel").GetComponent<WheelCollider>();
    }

    void Start()
    {
        rb.centerOfMass = new Vector3(0, -0.6f, 0);
        rb.linearDamping = 1.2f;
        rb.angularDamping = 2.5f;

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        // 🔒 chống lỗi quên gán GroundCheck
        if (groundCheckPoint == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheckPoint = gc.transform;
        }
    }

    void FixedUpdate()
    {
        // INPUT
        if (isPlayer)
        {
            MoveInput = Input.GetAxis("Vertical");
            TurnInput = Input.GetAxis("Horizontal");
        }

        // GROUND CHECK
        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );

        if (isGrounded)
        {
            Move();
            Turn();

            if (isPlayer && Input.GetKey(KeyCode.Space))
                Brake();
        }

        SetWheel();
    }

    void Move()
    {
        float currentMaxSpeed = baseMaxSpeed * MaxSpeedMultiplier;

        if (rb.linearVelocity.magnitude < currentMaxSpeed)
        {
            rb.AddRelativeForce(
                Vector3.forward * MoveInput * speedForce * SpeedMultiplier * Time.fixedDeltaTime,
                ForceMode.Acceleration
            );
        }
    }

    void Turn()
    {
        if (Mathf.Abs(MoveInput) < 0.1f) return;

        float turn = TurnInput * turnForce * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    void Brake()
    {
        rb.AddRelativeForce(
            -Vector3.forward * brakeForce * Time.fixedDeltaTime,
            ForceMode.Acceleration
        );
    }

    // =======================
    // WHEEL VISUAL (CODE CŨ)
    // =======================
    void SetWheel()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;

            wheels[i].motorTorque = MoveInput * speedForce;
            wheels[i].GetWorldPose(out pos, out rot);

            GameObject wheelMesh = wheels[i].transform.GetChild(0).gameObject;
            wheelMesh.transform.position = pos;

            Quaternion wheelRotation = rot;

            // Bánh trước rẽ
            if (i > 1)
            {
                Quaternion steerRotation = Quaternion.Euler(0, 20 * TurnInput, 0);
                wheelRotation = steerRotation * wheelRotation;
            }

            // Lật bánh trái/phải
            if (i % 2 != 0)
            {
                wheelRotation *= Quaternion.Euler(0, 180, 0);
            }

            wheelMesh.transform.rotation = wheelRotation;
        }
    }
}
