using UnityEngine;

public class CarSound : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip idleClip;
    public AudioClip runClip;
    public AudioClip heavyClip;

    // SOUND EFFECT
    public AudioClip shootClip;
    public AudioClip hitMetalClip;
    public AudioClip crashClip;

    public float minSpeed = 0.5f;
    public float maxSpeed = 20f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed < minSpeed)
        {
            ChangeSound(idleClip, 0.8f, 0.5f);
        }
        else if (speed < maxSpeed)
        {
            ChangeSound(runClip, 1.1f, 0.8f);
        }
        else
        {
            ChangeSound(heavyClip, 1.3f, 1f);
        }

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

    // SOUND BẮN
    public void PlayShootSound()
    {
        AudioSource.PlayClipAtPoint(shootClip, transform.position);
    }

    // SOUND ĐẠN TRÚNG KIM LOẠI
    public void PlayMetalHit()
    {
        AudioSource.PlayClipAtPoint(hitMetalClip, transform.position);
    }

    // VA CHẠM
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 3f)
        {
            AudioSource.PlayClipAtPoint(crashClip, transform.position);
        }

        if (collision.gameObject.CompareTag("Metal"))
        {
            AudioSource.PlayClipAtPoint(hitMetalClip, transform.position);
        }
    }
}