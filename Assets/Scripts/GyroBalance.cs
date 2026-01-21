using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GyroBalancePhysics : MonoBehaviour
{
    [Header("Balance Settings")]
    [Tooltip("Strength of the stabilizing torque")]
    public float torqueStrength = 5f;

    [Tooltip("Damping factor to reduce oscillation")]
    public float damping = 2f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 20f; // safety cap
    }

    void FixedUpdate()
    {
        if (!this.enabled) return;

        // Target upright rotation (keep Y free)
        Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        // Difference between current and target
        Quaternion delta = targetRotation * Quaternion.Inverse(transform.rotation);

        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        // Ignore Y axis correction
        axis.y = 0f;

        // Corrective torque proportional to tilt
        Vector3 correctiveTorque = axis.normalized * angle * torqueStrength;

        // Apply damping against current angular velocity
        Vector3 dampingTorque = -rb.angularVelocity * damping;

        rb.AddTorque(correctiveTorque + dampingTorque);
    }
}