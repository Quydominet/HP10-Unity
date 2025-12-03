using UnityEngine;

public class giatoc : MonoBehaviour
{
    [Header("Tăng tốc")]
    [SerializeField] private float giaToc = 25f;       // lực tăng tốc
    [SerializeField] private float tocDoToiDa = 50f;   // tốc độ tối đa
    private float tocDoHienTai = 0f;

    [Header("Xoay xe")]
    [SerializeField] private float lucReXe = 80f;

    [Header("Ground Check")]
    [SerializeField] private float chieuCaoRay = 1.2f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate; // mượt hơn
    }

    private void FixedUpdate()
    {
        float inputTien = Input.GetAxis("Vertical");
        float inputRe = Input.GetAxis("Horizontal");

        bool grounded = IsGrounded(out Vector3 groundNormal);

        // ⭐ Nếu xe chạm đất → dùng lực theo mặt dốc
        if (grounded)
        {
            TangToc(inputTien);
            DiChuyenTheoMatDoc(groundNormal);
            ReXe(inputRe);
        }
        else
        {
            // ⭐ Nếu xe đang bay ramp → vẫn cộng lực tiến như thật
            rb.AddForce(transform.forward * tocDoHienTai * 0.5f, ForceMode.Acceleration);
        }
    }

    // ============================================================
    // 1️⃣ TĂNG TỐC
    // ============================================================
    private void TangToc(float inputTien)
    {
        if (inputTien > 0)
        {
            tocDoHienTai += giaToc * Time.deltaTime;
            tocDoHienTai = Mathf.Clamp(tocDoHienTai, 0, tocDoToiDa);
        }
        else
        {
            tocDoHienTai -= giaToc * Time.deltaTime;
            tocDoHienTai = Mathf.Clamp(tocDoHienTai, 0, tocDoToiDa);
        }
    }

    // ============================================================
    // 2️⃣ DI CHUYỂN THEO MẶT DỐC
    // ============================================================
    private void DiChuyenTheoMatDoc(Vector3 slopeNormal)
    {
        Vector3 huongPhong = Vector3.ProjectOnPlane(transform.forward, slopeNormal).normalized;
        rb.AddForce(huongPhong * tocDoHienTai, ForceMode.Acceleration);
    }

    // ============================================================
    // 3️⃣ XOAY XE
    // ============================================================
    private void ReXe(float input)
    {
        if (input != 0)
        {
            Quaternion rot = Quaternion.Euler(0, input * lucReXe * Time.deltaTime, 0);
            rb.MoveRotation(rb.rotation * rot);
        }
    }

    // ============================================================
    // CHECK GROUND
    // ============================================================
    private bool IsGrounded(out Vector3 groundNormal)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, chieuCaoRay))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }
}
