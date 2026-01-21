using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Movement")]
    public float moveForce = 1800f;
    public float turnForce = 120f;
    public float maxSpeed = 25f;

    private float moveInput;
    private float turnInput;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, -0.6f, 0); // chống lật
    }

    void Update()
    {
        moveInput = Input.GetAxis("Vertical");   // ↑ ↓
        turnInput = Input.GetAxis("Horizontal"); // ← →
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * moveForce * Time.fixedDeltaTime);
        }

        if (rb.linearVelocity.magnitude > 1f)
        {
            rb.AddTorque(Vector3.up * turnInput * turnForce * Time.fixedDeltaTime);
        }
    }
}
