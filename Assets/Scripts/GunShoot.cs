using UnityEngine;

public class GunShoot : MonoBehaviour
{
    [Header("Cấu hình bắn")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Hiệu ứng & âm thanh")]
    public GameObject muzzleFlashPrefab;
    public AudioClip shootSfx;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ✅ Tự đảm bảo không bị vật lý đẩy
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Thiếu bulletPrefab hoặc firePoint!");
            return;
        }

        // 🔹 Tạo viên đạn độc lập (không gắn với súng)
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.transform.parent = null; // Tách đạn ra khỏi cây súng
        bullet.transform.parent = null; // đảm bảo không làm con của súng

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;
        }

        // 🔹 Bỏ va chạm giữa đạn và súng
        Collider gunCollider = GetComponent<Collider>();
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (gunCollider != null && bulletCollider != null)
        {
            Physics.IgnoreCollision(gunCollider, bulletCollider);
        }

        // 🔹 Hiệu ứng tia lửa
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint);
            Destroy(flash, 0.3f);
        }

        // 🔹 Âm thanh bắn
        if (audioSource != null && shootSfx != null)
        {
            audioSource.PlayOneShot(shootSfx);
        }

        // 🔹 Xoá viên đạn sau 3 giây
        Destroy(bullet, 3f);
    }
}
