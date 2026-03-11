using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [Header("Movement")]
    public float tocdoxe = 100f;     // motor force
    public float lucReXe = 60f;      // steering torque
    public float lucPhanh = 40f;     // brake force

    float dauvaodichuyen;
    float dauVaoRe;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        dauvaodichuyen = Input.GetAxis("Vertical");
        dauVaoRe = Input.GetAxis("Horizontal");

        Dichuyenxe();
        ReXe();

        if (dauvaodichuyen > 0 && Input.GetKey(KeyCode.LeftShift))
        {
            PhanhXe();
        }
    }

    void Dichuyenxe()
    {
        rb.AddForce(transform.forward * dauvaodichuyen * tocdoxe, ForceMode.Acceleration);
    }

    void ReXe()
    {
        rb.AddTorque(Vector3.up * dauVaoRe * lucReXe, ForceMode.Acceleration);
    }

    void PhanhXe()
    {
        Vector3 brakeForce = -rb.linearVelocity.normalized * lucPhanh;
        rb.AddForce(brakeForce, ForceMode.Acceleration);
    }
}
