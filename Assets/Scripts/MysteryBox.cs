using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    public float effectDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            int randomEffect = Random.Range(0, 2); // 0 hoặc 1

            if (randomEffect == 0)
            {
                Debug.Log("NITRO");
                other.GetComponent<CarEffects>()?.ActivateNitro(effectDuration);
            }
            else
            {
                Debug.Log("FLY");
                other.GetComponent<CarEffects>()?.ActivateFly(effectDuration);
            }

            Destroy(gameObject);
        }
    }
}
