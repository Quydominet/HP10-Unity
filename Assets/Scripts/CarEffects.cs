using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Rigidbody))]
public class CarEffects : MonoBehaviour
{
    private CarController car;
    private Rigidbody rb;

    [Header("Nitro")]
    public float nitroMultiplier = 3f;

    [Header("Fly")]
    public float flyForce = 12f;
    public float maxFlyHeight = 3f;   // 👈 CAO TỐI ĐA (mét)

    private Coroutine nitroCoroutine;
    private Coroutine flyCoroutine;

    void Awake()
    {
        car = GetComponent<CarController>();
        rb = GetComponent<Rigidbody>();
    }

    // ================= NITRO =================
    public void ActivateNitro(float duration)
    {
        if (nitroCoroutine != null)
            StopCoroutine(nitroCoroutine);

        nitroCoroutine = StartCoroutine(NitroRoutine(duration));
    }

    IEnumerator NitroRoutine(float duration)
    {
        float oldSpeed = car.SpeedForce;
        car.SpeedForce = nitroMultiplier;

        yield return new WaitForSeconds(duration);

        car.SpeedForce = oldSpeed;
        nitroCoroutine = null;
    }

    // ================= FLY (CÓ GIỚI HẠN) =================
    public void ActivateFly(float duration)
    {
        if (flyCoroutine != null)
            StopCoroutine(flyCoroutine);

        flyCoroutine = StartCoroutine(FlyRoutine(duration));
    }

    IEnumerator FlyRoutine(float duration)
    {
        float startY = transform.position.y;
        float targetY = startY + maxFlyHeight;

        float timer = 0f;

        while (timer < duration)
        {
            // 👉 CHỈ BAY KHI CHƯA ĐẠT ĐỘ CAO TỐI ĐA
            if (transform.position.y < targetY)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    flyForce,
                    rb.linearVelocity.z
                );
            }
            else
            {
                // Giữ độ cao, không bay thêm
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    0f,
                    rb.linearVelocity.z
                );
            }

            timer += Time.deltaTime;
            yield return null;
        }

        flyCoroutine = null;
    }
}
