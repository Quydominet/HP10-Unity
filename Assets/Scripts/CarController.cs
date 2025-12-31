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