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

        // Start smoke effect
       // smokeEffect.Play();
    }
}
