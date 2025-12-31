using UnityEngine;

public class CarCollision : MonoBehaviour
{
    public float impactThreshold = 6f;     // đụng hơi mạnh là bật
    public float bounceForce = 250f;       // lực bật lên nhẹ
    public float spinForce = 60f;          // lực xoay nhẹ

    private Rigidbody carRb;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;

        // Chỉ bật khi đủ lực
        if (impact > impactThreshold)
        {
            // Bật nhẹ lên trên
            carRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

            Vector3 spin = new Vector3(
              Random.Range(-4f, 4f),
              Random.Range(-2f, 2f),
              Random.Range(-4f, 4f)
          );



            carRb.AddTorque(spin * 400f, ForceMode.Impulse);
        }
    }
}
