using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private Collider Collider;
    private float CurrentHealth = 100;
    private CarExplosionEffect ExplosionEffect;

    void Start()
    {
        CurrentHealth = MaxHealth;
        if (!Collider) Collider = GetComponent<Collider>();
        if (!ExplosionEffect) ExplosionEffect = GetComponent<CarExplosionEffect>();
    }
    public void Die()
    {
        Collider.enabled = false;
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

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    void Update()
    {
        
    }
}
