using UnityEngine;
using System.Collections;
using System;
using Unity.Mathematics;

public class Projectile : MonoBehaviour
{
    [Header("Objects")]
    /*[SerializeField]*/ private GameObject ProjectileObject;
    [SerializeField] private GameObject RayObject;
    [SerializeField] private Animator GunAnimator;
    public Transform BarrelPos;
    [SerializeField] private AudioSource SoundEmitter;
    [SerializeField] private AudioClip FireSound;
    [SerializeField] private AudioClip ReloadSound;
    [Header("Projectiles")]
    [SerializeField] private float Damage = 1;
    [SerializeField] private float Cooldown = 1;
    [SerializeField] private float Range = 15;
    [SerializeField] private float SpreadAngle = 15;
    [Header("Ammo")]
    [SerializeField] private int MaxAmmo = int.MaxValue;
    [SerializeField] private float ReloadTime = 1;
    private int CurrentAmmo = 0;
    /*[SerializeField]*/ private bool RayMode = true;

    private float LastShotTime;
    private float FadeDuration = 0.15f;
    private CarController car;

    [SerializeField] private float TrailLength = 0.3f; // fixed length of the visible streak, in 0-1 of total ray

    private bool controlled = false;
    [HideInInspector] public bool firing = false;

    /*
    private IEnumerator FadeLine(LineRenderer line, GameObject rayObj)
    {
        Vector3 originalStart = line.GetPosition(0);
        Vector3 originalEnd = line.GetPosition(1);
        Vector3 originalDirection = (originalEnd - originalStart);
        float originalWidth = line.startWidth;

        for (float width = 0; width <= FadeDuration; width += 0.01f)
        {
            if (line == null)
                yield break;

            float newWidth = originalWidth * ((FadeDuration - width) / FadeDuration);

            line.SetPosition(0, originalStart + originalDirection * math.clamp(width / FadeDuration - .3f, 0f, 1f));
            line.SetPosition(1, originalStart + originalDirection * math.clamp(width / FadeDuration, 0f, 1f));
            line.startWidth = newWidth;
            line.endWidth = newWidth;

            yield return new WaitForSeconds(0.01f);
        }

        Destroy(rayObj);
    }
    */
    private IEnumerator FadeLine(LineRenderer line, GameObject rayObj)
    {
        Vector3 originalStart = line.GetPosition(0);
        Vector3 originalEnd = line.GetPosition(1);
        Vector3 direction = originalEnd - originalStart;
        float originalWidth = line.startWidth;

        float elapsed = 0f;

        while (elapsed < FadeDuration)
        {
            if (line == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / FadeDuration;

            // Head moves from 0 to 1+TrailLength so it fully exits past the end
            float headT = Mathf.Lerp(0f, 1f + TrailLength, t);
            float tailT = headT - TrailLength;

            // Clamp both so the segment never extends before the start or past the end
            float clampedHead = Mathf.Clamp01(headT);
            float clampedTail = Mathf.Clamp01(tailT);

            line.SetPosition(0, originalStart + direction * clampedTail);
            line.SetPosition(1, originalStart + direction * clampedHead);

            // Fade out only near the very end, once the whole streak has exited
            float fadeOut = 1f - Mathf.Clamp01((t - 1f) / TrailLength);
            line.startWidth = originalWidth * fadeOut;
            line.endWidth = originalWidth * fadeOut;

            yield return null;
        }

        Destroy(rayObj);
    }
    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        // Convert angle to radians
        float spreadRadius = Mathf.Tan(angle/2 * Mathf.Deg2Rad);

        // Random offset within unit circle
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadRadius;

        // Create spread direction
        Vector3 spreadDir = forward + BarrelPos.right * randomOffset.x + BarrelPos.up * randomOffset.y;
        return spreadDir.normalized;
    }

    private void FireProjectile()
    {

    }
    private void FireRay()
    {        
        // Calculate spread direction
        Vector3 spreadDirection = GetSpreadDirection(BarrelPos.forward, SpreadAngle);

        // Raycast setup
        Ray ray = new Ray(BarrelPos.position, spreadDirection);

        RaycastHit hitInfo;

        bool hit = Physics.Raycast(ray, out hitInfo, Range, ~0, QueryTriggerInteraction.Collide);

        // Determine hit point
        Vector3 hitPoint = hit ? hitInfo.point : ray.origin + ray.direction * Range;

        // Apply damage if hit
        if (hitInfo.collider != null)
        {
            //hitInfo.transform.SendMessage("TakeDamage", (gameObject, Damage), SendMessageOptions.DontRequireReceiver);

            Health health = hitInfo.transform.GetComponent<Health>();
            MeshDamageAPI damage = hitInfo.transform.GetComponent<MeshDamageAPI>();

            if (health != null) health.TakeDamage(gameObject, Damage);
            if (damage != null) damage.ApplyDent(hitInfo.point, hitInfo.normal, Damage);
        }

        // Create visual ray effect
        GameObject newRay = Instantiate(RayObject);
        newRay.transform.position = BarrelPos.position;

        LineRenderer line = newRay.GetComponent<LineRenderer>();

        if (line != null)
        {
            line.SetPosition(0, BarrelPos.position);
            line.SetPosition(1, hitPoint);
            StartCoroutine(FadeLine(line, newRay));
        }
    }
    public int GetMaxAmmo()
    {
        return MaxAmmo;
    }
    public int GetAmmo()
    {
        return CurrentAmmo;
    }
    void Start()
    {
        LastShotTime = Time.time;
        car = GetComponent<CarController>();
        controlled = (transform.tag == "Player");
        CurrentAmmo = MaxAmmo;
    }
    void FixedUpdate()
    {
        if (CurrentAmmo <= 0)
        {
            if (Time.time - LastShotTime >= ReloadTime)
            {
                CurrentAmmo = MaxAmmo;

                if (SoundEmitter != null && ReloadSound != null)
                {
                    SoundEmitter.PlayOneShot(ReloadSound);
                }
            }
        }

        bool Shooting = (Input.GetButton("Fire1") && controlled) || (!controlled && firing);

        if (Shooting)
        {
            if ((Time.time - LastShotTime >= Cooldown) && CurrentAmmo > 0)
            {
                LastShotTime = Time.time;
                CurrentAmmo--;

                if (ProjectileObject != null && !RayMode)
                    FireProjectile();
                else
                    FireRay();

                if (SoundEmitter != null && FireSound != null)
                {
                    SoundEmitter.PlayOneShot(FireSound);
                }
            }
        }

        // Play gun animation
        if (GunAnimator != null)
        {
            GunAnimator.SetBool("Shoot", Shooting);
            GunAnimator.speed = GunAnimator.GetCurrentAnimatorClipInfo(0).Length / Cooldown;
        }
    }
}
