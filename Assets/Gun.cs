using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (bulletPrefab == null)
            {
                Debug.LogError("Bullet prefab is not assigned in the Inspector!");
                return;
            }

            if (bulletSpawnPoint == null)
            {
                Debug.LogError("Bullet spawn point is not assigned in the Inspector!");
                return;
            }

            var spawnPos = bulletSpawnPoint.position + bulletSpawnPoint.forward * 0.5f;
            var bullet = Instantiate(bulletPrefab, spawnPos, bulletSpawnPoint.rotation);

            var rb = bullet.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = bulletSpawnPoint.forward * bulletSpeed;
            }
            else
            {
                Debug.LogWarning("Bullet prefab has no Rigidbody component!");
            }
        }
    }
}
