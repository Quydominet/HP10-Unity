using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;

    void Start()
    {
        Destroy(gameObject, 3f); // tự hủy sau 3 giây
    }

    void OnCollisionEnter(Collision collision)
    {
        // Nếu đụng vật có script Health thì gây sát thương
        IDamageable target = collision.collider.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        // Sau khi chạm thì hủy viên đạn
        Destroy(gameObject);
    }
}
