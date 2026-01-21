using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float normalSpeed = 20f;
    public float nitroSpeed = 50f;
    public GameObject bulletPrefab; // Kéo model viên đạn vào đây
    public Transform firePoint;     // Vị trí nòng súng trên xe

    private bool isFlying = false;
    private bool canShoot = false;

    public void ActivatePowerUp(int type)
    {
        switch (type)
        {
            case 1: StartCoroutine(NitroRoutine()); break;
            case 2: StartCoroutine(FlyRoutine()); break;
            case 3: StartCoroutine(ShootRoutine()); break;
        }
    }

    // --- 1. Chức năng Nitro (3 giây) ---
    IEnumerator NitroRoutine()
    {
        Debug.Log("Kích hoạt Nitro!");
        float originalSpeed = normalSpeed;
        normalSpeed = nitroSpeed; // Tăng tốc độ
        yield return new WaitForSeconds(3f);
        normalSpeed = originalSpeed; // Trả lại tốc độ cũ
    }

    // --- 2. Chức năng Bay (3 giây) ---
    IEnumerator FlyRoutine()
    {
        Debug.Log("Xe đang bay!");
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Tắt trọng lực để bay
        transform.position += new Vector3(0, 5, 0); // Nhấc xe lên cao
        yield return new WaitForSeconds(3f);
        rb.useGravity = true;
    }

    // --- 3. Chức năng Bắn súng (5 giây) ---
    IEnumerator ShootRoutine()
    {
        Debug.Log("Súng đã sẵn sàng! Nhấn Space để bắn.");
        canShoot = true;
        yield return new WaitForSeconds(5f);
        canShoot = false;
    }

    void Update()
    {
        // Logic bắn súng khi có item
        if (canShoot && Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }

        // Code di chuyển xe của bạn sẽ nằm ở đây và sử dụng biến normalSpeed
    }
}