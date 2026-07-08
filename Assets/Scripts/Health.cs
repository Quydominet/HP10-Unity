using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private Collider Collider;
    [SerializeField] private SpawnPicker SpawnPicker;
    private float CurrentHealth = 100;
    private CarExplosionEffect ExplosionEffect;
    float lastHitTime = 0f;

    void Start()
    {
        CurrentHealth = MaxHealth;
        if (!Collider) Collider = GetComponent<Collider>();
        if (!ExplosionEffect) ExplosionEffect = GetComponent<CarExplosionEffect>();
        if (!SpawnPicker) SpawnPicker = FindAnyObjectByType<SpawnPicker>();
    }

    private void Respawn()
    {
        Transform spawn = SpawnPicker?.GetBestSpawn();

        if (spawn != null)
            transform.SetPositionAndRotation(spawn.position, spawn.rotation);
        else
            Debug.LogWarning("No spawn point found for " + gameObject.name);

        CurrentHealth = MaxHealth;
        Collider.enabled = true;

        BroadcastMessage("HealthChange", SendMessageOptions.DontRequireReceiver);

        CarController Car = GetComponent<CarController>();
        CarNPC NPC = GetComponent<CarNPC>();

        foreach (Projectile r in GetComponentsInChildren<Projectile>(true))
            r.enabled = true;

        if (Car) Car.enabled = true;
        if (NPC) NPC.enabled = true;

        ExplosionEffect?.RemoveExplosion();
    }

    private void Die()
    {
        Collider.enabled = false;
        ExplosionEffect?.Explode();

        CarController Car = GetComponent<CarController>();
        Projectile Gun = GetComponent<Projectile>();
        CarNPC NPC = GetComponent<CarNPC>();

        foreach (Projectile r in GetComponentsInChildren<Projectile>(true))
            r.enabled = false;

        if (Car)
        {
            Car.MoveInput = 0f;
            Car.TurnInput = 0f;
            Car.enabled = false;
        }

        if (NPC)
        {
            NPC.enabled = false;
        }

        if (gameObject.tag == "NPC")
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (gameObject.CompareTag("NPC")) player.SendMessage("NPCDeath", SendMessageOptions.DontRequireReceiver);
        }

        Invoke(nameof(Respawn), 5f);
    }

    public void TakeDamage(GameObject source, float damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        lastHitTime = 0f;
        BroadcastMessage("HealthChange", SendMessageOptions.DontRequireReceiver);

        if (CurrentHealth <= 0) Die();
    }

    public float GetHealth() => CurrentHealth;
    public float GetMaxHealth() => MaxHealth;
    void Update()
    {
        lastHitTime += Time.deltaTime;

        if (lastHitTime > 5f && CurrentHealth < MaxHealth / 2 && CurrentHealth > 0)
        {
            CurrentHealth += MaxHealth * 0.1f * Time.deltaTime;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

            BroadcastMessage("HealthChange", SendMessageOptions.DontRequireReceiver);
        }
    }
}