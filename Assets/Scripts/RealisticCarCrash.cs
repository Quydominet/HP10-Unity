using UnityEngine;

public class RealisticCarCrash : MonoBehaviour
{
    public Rigidbody carRb;

    [Header("Tuning")]
    public float minCrashSpeed = 6f;        // tốc độ tối thiểu để xảy ra phản ứng
    public float upwardImpulse = 3f;        // lực bật lên (impulse)
    public float torqueMultiplier = 120f;   // nhân vào moment lật
    public float forceAtPointMultiplier = 30f; // lực áp vào điểm chạm
    public bool forceAtContact = true;      // true = AddForceAtPosition (thực tế)
    public bool overrideCenterOfMass = false;
    public Vector3 centerOfMassOverride = new Vector3(0, 0.6f, 0); // thử đẩy lên để dễ lật

    void Start()
    {
        if (carRb == null) carRb = GetComponent<Rigidbody>();

        if (overrideCenterOfMass && carRb != null)
        {
            carRb.centerOfMass = centerOfMassOverride;
            Debug.Log("[CarCollisionDebug] COM override = " + centerOfMassOverride);
        }

        // Tự kiểm tra các setting quan trọng
        if (carRb != null)
        {
            Debug.Log($"[CarCollisionDebug] mass={carRb.mass}, isKinematic={carRb.isKinematic}, freezeRotX={carRb.constraints.HasFlag(RigidbodyConstraints.FreezeRotationX)}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Chỉ xử lý wall (bạn có thể sửa/loại trừ)
        if (!collision.collider.CompareTag("Wall")) return;

        ContactPoint contact = collision.contacts[0];
        Vector3 hitNormal = contact.normal;
        float impactSpeed = collision.relativeVelocity.magnitude;

        Debug.Log($"[CarCollisionDebug] Hit {collision.collider.name} at speed={impactSpeed:F2}, contactPos={contact.point}, normal={hitNormal}");

        // vẽ normal
        Debug.DrawRay(contact.point, hitNormal, Color.red, 3f);

        if (impactSpeed < minCrashSpeed) return;

        // 1) giảm tốc (không đặt =0 để tránh "bật cứng")
        carRb.linearVelocity *= 0.35f;

        // 2) bật nhẹ lên
        carRb.AddForce(Vector3.up * upwardImpulse, ForceMode.Impulse);

        // 3) Tính moment lật: lấy hướng vuông góc với hit normal
        Vector3 rollDir = Vector3.Cross(hitNormal, Vector3.up).normalized;

        // 4) torque mong muốn theo tốc độ
        float torqueStrength = torqueMultiplier * impactSpeed;

        // 5) Thử áp lực tại điểm chạm (tạo moment hiệu quả hơn)
        if (forceAtContact)
        {
            // lực hướng song song với rollDir, đặt tại điểm chạm -> tạo moment xoay
            Vector3 forceAtPoint = rollDir * (impactSpeed * forceAtPointMultiplier);
            carRb.AddForceAtPosition(forceAtPoint, contact.point, ForceMode.Impulse);

            Debug.Log($"[CarCollisionDebug] AddForceAtPosition force={forceAtPoint}");
        }
        else
        {
            // fallback: AddTorque trực tiếp
            carRb.AddTorque(rollDir * torqueStrength, ForceMode.Impulse);
            Debug.Log($"[CarCollisionDebug] AddTorque rollDir={rollDir} * {torqueStrength}");
        }
    }
}
