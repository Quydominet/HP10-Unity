using UnityEngine;

public class TurboBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostIncreasePerSecond = 5f; // how fast speed grows while holding E
    public float maxBoostMultiplier = 4f;     // 4x speed cap

    [Header("References")]
    public Car car;
    public TrailRenderer boostLeft;
    public TrailRenderer boostRight;

    private float originalSpeed;
    private float currentMultiplier = 1f;

    void Start()
    {
        if (car == null) car = GetComponent<Car>();
        originalSpeed = car.tocdoxe;

        if (boostLeft != null)
        {
             boostLeft.emitting = false;
        }
        if (boostRight != null)
        {
        boostRight.emitting = false;
        }
    }

    void Update()
    {
        // HOLD BOOST
        if (Input.GetKey(KeyCode.E))
        {
            // Increase speed multiplier
            currentMultiplier += boostIncreasePerSecond * Time.deltaTime;
            currentMultiplier = Mathf.Clamp(currentMultiplier, 1f, maxBoostMultiplier);

            car.tocdoxe = originalSpeed * currentMultiplier;

            if (boostLeft != null) 
            {
                boostLeft.emitting = true;
            }
            if (boostRight != null) 
            {
                boostRight.emitting = true;
            }
        }
        else
        {
            // Reset when key released
            currentMultiplier = 1f;
            car.tocdoxe = originalSpeed;

            if (boostLeft != null) 
            {
                boostLeft.emitting = false;
            }
            if (boostRight != null) 
            {
            boostRight.emitting = false;
            }
        }
    }
}
