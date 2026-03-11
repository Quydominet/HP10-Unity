using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GroundAlignGTA : MonoBehaviour
{
    public float rayDistance = 2.5f;
    public float rotationSmooth = 10f;
    public float downForce = 60f;
    public LayerMask groundLayer;

    Rigidbody rb;
    bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, groundLayer))
        {
            grounded = true;

            Vector3 groundNormal = hit.normal;

            // Project forward direction onto ground
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

            if (forward.sqrMagnitude < 0.01f)
                forward = transform.forward;

            // Create slope-aligned rotation
            Quaternion targetRotation = Quaternion.LookRotation(forward, groundNormal);

            // Smooth rotation
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSmooth * Time.fixedDeltaTime
            ));

            // Stick car to ground
            rb.AddForce(-groundNormal * downForce, ForceMode.Acceleration);
        }
        else
        {
            grounded = false;
        }
    }
}
