using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPicker : MonoBehaviour
{
    private List <SpawnPoint> spawns = new List <SpawnPoint>();

    void Start()
    {
        ReloadSpawns();
    }

    private void OnTransformChildrenChanged()
    {
        ReloadSpawns();
    }

    private void ReloadSpawns()
    {
        spawns.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out SpawnPoint spawn))
                spawns.Add(spawn);
        }
    }

    public Transform GetBestSpawn()
    {
        // Prefer unoccupied spawns, fall back to any spawn
        SpawnPoint best = spawns.FirstOrDefault(s => !s.IsOccupied)
                          ?? spawns.FirstOrDefault();

        if (best == null)
            Debug.LogWarning("No spawns available!");

        return best?.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.gameObject.name} entered the trigger");

        if (other.GetComponentInParent<CarController>() != null)
        {
            CarController car = other.GetComponentInParent<CarController>();
            Transform bestSpawn = GetBestSpawn();

            if (bestSpawn != null)
            {
                car.transform.SetLocalPositionAndRotation(bestSpawn.position, bestSpawn.rotation);
            }
        }
    }
}