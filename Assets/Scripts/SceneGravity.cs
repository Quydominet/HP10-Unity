using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    public Rigidbody rb;

    void FixedUpdate()
    {
        Physics.gravity = gravity;
        Physics.gravity = new Vector3(0, -80f, 0);
        gravity = transform.forward * 9.81f;
        rb.AddForce(-transform.up * 50f, ForceMode.Acceleration);

    }
}
