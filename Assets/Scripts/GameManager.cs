using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    SaveHandler IngameSave = new SaveHandler();

    private static readonly Dictionary<string, int> Defaults = new Dictionary<string, int>
    {
        ["Kills"] = 100,
        ["Level"] = 1,
        ["Exp"] = 0,

        ["Unlock0"] = 0, // Sedan -> 0, Muscle Car, F1 -> 1, Truck -> 2
        ["Unlock1"] = 0, // Brownings -> 0, Gatlings -> 1, Miniguns -> 2
        ["Unlock2"] = 0, // 50 Cal -> 0, 20mm -> 1, 40mm -> 2
        ["Unlock3"] = 0, // Armor Piercing -> 0, Explosive -> 1, Incendiary -> 2
        ["Unlock4"] = 0, // None -> 0, More HP -> 1, More HP -> 2
        ["Unlock5"] = 0, // None -> 0, Nitrous Upgrade -> 1, Acceleration Increase -> 2

        ["Slot0"] = 0, // Car
        ["Slot1"] = 0, // Gun
        ["Slot2"] = 0, // Ammo
        ["Slot3"] = 0, // Type
    };
    void Awake()
    {
#if !UNITY_EDITOR
        IngameSave.LoadData();

        foreach (var entry in Defaults)
        {
            if (!IngameSave.data.ContainsKey(entry.Key))
                IngameSave.data[entry.Key] = entry.Value;
        }
#endif
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        IngameSave.SaveData();
#endif
    }
}