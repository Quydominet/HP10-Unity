using UnityEngine;

public class AntiCheeseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        FindObjectOfType<NextScene>().AntiCheeseTouched();
        Debug.Log("YURI");
    }
}
