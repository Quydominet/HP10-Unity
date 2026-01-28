using UnityEngine;

public class TurboBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostIncreasePerSecond = 5f;
    public float maxBoostMultiplier = 4f;

    [Header("References")]
    public Car car;
    public TrailRenderer boostLeft;
    public TrailRenderer boostRight;

    private float originalSpeed;
    private float currentMultiplier = 1f;

    [Header("Conditions")]
    public float minSpeedToBoost = 1f;
    public float groundCheckDistance = 1.2f;
    public float turnThreshold = 0.2f;

    [Header("Effects")]
    public ParticleSystem boostSmoke;
    public ParticleSystem boostSmoke2;
    public ParticleSystem boostFire;

    public float boostSmokeRate = 120f;
    public float boostFireRate = 60f;

    private Rigidbody rb;

    void Start()
    {
        if (car == null) car = GetComponent<Car>();
        rb = GetComponent<Rigidbody>();

        originalSpeed = car.tocdoxe;

        if (boostLeft != null) boostLeft.emitting = false;
        if (boostRight != null) boostRight.emitting = false;

        if (boostSmoke != null)
            boostSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (boostFire != null)
            boostFire.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        bool grounded = IsGrounded();
        float turnInput = Input.GetAxis("Horizontal");
        bool isTurning = Mathf.Abs(turnInput) > turnThreshold;

        bool canBoost =
            Input.GetKey(KeyCode.E) &&
            grounded &&
            speed > minSpeedToBoost;

        if (canBoost)
        {
            // 🚀 BOOST
            currentMultiplier += boostIncreasePerSecond * Time.deltaTime;
            currentMultiplier = Mathf.Clamp(currentMultiplier, 1f, maxBoostMultiplier);
            car.tocdoxe = originalSpeed * currentMultiplier;

            if (boostLeft != null) boostLeft.emitting = true;
            if (boostRight != null) boostRight.emitting = true;

            // 💨 KHÓI (chỉ khi boost)
            if (boostSmoke != null)
            {
                if (!boostSmoke.isPlaying) boostSmoke.Play();
                var smokeEmission = boostSmoke.emission;
                smokeEmission.rateOverTime = boostSmokeRate;
            }

            // 🔥 LỬA (chỉ khi rẽ)
            if (boostFire != null)
            {
                if (isTurning)
                {
                    if (!boostFire.isPlaying) boostFire.Play();
                    var fireEmission = boostFire.emission;
                    fireEmission.rateOverTime = boostFireRate;
                }
                else
                {
                    if (boostFire.isPlaying) boostFire.Stop();
                }
            }
        }
        else
        {
            // ⛔ RESET
            currentMultiplier = 1f;
            car.tocdoxe = originalSpeed;

            if (boostLeft != null) boostLeft.emitting = false;
            if (boostRight != null) boostRight.emitting = false;

            if (boostSmoke != null && boostSmoke.isPlaying) boostSmoke.Stop();
            if (boostFire != null && boostFire.isPlaying) boostFire.Stop();
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position + Vector3.up * 0.2f,
            Vector3.down,
            groundCheckDistance
        );
    }
}
