using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(Rigidbody))]
public class GroundGlue : MonoBehaviour
{
    public float rayDistance = 3f;
    public float rideHeight = 0.6f;
    public float positionSmooth = 15f;
    public float rotationSmooth = 10f;
    public LayerMask groundLayer;
    public float extraGravity = 30f;
    public float maxFallSpeed = 50f;


    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;


        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            
                // Apply stronger gravity
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

                // Clamp fall speed
                Vector3 vel = rb.linearVelocity;
                vel.y = Mathf.Max(vel.y, -maxFallSpeed);
                rb.linearVelocity = vel;


            float currentHeight = Vector3.Dot(transform.position - hit.point, hit.normal);
            float heightError = rideHeight - currentHeight;

            Vector3 correction = hit.normal * heightError;
            rb.MovePosition(rb.position + correction * positionSmooth * Time.fixedDeltaTime);

         
            Quaternion targetRotation =
                Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;


            rb.MoveRotation
                (
                Quaternion.Slerp(rb.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime)

            );

        }
    }
}
