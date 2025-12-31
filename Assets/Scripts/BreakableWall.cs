using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public GameObject brokenWallPrefab; // Prefab tường vỡ
    public float breakForce = 10f;      // Lực cần để làm vỡ

    private bool isBroken = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        // Nếu xe va vào mạnh
        if (collision.relativeVelocity.magnitude > breakForce)
        {
            BreakWall();
        }
    }

    void BreakWall()
    {
        isBroken = true;

        // Spawn tường vỡ đúng vị trí tường cũ
        GameObject bw = Instantiate(brokenWallPrefab, transform.position, transform.rotation);

        // Cho các mảnh văng ra
        foreach (Rigidbody rb in bw.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(300f, transform.position, 2f);
        }

        // Xóa tường nguyên
        Destroy(gameObject);
    }
}
