using UnityEngine;

public class Bullet : MonoBehaviour
{
    public AudioClip hitSound; // âm thanh khi đạn trúng

    void OnCollisionEnter(Collision collision)
    {
        // Lấy vị trí va chạm
        Vector3 hitPoint = collision.contacts[0].point;

        // Phát âm thanh tại vị trí đó
        AudioSource.PlayClipAtPoint(hitSound, hitPoint, Random.Range(0.8f, 1.2f));

        // Hủy đạn
        Destroy(gameObject);
    }
}