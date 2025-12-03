using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100;
    private float CurrentHealth = 100;
    private Collider Collider;
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
        //Destroy(gameObject, 2f);
    }

    public void TakeDamage(float damage)
    {
        print("Taking Damage: " + damage);
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
