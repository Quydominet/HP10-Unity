//using UnityEngine;

//public class CarController : MonoBehaviour
//{
//    [Header("Car Settings")]
//    [SerializeField] private float SpeedForce = 100f;
//    [SerializeField] private float TurnForce = 100f;
//    [SerializeField] private float BrakeForce = 50f;
//    [SerializeField] private GameObject BrakeEffect;

//    [Header("Ground Check Settings")]
//    [SerializeField] private Transform groundCheckPoint; // empty GameObject under car
//    [SerializeField] private float groundCheckRadius = 0.5f;
//    [SerializeField] private LayerMask groundLayer;

<<<<<<< Updated upstream
//    [HideInInspector] public float MoveInput;
//    [HideInInspector] public float TurnInput;
//    private Rigidbody rb;
//    [SerializeField] private bool controlled = false;
//    private bool isGrounded = false;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        controlled = (transform.tag == "Player");
//    }
=======
    [HideInInspector] public float MoveInput;
    [HideInInspector] public float TurnInput;
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
>>>>>>> Stashed changes

//    private void FixedUpdate()
//    {
//        // Inputs
//        if (controlled)
//        {
//            MoveInput = Input.GetAxis("Vertical");
//            TurnInput = Input.GetAxis("Horizontal");
//        }

//        // Ground check
//        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);

//        // Only allow control if grounded
//        if (isGrounded)
//        {
//            Move();
//            Turn();

<<<<<<< Updated upstream
//            if (MoveInput > 0 && Input.GetKey(KeyCode.Space)) Brake();
//        }
//        else
//        {
//            // In air: keep momentum, no new forces
//            //BrakeEffect.SetActive(false);
//        }
//    }

//    public void Move()
//    {
//        rb.AddRelativeForce(Vector3.forward * MoveInput * SpeedForce);
//        //BrakeEffect.SetActive(false);
//    }
=======
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
        for (int wheel = 0; wheel < wheels.Length; wheel++)
        {
            Vector3 pos;
            Quaternion rot;

            wheels[wheel].motorTorque = MoveInput * SpeedForce;
            wheels[wheel].GetWorldPose(out pos, out rot);

            GameObject Mesh = wheels[wheel].gameObject;
            Mesh = Mesh.transform.GetChild(0).gameObject;

            Mesh.transform.position = pos;

            if (wheel % 2 != 0) rot *= Quaternion.Euler(0, 180, 0);
            Mesh.transform.rotation = rot;
        }
    }

    public void Move()
    {
        //rb.AddRelativeForce(Vector3.forward * MoveInput * SpeedForce);
        //BrakeEffect.SetActive(false);
    }
>>>>>>> Stashed changes

//    public void Turn()
//    {
//        Quaternion re = Quaternion.Euler(Vector3.up * TurnInput * TurnForce * Time.deltaTime);
//        rb.MoveRotation(rb.rotation * re);
//    }

//    public void Brake()
//    {
//        if (rb.linearVelocity.z != 0)
//        {
//            rb.AddRelativeForce(-Vector3.forward * BrakeForce);
//            //BrakeEffect.SetActive(true);
//        }
//    }
//}