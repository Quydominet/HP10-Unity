using UnityEngine;

public class ExtraGravityCar : MonoBehaviour
{
    public float extraGravity = 30f;   // chỉnh mạnh nhẹ tùy bạn
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Gravity mặc định của Unity là 9.81
        // Lực thêm này giúp xe bám đường mạnh hơn
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }
}
