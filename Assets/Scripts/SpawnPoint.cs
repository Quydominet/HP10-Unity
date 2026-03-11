using UnityEngine;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
    private HashSet <Collider> occupants = new HashSet<Collider>();

    public bool IsOccupied => occupants.Count > 0;

    private void OnTriggerEnter(Collider other)
    {
        occupants.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        occupants.Remove(other);
    }
}