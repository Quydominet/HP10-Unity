using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
public class CarExplosionEffect : MonoBehaviour
{
    public Material burntMaterial; // universal burnt material
    public ParticleSystem explosionEffect;
    public ParticleSystem smokeEffect;

    private MeshRenderer[] meshRenderers;

    void Start()
    {
        // Collect all mesh renderers in the car hierarchy
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    public void Explode()
    {
        // Play explosion particles
        //explosionEffect.Play();

        // Swap all materials to burnt instantly
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.material = burntMaterial;
        }

        Rigidbody rb = FindFirstObjectByType<Rigidbody>();

        if (rb != null)
        {
            Vector3 explosionForce = new Vector3(UnityEngine.Random.Range(-100, 100), 500f, UnityEngine.Random.Range(-100, 100));
            rb.AddForce(explosionForce);
            
            Vector3 spinTorque = new Vector3(UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-500, 500), 0f);
            rb.AddTorque(spinTorque);
        }

        // Start smoke effect
        // smokeEffect.Play();
    }
}
