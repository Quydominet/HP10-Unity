using UnityEngine;

public class FloatingSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject obstaclePrefab;
    public int minAmount = 3;
    public int maxAmount = 8;
    public float spawnRadius = 100f;

    void Start()
    {
        int amount = Random.Range(minAmount, maxAmount + 1);

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPos = transform.position +
                                Random.insideUnitSphere * spawnRadius;

            randomPos.y = transform.position.y + Random.Range(1f, 4f);

            Instantiate(obstaclePrefab, randomPos, Random.rotation);
        }

        // Ẩn model gốc
        gameObject.SetActive(false);
    }
}
