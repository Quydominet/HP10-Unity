using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private Collider Collider;
    private float CurrentHealth = 100;
    private CarExplosionEffect ExplosionEffect;

    float lastHitTime = 0f;

    void Start()
    {
        CurrentHealth = MaxHealth;
        if (!Collider) Collider = GetComponent<Collider>();
        if (!ExplosionEffect) ExplosionEffect = GetComponent<CarExplosionEffect>();
    }
    private void Die()
    {
        Collider.enabled = true;
        ExplosionEffect.Explode();

        GyroBalancePhysics Gyro = FindAnyObjectByType<GyroBalancePhysics>();
        CarController Car = FindAnyObjectByType<CarController>();
        Projectile Gun = FindAnyObjectByType<Projectile>();
        CarNPC NPC = FindAnyObjectByType<CarNPC>();

        if (Gyro != null && Gyro.gameObject == this.gameObject)
            Gyro.enabled = false;

        if (Car != null && Car.gameObject == this.gameObject)
        {
            Car.MoveInput = 0f;
            Car.TurnInput = 0f;
            Car.enabled = false;
        }

        if (Gun != null && Gun.gameObject == this.gameObject)
        {
            Gun.firing = false;
            Gun.enabled = false;
        }

        if (NPC != null && NPC.gameObject == this.gameObject)
        {
            NPC.enabled = false;
            Destroy(gameObject, 5f);
        }

    }

    public void TakeDamage(GameObject source, float damage)
    {
        print("Taking Damage: " + damage);
        print("Damage Source: " + source.name);
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        lastHitTime = 0f;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public float GetHealth()
    {
        return CurrentHealth;
    }
    public float GetMaxHealth()
    {
        return MaxHealth;
    }

    void Update()
    {
        lastHitTime += Time.deltaTime;

        if (lastHitTime > 5f && CurrentHealth < MaxHealth/2 && CurrentHealth > 0)
        {
            CurrentHealth += MaxHealth * 0.1f * Time.deltaTime; // Regenerate 10% per second

            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
        }
    }
}
