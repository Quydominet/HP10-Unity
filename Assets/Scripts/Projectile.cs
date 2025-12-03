using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject ProjectileObject;
    [SerializeField] private GameObject RayObject;
    [SerializeField] private Transform BarrelPos;
    [SerializeField] private float Damage = 1;
    [SerializeField] private float Cooldown = 1;
    [SerializeField] private float Range = 15;
    [SerializeField] private float SpreadAngle = 15;
    [SerializeField] private bool RayMode = true;

    private float LastShotTime;
    private float FadeDuration = 0.1f;
    private Car car;

    private bool controlled = false;
    [HideInInspector] public bool firing = false;

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

            line.SetPosition(0, originalStart + originalDirection * (width / FadeDuration));
            line.startWidth = newWidth;
            line.endWidth = newWidth;

            yield return new WaitForSeconds(0.01f);
        }

        Destroy(rayObj);
    }
    private Vector3 GetSpreadDirection(Vector3 forward, float angle)
    {
        // Convert angle to radians
        float spreadRadius = Mathf.Tan(angle/2 * Mathf.Deg2Rad);

        // Random offset within unit circle
        Vector2 randomOffset = Random.insideUnitCircle * spreadRadius;

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

        bool hit = Physics.Raycast(ray, out hitInfo, Range, ~0, QueryTriggerInteraction.Ignore);

        // Determine hit point
        Vector3 hitPoint = hit ? hitInfo.point : ray.origin + ray.direction * Range;

        // Apply damage if hit
        if (hitInfo.collider != null)
        {
            hitInfo.transform.SendMessage("TakeDamage", Damage, SendMessageOptions.DontRequireReceiver);

            Health health = hitInfo.transform.GetComponent<Health>();
            MeshDamageAPI damage = hitInfo.transform.GetComponent<MeshDamageAPI>();

            if (health != null) health.TakeDamage(Damage);
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

    void Start()
    {
        LastShotTime = Time.time;
        car = GetComponent<Car>();
        controlled = (transform.tag == "Player");
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButton("Fire1") && controlled) || (!controlled && firing))
        {
            if (Time.time - LastShotTime > Cooldown)
            {
                LastShotTime = Time.time;

                if (ProjectileObject != null && !RayMode)
                    FireProjectile();
                else
                    FireRay();
            }
        }
    }
}
