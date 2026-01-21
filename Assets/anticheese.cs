using UnityEngine;

public class AntiCheeseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        FindFirstObjectByType<NextScene>().AntiCheeseTouched();
        Debug.Log("YURI");
    }
}
