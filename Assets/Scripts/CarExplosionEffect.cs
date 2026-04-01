using UnityEngine;

public class CarExplosionEffect : MonoBehaviour
{
    public Material burntMaterial;
    public ParticleSystem explosionEffect;
    public ParticleSystem smokeEffect;

    private MeshRenderer[] meshRenderers;
    private Material[][] originalMaterials;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Cache all original materials (each renderer can have multiple)
        originalMaterials = new Material[meshRenderers.Length][];
        for (int i = 0; i < meshRenderers.Length; i++)
            originalMaterials[i] = meshRenderers[i].materials;
    }

    public void Explode()
    {
        foreach (MeshRenderer renderer in meshRenderers)
            renderer.material = burntMaterial;

        //explosionEffect?.Play();
        //smokeEffect?.Play();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(new Vector3(Random.Range(-100, 100), 500f, Random.Range(-100, 100)));
            rb.AddTorque(new Vector3(Random.Range(-500, 500), Random.Range(-500, 500), 0f));
        }
    }

    public void RemoveExplosion()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].materials = originalMaterials[i];

        if (explosionEffect != null) explosionEffect.Stop();
        if (smokeEffect != null) smokeEffect.Stop();
    }
}