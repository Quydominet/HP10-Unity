using UnityEngine;

public class CarSound : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip idleClip;
    public AudioClip runClip;
    public AudioClip heavyClip;

    public float minSpeed = 0.5f;
    public float maxSpeed = 20f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    [System.Obsolete]
    void Update()
    {
        float speed = rb.velocity.magnitude;

        // ===== ĐỨNG YÊN =====
        if (speed < minSpeed)
        {
            ChangeSound(idleClip, 0.8f, 0.5f);
        }
        // ===== CHẠY =====
        else if (speed >= minSpeed && speed < maxSpeed)
        {
            ChangeSound(runClip, 1.1f, 0.8f);
        }
        // ===== NẶNG MÁY =====
        else
        {
            ChangeSound(heavyClip, 1.3f, 1f);
        }

        // Pitch theo tốc độ
        audioSource.pitch = 0.8f + (speed / maxSpeed);
    }

    void ChangeSound(AudioClip clip, float pitch, float volume)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }

        audioSource.pitch = Mathf.Lerp(audioSource.pitch, pitch, Time.deltaTime * 2f);
        audioSource.volume = Mathf.Lerp(audioSource.volume, volume, Time.deltaTime * 2f);
    }
}
