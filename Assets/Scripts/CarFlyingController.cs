using TMPro; // TextMeshPro
using UnityEngine;

public class CarFlyingController : MonoBehaviour
{
    [Header("Xe chạy")]
    [SerializeField] private float giaToc = 20f;       // lực tăng tốc
    [SerializeField] private float tocDoToiDa = 50f;   // tốc độ tối đa
    [SerializeField] private float lucReXe = 80f;      // lực rẽ

    public float tocDoHienTai = 0f;
    private float dauVaoDiChuyen;
    private float dauVaoRe;

    [Header("Ground Check")]
    [SerializeField] private float chieuCaoRay = 1.2f;

    [Header("Bánh xe")]
    [SerializeField] private Transform wheelFrontLeft;
    [SerializeField] private Transform wheelFrontRight;
    [SerializeField] private Transform wheelBackLeft;
    [SerializeField] private Transform wheelBackRight;
    [SerializeField] private float maxSteerAngle = 30f; // góc xoay tối đa bánh trước

    [Header("UI - TextMeshPro")]
    public TMP_Text speedText; // kéo thả TextMeshPro từ Canvas

    private Rigidbody rb;
    private float currentSteerAngle = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (speedText != null)
            speedText.text = "Speed: 0 km/h"; // đảm bảo Text hiển thị
    }

    private void FixedUpdate()
    {
        dauVaoDiChuyen = Input.GetAxis("Vertical");
        dauVaoRe = Input.GetAxis("Horizontal");

        bool grounded = IsGrounded(out Vector3 groundNormal);

        TangToc(dauVaoDiChuyen);

        if (grounded)
        {
            DiChuyenTheoMatDoc(groundNormal);
            ReXe(dauVaoRe);
        }
        else
        {
            // Xe đang bay → vẫn trôi theo hướng trước
            rb.AddForce(transform.forward * tocDoHienTai * 0.3f, ForceMode.Acceleration);
        }

        QuayBanh(dauVaoDiChuyen);
        UpdateSpeedUI();
    }

    // ==================== TĂNG TỐC ====================
    private void TangToc(float input)
    {
        if (input > 0)
        {
            tocDoHienTai += giaToc * Time.deltaTime;
            tocDoHienTai = Mathf.Clamp(tocDoHienTai, 0, tocDoToiDa);
        }
        else if (input < 0)
        {
            tocDoHienTai -= giaToc * Time.deltaTime;
            tocDoHienTai = Mathf.Clamp(tocDoHienTai, 0, tocDoToiDa);
        }
        else
        {
            tocDoHienTai = Mathf.MoveTowards(tocDoHienTai, 0, giaToc * Time.deltaTime * 0.5f);
        }
    }

    // ==================== DI CHUYỂN THEO MẶT DỐC ====================
    private void DiChuyenTheoMatDoc(Vector3 groundNormal)
    {
        Vector3 huongTheoDoc = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
        rb.AddForce(huongTheoDoc * tocDoHienTai, ForceMode.Acceleration);
    }

    // ==================== XOAY XE VÀ BÁNH ====================
    private void ReXe(float input)
    {
        if (tocDoHienTai > 0.1f && input != 0)
        {
            Quaternion re = Quaternion.Euler(0, input * lucReXe * Time.deltaTime, 0);
            rb.MoveRotation(rb.rotation * re);
        }
    }

    // ==================== BÁNH QUAY KHI XE LĂN ====================
    private void QuayBanh(float input)
    {
        float distance = tocDoHienTai * Time.deltaTime; // khoảng di chuyển
        float rotationAngle = distance / 0.3f * Mathf.Rad2Deg; // bán kính bánh ~0.3

        wheelFrontLeft.Rotate(rotationAngle, 0, 0);
        wheelFrontRight.Rotate(rotationAngle, 0, 0);
        wheelBackLeft.Rotate(rotationAngle, 0, 0);
        wheelBackRight.Rotate(rotationAngle, 0, 0);

        float targetAngle = dauVaoRe * maxSteerAngle;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.deltaTime * 5f);

        wheelFrontLeft.localRotation = Quaternion.Euler(0, currentSteerAngle, 0);
        wheelFrontRight.localRotation = Quaternion.Euler(0, currentSteerAngle, -180);
    }

    // ==================== HIỂN THỊ TỐC ĐỘ UI ====================
    private void UpdateSpeedUI()
    {
        if (speedText != null)
        {
            float speedKMH = tocDoHienTai * 3.6f; // m/s → km/h
            speedText.text = "Speed: " + speedKMH.ToString("F0") + " km/h";
        }
    }

    // ==================== CHECK GROUNDED ====================
    private bool IsGrounded(out Vector3 groundNormal)
    {
        Vector3 start = transform.position + Vector3.up * 1f;

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, chieuCaoRay))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }
}
