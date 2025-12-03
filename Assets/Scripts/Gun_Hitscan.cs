// Gun_Hitscan.cs
using UnityEngine;
using System.Collections;

public class Gun_Hitscan : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.2f; // tối thiểu giữa 2 lần bắn (s) -> control tốc độ bấm nhanh
    public int magazineSize = 12;
    public float reloadTime = 1.5f;

    [Header("References")]
    public Camera fpsCamera;            // Camera để raycast (gán Camera.main nếu null)
    public ParticleSystem muzzleFlash;  // optional
    public GameObject impactEffect;     // optional
    public AudioClip shootSfx;
    public AudioClip reloadSfx;
    public LayerMask hitMask = ~0;

    int currentAmmo;
    float lastFireTime = -999f;
    bool isReloading = false;
    AudioSource audioSource;

    void Awake()
    {
        currentAmmo = magazineSize;
        audioSource = GetComponent<AudioSource>();
        if (fpsCamera == null) fpsCamera = Camera.main;
    }

    void Update()
    {
        if (isReloading) return;

        // Tải đạn khi nhấn R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // Bắn 1 viên mỗi lần nhấn chuột trái (GetButtonDown hoặc GetMouseButtonDown)
        if (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0))
        {
            // Kiểm tra rate của súng
            if (Time.time - lastFireTime >= fireRate && currentAmmo > 0)
            {
                Shoot();
                lastFireTime = Time.time;
            }
            else if (currentAmmo <= 0)
            {
                // optional: play empty click sound
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource && reloadSfx) audioSource.PlayOneShot(reloadSfx);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;
        if (muzzleFlash) muzzleFlash.Play();
        if (audioSource && shootSfx) audioSource.PlayOneShot(shootSfx);

        RaycastHit hit;
        Vector3 origin = fpsCamera.transform.position;
        Vector3 dir = fpsCamera.transform.forward;

        if (Physics.Raycast(origin, dir, out hit, range, hitMask))
        {
            // Damage
            var dmg = hit.collider.GetComponentInParent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(damage);

            // Impact effect
            if (impactEffect != null)
            {
                var eff = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(eff, 3f);
            }
        }

        // Optional: recoil/camera kick
        StartCoroutine(SimpleRecoil());
    }

    IEnumerator SimpleRecoil()
    {
        float recoil = 2f;
        float t = 0f, dur = 0.05f;
        Quaternion original = fpsCamera.transform.localRotation;
        while (t < dur)
        {
            t += Time.deltaTime;
            fpsCamera.transform.localRotation = original * Quaternion.Euler(-recoil * (1 - t / dur), 0f, 0f);
            yield return null;
        }
        fpsCamera.transform.localRotation = original;
    }
}
