using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDamageAPI : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageThreshold = 5f;   // Minimum impact force
    public float maxDentDepth = 0.3f;    // Maximum vertex displacement
    public float dentRadius = 0.5f;      // Radius around impact point
    public bool accumulateDamage = true; // Allow multiple dents
    public bool onImpact = true;         // Allow impact-sourced dents

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private Vector3[] normals;

    void Awake()
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        if (mf.sharedMesh == null || !mf.sharedMesh.isReadable)
        {
            Debug.LogError($"MeshDamageAPI on {gameObject.name}: Mesh not readable. Enable 'Read/Write Enabled' in import settings.");
            enabled = false;
            return;
        }

        // Clone mesh so we can safely modify it
        mesh = Instantiate(mf.sharedMesh);
        mf.mesh = mesh;

        originalVertices = mesh.vertices;
        deformedVertices = (Vector3[])mesh.vertices.Clone();
        normals = mesh.normals;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!onImpact) return;

        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= damageThreshold)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                ApplyDent(contact.point, contact.normal, impactForce);
            }
        }
    }

    /// <summary>
    /// Apply dent at a point with given force
    /// </summary>
    public void ApplyDent(Vector3 worldPoint, Vector3 normal, float force)
    {
        // Convert impact into local space
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector3 localNormal = transform.InverseTransformDirection(normal);

        for (int i = 0; i < deformedVertices.Length; i++)
        {
            Vector3 localPos = originalVertices[i];
            float distance = Vector3.Distance(localPos, localPoint);

            if (distance < dentRadius)
            {
                // Falloff strength
                float strength = (1f - (distance / dentRadius)) * (force / damageThreshold);
                strength = Mathf.Clamp01(strength);

                // Offset along local normal
                Vector3 offset = -localNormal * (strength * maxDentDepth);

                // Apply offset
                Vector3 localDeformed = deformedVertices[i] + offset;

                // Clamp displacement so it never exceeds maxDentDepth
                float displacement = (localDeformed - localPos).magnitude;
                if (displacement > maxDentDepth)
                {
                    localDeformed = localPos + (localDeformed - localPos).normalized * maxDentDepth;
                }

                deformedVertices[i] = localDeformed;
            }
        }

        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Reset mesh back to original state
    /// </summary>
    public void ResetDamage()
    {
        deformedVertices = (Vector3[])originalVertices.Clone();
        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();
    }
}