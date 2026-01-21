using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float SpeedForce = 100f;
    [SerializeField] private float TurnForce = 100f;
    [SerializeField] private float BrakeForce = 50f;
    [SerializeField] private GameObject BrakeEffect;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint; // empty GameObject under car
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    [HideInInspector] public float MoveInput = 0;
    [HideInInspector] public float TurnInput = 0;
    [SerializeField] private bool controlled = false;

    WheelCollider[] wheels;

    private Rigidbody rb;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controlled = (transform.tag == "Player");

        wheels = new WheelCollider[4];
        wheels[0] = transform.Find("RBwheel").GetComponent<WheelCollider>();
        wheels[1] = transform.Find("LBwheel").GetComponent<WheelCollider>();
        wheels[2] = transform.Find("RFwheel").GetComponent<WheelCollider>();
        wheels[3] = transform.Find("LFwheel").GetComponent<WheelCollider>();
    }
private void FixedUpdate()
{
    // Inputs
    if (controlled)
    {
        MoveInput = Input.GetAxis("Vertical");
        TurnInput = Input.GetAxis("Horizontal");
    }

    // Ground check
    isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

    // Only allow control if grounded
    if (isGrounded)
    {
        Move();
        Turn();

        if (MoveInput > 0 && Input.GetKey(KeyCode.Space)) Brake();
        }
        else
        {
            // In air: keep momentum, no new forces
            //BrakeEffect.SetActive(false);
        }

        SetWheel();
    }

    public void SetWheel()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Vector3 pos;
            Quaternion rot;

            wheels[i].motorTorque = MoveInput * SpeedForce;
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

    public void Move()
    {
        //rb.AddRelativeForce(Vector3.forward * MoveInput * SpeedForce);
        //BrakeEffect.SetActive(false);
    }

    public void Turn()
    {
        float speed = rb.linearVelocity.normalized.magnitude;

        Quaternion re = Quaternion.Euler(Vector3.up * (TurnInput * speed) * TurnForce * Time.deltaTime);
        rb.MoveRotation(rb.rotation * re);
    }

    public void Brake()
    {
        if (rb.linearVelocity.z != 0)
        {
            rb.AddRelativeForce(-Vector3.forward);
            //BrakeEffect.SetActive(true);
        }
    }
}